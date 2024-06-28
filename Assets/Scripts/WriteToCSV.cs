using System;
using System.IO;
using System.Text;
using UnityEngine;

// csvに保存するためのコード
// SaveCsvへアタッチ
public class WriteToCSV : MonoBehaviour
{
    // System.IO
    private StreamWriter sw;

    //被験者の名前
    public string subjectName = "unknown";

    // Start is called before the first frame update
    void Start()
    {
        DateTime now = DateTime.Now;
        string fi = Application.dataPath + "/results/" + now.ToString($"{now:yyyyMMdd}") + "_" + subjectName + ".csv";
        Debug.Log(fi);

        //保存用のExcelを検索
        /*if (!isExistsSaveFile(fi))
        {
            Debug.Log("make");
            writeDataToFile(fi);
        }*/
        writeDataToFile(fi);

    }

    private void writeDataToFile(string fi)
    {
        DateTime now = DateTime.Now;
        try
        {
            // ファイルを開く、false：上書き
            StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8);
            //ファイルに書き込む
            for (int i = 0; i < 5; i++)
            {
                file.WriteLine(now.ToString($"{now:yyyyMMddHHmmss}") + ",3ArashiKashiwagi," + "false");
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
