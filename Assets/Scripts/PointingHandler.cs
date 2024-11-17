using UnityEngine;
using UnityEngine.UI;

public class PointingHandler : MonoBehaviour
{
    // ボタンのビジュアル表現を保持するための配列
    [SerializeField] private Image[] buttonImages;
    // ボタンコンポーネントを保持するための配列
    [SerializeField] private Button[] buttons;
    // ボタンの管理とタスク進行を担うButtonsManagerの参照
    [SerializeField] private ButtonsManager buttonsManager;

    // ボタンのデフォルト色
    [SerializeField] private Color defaultColor = Color.white;
    // 現在のターゲットボタンを示す色
    [SerializeField] private Color targetColor = Color.red;
    // ボタンがクリックされた際の色
    [SerializeField] private Color clickColor = Color.green;

    public GameObject panel;

    // 現在のターゲットボタンのインデックス
    private int currentTargetIndex = -1;

    void Start()
    {
        // ボタンの初期化と最初のターゲット設定
        InitializeButtons();
        UpdateTargetButton(-1);
    }

    void Update()
    {
        //if ((CopperSwitch.Instance.isTriggered || Input.GetKeyDown(KeyCode.Return)) && panel.activeSelf) //タップ検出（デバッグも）
        if (Input.GetKeyDown(KeyCode.Return) && panel.activeSelf)
        {
            CheckAndProcessButtonClick();
        }

        //隠し用のパネルを解除
        if (Input.GetKeyDown(KeyCode.Space))
        {
            panel.SetActive(false);
        }
    }

    // 全ボタンの初期化と配列への登録
    private void InitializeButtons()
    {
        buttonImages = new Image[9];
        buttons = new Button[9];

        for (int i = 0; i < 9; i++)
        {
            string buttonName = "Button (" + (i + 1) + ")";
            GameObject buttonObj = GameObject.Find(buttonName);
            if (buttonObj)
            {
                buttonImages[i] = buttonObj.GetComponent<Image>();
                buttons[i] = buttonObj.GetComponent<Button>();
                buttonImages[i].color = defaultColor;
            }
            else
            {
                Debug.LogError(buttonName + " not found.");
            }
        }
    }

    // ボタンクリックのチェックと処理
    private void CheckAndProcessButtonClick()
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] != null && IsPointerInCircle(buttonImages[i]))
            {
                if (i == currentTargetIndex)
                {
                    Debug.Log("Correct button clicked: Button (" + (i + 1) + ")");
                    int previousTargetIndex = buttonsManager.GetNextTargetButton();
                    if (!buttonsManager.SetNextTask()) // ターゲット・タスクを更新し、全ターゲット・タスクが終了しているなら終了
                    {
                        Debug.Log("All tasks completed. Restarting...");
                    }
                    else
                    {
                        UpdateTargetButton(previousTargetIndex); //ターゲットを更新
                    }
                }
                else
                {
                    Debug.LogWarning("Incorrect button clicked: Button (" + (i + 1) + ")");
                }
            }
        }
    }

    // 現在のターゲットボタンの更新
    private bool UpdateTargetButton(int prev)
    {
        currentTargetIndex = buttonsManager.GetNextTargetButton();
        if (currentTargetIndex != -1)
        {
            Debug.Log("Current target is Button (" + (currentTargetIndex + 1) + ")");
            buttonImages[currentTargetIndex].color = targetColor;
            if (prev != -1)
            {
                buttonImages[prev].color = defaultColor;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    // ボタンとポインタ位置が円形範囲内にあるかを判定
    private bool IsPointerInCircle(Image buttonImage)
    {
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition;
        Vector2 localPointerPosition = transform.localPosition;
        Vector2 relativePosition = localPointerPosition - buttonCenter;

        return relativePosition.magnitude < (rectTransform.rect.width * 0.42f);
    }
}
