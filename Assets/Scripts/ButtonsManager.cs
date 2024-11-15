using UnityEngine;
using System.Collections.Generic;

public class ButtonsManager : MonoBehaviour
{
    private GameObject[] buttons = new GameObject[9]; // �{�^�����Ǘ�����z��
    private List<int> taskOrder; // ���݂̃^�X�N�̏���
    private int currentTaskIndex = 0; // ���݂̃^�X�N�̃C���f�b�N�X
    private int currentRound = 0; // ���݂̃��E���h�i9��̃^�X�N��1���E���h�j
    private const int TotalRounds = 2; // �����E���h��
    private const int ButtonsCount = 9; // �{�^���̑���

    private void Start()
    {
        // �{�^���𖼑O�Ō������Ĕz��Ɋi�[
        for (int i = 0; i < ButtonsCount; i++)
        {
            buttons[i] = GameObject.Find("Button (" + (i + 1) + ")");
            if (buttons[i] == null)
            {
                Debug.LogError("Button (" + (i + 1) + ") ��������܂���");
            }
        }

        InitializeNewRound();
    }

    private void InitializeNewRound()
    {
        taskOrder = GenerateTaskOrder();
        currentTaskIndex = 0;
        currentRound++;

        Debug.Log($"���E���h {currentRound} �J�n�B�^�X�N����: {string.Join(", ", taskOrder)}");
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
            Debug.Log($"�������{�^�����N���b�N����܂���: Button ({buttonIndex + 1})");

            currentTaskIndex++;
            if (currentTaskIndex >= taskOrder.Count)
            {
                if (currentRound < TotalRounds)
                {
                    InitializeNewRound();
                }
                else
                {
                    Debug.Log("���ׂẴ^�X�N���������܂����I");
                }
            }
        }
        else
        {
            Debug.LogWarning($"�Ԉ�����{�^�����N���b�N����܂���: Button ({buttonIndex + 1})");
        }
    }
}
