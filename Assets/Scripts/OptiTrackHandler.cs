using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptiTrackHandler : MonoBehaviour
{
    public GameObject midAirDisplay;
    public OptitrackStreamingClient optitrackClient;

    public float pixelX = 1920;
    public float pixelY = 1080;

    OptitrackMarkerState marker1State;
    OptitrackMarkerState marker2State;

    private const int BufferSize = 8;
    private Queue<Vector3> pointerPosHistory = new Queue<Vector3>();
    private Vector3 filteredPointerPos = Vector3.zero;

    private float sx;
    private float sy;
    private Vector3 poiPos2d = Vector3.zero;

    public float difX = 0, difY = 0;

    private float previousZpos = 0.0f;

    float t = 0.0f;

    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    float a, b, c, d;

    public int marker1ID = 1;
    public int marker2ID = 2;

    // ボタンのビジュアル表現を保持するための配列
    [SerializeField] private Image[] buttonImages;
    // ボタンコンポーネントを保持するための配列
    [SerializeField] private Button[] buttons;
    // ボタンの管理とタスク進行を担うButtonsManagerの参照
    [SerializeField] private ButtonsManager buttonsManager;

    // ボタンのデフォルト色
    [SerializeField] private Color defaultColor = Color.white;
    // 現在のターゲットボタンを示す色
    [SerializeField] private Color targetColor = Color.green;

    public GameObject panel;

    // 現在のターゲットボタンのインデックス
    private int currentTargetIndex = -1;

    // シングルトンインスタンス
    public static OptiTrackHandler Instance { get; private set; }

    // トリガーフラグ
    public bool isTriggered { get; private set; } = false;

    private void Awake()
    {
        // インスタンスが既に存在する場合は破棄し、存在しない場合はこのインスタンスを設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンが切り替わっても破棄されないように設定
        }

        // 全ボタンの初期化と配列への登録
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

    // トリガーを発生させるメソッド
    public void SetTrigger()
    {
        isTriggered = true;
    }

    // トリガーをリセットするメソッド
    public void ResetTrigger()
    {
        isTriggered = false;
    }

    void Start()
    {
        // 最初のターゲット設定
        UpdateTargetButton(-1);

        // 画面サイズとポインターのスケーリングを設定
        sx = pixelX / midAirDisplay.transform.localScale.x;
        sy = pixelY / midAirDisplay.transform.localScale.y;

        // OptiTrackの平面計算用
        p1.z = -midAirDisplay.transform.position.z; p2.z = -midAirDisplay.transform.position.z; p3.z = -midAirDisplay.transform.position.z;

        Vector3 AB = p2 - p1;
        Vector3 AC = p3 - p1;
        Vector3 normal = Vector3.Cross(AB, AC);

        a = normal.x;
        b = normal.y;
        c = normal.z;
        d = -(a * p1.x + b * p1.y + c * p1.z);
    }

    void Update()
    {
        if ((isTriggered || Input.GetKeyDown(KeyCode.Return)) && !panel.activeSelf) //指さしのトリガーが来た
        {
            TriggeredButton(transform.localPosition);
            ResetTrigger();
        }

        //隠し用のパネルを解除
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("task start");
            UpdateTargetButton(-1);
            panel.SetActive(false);
        }

        /* OptiTrackからマーカ情報を取得し、指さし入力のポインタを表示 */
        if (optitrackClient == null)
        {
            Debug.LogError("OptitrackStreamingClient is not assigned.");
            return;
        }

        marker1State = optitrackClient.GetLatestMarkerStates()[0];
        marker2State = optitrackClient.GetLatestMarkerStates()[1];
        if (marker1State != null && marker2State != null)
        {
            CalculatePointerPosition(marker1State.Position, marker2State.Position);

            if (Input.GetKey(KeyCode.R))
            {
                difX = poiPos2d.x;
                difY = poiPos2d.y;
            }

            filteredPointerPos = FilteringPosition(pointerPosHistory);
            transform.localPosition = new Vector3(filteredPointerPos.x - difX, filteredPointerPos.y - difY, filteredPointerPos.z);

            if (marker1State.Position.z < marker2State.Position.z)
            {
                pushButton(marker2State.Position.z);
            }
            else
            {
                pushButton(marker1State.Position.z);
            }
        }
        else
        {
            Debug.LogWarning("Marker data not available.");
        }//*/

        /* デバッグ用 */
        /*// 矢印キーでポインタを動かす
        float moveSpeed = 2.0f;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            poiPos2d.y += moveSpeed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            poiPos2d.y -= moveSpeed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            poiPos2d.x -= moveSpeed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            poiPos2d.x += moveSpeed;
        }

        // ポインタの位置を更新
        transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);//*/
    }

    void pushButton(float zPos)
    {
        Vector2 pushPos = new Vector2(
            pointerPosHistory.ToArray()[pointerPosHistory.Count - 1].x - difX,
            pointerPosHistory.ToArray()[pointerPosHistory.Count - 1].y - difY
            );
        if (previousZpos <= midAirDisplay.transform.position.z && zPos > midAirDisplay.transform.position.z)
        {
            Debug.Log("pushed!");
            TriggeredButton(pushPos);
        }

        previousZpos = zPos;
    }

    void AddToHistory(Vector3 pos, Queue<Vector3> history)
    {
        if (history.Count >= BufferSize)
        {
            history.Dequeue();
        }
        history.Enqueue(pos);
    }

    Vector3 FilteringPosition(Queue<Vector3> history)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 pos in history)
        {
            sum += pos;
        }
        return sum / history.Count;
    }

    void CalculatePointerPosition(Vector3 pos1, Vector3 pos2)
    {
        t = (d - (a * pos1.x + b * pos1.y + c * pos1.z))
            / (a * (pos2.x - pos1.x) + b * (pos2.y - pos1.y) + c * (pos2.z - pos1.z));

        Vector3 poiPos3d = new Vector3(
            pos1.x + t * (pos2.x - pos1.x),
            pos1.y + t * (pos2.y - pos1.y),
            pos1.z + t * (pos2.z - pos1.z)
        );

        poiPos2d.x = poiPos3d.x * sx;
        poiPos2d.y = (poiPos3d.y - midAirDisplay.transform.position.y) * sy;
        poiPos2d.z = 0;

        AddToHistory(poiPos2d, pointerPosHistory);
    }

    // ボタンクリックのチェックと処理
    private void TriggeredButton(Vector2 triggeredPosition)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] != null && IsPointerInCircle(buttonImages[i], triggeredPosition))
            {
                if (i == currentTargetIndex)
                {
                    Debug.Log("Correct button clicked: Button (" + (i + 1) + ")");
                    int previousTargetIndex = buttonsManager.GetNextTargetButton();
                    if (!buttonsManager.SetNextTask()) // ターゲット・タスクを更新し、全ターゲット・タスクが終了しているなら終了
                    {
                        Debug.Log("All tasks completed.");
                        Application.Quit();
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
            //Debug.Log("Current target is Button (" + (currentTargetIndex + 1) + ")");
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
    private bool IsPointerInCircle(Image buttonImage, Vector2 pointerPosition)
    {
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition;
        Vector2 relativePosition = pointerPosition - buttonCenter;

        return relativePosition.magnitude < (rectTransform.rect.width * 0.42f);
    }
}
