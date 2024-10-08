using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class IndexFingerDetection : MonoBehaviour
{
    // 平滑化のためのバッファサイズ
    private const int BufferSize = 8;

    // pos1とpos2の座標の履歴
    private Queue<Vector3> pos1History = new Queue<Vector3>();
    private Queue<Vector3> pos2History = new Queue<Vector3>();

    // pos1とpos2の平滑化された座標
    private Vector3 smoothedPos1 = Vector3.zero;
    private Vector3 smoothedPos2 = Vector3.zero;

    public Camera mainCamera;

    // OptiTrackのクライアント
    public OptitrackStreamingClient optitrackClient;

    public TextMeshProUGUI accelerationText;

    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;
    public GameObject midAirButton;

    //ボタン
    public GameObject buttonObject;

    public RectTransform canvasRectTransform; // Canvas の RectTransform
    public RectTransform buttonRectTransform; // Button の RectTransform

    //実験環境に関する定数(全てメートル)
    public float pixelX = 1920; //解像度X
    public float pixelY = 1080; //解像度Y

    //空中像ディスプレイの位置取得のため
    public GameObject display;

    //ポインタ表示用の媒介変数
    float t = 0.0f;

    //平面の3点（垂直）
    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    //平面の方程式の定数
    float a, b, c, d;

    //3Dポインタ座標
    Vector3 poiPos3d = new Vector3(0f, 0f, 0f);

    //2Dポインタ座標
    Vector3 poiPos2d = new Vector3(0f, 0f, 0f);

    //2Dポインタ中心調整用
    public float difX = 0, difY = 0;

    //3Dから2Dへの変換定数
    float sx;
    float sy;

    //前回の時間（微分用）
    private float previousTime = 0.0f;
    // センサデータの更新間隔
    public float updateInterval = 0.1f;
    //加速度の閾値
    private float[] thresholdAcceleration = { 100, 100, 200 };
    //2回微分用データ
    private float previousDistance1 = 0.0f;
    private float previousDistance2 = 0.0f;

    //タッチ入力を認識するための変数
    private float previousZpos = 0.0f;

    //クールダウンタイム
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    // マーカーのID（OptiTrackでトラッキングする2つのポイント）
    public int marker1ID = 1; // 指の根元に相当するマーカーID
    public int marker2ID = 2; // 指の先端に相当するマーカーID

    void Start()
    {
        //実験環境の定数を代入
        sx = pixelX / display.transform.localScale.x;
        sy = pixelY / display.transform.localScale.y;
        p1.z = -display.transform.position.z; p2.z = -display.transform.position.z; p3.z = -display.transform.position.z;

        // ベクトルABとACを計算
        Vector3 AB = p2 - p1;
        Vector3 AC = p3 - p1;

        // ABとACの外積を計算して法線ベクトルを得る
        Vector3 normal = Vector3.Cross(AB, AC);

        // 法線ベクトルの成分をa, b, cに代入
        a = normal.x;
        b = normal.y;
        c = normal.z;
        d = -(a * p1.x + b * p1.y + c * p1.z);

        // 初期化
        previousTime = Time.time;
    }

    void Update()
    {
        if (optitrackClient == null)
        {
            UnityEngine.Debug.LogError("OptitrackStreamingClient is not assigned.");
            return;
        }

        // マーカーの位置情報を取得
        var marker1State = optitrackClient.GetLatestMarkerStates()[0];
        var marker2State = optitrackClient.GetLatestMarkerStates()[1];

        if (marker1State != null && marker2State != null)
        {
            Vector3 pos1 = marker1State.Position;
            Vector3 pos2 = marker2State.Position;

            // 履歴に座標を追加して平滑化を行う
            AddToHistory(pos1, pos1History);
            AddToHistory(pos2, pos2History);

            smoothedPos1 = CalculateSmoothedPosition(pos1History);
            smoothedPos2 = CalculateSmoothedPosition(pos2History);

            // 平滑化された座標で以降の計算を実行
            CalculatePointerPosition(smoothedPos1, smoothedPos2);

            if (Input.GetKey(KeyCode.Space))
            {
                difX = poiPos2d.x;
                difY = poiPos2d.y;
            }

            pointer2d.transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);

            //タッチ入力方法
            pushButton(smoothedPos2);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Marker data not available.");
        }
    }

    void pushButton(Vector3 pos)
    {
        // 1つ前のフレームでは超えていないが、現在のフレームでは超えた場合
        if (previousZpos <= display.transform.position.z && pos.z > display.transform.position.z)
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
                Cooldown();
                buttonObject.SetActive(false);
            }
        }

        // 現在のpos.zを次のフレームのために保存
        previousZpos = pos.z;
    }

    bool IsButtonAtPosition(Vector3 position)
    {
        RectTransform buttonRectTransform = midAirButton.GetComponent<RectTransform>();

        Vector3 buttonPosition = midAirButton.transform.localPosition;
        Vector2 buttonScale = midAirButton.transform.localScale;

        float buttonMinX = buttonPosition.x - buttonScale.x / 2;
        float buttonMaxX = buttonPosition.x + buttonScale.x / 2;
        float buttonMinY = buttonPosition.y - buttonScale.y / 2;
        float buttonMaxY = buttonPosition.y + buttonScale.y / 2;

        if (position.x >= buttonMinX && position.x <= buttonMaxX &&
            position.y >= buttonMinY && position.y <= buttonMaxY)
        {
            return true;
        }
        return false;
    }

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
        // スクリーン平面と指直線の交点を計算する既存のロジックをここに適用
        // t, poiPos3d.x, poiPos3d.y, poiPos3d.z の計算など
        // この部分のコードは上記の更新前のロジックに基づく
        //スクリーン平面と指直線の交点を計算
        t = (d - (a * pos1.x + b * pos1.y + c * pos1.z))
            / (a * (pos2.x - pos1.x) + b * (pos2.y - pos1.y) + c * (pos2.z - pos1.z));

        poiPos3d.x = pos1.x + t * (pos2.x - pos1.x);
        poiPos3d.y = pos1.y + t * (pos2.y - pos1.y);
        poiPos3d.z = pos1.z + t * (pos2.z - pos1.z);

        //2Dに座標変換
        poiPos2d.x = poiPos3d.x * sx;
        poiPos2d.y = (poiPos3d.y - display.transform.position.y) * sy;
        poiPos2d.z = 0;
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
