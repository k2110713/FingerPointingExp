using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointingHandlerV2 : MonoBehaviour
{
    public GameObject midAirDisplay;
    public OptitrackStreamingClient optitrackClient;
    public SerialHandler serialHandler; // CopperSwitchのシリアル通信を追加

    public float pixelX = 1920;
    public float pixelY = 1080;

    OptitrackMarkerState marker1State;

    private const int BufferSize = 1;
    private Queue<Vector3> pointerPosHistory = new Queue<Vector3>();
    private Vector3 filteredPointerPos = Vector3.zero;

    private float sx;
    private float sy;
    public float coefficientX = 1.0f;
    public float coefficientY = 1.0f;
    private Vector3 poiPos2d = Vector3.zero;

    public float difX = 111.8f, difY = 287.2f;

    float t = 0.0f;

    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    float a, b, c, d;

    public int marker1ID = 1;

    [SerializeField] private Image[] buttonImages;
    [SerializeField] private Button[] buttons;
    [SerializeField] private ButtonsManager buttonsManager;

    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color targetColor = Color.green;

    public GameObject panel;

    private int currentTargetIndex = -1;

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
        if (marker1State != null)
        {
            CalculatePointerPosition(marker1State.Position);

            if (Input.GetKey(KeyCode.R))
            {
                difX = marker1State.Position.x * sx;
                difY = (marker1State.Position.y - midAirDisplay.transform.position.y) * sy;
            }

            filteredPointerPos = FilteringPosition(pointerPosHistory);
            transform.localPosition = filteredPointerPos;
        }
        else
        {
            Debug.LogWarning("Marker data not available.");
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

    void CalculatePointerPosition(Vector3 pos1)
    {
        poiPos2d.x = (pos1.x * sx - difX) * coefficientX;
        poiPos2d.y = ((pos1.y - midAirDisplay.transform.position.y) * sy - difY) * coefficientY;
        poiPos2d.z = 0;

        //transform.localPosition = poiPos2d;
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
