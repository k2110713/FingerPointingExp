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
        fi = Application.dataPath + "/results/" + subjectName + "_" + Begin.modeStatic + "_" + now.ToString($"{now:yyyyMMdd}") + ".csv";
        Debug.Log(fi);
    }

    private void Update()
    {
        //本番フェーズで、ボタンがアクティブ（テスト中ということ）
        if (Begin.currentNum > Begin.practiceNum && button.activeSelf)
        {
            try
            {
                // ファイルを開く、false：上書き
                StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8);
                //ポインタの座標を書き込む
                file.WriteLine(
                    Begin.stopwatch.ElapsedMilliseconds + "," +
                    Begin.cnt + "," + Begin.correctCount + "," +
                    //pointer.transform.position.x + "," +
                    //pointer.transform.position.y + "," +
                    Begin.modeStatic + "," + //入力モード
                    (Begin.currentNum - 2) //テスト番号(1~5)
                    );
                //ファイルを閉じる
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); // 例外検出時にエラーメッセージを表示
            }
        }
        if (Begin.cnt == 10 && flg)
        {
            try
            {
                // ファイルを開く、false：上書き
                StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8);
                //ポインタの座標を書き込む
                file.WriteLine(
                    Begin.stopwatch.ElapsedMilliseconds + "," +
                    Begin.cnt + "," + Begin.correctCount + "," +
                    //pointer.transform.position.x + "," +
                    //pointer.transform.position.y + "," +
                    Begin.modeStatic + "," + //入力モード
                    (Begin.currentNum - 2) //テスト番号(1~5)
                    );
                //ファイルを閉じる
                file.Close();
                flg = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); // 例外検出時にエラーメッセージを表示
            }
        }
    }

    private bool isExistsSaveFile(string fi)
    {
        bool isExist = File.Exists(fi);
        return isExist;
    }
}
