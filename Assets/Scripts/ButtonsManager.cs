using UnityEngine;
using System.Collections.Generic;

public class ButtonsManager : MonoBehaviour
{
    private GameObject[] buttons = new GameObject[9]; // ボタンを管理する配列
    private List<int> taskOrder; // 現在のタスクの順序
    private int currentTaskIndex = 0; // 現在のタスクのインデックス
    private int currentRound = 0; // 現在のラウンド（9回のタスクが1ラウンド）
    private const int TotalRounds = 2; // 総ラウンド数
    private const int ButtonsCount = 9; // ボタンの総数

    private void Start()
    {
        // ボタンを名前で検索して配列に格納
        for (int i = 0; i < ButtonsCount; i++)
        {
            buttons[i] = GameObject.Find("Button (" + (i + 1) + ")");
            if (buttons[i] == null)
            {
                Debug.LogError("Button (" + (i + 1) + ") が見つかりません");
            }
        }

        InitializeNewRound();
    }

    private void InitializeNewRound()
    {
        taskOrder = GenerateTaskOrder();
        currentTaskIndex = 0;
        currentRound++;

        Debug.Log($"ラウンド {currentRound} 開始。タスク順序: {string.Join(", ", taskOrder)}");
    }

    private List<int> GenerateTaskOrder()
    {
        int startButton = Random.Range(0, ButtonsCount);
        List<int> order = new List<int> { startButton };

        while (order.Count < ButtonsCount)
        {
            int farthestButton = GetFarthestButton(order);
            order.Add(farthestButton);
        }

        return order;
    }

    private int GetFarthestButton(List<int> currentOrder)
    {
        int lastButton = currentOrder[currentOrder.Count - 1];
        float maxDistance = float.MinValue;
        int farthestButton = -1;

        for (int i = 0; i < ButtonsCount; i++)
        {
            if (!currentOrder.Contains(i))
            {
                float distance = Vector2.Distance(buttons[lastButton].transform.localPosition, buttons[i].transform.localPosition);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestButton = i;
                }
            }
        }

        return farthestButton;
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex == taskOrder[currentTaskIndex])
        {
            Debug.Log($"正しいボタンがクリックされました: Button ({buttonIndex + 1})");

            currentTaskIndex++;
            if (currentTaskIndex >= taskOrder.Count)
            {
                if (currentRound < TotalRounds)
                {
                    InitializeNewRound();
                }
                else
                {
                    Debug.Log("すべてのタスクが完了しました！");
                }
            }
        }
        else
        {
            Debug.LogWarning($"間違ったボタンがクリックされました: Button ({buttonIndex + 1})");
        }
    }
}
