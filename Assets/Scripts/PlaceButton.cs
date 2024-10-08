using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlaceButton : MonoBehaviour
{
    public RectTransform canvasRectTransform; // Canvas �� RectTransform
    public RectTransform buttonRectTransform; // Button �� RectTransform

    void Start()
    {
        PlaceButtonRandomly();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaceButtonRandomly();
        }
    }

    public void PlaceButtonRandomly()
    {
        // Canvas �̃T�C�Y���擾

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        // Button �̃T�C�Y���擾
        float buttonWidth = buttonRectTransform.rect.width;
        float buttonHeight = buttonRectTransform.rect.height;

        // �����_���Ȉʒu���v�Z (�{�^������ʊO�ɏo�Ȃ��悤��)
        float randomX = Random.Range(buttonWidth / 2, canvasWidth - buttonWidth / 2);
        float randomY = Random.Range(buttonHeight / 2, canvasHeight - buttonHeight / 2);

        // �{�^���̈ʒu��ݒ�
        buttonRectTransform.position = new Vector3(randomX, randomY, 1);
    }
}
