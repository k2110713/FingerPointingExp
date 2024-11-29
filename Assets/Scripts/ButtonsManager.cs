using UnityEngine;
using System.Collections.Generic;

public class ButtonsManager : MonoBehaviour
{
    public int setCount = 1; // セット数
    public int buttonCount = 9; // ボタンの数
    public float radius = 300f; // 配置する円の半径
    public GameObject panel;
    private List<List<int>> allTaskOrders = new List<List<int>>(); // すべてのタスク順序を保持するリスト
    private int currentTargetIndex = 0;
    private int currentTaskIndex = 0;
    private System.Random random = new System.Random(); // 乱数生成器

    public WriteToCSV csvWriter;  // Inspectorからアサイン

    private void Awake()
    {
        GenerateAllTaskOrders();
        ShuffleTaskOrders(); // シャッフル
        ResetToFirstTask();

        // すべてのボタンを配置
        for (int i = 1; i <= buttonCount; i++)
        {
            string buttonName = $"Button ({i})";
            GameObject buttonObj = GameObject.Find(buttonName);
            if (buttonObj != null)
            {
                PlaceButton(buttonObj, i);
            }
            else
            {
                Debug.LogError($"ボタンが見つかりません: {buttonName}");
            }
        }
    }

    void Start()
    {
        //csv書き込みの初期化
        csvWriter.SetupFile(radius);
        csvWriter.LogExperimentInfo(allTaskOrders);
    }

    void PlaceButton(GameObject buttonObj, int index)
    {
        // ボタンの新しい座標を計算
        float angle = Mathf.Deg2Rad * (-40 * (index - 1) + 90); // 40度ずつ角度を変えて円形に配置
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);

        // ボタンの位置を設定
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = new Vector3(x, y, 0);
        }
        else
        {
            Debug.LogWarning($"RectTransformが見つかりません: {buttonObj.name}");
        }
    }

    // すべてのタスク順序を生成
    void GenerateAllTaskOrders()
    {
        for (int set = 0; set < setCount; set++)
        {
            for (int startButton = 1; startButton <= buttonCount; startButton++)
            {
                for (int step = 4; step <= 5; step++) // ステップ値を4と5で試行
                {
                    allTaskOrders.Add(GenerateTaskOrder(startButton, step));
                }
            }
        }
    }

    // タスクオーダーをランダムにシャッフル
    void ShuffleTaskOrders()
    {
        int count = allTaskOrders.Count;
        while (count > 1)
        {
            count--;
            int k = random.Next(count + 1);
            List<int> value = allTaskOrders[k];
            allTaskOrders[k] = allTaskOrders[count];
            allTaskOrders[count] = value;
        }
    }

    // 1つのタスク順序を生成
    List<int> GenerateTaskOrder(int startButton, int step)
    {
        List<int> order = new List<int> { startButton };
        int nextButton = startButton;
        for (int i = 1; i < buttonCount; i++)
        {
            nextButton = (nextButton + step - 1) % 9 + 1;
            order.Add(nextButton);
        }
        return order;
    }

    // タスクをリセットして最初から開始
    public void ResetToFirstTask()
    {
        currentTargetIndex = 0;
        currentTaskIndex = 0;
        SetTaskOrder(currentTaskIndex);
    }

    // 現在のタスクオーダーを設定
    void SetTaskOrder(int taskIndex)
    {
        var currentOrder = allTaskOrders[taskIndex];
        Debug.Log("Task (" + (taskIndex + 1) + ") Order: " + string.Join(", ", currentOrder));
    }

    // 次のターゲットボタンのインデックスを取得
    public int GetNextTargetButton()
    {
        if (currentTargetIndex < buttonCount && currentTaskIndex < allTaskOrders.Count)
        {
            return allTaskOrders[currentTaskIndex][currentTargetIndex] - 1; // 0-indexedに調整
        }
        return -1; // タスクが終了した場合
    }

    // 次のタスクを設定
    public bool SetNextTask()
    {
        if (currentTargetIndex < buttonCount - 1)
        {
            csvWriter.LogExperimentResult(currentTaskIndex, allTaskOrders[currentTaskIndex][currentTargetIndex]);
            currentTargetIndex++;
            return true;
        }
        else if (currentTaskIndex < allTaskOrders.Count - 1)
        {
            csvWriter.LogExperimentResult(currentTaskIndex, allTaskOrders[currentTaskIndex][currentTargetIndex]);
            currentTaskIndex++;
            currentTargetIndex = 0;
            panel.SetActive(true);
            SetTaskOrder(currentTaskIndex);
            return true;
        }
        csvWriter.LogExperimentResult(currentTaskIndex, allTaskOrders[currentTaskIndex][currentTargetIndex]);
        return false; // すべてのタスクが終了
    }
}
