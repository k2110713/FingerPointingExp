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

    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;

    //ボタン
    public GameObject buttonObject;

    //クールダウンタイム
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        //ランダム配置
        buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();

        //指さし入力方法（押す動作）
        if (Begin.modeStatic == 2 || Begin.modeStatic == 3)
        {
            serialHandler.OnDataReceived += OnDataReceived;
            Begin.stopwatch = Stopwatch.StartNew();
        }
    }

    //受信した信号(message)に対する処理
    void OnDataReceived(string message)
    {
        var data = message.Split(
                new string[] { "\n" }, System.StringSplitOptions.None);
        try
        {
            UnityEngine.Debug.Log(data[0]);//Unityのコンソールに受信データを表示
            if (data[0] == "1")
            {
                //ボタン上かどうか判定
                if (IsButtonAtPositionRay(pointer2d.transform.position))
                {
                    //PushedOrNot.text = "Pushed";
                    //UnityEngine.Debug.Log("Pushed");
                    Begin.correctCount++;
                }
                Begin.cnt++;
                //UnityEngine.Debug.Log(Begin.correctCount.ToString() + Begin.cnt.ToString());

                if (Begin.cnt < Begin.testNumInOnce)
                //if (true)
                {
                    buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
                }
                else
                {
                    Begin.stopwatch.Stop();
                    Cooldown();
                    buttonObject.SetActive(false);
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning(e.Message);//エラーを表示
        }
    }

    // 指定された座標にボタンが存在しているかどうかを確認する関数
    bool IsButtonAtPositionRay(Vector3 position)
    {
        // レイキャスト用のデータを作成
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        // レイキャスト結果を格納するリストを作成
        List<RaycastResult> results = new List<RaycastResult>();

        // レイキャストを実行
        EventSystem.current.RaycastAll(pointerData, results);

        // レイキャスト結果を確認
        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                // 指定された座標にボタンが存在する場合
                return true;
            }
        }

        // 指定された座標にボタンが存在しない場合
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        //１試行の終了
        if (Input.GetKeyUp(KeyCode.Return) && Begin.cnt >= Begin.testNumInOnce)
        {
            if (Begin.currentNum == Begin.practiceNum + Begin.testNum)
            {
                //全試行の終了
                Application.Quit();
            }
            else
            {
                //次の試行に移る
                Begin.currentNum++;
                serialHandler.Close();
                SceneManager.LoadScene("TestTrial1");
            }
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
