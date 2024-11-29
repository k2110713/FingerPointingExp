using UnityEngine;
using System.Collections.Generic;

public class ButtonsManager : MonoBehaviour
{
    public int setCount = 1; // �Z�b�g��
    public int buttonCount = 9; // �{�^���̐�
    public float radius = 300f; // �z�u����~�̔��a
    public GameObject panel;
    private List<List<int>> allTaskOrders = new List<List<int>>(); // ���ׂẴ^�X�N������ێ����郊�X�g
    private int currentTargetIndex = 0;
    private int currentTaskIndex = 0;
    private System.Random random = new System.Random(); // ����������

    public WriteToCSV csvWriter;  // Inspector����A�T�C��

    private void Awake()
    {
        GenerateAllTaskOrders();
        ShuffleTaskOrders(); // �V���b�t��
        ResetToFirstTask();

        // ���ׂẴ{�^����z�u
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
                Debug.LogError($"�{�^����������܂���: {buttonName}");
            }
        }
    }

    void Start()
    {
        //csv�������݂̏�����
        csvWriter.SetupFile(radius);
        csvWriter.LogExperimentInfo(allTaskOrders);
    }

    void PlaceButton(GameObject buttonObj, int index)
    {
        // �{�^���̐V�������W���v�Z
        float angle = Mathf.Deg2Rad * (-40 * (index - 1) + 90); // 40�x���p�x��ς��ĉ~�`�ɔz�u
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

    // ���ׂẴ^�X�N�����𐶐�
    void GenerateAllTaskOrders()
    {
        for (int set = 0; set < setCount; set++)
        {
            for (int startButton = 1; startButton <= buttonCount; startButton++)
            {
                for (int step = 4; step <= 5; step++) // �X�e�b�v�l��4��5�Ŏ��s
                {
                    allTaskOrders.Add(GenerateTaskOrder(startButton, step));
                }
            }
        }
    }

    // �^�X�N�I�[�_�[�������_���ɃV���b�t��
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

    // 1�̃^�X�N�����𐶐�
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

    // �^�X�N�����Z�b�g���čŏ�����J�n
    public void ResetToFirstTask()
    {
        currentTargetIndex = 0;
        currentTaskIndex = 0;
        SetTaskOrder(currentTaskIndex);
    }

    // ���݂̃^�X�N�I�[�_�[��ݒ�
    void SetTaskOrder(int taskIndex)
    {
        var currentOrder = allTaskOrders[taskIndex];
        Debug.Log("Task (" + (taskIndex + 1) + ") Order: " + string.Join(", ", currentOrder));
    }

    // ���̃^�[�Q�b�g�{�^���̃C���f�b�N�X���擾
    public int GetNextTargetButton()
    {
        if (currentTargetIndex < buttonCount && currentTaskIndex < allTaskOrders.Count)
        {
            return allTaskOrders[currentTaskIndex][currentTargetIndex] - 1; // 0-indexed�ɒ���
        }
        return -1; // �^�X�N���I�������ꍇ
    }

    // ���̃^�X�N��ݒ�
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
        return false; // ���ׂẴ^�X�N���I��
    }
}
