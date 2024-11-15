using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class IndexFingerDetection : MonoBehaviour
{
    public TextMeshProUGUI accelerationText;
    public GameObject midAirButton;
    public GameObject midAirDisplay;
    public RectTransform canvasRectTransform;
    public OptitrackStreamingClient optitrackClient;

    public float pixelX = 1920;
    public float pixelY = 1080;

    private const int BufferSize = 8;
    private Queue<Vector3> pos1History = new Queue<Vector3>();
    private Queue<Vector3> pos2History = new Queue<Vector3>();
    private Vector3 smoothedPos1 = Vector3.zero;
    private Vector3 smoothedPos2 = Vector3.zero;

    private float sx;
    private float sy;
    private Vector3 poiPos2d = Vector3.zero;

    public float difX = 0, difY = 0;

    private float previousZpos = 0.0f;
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    float t = 0.0f;

    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    float a, b, c, d;

    public int marker1ID = 1;
    public int marker2ID = 2;

    void Start()
    {
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
        // OptiTrackのデータ取得処理 (将来の利用のためにコメントアウト)
        /*
        if (optitrackClient == null)
        {
            UnityEngine.Debug.LogError("OptitrackStreamingClient is not assigned.");
            return;
        }

        var marker1State = optitrackClient.GetLatestMarkerStates()[0];
        var marker2State = optitrackClient.GetLatestMarkerStates()[1];

        if (marker1State != null && marker2State != null)
        {
            Vector3 pos1 = marker1State.Position;
            Vector3 pos2 = marker2State.Position;

            AddToHistory(pos1, pos1History);
            AddToHistory(pos2, pos2History);

            smoothedPos1 = CalculateSmoothedPosition(pos1History);
            smoothedPos2 = CalculateSmoothedPosition(pos2History);

            CalculatePointerPosition(smoothedPos1, smoothedPos2);

            if (Input.GetKey(KeyCode.Space))
            {
                difX = poiPos2d.x;
                difY = poiPos2d.y;
            }

            transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);

            pushButton(smoothedPos2);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Marker data not available.");
        }
        */

        // 矢印キーでポインタを動かす
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
        transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);

        // ボタンを押す処理
        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }

    /*void pushButton(Vector3 pos)
    {
        if (previousZpos <= midAirDisplay.transform.position.z && pos.z > midAirDisplay.transform.position.z)
        {
            if (IsButtonAtPosition(pos))
            {
                Begin.correctCount++;
            }
            Begin.cnt++;
            if (Begin.cnt < Begin.testNumInOnce)
            {
                buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
            }
            else
            {
                Begin.stopwatch.Stop();
                StartCoroutine(Cooldown());
                buttonObject.SetActive(false);
            }
        }

        previousZpos = pos.z;
    }

    bool IsButtonAtPosition(Vector3 position)
    {
        // ボタン位置の検知処理
        RectTransform buttonRectTransform = midAirButton.GetComponent<RectTransform>();

        Vector3 buttonPosition = midAirButton.transform.localPosition;
        Vector2 buttonScale = midAirButton.transform.localScale;

        float buttonMinX = buttonPosition.x - buttonScale.x / 2;
        float buttonMaxX = buttonPosition.x + buttonScale.x / 2;
        float buttonMinY = buttonPosition.y - buttonScale.y / 2;
        float buttonMaxY = buttonPosition.y + buttonScale.y / 2;

        return (position.x >= buttonMinX && position.x <= buttonMaxX &&
                position.y >= buttonMinY && position.y <= buttonMaxY);
    }*/

    void AddToHistory(Vector3 pos, Queue<Vector3> history)
    {
        if (history.Count >= BufferSize)
        {
            history.Dequeue();
        }
        history.Enqueue(pos);
    }

    Vector3 CalculateSmoothedPosition(Queue<Vector3> history)
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
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
