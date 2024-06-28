using System.Collections.Generic;
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

    public TextMeshProUGUI PushedOrNot;

    //ボタンを押せた回数
    public static int correctCount = 0;

    public RectTransform canvasRectTransform; // Canvas の RectTransform
    public RectTransform buttonRectTransform; // Button の RectTransform

    // Start is called before the first frame update
    void Start()
    {
        //ランダム配置
        PlaceButtonRandomly();
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
            Debug.Log(data[0]);//Unityのコンソールに受信データを表示
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
                    Debug.Log(result.ToString());
                    // 結果がButtonコンポーネントを持つ場合
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        PushedOrNot.text = "Pushed";
                        Debug.Log("Pushed");
                        correctCount++;
                        break;
                    }
                }
                Debug.Log(correctCount);
                PlaceButtonRandomly();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);//エラーを表示
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlaceButtonRandomly()
    {
        // Canvas のサイズを取得

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        // Button のサイズを取得
        float buttonWidth = buttonRectTransform.rect.width;
        float buttonHeight = buttonRectTransform.rect.height;

        // ランダムな位置を計算 (ボタンが画面外に出ないように)
        float randomX = Random.Range(buttonWidth / 2, canvasWidth - buttonWidth / 2);
        float randomY = Random.Range(buttonHeight / 2, canvasHeight - buttonHeight / 2);

        // ボタンの位置を設定
        buttonRectTransform.position = new Vector3(randomX, randomY, 1);
    }
}
