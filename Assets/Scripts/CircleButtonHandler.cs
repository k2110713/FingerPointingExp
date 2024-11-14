using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CircleButtonHandler : MonoBehaviour
{
    private Image[] buttonImages;
    private Button[] buttons;

    // 色の設定
    [SerializeField] private Color defaultColor = Color.white;   // デフォルトの色
    [SerializeField] private Color enterColor = Color.green;     // マウスが乗ったときの色
    [SerializeField] private Color clickColor = Color.red;       // クリック時の色

    private bool[] wasInCircle; // 各ボタンの状態を管理するフラグ

    private void Start()
    {
        // 8個のボタンを配列に格納
        buttonImages = new Image[8];
        buttons = new Button[8];
        wasInCircle = new bool[8];

        for (int i = 0; i < 8; i++)
        {
            GameObject buttonObj = GameObject.Find("Button (" + (i + 1) + ")");
            if (buttonObj != null)
            {
                buttonImages[i] = buttonObj.GetComponent<Image>();
                buttons[i] = buttonObj.GetComponent<Button>();
                buttonImages[i].color = defaultColor; // 初期色をデフォルト色に設定
            }
            else
            {
                Debug.LogWarning("Button (" + (i + 1) + ") not found.");
            }
        }
    }

    private void Update()
    {
        // 各ボタンの円形範囲にポインタが入っているかチェック
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] != null)
            {
                bool isInCircle = IsPointerInCircle(buttonImages[i]);

                if (!wasInCircle[i] && isInCircle)
                {
                    // enter状態
                    Debug.Log("Enter Button (" + (i + 1) + ")");
                    buttonImages[i].color = enterColor;
                }
                else if (wasInCircle[i] && !isInCircle)
                {
                    // exit状態
                    Debug.Log("Exit Button (" + (i + 1) + ")");
                    buttonImages[i].color = defaultColor;
                }

                // クリックチェック
                if (isInCircle && Input.GetKeyDown(KeyCode.Return)) // 左クリック
                {
                    Debug.Log("Click Button (" + (i + 1) + ")");
                    buttonImages[i].color = clickColor;
                    buttons[i].onClick.Invoke();
                }

                // トリガーが発生しているかチェック
                if (isInCircle && CopperSwitch.Instance.isTriggered)
                {
                    // トリガー発生時の処理
                    Debug.Log("トリガーが発生しました！");

                    // 処理が終わったらトリガーをリセット
                    CopperSwitch.Instance.ResetTrigger();
                }

                // 状態を更新
                wasInCircle[i] = isInCircle;
            }
        }
    }

    private bool IsPointerInCircle(Image buttonImage)
    {
        // ボタンとポインタのローカル位置を計算
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition; // ボタンの中心位置
        Vector2 localPointerPosition = transform.localPosition; // ポインタ（自身）の位置

        // ポインタからボタン中心への相対位置
        Vector2 relativePosition = localPointerPosition - buttonCenter;
        float radius = rectTransform.rect.width * 0.42f;

        // 円の範囲内にいるかを判定
        return relativePosition.magnitude < radius;
    }
}
