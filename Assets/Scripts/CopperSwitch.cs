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

    //クールダウンタイム
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    // シングルトンインスタンス
    public static CopperSwitch Instance { get; private set; }

    // トリガーフラグ
    public bool isTriggered { get; private set; } = false;

    private void Awake()
    {
        // インスタンスが既に存在する場合は破棄し、存在しない場合はこのインスタンスを設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンが切り替わっても破棄されないように設定
        }
    }

    // トリガーを発生させるメソッド
    public void ActivateTrigger()
    {
        isTriggered = true;
    }

    // トリガーをリセットするメソッド
    public void ResetTrigger()
    {
        isTriggered = false;
    }

    // Start is called before the first frame update
    void Start()
    {
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
                isTriggered = true;
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
