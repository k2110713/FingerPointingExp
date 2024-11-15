using UnityEngine;
using System.Collections.Generic;

public class ButtonsManager : MonoBehaviour
{
    private GameObject[] buttons = new GameObject[9];
    private List<int> taskOrder; // ���݂̃^�X�N����
    private int currentTaskIndex = 0; // ���݂̃^�X�N�C���f�b�N�X

    public float radius = 300f; // �z�u����~�̔��a
    public int buttonCount = 9; // �{�^���̐�

    private void Start()
    {
        // �{�^���𖼑O�Ō������Ĕz�u�Ɗi�[���s��
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
                Debug.LogError($"�{�^����������܂���: {buttonName}");
            }
        }

        // �^�X�N�����𐶐�
        taskOrder = GenerateTaskOrder();

        Debug.Log("�^�X�N����: " + string.Join(", ", taskOrder));
    }

    private void PlaceButton(GameObject buttonObj, int index)
    {
        // �{�^���̐V�������W���v�Z
        float angle = Mathf.Deg2Rad * (40 * (index - 1) + 90); // ���W�A���ɕϊ� (90�x�̓�/2)
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);

        // �{�^���̈ʒu��ݒ�
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = new Vector3(x, y, 0);
        }
        else
        {
            Debug.LogWarning($"RectTransform��������܂���: {buttonObj.name}");
        }
    }

    private List<int> GenerateTaskOrder()
    {
        // �J�n�{�^���������_���ɑI��
        int startButton = Random.Range(0, buttonCount);
        List<int> order = new List<int> { startButton };

        // �܂��I������Ă��Ȃ����ōł����ꂽ�{�^���������ǉ�
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
            Debug.Log($"�������{�^�����N���b�N����܂���: Button ({buttonIndex + 1})");
            currentTaskIndex++;
        }
        else
        {
            Debug.LogWarning($"�Ԉ�����{�^�����N���b�N����܂���: Button ({buttonIndex + 1})");
        }
    }

    public int GetNextTargetButton()
    {
        if (currentTaskIndex < taskOrder.Count)
        {
            return taskOrder[currentTaskIndex];
        }
        return -1; // �^�X�N���I�������ꍇ
    }
}
