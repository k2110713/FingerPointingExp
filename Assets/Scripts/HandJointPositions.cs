using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
using System.Runtime.CompilerServices;

public class HandJointPositions : MonoBehaviour
{
    public Camera mainCamera;

    public LeapServiceProvider leapProvider;
    
    //ポインタオブジェクト(2次元)
    public GameObject pointer2d;
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
    //2Dポインタ座標
    Vector3[] poiPos2d = 
        new[] { 
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f), 
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f)
        };
    Vector3 poiPos2dMean = new Vector3();
    //3Dから2Dへの変換定数
    float sx;
    float sy;

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
                Bone bone = indexFinger.Bone(0);
                Vector3 pos1 = bone.NextJoint;
                
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

                //5回の平均をとる
                poiPos2dMean = poiPos2d[0];
                for(int i = 1; i < 5; i++)
                {
                    poiPos2dMean += poiPos2d[i];
                    poiPos2d[i - 1] = poiPos2d[i];
                }
                poiPos2dMean /= 5;

                //2Dポインタの座標を変更
                //pointer2d.transform.localPosition = poiPos2d[4];
                pointer2d.transform.localPosition = poiPos2dMean;

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

                Debug.Log((midFinPos1 - thumbPos).sqrMagnitude * 1000);
                /*if((midFinPos1 - thumbPos).sqrMagnitude * 1000 < 1.5)
                {
                    MouseDown(poiPos2dMean);
                }
                else
                {
                    MouseUp(poiPos2dMean);
                }*/
            }
        }
    }

    void MouseDown(Vector2 screenPosition)
    {
        // スクリーン座標からワールド座標に変換
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));

        // マウスイベントをシミュレートするためのRaycast
        Ray ray = new Ray(worldPosition, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // クリックイベントのデータを作成
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
                position = screenPosition,
                clickTime = Time.time
            };

            // クリックイベントを送信
            ExecuteEvents.Execute(hit.collider.gameObject, pointerEventData, ExecuteEvents.pointerDownHandler);
        }
    }

    void MouseUp(Vector2 screenPosition)
    {
        // スクリーン座標からワールド座標に変換
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));

        // マウスイベントをシミュレートするためのRaycast
        Ray ray = new Ray(worldPosition, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // クリックイベントのデータを作成
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
                position = screenPosition,
                clickTime = Time.time
            };

            // クリックイベントを送信
            ExecuteEvents.Execute(hit.collider.gameObject, pointerEventData, ExecuteEvents.pointerUpHandler);
        }
    }
}