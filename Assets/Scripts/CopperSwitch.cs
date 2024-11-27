using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CopperSwitch : MonoBehaviour
{
    //�V���A���ʐM�n���h���[�i�����j
    public SerialHandler serialHandler;

    // Start is called before the first frame update
    void Start()
    {
        if (serialHandler != null)
        {
            serialHandler.OnDataReceived += OnDataReceived;
        }
        else
        {
            UnityEngine.Debug.LogWarning("SerialHandler is not assigned.");
        }
    }

    //��M�����M��(message)�ɑ΂��鏈��
    void OnDataReceived(string message)
    {
        //UnityEngine.Debug.Log("received");
        var data = message.Split(
                new string[] { "\n" }, System.StringSplitOptions.None);
        try
        {
            //UnityEngine.Debug.Log(data[0]);//Unity�̃R���\�[���Ɏ�M�f�[�^��\��
            if (data[0] == "1")
            {
                UnityEngine.Debug.Log("tapped!");
                OptiTrackHandler.Instance.SetTrigger(); //�g���K�[���M
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning(e.Message);//�G���[��\��
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
