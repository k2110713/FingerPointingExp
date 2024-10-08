using System;
using System.IO;
using System.Text;
using UnityEngine;

// csv�ɕۑ����邽�߂̃R�[�h
// SaveCsv�փA�^�b�`
public class WriteToCSV : MonoBehaviour
{
    // System.IO
    private StreamWriter sw;

    //�팱�҂̖��O
    public string subjectName = "unknown";

    // Start is called before the first frame update
    void Start()
    {
        DateTime now = DateTime.Now;
        string fi = Application.dataPath + "/results/" + now.ToString($"{now:yyyyMMdd}") + "_" + subjectName + ".csv";
        Debug.Log(fi);

        //�ۑ��p��Excel������
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
            // �t�@�C�����J���Afalse�F�㏑��
            StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8);
            //�t�@�C���ɏ�������
            for (int i = 0; i < 5; i++)
            {
                file.WriteLine(now.ToString($"{now:yyyyMMddHHmmss}") + ",3ArashiKashiwagi," + "false");
            }
            //�t�@�C�������
            file.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message); // ��O���o���ɃG���[���b�Z�[�W��\��
        }
    }

    private bool isExistsSaveFile(string fi)
    {
        bool isExist = File.Exists(fi);
        return isExist;
    }
}
