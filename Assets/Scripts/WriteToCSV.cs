using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum InputMethod
{
    Touching,
    Pointing
}

public class WriteToCSV : MonoBehaviour
{
    public string subjectName = "unknown"; // �f�t�H���g�̖��O
    public InputMethod inputMethod;
    private string projectRoot = Application.dataPath + "/.."; // Assets�t�H���_�̐e�f�B���N�g��
    private string experimentFilePath;
    private string logFilePath;
    private string infoFilePath;
    private string formattedDate;
    private float radius;
    private DateTime startTime;
    private LogType logType = LogType.Log;

    void Start()
    {
        // formattedDate����x�����錾���A���ׂẴt�@�C���ɓ����������g�p
        formattedDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // ���O�t�@�C���Ǝ������ʃt�@�C���̃p�X��ݒ�
        SetupLogFile();
        SetupExperimentFile();

        // �������ʃ��O�Ƀw�b�_�[��ǉ�
        File.AppendAllText(experimentFilePath, "ElapsedTime,TaskNumber,TargetNumber\n");
    }

    // ���O�t�@�C���̃Z�b�g�A�b�v�i���O�̓��e��CSV�ɋL�^�j
    private void SetupLogFile()
    {
        logFilePath = Path.Combine(projectRoot, "MyLogs", $"{subjectName}_Log_{formattedDate}.csv");

        // �w�b�_�[��ǉ�
        File.AppendAllText(logFilePath, "Time,LogType,Message\n");

        // ���O�C�x���g�Ƀ��X�i�[��o�^
        Application.logMessageReceived += HandleLog;
    }

    // �������ʃt�@�C���̃Z�b�g�A�b�v�i���ʂ�CSV�ɋL�^�j
    private void SetupExperimentFile()
    {
        string method = inputMethod.ToString();
        experimentFilePath = Path.Combine(projectRoot, "results", $"{subjectName}_Experiment_{formattedDate}_Radius{radius}_{method}.csv");

        // �w�b�_�[��ǉ�
        File.AppendAllText(experimentFilePath, "ElapsedTime,TaskNumber,TargetNumber\n");
    }

    // �������t�@�C���̃Z�b�g�A�b�v�i���������L�^�j
    public void SetupExperimentInfoFile(List<List<int>> allTaskOrders)
    {
        string infoFileName = $"{subjectName}_Info_{formattedDate}_Radius{radius}.csv";
        infoFilePath = Path.Combine(projectRoot, "results", infoFileName);

        try
        {
            using (StreamWriter file = new StreamWriter(infoFilePath, false, Encoding.UTF8))
            {
                file.WriteLine($"SubjectName,{subjectName}");
                file.WriteLine($"Date,{formattedDate}");
                file.WriteLine($"InputMethod,{inputMethod}");
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
            Debug.LogError($"Failed to write experiment info to file {infoFilePath}: {e.Message}");
        }
    }

    // �^�X�N�f�[�^�̋L�^�i�������ʂ�CSV�ɏ������ށj
    public void LogData(int taskNumber, int targetNumber)
    {
        TimeSpan elapsedTime = DateTime.Now - startTime;
        string line = $"{elapsedTime.TotalSeconds},{taskNumber},{targetNumber}";

        try
        {
            using (StreamWriter file = new StreamWriter(experimentFilePath, true, Encoding.UTF8))
            {
                file.WriteLine(line);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to file {experimentFilePath}: {e.Message}");
        }
    }

    // �����J�n���ɌĂ΂��i�J�n���Ԃ̐ݒ�j
    public void SetupFile(float radius)
    {
        this.radius = radius;
        startTime = DateTime.Now;  // �^�X�N�̃X�g�b�v�E�H�b�`���J�n
    }

    // ���O�C�x���g��CSV�ɋL�^�i�G���[���O��f�o�b�O���O�Ȃǁj
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // ���݂̎��Ԃ��~���b�܂Ŏ擾
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // CSV�t�H�[�}�b�g�ɐ��`
        string logEntry = $"{time},{type},{EscapeForCSV(logString)}\n";

        // ���O���t�@�C���ɏ�������
        try
        {
            File.AppendAllText(logFilePath, logEntry);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to log file {logFilePath}: {e.Message}");
        }
    }

    // CSV�t�H�[�}�b�g�ɓK�����G�X�P�[�v����
    private string EscapeForCSV(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            value = $"\"{value}\"";
        }
        return value;
    }

    // ���X�i�[�̉���
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
