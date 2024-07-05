using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

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

    public Camera mainCamera;

    public LeapServiceProvider leapProvider;

    public TextMeshProUGUI accelerationText;

    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;
    public GameObject midAirButton;

    //ボタン
    public GameObject buttonObject;

    public RectTransform canvasRectTransform; // Canvas の RectTransform
    public RectTransform buttonRectTransform; // Button の RectTransform

    //テキスト
    //public TextMeshProUGUI distanceText;
    //public TextMeshProUGUI accelerationText;

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

    //決定動作のモード(0：銅箔スイッチ、1：人さし指の動き)
    public int mode = 0;

    //タッチ入力を認識するための変数
    private float previousZpos = 0.0f;

    //クールダウンタイム
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

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

        Begin.cnt = 0;
        Begin.correctCount = 0;

        //実験者確認用
        UnityEngine.Debug.Log(
            "mode: " + Begin.modeStatic +
            ", practice: " + Begin.practiceNum +
            ", test: " + Begin.testNum +
            ", current: " + Begin.currentNum
            );

        if (Begin.modeStatic == 0)
        {
            pointer2d.SetActive(false);
        }
        else
        {
            pointer2d.SetActive(true);
        }

        if (Begin.modeStatic == 0 || Begin.modeStatic == 1)
        {
            Begin.stopwatch = Stopwatch.StartNew();
        }
    }

    void Update()
    {
        midAirButton.transform.position = new Vector3(buttonObject.transform.localPosition.x / sx, buttonObject.transform.localPosition.y / sy + display.transform.position.y, -0.232f);
        Frame frame = leapProvider.CurrentFrame;
        foreach (Hand hand in frame.Hands)
        {
            if (hand.IsRight)
            {
                //人さし指
                Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                //人さし指の根本
                Bone boneBase = indexFinger.Bone((Bone.BoneType)0);
                Vector3 pos1 = boneBase.NextJoint;
                //人さし指の先端
                Bone boneTip = indexFinger.Bone((Bone.BoneType)3);
                Vector3 pos2 = boneTip.NextJoint;

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

                //UnityEngine.Debug.Log("pointer: " + poiPos2d);
                pointer2d.transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);

                //タッチ入力方法
                if (Begin.modeStatic == 0)
                {
                    pushButton(smoothedPos2);
                }

                //指さし入力方法（押す動作）
                if (Begin.modeStatic == 1)
                {
                    pushImaginaryButton(-smoothedPos2.z);
                }
            }
        }
    }

    //人さし指でボタンを押す動作をした判定
    void pushImaginaryButton(float zpos)
    {
        //2階微分して加速度で親指の動きを検知する
        // 一定間隔でデータを更新
        if (Time.time - previousTime >= updateInterval)
        {
            // 1回微分（速度）を計算
            float velocity1 = (zpos - previousDistance1) / updateInterval;
            float velocity2 = (previousDistance1 - previousDistance2) / updateInterval;

            // 2回微分（加速度）を計算
            float acceleration = (velocity1 - velocity2) / updateInterval;
            accelerationText.text = acceleration.ToString();

            // 加速度が閾値を超えた場合にボタンを押す処理
            if (acceleration > 4)
            //if (Input.GetKeyUp(KeyCode.Return))
            //if (currentDistance < 1.5 && velocity1 < 0 && velocity2 > 0)
            {
                //ボタン上かどうか判定
                if (IsButtonAtPositionRay(pointer2d.transform.position))
                {
                    //PushedOrNot.text = "Pushed";
                    //UnityEngine.Debug.Log("Pushed");
                    Begin.correctCount++;
                }
                Begin.cnt++;
                //UnityEngine.Debug.Log(Begin.correctCount.ToString() + Begin.cnt.ToString());

                if (Begin.cnt < Begin.testNumInOnce)
                //if (true)
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

            // 前回の距離データを更新
            previousDistance2 = previousDistance1;
            previousDistance1 = zpos;
            previousTime = Time.time;
        }
    }

    void pushButton(Vector3 pos)
    {
        // 1つ前のフレームでは超えていないが、現在のフレームでは超えた場合
        if (previousZpos <= display.transform.position.z && pos.z > display.transform.position.z)
        {
            if (IsButtonAtPosition(pos))
            {
                //UnityEngine.Debug.Log("Pushed");
                Begin.correctCount++;
            }
            /*if (IsButtonAtPosition(pos))
            {
                UnityEngine.Debug.Log("success"); 
            }
            else
            {
                UnityEngine.Debug.Log("failed");
            }*/
            Begin.cnt++;
            //UnityEngine.Debug.Log(Begin.correctCount.ToString() + Begin.cnt.ToString());
            if (Begin.cnt < Begin.testNumInOnce)
            //if (true)
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

    bool IsButtonAtPositionRay(Vector3 position)
    {
        // レイキャスト用のデータを作成
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        // レイキャスト結果を格納するリストを作成
        List<RaycastResult> results = new List<RaycastResult>();

        // レイキャストを実行
        EventSystem.current.RaycastAll(pointerData, results);

        // レイキャスト結果を確認
        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                // 指定された座標にボタンが存在する場合
                return true;
            }
        }

        // 指定された座標にボタンが存在しない場合
        return false;
    }

    // 指定された座標にボタンが存在しているかどうかを確認する関数
    bool IsButtonAtPosition(Vector3 position)
    {
        // ボタンのRectTransformを取得
        RectTransform buttonRectTransform = midAirButton.GetComponent<RectTransform>();

        // ボタンの中心座標とサイズを取得
        Vector3 buttonPosition = midAirButton.transform.localPosition;
        Vector2 buttonScale = midAirButton.transform.localScale;

        // ボタンの領域を計算
        float buttonMinX = buttonPosition.x - buttonScale.x / 2;
        float buttonMaxX = buttonPosition.x + buttonScale.x / 2;
        float buttonMinY = buttonPosition.y - buttonScale.y / 2;
        float buttonMaxY = buttonPosition.y + buttonScale.y / 2;
        // デバッグログに変数を出力
        /*UnityEngine.Debug.Log("Button Position: " + buttonPosition);
        UnityEngine.Debug.Log("Button Scale: " + buttonScale);
        UnityEngine.Debug.Log("Button MinX: " + buttonMinX);
        UnityEngine.Debug.Log("Button MaxX: " + buttonMaxX);
        UnityEngine.Debug.Log("Button MinY: " + buttonMinY);
        UnityEngine.Debug.Log("Button MaxY: " + buttonMaxY);
        UnityEngine.Debug.Log("Pointer Position: " + position);*/

        // 指定された座標がボタンの領域内にあるかを確認
        if (position.x >= buttonMinX && position.x <= buttonMaxX &&
            position.y >= buttonMinY && position.y <= buttonMaxY)
        {
            return true;
        }

        // 指定された座標がボタンの領域外にある場合
        return false;
    }

    void AddToHistory(Vector3 pos, Queue<Vector3> history)
    {
        if (history.Count >= BufferSize)
        {
            history.Dequeue(); // 古いデータを削除
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