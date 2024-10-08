using System;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

// csv�ɕۑ����邽�߂̃R�[�h
// SaveCsv�փA�^�b�`
public class WriteToCSV : MonoBehaviour
{
    // System.IO
    private StreamWriter sw;

    public GameObject pointer;
    public GameObject button;

    //�팱�҂̖��O
    public string subjectName = "unknown";

    private string fi;

    private bool flg = true;

    // Start is called before the first frame update
    void Start()
    {
        DateTime now = DateTime.Now;
        fi = Application.dataPath + "/results/" + subjectName + "_" + Begin.modeStatic + "_" + now.ToString("yyyyMMdd") + ".csv";
        Debug.Log(fi);

        // �t�@�C�������݂��Ȃ��ꍇ�Ƀw�b�_�[�s��ǉ�
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
        //�{�ԃt�F�[�Y�ŁA�{�^�����A�N�e�B�u�i�e�X�g���Ƃ������Ɓj
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
            // �t�@�C�����J���Atrue�F�ǋL
            using (StreamWriter file = new StreamWriter(fi, true, Encoding.UTF8))
            {
                //�|�C���^�̍��W����������
                file.WriteLine(
                    Begin.stopwatch.ElapsedMilliseconds + "," +
                    Begin.cnt + "," + Begin.correctCount + "," +
                    //pointer.transform.position.x + "," +
                    //pointer.transform.position.y + "," +
                    Begin.modeStatic + "," + //���̓��[�h
                    (Begin.currentNum - 2) //�e�X�g�ԍ�(1~5)
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message); // ��O���o���ɃG���[���b�Z�[�W��\��
        }
    }

    private bool isExistsSaveFile(string fi)
    {
        return File.Exists(fi);
    }
}
