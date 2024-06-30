using System;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

// csvに保存するためのコード
// SaveCsvへアタッチ
public class WriteToCSV : MonoBehaviour
{
    // System.IO
    private StreamWriter sw;

    public GameObject pointer;

    //被験者の名前
    public string subjectName = "unknown";

    private string fi;

    // Start is called before the first frame update
    void Start()
    {
        DateTime now = DateTime.Now;
        fi = Application.dataPath + "/results/" + now.ToString($"{now:yyyyMMdd}") + "_" + subjectName + ".csv";
        Debug.Log(fi);

        //保存用のExcelを検索
        /*if (!isExistsSaveFile(fi))
        {
            Debug.Log("make");
            writeDataToFile(fi);
        }*/
    }

    private void Update()
    {
        try
        {
            // ファイルを開く、false：上書き
            StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8);
            //ポインタの座標を書き込む
            file.WriteLine(
                Begin.stopwatch.ElapsedMilliseconds + "," +
                Begin.cnt + "," + Begin.correctCount + "," +
                pointer.transform.position.x + "," +
                pointer.transform.position.y
                );

            //終了時
            //if (Begin.cnt == 10) //通常
            if (Input.GetKeyUp(KeyCode.Return)) //エンドレス
            {
                //ファイルを閉じる
                file.Close();
                //終了
                Debug.Log("quit");
                Application.Quit();
            }
            //ファイルを閉じる
            file.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message); // 例外検出時にエラーメッセージを表示
        }
    }

    private bool isExistsSaveFile(string fi)
    {
        bool isExist = File.Exists(fi);
        return isExist;
    }
}
