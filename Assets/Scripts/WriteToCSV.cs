using System;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

// csv�ɕۑ����邽�߂̃R�[�h
// SaveCsv�փA�^�b�`
public class WriteToCSV : MonoBehaviour
{
    // System.IO
    private StreamWriter sw;

    public GameObject pointer;

    //�팱�҂̖��O
    public string subjectName = "unknown";

    private string fi;

    // Start is called before the first frame update
    void Start()
    {
        DateTime now = DateTime.Now;
        fi = Application.dataPath + "/results/" + now.ToString($"{now:yyyyMMdd}") + "_" + subjectName + ".csv";
        Debug.Log(fi);

        //�ۑ��p��Excel������
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
            // �t�@�C�����J���Afalse�F�㏑��
            StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8);
            //�|�C���^�̍��W����������
            file.WriteLine(
                Begin.stopwatch.ElapsedMilliseconds + "," +
                Begin.cnt + "," + Begin.correctCount + "," +
                pointer.transform.position.x + "," +
                pointer.transform.position.y
                );

            //�I����
            //if (Begin.cnt == 10) //�ʏ�
            if (Input.GetKeyUp(KeyCode.Return)) //�G���h���X
            {
                //�t�@�C�������
                file.Close();
                //�I��
                Debug.Log("quit");
                Application.Quit();
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
