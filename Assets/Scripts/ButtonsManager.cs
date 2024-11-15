using UnityEngine;
using System.Collections.Generic;

public class ButtonsManager : MonoBehaviour
{
    private GameObject[] buttons = new GameObject[9];
    private List<int> taskOrder; // 現在のタスク順序
    private int currentTaskIndex = 0; // 現在のタスクインデックス

    public float radius = 300f; // 配置する円の半径
    public int buttonCount = 9; // ボタンの数

    private void Start()
    {
        // ボタンを名前で検索して配置と格納を行う
        for (int i = 1; i <= buttonCount; i++)
        {
            string buttonName = $"Button ({i})";
            GameObject buttonObj = GameObject.Find(buttonName);

            if (buttonObj != null)
            {
                buttons[i - 1] = buttonObj;
                PlaceButton(buttonObj, i);
            }
            else
            {
                Debug.LogError($"ボタンが見つかりません: {buttonName}");
            }
        }

        // タスク順序を生成
        taskOrder = GenerateTaskOrder();

        Debug.Log("タスク順序: " + string.Join(", ", taskOrder));
    }

    private void PlaceButton(GameObject buttonObj, int index)
    {
        // ボタンの新しい座標を計算
        float angle = Mathf.Deg2Rad * (40 * (index - 1) + 90); // ラジアンに変換 (90度はπ/2)
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

    private List<int> GenerateTaskOrder()
    {
        // 開始ボタンをランダムに選択
        int startButton = Random.Range(0, buttonCount);
        List<int> order = new List<int> { startButton };

        // まだ選択されていない中で最も離れたボタンを順次追加
        while (order.Count < buttonCount)
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

        for (int i = 0; i < buttonCount; i++)
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
        }
        else
        {
            Debug.LogWarning($"間違ったボタンがクリックされました: Button ({buttonIndex + 1})");
        }
    }

    public int GetNextTargetButton()
    {
        if (currentTaskIndex < taskOrder.Count)
        {
            return taskOrder[currentTaskIndex];
        }
        return -1; // タスクが終了した場合
    }
}
