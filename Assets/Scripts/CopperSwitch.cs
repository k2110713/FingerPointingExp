using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CopperSwitch : MonoBehaviour
{
    //シリアル通信ハンドラー（呪文）
    public SerialHandler serialHandler;

    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;

    //ボタン
    public GameObject buttonObject;

    public TextMeshProUGUI PushedOrNot;

    // Start is called before the first frame update
    void Start()
    {
        //ランダム配置
        buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
        PushedOrNot.text = "Not Pushed";
        Begin.stopwatch.Start();
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
                // レイキャスト用のデータを作成
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = pointer2d.transform.position
                };
                //Debug.Log(pointerData.position);

                // レイキャスト結果を格納するリストを作成
                List<RaycastResult> results = new List<RaycastResult>();

                // レイキャストを実行
                EventSystem.current.RaycastAll(pointerData, results);

                // レイキャスト結果を確認
                foreach (RaycastResult result in results)
                {
                    UnityEngine.Debug.Log(result.ToString());
                    // 結果がButtonコンポーネントを持つ場合
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        //PushedOrNot.text = "Pushed";
                        UnityEngine.Debug.Log("Pushed");
                        Begin.correctCount++;
                        break;
                    }
                }
                Begin.cnt++;
                UnityEngine.Debug.Log(Begin.correctCount.ToString() + Begin.cnt.ToString());
                //if (Begin.cnt < Begin.testNumInOnce)
                if (true)
                {
                    buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
                }
                else
                {
                    Begin.stopwatch.Stop();
                }
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
