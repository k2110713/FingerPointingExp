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
    private string projectRoot = Application.dataPath + "/.."; // Assetsフォルダの親ディレクトリ
    private string debugLogFilePath;

    void Start()
    {
        // ログ記録用ファイルのセットアップ
        SetupDebugLogFile();

        // Unityのログイベントをリッスン
        Application.logMessageReceived += LogDebugMessages;

        // 以下、既存コード
        MonoBehaviour[] scripts = pointer.GetComponents<MonoBehaviour>();

        bool hasTouchingHandler = scripts.OfType<TouchingHandler>().Any();
        bool hasPointingHandler = scripts.OfType<PointingHandler>().Any();

        if (hasTouchingHandler)
        {
            var touchingHandler = scripts.OfType<TouchingHandler>().First();
            if (touchingHandler.enabled)
            {
                method = "Touching";
            }
        }

        if (hasPointingHandler)
        {
            var pointingHandler = scripts.OfType<PointingHandler>().First();
            if (pointingHandler.enabled)
            {
                method = "Pointing";
            }
        }
    }

    private void SetupDebugLogFile()
    {
        DateTime now = DateTime.Now;
        formattedDate = now.ToString("yyyyMMddHHmmss");
        debugLogFilePath = Path.Combine(projectRoot, "logs", $"{subjectName}_{formattedDate}_debug.csv");

        if (!Directory.Exists(Path.Combine(projectRoot, "logs")))
        {
            Directory.CreateDirectory(Path.Combine(projectRoot, "logs"));
        }

        // ヘッダー行を作成
        if (!File.Exists(debugLogFilePath))
        {
            using (StreamWriter file = new StreamWriter(debugLogFilePath, false, Encoding.UTF8))
            {
                file.WriteLine("Timestamp,LogType,Message");
            }
        }
    }

    private void LogDebugMessages(string condition, string stackTrace, LogType type)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logType = type.ToString();
        string logMessage = $"{condition.Replace(",", ";")}";

        try
        {
            using (StreamWriter file = new StreamWriter(debugLogFilePath, true, Encoding.UTF8))
            {
                file.WriteLine($"{timestamp},{logType},{logMessage}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write debug log to file {debugLogFilePath}: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        // ログイベントを解除
        Application.logMessageReceived -= LogDebugMessages;
    }

    // 既存のCSV書き込みコード（省略）

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

        startTime = DateTime.Now; // タスクのストップウォッチを開始
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

                // allTaskOrders の内容をログに書き込む
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
