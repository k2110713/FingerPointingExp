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
    public Camera mainCamera;

    public LeapServiceProvider leapProvider;

    //ボタンを押した回数
    private int pushCount = 0;

    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;

    //テキスト
    //public TextMeshProUGUI distanceText;
    //public TextMeshProUGUI accelerationText;

    //実験環境に関する定数(全てメートル)
    public float displayX; //解像度X
    public float displayY; //解像度Y
    public float displayWidth; //ディスプレイ幅
    public float displayHeight; //ディスプレイ高さ
    public float sensorPosY; //ディスプレイ下辺の中点からセンサの相対距離(高さ)
    public float sensorPosZ; //ディスプレイ下辺の中点からセンサの相対距離(奥行)

    //ポインタ表示用の媒介変数
    float t = 0.0f;

    //平面の3点(Leap Motion から displayDistance メートル離れた垂直平面)
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

    //2Dポインタ中心調整用
    float difX = 0, difY = 0;

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

    //チャタリング防止のためのクールダウンタイム
    private bool isCooldown = false;
    private float cooldownTime = 0.5f;

    //決定動作のモード(0：銅箔スイッチ、1：人さし指の動き)
    public int mode = 0;

    void Start()
    {
        //実験環境の定数を代入
        sx = displayX / displayWidth;
        sy = displayY / displayHeight;
        p1.z = sensorPosZ; p2.z = sensorPosZ; p3.z = sensorPosZ;

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

                //2Dに座標変換
                poiPos2d[4].x = poiPos3d.x * sx;
                poiPos2d[4].y = (poiPos3d.y + sensorPosY) * sy - displayY / 2;
                poiPos2d[4].z = 0;

                //5回の平均をとる
                poiPos2dMean = poiPos2d[0];
                for (int i = 1; i < 5; i++)
                {
                    poiPos2dMean += poiPos2d[i];
                    poiPos2d[i - 1] = poiPos2d[i];
                }
                poiPos2dMean /= 5;

                if (Input.GetKey(KeyCode.Space))
                {
                    difX = poiPos2dMean.x;
                    difY = poiPos2dMean.y;
                }

                Debug.Log("pointer: " + poiPos2dMean);
                pointer2d.transform.localPosition = new Vector3(poiPos2dMean.x - difX, poiPos2dMean.y - difY, poiPos2dMean.z);

                pushImaginaryButton(-pos2.z);
            }
        }
    }

    //人さし指でボタンを押す動作をした判定
    void pushImaginaryButton(float currentDistance)
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
            //accelerationText.text = acceleration.ToString();

            // 加速度が閾値を超えた場合にボタンを押す処理
            if (acceleration > thresholdAcceleration[mode])
            //if (currentDistance < 1.5 && velocity1 < 0 && velocity2 > 0)
            {
                // レイキャスト用のデータを作成
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = pointer2d.transform.position
                };
                //Debug.Log(pointerData.position);

                // レイキャスト結果を格納するリストを作成
                List<RaycastResult> results = new List<RaycastResult>();

                // レイキャストを実行
                EventSystem.current.RaycastAll(pointerData, results);

                // レイキャスト結果を確認
                foreach (RaycastResult result in results)
                {
                    UnityEngine.Debug.Log(result.ToString());
                    // 結果がButtonコンポーネントを持つ場合
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        //PushedOrNot.text = "Pushed";
                        UnityEngine.Debug.Log("Pushed");
                        Begin.correctCount++;
                        break;
                    }
                }
            }

            // 前回の距離データを更新
            previousDistance2 = previousDistance1;
            previousDistance1 = currentDistance;
            previousTime = Time.time;
        }
    }

    /*private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }*/
}
