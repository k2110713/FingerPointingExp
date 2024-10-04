using System;
using System.Collections.Generic;
using UnityEngine;
using NaturalPoint.NatNetLib;
using System.Net;

public class OptitrackMarkerDisplay : MonoBehaviour
{
    // サーバーアドレスとローカルアドレス
    public string ServerAddress = "127.0.0.1";
    public string LocalAddress = "127.0.0.1";

    // NatNetクライアント
    private NatNetClient m_client;

    // マーカーの最新状態を保持するディクショナリ
    private Dictionary<Int32, OptitrackMarkerState> m_latestMarkerStates = new Dictionary<Int32, OptitrackMarkerState>();

    // フレームデータのロック
    private object m_frameDataUpdateLock = new object();

    // 更新処理
    void Update()
    {
        // マーカーの座標とIDを表示する処理
        lock (m_frameDataUpdateLock)
        {
            foreach (KeyValuePair<Int32, OptitrackMarkerState> markerEntry in m_latestMarkerStates)
            {
                Debug.Log($"Marker ID: {markerEntry.Value.Id}, Position: {markerEntry.Value.Position}");
            }
        }
    }

    // NatNetフレーム受信時の処理
    private void OnNatNetFrameReceived(object sender, NatNetClient.NativeFrameReceivedEventArgs eventArgs)
    {
        IntPtr pFrame = eventArgs.NativeFramePointer;
        NatNetError result;

        // マーカー数の取得
        Int32 MarkerCount;
        result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetLabeledMarkerCount(pFrame, out MarkerCount);

        // 最新のマーカー状態をクリア
        m_latestMarkerStates.Clear();

        // すべてのマーカーの座標を取得
        for (int markerIdx = 0; markerIdx < MarkerCount; ++markerIdx)
        {
            sMarker marker = new sMarker();
            result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetLabeledMarker(pFrame, markerIdx, out marker);

            // マーカーの座標を取得して保存
            OptitrackMarkerState markerState = GetOrCreateMarkerState(marker.Id);
            markerState.Position = new Vector3(-marker.X, marker.Y, marker.Z); // X軸は符号を反転
            markerState.Id = marker.Id;

            // 状態をディクショナリに保存
            m_latestMarkerStates[marker.Id] = markerState;
        }
    }

    // マーカーの状態を作成または取得する
    private OptitrackMarkerState GetOrCreateMarkerState(Int32 markerId)
    {
        if (!m_latestMarkerStates.ContainsKey(markerId))
        {
            m_latestMarkerStates[markerId] = new OptitrackMarkerState();
        }
        return m_latestMarkerStates[markerId];
    }

    // サーバーに接続
    void OnEnable()
    {
        IPAddress serverAddr = IPAddress.Parse(ServerAddress);
        IPAddress localAddr = IPAddress.Parse(LocalAddress);

        m_client = new NatNetClient();
        m_client.Connect(NatNetConnectionType.NatNetConnectionType_Unicast, localAddr, serverAddr);

        m_client.NativeFrameReceived += OnNatNetFrameReceived;
    }

    // サーバーから切断
    void OnDisable()
    {
        m_client.NativeFrameReceived -= OnNatNetFrameReceived;
        m_client.Disconnect();
        m_client = null;
    }
}
