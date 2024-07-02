using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class IndexFingerDetection : MonoBehaviour
{
    // 平滑化のためのバッファサイズ
    private const int BufferSize = 10;

    // pos1とpos2の座標の履歴
    private Queue<Vector3> pos1History = new Queue<Vector3>();
    private Queue<Vector3> pos2History = new Queue<Vector3>();

    // pos1とpos2の平滑化された座標
    private Vector3 smoothedPos1 = Vector3.zero;
    private Vector3 smoothedPos2 = Vector3.zero;

    // 主要なUnityオブジェクトとコンポーネント
    public Camera mainCamera;
    public LeapServiceProvider leapProvider;
    public TextMeshProUGUI accelerationText;
    public GameObject pointer2d;
    public GameObject buttonObject;
    public RectTransform canvasRectTransform;
    public RectTransform buttonRectTransform;

    // ディスプレイとセンサの設定
    public float displayX;
    public float displayY;
    public float displayWidth;
    public float displayHeight;
    public float sensorPosY;
    public float sensorPosZ;

    // 計算に使用する中間変数
    private float t = 0.0f;
    private Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    private Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);
    private float a, b, c, d;
    private Vector3 poiPos3d = new Vector3(0f, 0f, 0f);
    private Vector3 poiPos2d = new Vector3(0f, 0f, 0f);
    public float difX = 0, difY = 0;
    private float sx;
    private float sy;
    private float previousTime = 0.0f;
    public float updateInterval = 0.1f;
    private float[] thresholdAcceleration = { 100, 100, 200 };
    private float previousDistance1 = 0.0f;
    private float previousDistance2 = 0.0f;
    private bool isCooldown = false;
    private float cooldownTime = 0.5f;
    public int mode = 0;

    void Start()
    {
        InitializeEnvironment();
    }

    void Update()
    {
        Frame frame = leapProvider.CurrentFrame;
        foreach (Hand hand in frame.Hands)
        {
            if (hand.IsRight)
            {
                ProcessIndexFinger(hand);
            }
        }
    }

    // 実験環境の初期設定
    private void InitializeEnvironment()
    {
        sx = displayX / displayWidth;
        sy = displayY / displayHeight;
        p1.z = sensorPosZ;
        p2.z = sensorPosZ;
        p3.z = sensorPosZ;

        Vector3 AB = p2 - p1;
        Vector3 AC = p3 - p1;
        Vector3 normal = Vector3.Cross(AB, AC);

        a = normal.x;
        b = normal.y;
        c = normal.z;
        d = -(a * p1.x + b * p1.y + c * p1.z);

        previousTime = Time.time;
    }

    // 人差し指の処理を行う
    private void ProcessIndexFinger(Hand hand)
    {
        Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
        Bone boneBase = indexFinger.Bone((Bone.BoneType)0);
        Vector3 pos1 = boneBase.NextJoint;
        Bone boneTip = indexFinger.Bone((Bone.BoneType)3);
        Vector3 pos2 = boneTip.NextJoint;

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

        Debug.Log("pointer: " + poiPos2d);
        pointer2d.transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);

        CheckButtonPress(-smoothedPos2.z);
    }

    // 加速度に基づいてボタン押下を判断
    private void CheckButtonPress(float currentDistance)
    {
        if (Time.time - previousTime >= updateInterval)
        {
            float velocity1 = (currentDistance - previousDistance1) / updateInterval;
            float velocity2 = (previousDistance1 - previousDistance2) / updateInterval;
            float acceleration = (velocity1 - velocity2) / updateInterval;

            accelerationText.text = acceleration.ToString();

            if (Input.GetKeyUp(KeyCode.Return))
            {
                PerformRaycast();
                Begin.correctCount++;
                Begin.cnt++;
                Debug.Log(Begin.correctCount.ToString() + Begin.cnt.ToString());

                if (true)
                {
                    buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
                }
                else
                {
                    Begin.stopwatch.Stop();
                }
            }

            previousDistance2 = previousDistance1;
            previousDistance1 = currentDistance;
            previousTime = Time.time;
        }
    }

    // レイキャストを実行し、ボタンが押されたか確認
    private void PerformRaycast()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = pointer2d.transform.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            UnityEngine.Debug.Log(result.ToString());
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                UnityEngine.Debug.Log("Pushed");
                Begin.correctCount++;
                break;
            }
        }
    }

    // 座標履歴にデータを追加
    private void AddToHistory(Vector3 pos, Queue<Vector3> history)
    {
        if (history.Count >= BufferSize)
        {
            history.Dequeue();
        }
        history.Enqueue(pos);
    }

    // 座標履歴から平滑化された位置を計算
    private Vector3 CalculateSmoothedPosition(Queue<Vector3> history)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 pos in history)
        {
            sum += pos;
        }
        return sum / history.Count;
    }

    // スクリーン上でのポインタ位置を計算
    private void CalculatePointerPosition(Vector3 pos1, Vector3 pos2)
    {
        t = (d - (a * pos1.x + b * pos1.y + c * pos1.z))
            / (a * (pos2.x - pos1.x) + b * (pos2.y - pos1.y) + c * (pos2.z - pos1.z));

        poiPos3d.x = pos1.x + t * (pos2.x - pos1.x);
        poiPos3d.y = pos1.y + t * (pos2.y - pos1.y);
        poiPos3d.z = pos1.z + t * (pos2.z - pos1.z);

        poiPos2d.x = poiPos3d.x * sx;
        poiPos2d.y = (poiPos3d.y + sensorPosY) * sy - displayY / 2;
        poiPos2d.z = 0;
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
