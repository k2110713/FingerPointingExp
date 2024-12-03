using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointingHandler : MonoBehaviour
{
    public GameObject midAirDisplay;
    public OptitrackStreamingClient optitrackClient;
    public SerialHandler serialHandler; // CopperSwitchのシリアル通信を追加

    public float pixelX = 1920;
    public float pixelY = 1080;

    OptitrackMarkerState marker1State;
    OptitrackMarkerState marker2State;

    public int BufferSize = 1;
    private Queue<Vector3> pointerPosHistory = new Queue<Vector3>();
    private Vector3 filteredPointerPos = Vector3.zero;

    private float sx;
    private float sy;
    private Vector3 poiPos2d = Vector3.zero;

    public float difX = 0, difY = 0;

    float t = 0.0f;

    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    float a, b, c, d;

    public int marker1ID = 1;
    public int marker2ID = 2;

    [SerializeField] private Image[] buttonImages;
    [SerializeField] private Button[] buttons;
    [SerializeField] private ButtonsManager buttonsManager;

    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color targetColor = Color.green;

    public GameObject panel;

    private int currentTargetIndex = -1;

    public float magneticRadius = 200.0f; // 磁気が作用する範囲
    public float pullStrength = 0.001f;    // 磁気の引っ張る強さ (0.0 - 1.0)

    private void Awake()
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

        if (serialHandler != null)
        {
            serialHandler.OnDataReceived += OnDataReceived; // シリアル通信の初期化
        }
        else
        {
            Debug.LogWarning("SerialHandler is not assigned.");
        }
    }

    void Start()
    {
        UpdateTargetButton(-1);

        sx = pixelX / midAirDisplay.transform.localScale.x;
        sy = pixelY / midAirDisplay.transform.localScale.y;

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("task start");
            UpdateTargetButton(-1);
            panel.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Return) && !panel.activeSelf)
        {
            TriggeredButton(transform.localPosition);
        }

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
            //ApplyMagneticPull(FilteringPosition(pointerPosHistory));
        }
        else
        {
            Debug.LogWarning("Marker data not available.");
        }
    }

    //リダイレクション（磁気的にポインタを引っ張る）
    private void ApplyMagneticPull(Vector3 poipos)
    {
        RectTransform nearestButton = null;
        float nearestDistance = float.MaxValue;

        // 最も近いボタンを探す
        foreach (var buttonImage in buttonImages)
        {
            if (buttonImage == null) continue;

            RectTransform rectTransform = buttonImage.rectTransform;
            Vector2 buttonCenter = rectTransform.localPosition;

            float distance = Vector2.Distance(buttonCenter, poipos);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestButton = rectTransform;
            }
        }

        // 最も近いボタンが磁気範囲内の場合に引っ張りを適用
        if (nearestButton != null && nearestDistance <= magneticRadius)
        {
            Vector2 buttonCenter = nearestButton.localPosition;
            Vector2 newPosition = Vector2.Lerp(poipos, buttonCenter, pullStrength);

            // ポインターの位置を更新
            transform.localPosition = new Vector3(newPosition.x, newPosition.y, 0.0f);
        }
        else
        {
            // 磁気範囲外では元の位置を保持
            transform.localPosition = poipos;
        }
    }


    void OnDataReceived(string message)
    {
        var data = message.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        try
        {
            if (data[0] == "1" && !panel.activeSelf)
            {
                Debug.Log("tapped!");
                TriggeredButton(transform.localPosition);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
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
                    if (!buttonsManager.SetNextTask())
                    {
                        Debug.Log("All tasks completed.");
                        Application.Quit();
                    }
                    else
                    {
                        UpdateTargetButton(previousTargetIndex);
                    }
                }
                else
                {
                    Debug.LogWarning("Incorrect button clicked: Button (" + (i + 1) + ")");
                }
            }
        }
    }

    private bool UpdateTargetButton(int prev)
    {
        currentTargetIndex = buttonsManager.GetNextTargetButton();
        if (currentTargetIndex != -1)
        {
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

    private bool IsPointerInCircle(Image buttonImage, Vector2 pointerPosition)
    {
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition;
        Vector2 relativePosition = pointerPosition - buttonCenter;

        return relativePosition.magnitude < (rectTransform.rect.width * 0.42f);
    }
}
