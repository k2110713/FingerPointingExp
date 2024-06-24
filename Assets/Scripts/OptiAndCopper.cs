using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class OptiAndCopper : MonoBehaviour
{
    public Camera mainCamera;

    public LeapServiceProvider leapProvider;

    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;
    //テキスト
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI accelerationText;
    //ラインオブジェクト
    //LineRenderer linerend;
    //ポインタオブジェクト(3次元)
    public GameObject pointer3d;
    //実験環境に関する定数(全てメートル)
    public float displayX; //解像度X
    public float displayY; //解像度Y
    public float displayWidth; //ディスプレイ幅
    public float displayHeight; //ディスプレイ高さ
    public float displayDistance; //Leap Motion とディスプレイの距離
    //ポインタ表示用の媒介変数
    float t = 0.0f;
    //直線表示用の2点
    //const float t1 = -100.0f;
    //const float t2 = 100.0f;
    //平面の3点(Leap Motion から1メートル離れた垂直平面)
    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);
    //平面の方程式の定数
    float a, b, c, d;
    //3Dポインタ座標
    Vector3 poiPos3d = new Vector3();
    //2Dポインタ座標(5個平均)
    Vector3[] poiPos2d =
        new[] {
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f)
        };
    Vector3 poiPos2dMean = new Vector3();
    //2Dポインタ座標(10個平均)
    Vector3[] poiPos2d20 =
        new[] {
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f)
        };
    public Vector3 poiPos2dMean20 = new Vector3();
    //2Dポインタ中心調整用
    float difX = 0, difY = 0;
    //3Dから2Dへの変換定数
    float sx;
    float sy;

    //指間の距離
    public float fingersDistance = 3.0f;
    //指間の距離(5個平均)
    float[] fingersDistances =
        new[] {
        0.0f,
        0.0f,
        0.0f,
        0.0f,
        0.0f
        };

    //前回の時間（微分用）
    private float previousTime = 0.0f;
    // センサデータの更新間隔
    public float updateInterval = 0.1f;
    //加速度の閾値
    private float[] thresholdAcceleration = { 100, 100, 200 };
    //2回微分用データ
    private float previousDistance1 = 0.0f;
    private float previousDistance2 = 0.0f;

    //チャタリング防止のためのクールダウンタイム
    private bool isCooldown = false;
    private float cooldownTime = 0.5f;

    //決定動作のモード(0,1,2)
    public int mode = 0;

    void Start()
    {
        //linerend = gameObject.GetComponent<LineRenderer>();

        //実験環境の定数を代入
        sx = displayX / displayWidth;
        sy = displayY / displayHeight;
        p1.z = -displayDistance; p2.z = -displayDistance; p3.z = -displayDistance;

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
        // 現在のフレームを取得
        Frame frame = leapProvider.CurrentFrame;

        // フレーム内の各手をループ
        foreach (Hand hand in frame.Hands)
        {
            // 一旦右手のみ
            if (hand.IsRight)
            {
                // 人差し指を取得
                Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                // 中手骨とその上の関節（指の根本）
                Bone bone = indexFinger.Bone((Bone.BoneType)2);
                Vector3 pos1 = bone.NextJoint;
                //pos1 = hand.PalmPosition;

                // 遠位指節骨とその上の関節（指先）
                bone = indexFinger.Bone((Bone.BoneType)3);
                Vector3 pos2 = bone.NextJoint;

                //直線描画
                /*Vector3 linepos1 = new Vector3(pos1.x + t1 * (pos2.x - pos1.x), pos1.y + t1 * (pos2.y - pos1.y), pos1.z + t1 * (pos2.z - pos1.z));
                Vector3 linepos2 = new Vector3(pos1.x + t2 * (pos2.x - pos1.x), pos1.y + t2 * (pos2.y - pos1.y), pos1.z + t2 * (pos2.z - pos1.z));
                linerend.SetPosition(0, linepos1);
                linerend.SetPosition(1, linepos2);*/

                //スクリーン平面と指直線の交点を計算
                t = (d - (a * pos1.x + b * pos1.y + c * pos1.z))
                    / (a * (pos2.x - pos1.x) + b * (pos2.y - pos1.y) + c * (pos2.z - pos1.z));

                poiPos3d.x = pos1.x + t * (pos2.x - pos1.x);
                poiPos3d.y = pos1.y + t * (pos2.y - pos1.y);
                poiPos3d.z = pos1.z + t * (pos2.z - pos1.z);

                //3Dポインタの座標を変更
                pointer3d.transform.position = poiPos3d;
                //Debug.Log(poiPos);

                //2Dに座標変換
                poiPos2d[4].x = poiPos3d.x * sx;
                poiPos2d[4].y = poiPos3d.y * sy - displayY / 2;
                poiPos2d[4].z = 0;
                poiPos2d20[19] = poiPos2d[4];

                //5回の平均をとる
                poiPos2dMean = poiPos2d[0];
                for (int i = 1; i < 5; i++)
                {
                    poiPos2dMean += poiPos2d[i];
                    poiPos2d[i - 1] = poiPos2d[i];
                }
                poiPos2dMean /= 5;

                //20回の平均をとる
                poiPos2dMean20 = poiPos2d20[0];
                for (int i = 1; i < 20; i++)
                {
                    poiPos2dMean20 += poiPos2d20[i];
                    poiPos2d20[i - 1] = poiPos2d20[i];
                }
                poiPos2dMean20 /= 20;

                if (Input.GetKey(KeyCode.Space))
                {
                    difX = poiPos2dMean.x;
                    difY = poiPos2dMean.y;
                }

                // 中指を取得
                Finger middleFinger = hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE];
                // 第二関節
                bone = middleFinger.Bone((Bone.BoneType)1);
                Vector3 midFinPos1 = bone.NextJoint;
                // 指先
                bone = middleFinger.Bone((Bone.BoneType)3);
                Vector3 midFinPos2 = bone.NextJoint;

                // 親指を取得
                Finger thumb = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];
                // 指先
                bone = thumb.Bone((Bone.BoneType)2);
                Vector3 thumbPos = bone.NextJoint;

                //2Dポインタの座標を変更
                //if((midFinPos1 - thumbPos).sqrMagnitude * 1000 > 2.0)
                //{
                //中指と親指が離れている（5回平均の方を使う）
                //pointer2d.transform.localPosition = poiPos2d[4];
                pointer2d.transform.localPosition = new Vector3(poiPos2dMean.x - difX, poiPos2dMean.y - difY, poiPos2dMean.z);
                //}
                //else
                //{
                //中指と親指が近い（10回平均の方を使う）
                //pointer2d.transform.localPosition = new Vector3(poiPos2dMean20.x - difX, poiPos2dMean20.y - difY, poiPos2dMean20.z);
                //}

                switch (mode)
                {
                    case 0:
                        //親指の先と中指の第二関節の距離を計算（5回平均）
                        fingersDistances[4] = (midFinPos1 - thumbPos).sqrMagnitude * 1000;
                        fingersDistance = fingersDistances[0];
                        for (int i = 1; i < 5; i++)
                        {
                            fingersDistance += fingersDistances[i];
                            fingersDistances[i - 1] = fingersDistances[i];
                        }
                        fingersDistance /= 5;
                        break;
                    case 1:
                        //親指の先と中指の先の距離を計算（5回平均）
                        fingersDistances[4] = (midFinPos2 - thumbPos).sqrMagnitude * 1000;
                        fingersDistance = fingersDistances[0];
                        for (int i = 1; i < 5; i++)
                        {
                            fingersDistance += fingersDistances[i];
                            fingersDistances[i - 1] = fingersDistances[i];
                        }
                        fingersDistance /= 5;
                        break;
                    case 2:
                        //人差し指の先の奥行き距離を計算（5回平均）
                        fingersDistances[4] = pos2.z * pos2.z * 1000;
                        fingersDistance = fingersDistances[0];
                        for (int i = 1; i < 5; i++)
                        {
                            fingersDistance += fingersDistances[i];
                            fingersDistances[i - 1] = fingersDistances[i];
                        }
                        fingersDistance /= 5;
                        break;
                    default:
                        break;
                }

                distanceText.text = fingersDistance.ToString();

                secondOrderDif(fingersDistance);
            }
        }
    }

    void secondOrderDif(float currentDistance)
    {
        //2階微分して加速度で親指の動きを検知する
        // 一定間隔でデータを更新
        if (Time.time - previousTime >= updateInterval)
        {
            // 1回微分（速度）を計算
            float velocity1 = (currentDistance - previousDistance1) / updateInterval;
            float velocity2 = (previousDistance1 - previousDistance2) / updateInterval;

            // 2回微分（加速度）を計算
            float acceleration = (velocity1 - velocity2) / updateInterval;
            accelerationText.text = acceleration.ToString();

            // 加速度が閾値を超えた場合にボタンを押す処理
            if (acceleration > thresholdAcceleration[mode])
            //if (currentDistance < 1.5 && velocity1 < 0 && velocity2 > 0)
            {
                // レイキャスト用のデータを作成
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = new Vector2(poiPos2dMean20.x + displayX / 2, poiPos2dMean20.y + displayY / 2)
                };
                Debug.Log(pointerData.position);

                // レイキャスト結果を格納するリストを作成
                List<RaycastResult> results = new List<RaycastResult>();

                // レイキャストを実行
                EventSystem.current.RaycastAll(pointerData, results);

                // レイキャスト結果を確認
                foreach (RaycastResult result in results)
                {
                    Debug.Log(result.ToString());
                    // 結果がButtonコンポーネントを持つ場合
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        doClick(result.gameObject, pointerData);
                        Debug.Log("Button Pressed! Acceleration: " + acceleration);
                    }
                }
            }

            // 前回の距離データを更新
            previousDistance2 = previousDistance1;
            previousDistance1 = currentDistance;
            previousTime = Time.time;
        }
    }
    private void doClick(GameObject gameObject, PointerEventData eventData)
    {
        if (isCooldown) return;
        StartCoroutine(Cooldown());

        ExecuteEvents.Execute<IPointerEnterHandler>(gameObject, eventData, (handler, ev) => handler.OnPointerEnter((PointerEventData)ev));
        ExecuteEvents.Execute<IPointerDownHandler>(gameObject, eventData, (handler, ev) => handler.OnPointerDown((PointerEventData)ev));
        ExecuteEvents.Execute<IPointerClickHandler>(gameObject, eventData, (handler, ev) => handler.OnPointerClick((PointerEventData)ev));
        ExecuteEvents.Execute<IPointerUpHandler>(gameObject, eventData, (handler, ev) => handler.OnPointerUp((PointerEventData)ev));
        ExecuteEvents.Execute<IPointerExitHandler>(gameObject, eventData, (handler, ev) => handler.OnPointerExit((PointerEventData)ev));
    }
    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }

}