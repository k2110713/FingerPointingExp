using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class WriteToCSV : MonoBehaviour
{
    public string subjectName = "unknown";
    public GameObject pointer;
    private string filePath;
    private string formattedDate;
    private float radius;
    private string method;
    private DateTime startTime;
    private string projectRoot = Application.dataPath + "/.."; // Assets�t�H���_�̐e�f�B���N�g��

    void Start()
    {
        // �Q�[���I�u�W�F�N�g�ɃA�^�b�`����Ă���S�Ă�MonoBehaviour�X�N���v�g���擾
        MonoBehaviour[] scripts = pointer.GetComponents<MonoBehaviour>();

        // TouchingHandler �� PointingHandler �����݂��邩�m�F
        bool hasTouchingHandler = scripts.OfType<TouchingHandler>().Any();
        bool hasPointingHandler = scripts.OfType<PointingHandler>().Any();

        // TouchingHandler�����݂���΂��̏�Ԃ�����
        if (hasTouchingHandler)
        {
            var touchingHandler = scripts.OfType<TouchingHandler>().First();
            if (touchingHandler.enabled)
            {
                method = "Touching";
            }
        }

        // PointingHandler�����݂���΂��̏�Ԃ�����
        if (hasPointingHandler)
        {
            var pointingHandler = scripts.OfType<PointingHandler>().First();
            if (pointingHandler.enabled)
            {
                method = "Pointing";
            }
        }
    }

    public void SetupFile(float radius)
    {
        this.radius = radius;
        DateTime now = DateTime.Now;
        formattedDate = now.ToString("yyyyMMddHHmm");
        string fileName = $"{subjectName}_{formattedDate}_{radius}_{method}.csv";
        filePath = Path.Combine(projectRoot, "results", fileName);

        if (!File.Exists(filePath))
        {
            using (StreamWriter file = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                file.WriteLine("ElapsedTime,TaskNumber,TargetNumber");
            }
        }

        startTime = DateTime.Now;  // �^�X�N�̃X�g�b�v�E�H�b�`���J�n
    }

    public void LogExperimentResult(int taskNumber, int targetNumber)
    {
        TimeSpan elapsedTime = DateTime.Now - startTime;
        string line = $"{elapsedTime.TotalSeconds},{taskNumber},{targetNumber}";

        try
        {
            using (StreamWriter file = new StreamWriter(filePath, true, Encoding.UTF8))
            {
                file.WriteLine(line);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to file {filePath}: {e.Message}");
        }
    }

    public void LogExperimentInfo(List<List<int>> allTaskOrders)
    {
        string infoFileName = $"{subjectName}_{formattedDate}_{radius}_{method}_info.csv";
        string infoPath = Path.Combine(projectRoot, "results", infoFileName);

        try
        {
            using (StreamWriter file = new StreamWriter(infoPath, false, Encoding.UTF8))
            {
                file.WriteLine($"SubjectName,{subjectName}");
                file.WriteLine($"Date,{formattedDate}");
                file.WriteLine($"InputMethod,{method}");
                file.WriteLine($"Radius,{radius}");

                // allTaskOrders �̓��e�����O�ɏ�������
                file.WriteLine("All Task Orders:");
                foreach (var taskOrder in allTaskOrders)
                {
                    file.WriteLine(string.Join(",", taskOrder));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write experiment info to file {infoPath}: {e.Message}");
        }
    }
}
