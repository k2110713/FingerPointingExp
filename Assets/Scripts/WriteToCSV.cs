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
    public string subjectName = "unknown"; // デフォルトの名前
    public InputMethod inputMethod;
    private string projectRoot = Application.dataPath + "/.."; // Assetsフォルダの親ディレクトリ
    private string experimentFilePath;
    private string logFilePath;
    private string infoFilePath;
    private string formattedDate;
    private float radius;
    private DateTime startTime;
    private LogType logType = LogType.Log;

    void Start()
    {
        // formattedDateを一度だけ宣言し、すべてのファイルに同じ日時を使用
        formattedDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // ログファイルと実験結果ファイルのパスを設定
        SetupLogFile();
        SetupExperimentFile();

        // 実験結果ログにヘッダーを追加
        File.AppendAllText(experimentFilePath, "ElapsedTime,TaskNumber,TargetNumber\n");
    }

    // ログファイルのセットアップ（ログの内容をCSVに記録）
    private void SetupLogFile()
    {
        logFilePath = Path.Combine(projectRoot, "MyLogs", $"{subjectName}_Log_{formattedDate}.csv");

        // ヘッダーを追加
        File.AppendAllText(logFilePath, "Time,LogType,Message\n");

        // ログイベントにリスナーを登録
        Application.logMessageReceived += HandleLog;
    }

    // 実験結果ファイルのセットアップ（結果をCSVに記録）
    private void SetupExperimentFile()
    {
        string method = inputMethod.ToString();
        experimentFilePath = Path.Combine(projectRoot, "results", $"{subjectName}_Experiment_{formattedDate}_Radius{radius}_{method}.csv");

        // ヘッダーを追加
        File.AppendAllText(experimentFilePath, "ElapsedTime,TaskNumber,TargetNumber\n");
    }

    // 実験情報ファイルのセットアップ（実験情報を記録）
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
            Debug.LogError($"Failed to write experiment info to file {infoFilePath}: {e.Message}");
        }
    }

    // タスクデータの記録（実験結果をCSVに書き込む）
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

    // 実験開始時に呼ばれる（開始時間の設定）
    public void SetupFile(float radius)
    {
        this.radius = radius;
        startTime = DateTime.Now;  // タスクのストップウォッチを開始
    }

    // ログイベントをCSVに記録（エラーログやデバッグログなど）
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 現在の時間をミリ秒まで取得
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // CSVフォーマットに整形
        string logEntry = $"{time},{type},{EscapeForCSV(logString)}\n";

        // ログをファイルに書き込む
        try
        {
            File.AppendAllText(logFilePath, logEntry);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to log file {logFilePath}: {e.Message}");
        }
    }

    // CSVフォーマットに適したエスケープ処理
    private string EscapeForCSV(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            value = $"\"{value}\"";
        }
        return value;
    }

    // リスナーの解除
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
