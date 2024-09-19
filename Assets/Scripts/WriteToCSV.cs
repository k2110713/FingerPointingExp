using System;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

// csvに保存するためのコード
// SaveCsvへアタッチ
public class WriteToCSV : MonoBehaviour
{
    // System.IO
    private StreamWriter sw;

    public GameObject pointer;
    public GameObject button;

    //被験者の名前
    public string subjectName = "unknown";

    private string fi;

    private bool flg = true;

    // Start is called before the first frame update
    void Start()
    {
        DateTime now = DateTime.Now;
        fi = Application.dataPath + "/results/" + subjectName + "_" + Begin.modeStatic + "_" + now.ToString("yyyyMMdd") + ".csv";
        Debug.Log(fi);

        // ファイルが存在しない場合にヘッダー行を追加
        if (!File.Exists(fi))
        {
            using (StreamWriter file = new StreamWriter(fi, false, Encoding.UTF8))
            {
                file.WriteLine("ElapsedMilliseconds,Count,CorrectCount,ModeStatic,TestNumber");
            }
        }
    }

    private void Update()
    {
        //本番フェーズで、ボタンがアクティブ（テスト中ということ）
        if (Begin.currentNum > Begin.practiceNum && button.activeSelf)
        {
            WriteDataToCSV();
        }

        if (Begin.cnt == 10 && flg)
        {
            WriteDataToCSV();
            flg = false;
        }
    }

    private void WriteDataToCSV()
    {
        try
        {
            // ファイルを開く、true：追記
            using (StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8))
            {
                //ポインタの座標を書き込む
                file.WriteLine(
                    Begin.stopwatch.ElapsedMilliseconds + "," +
                    Begin.cnt + "," + Begin.correctCount + "," +
                    //pointer.transform.position.x + "," +
                    //pointer.transform.position.y + "," +
                    Begin.modeStatic + "," + //入力モード
                    (Begin.currentNum - 2) //テスト番号(1~5)
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message); // 例外検出時にエラーメッセージを表示
        }
    }

    private bool isExistsSaveFile(string fi)
    {
        return File.Exists(fi);
    }
}
