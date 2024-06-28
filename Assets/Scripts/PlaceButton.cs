using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlaceButton : MonoBehaviour
{
    public RectTransform canvasRectTransform; // Canvas の RectTransform
    public RectTransform buttonRectTransform; // Button の RectTransform

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
        // Canvas のサイズを取得

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        // Button のサイズを取得
        float buttonWidth = buttonRectTransform.rect.width;
        float buttonHeight = buttonRectTransform.rect.height;

        // ランダムな位置を計算 (ボタンが画面外に出ないように)
        float randomX = Random.Range(buttonWidth / 2, canvasWidth - buttonWidth / 2);
        float randomY = Random.Range(buttonHeight / 2, canvasHeight - buttonHeight / 2);

        // ボタンの位置を設定
        buttonRectTransform.position = new Vector3(randomX, randomY, 1);
    }
}
