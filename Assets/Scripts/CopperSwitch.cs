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
    //シリアル通信ハンドラー（呪文）
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

    //受信した信号(message)に対する処理
    void OnDataReceived(string message)
    {
        //UnityEngine.Debug.Log("received");
        var data = message.Split(
                new string[] { "\n" }, System.StringSplitOptions.None);
        try
        {
            //UnityEngine.Debug.Log(data[0]);//Unityのコンソールに受信データを表示
            if (data[0] == "1")
            {
                UnityEngine.Debug.Log("tapped!");
                OptiTrackHandler.Instance.SetTrigger(); //トリガー送信
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning(e.Message);//エラーを表示
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
