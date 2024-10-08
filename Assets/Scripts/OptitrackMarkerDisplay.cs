using System;
using System.Collections.Generic;
using UnityEngine;
using NaturalPoint.NatNetLib;
using System.Net;

public class OptitrackMarkerDisplay : MonoBehaviour
{
    // �T�[�o�[�A�h���X�ƃ��[�J���A�h���X
    public string ServerAddress = "127.0.0.1";
    public string LocalAddress = "127.0.0.1";

    // NatNet�N���C�A���g
    private NatNetClient m_client;

    // �}�[�J�[�̍ŐV��Ԃ�ێ�����f�B�N�V���i��
    private Dictionary<Int32, OptitrackMarkerState> m_latestMarkerStates = new Dictionary<Int32, OptitrackMarkerState>();

    // �t���[���f�[�^�̃��b�N
    private object m_frameDataUpdateLock = new object();

    // �X�V����
    void Update()
    {
        // �}�[�J�[�̍��W��ID��\�����鏈��
        lock (m_frameDataUpdateLock)
        {
            foreach (KeyValuePair<Int32, OptitrackMarkerState> markerEntry in m_latestMarkerStates)
            {
                Debug.Log($"Marker ID: {markerEntry.Value.Id}, Position: {markerEntry.Value.Position}");
            }
        }
    }

    // NatNet�t���[����M���̏���
    private void OnNatNetFrameReceived(object sender, NatNetClient.NativeFrameReceivedEventArgs eventArgs)
    {
        IntPtr pFrame = eventArgs.NativeFramePointer;
        NatNetError result;

        // �}�[�J�[���̎擾
        Int32 MarkerCount;
        result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetLabeledMarkerCount(pFrame, out MarkerCount);

        // �ŐV�̃}�[�J�[��Ԃ��N���A
        m_latestMarkerStates.Clear();

        // ���ׂẴ}�[�J�[�̍��W���擾
        for (int markerIdx = 0; markerIdx < MarkerCount; ++markerIdx)
        {
            sMarker marker = new sMarker();
            result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetLabeledMarker(pFrame, markerIdx, out marker);

            // �}�[�J�[�̍��W���擾���ĕۑ�
            OptitrackMarkerState markerState = GetOrCreateMarkerState(marker.Id);
            markerState.Position = new Vector3(-marker.X, marker.Y, marker.Z); // X���͕����𔽓]
            markerState.Id = marker.Id;

            // ��Ԃ��f�B�N�V���i���ɕۑ�
            m_latestMarkerStates[marker.Id] = markerState;
        }
    }

    // �}�[�J�[�̏�Ԃ��쐬�܂��͎擾����
    private OptitrackMarkerState GetOrCreateMarkerState(Int32 markerId)
    {
        if (!m_latestMarkerStates.ContainsKey(markerId))
        {
            m_latestMarkerStates[markerId] = new OptitrackMarkerState();
        }
        return m_latestMarkerStates[markerId];
    }

    // �T�[�o�[�ɐڑ�
    void OnEnable()
    {
        IPAddress serverAddr = IPAddress.Parse(ServerAddress);
        IPAddress localAddr = IPAddress.Parse(LocalAddress);

        m_client = new NatNetClient();
        m_client.Connect(NatNetConnectionType.NatNetConnectionType_Unicast, localAddr, serverAddr);

        m_client.NativeFrameReceived += OnNatNetFrameReceived;
    }

    // �T�[�o�[����ؒf
    void OnDisable()
    {
        m_client.NativeFrameReceived -= OnNatNetFrameReceived;
        m_client.Disconnect();
        m_client = null;
    }
}
