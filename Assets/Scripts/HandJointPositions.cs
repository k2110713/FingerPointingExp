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
    
    //�|�C���^�I�u�W�F�N�g(2����)
    public GameObject pointer2d;
    //���C���I�u�W�F�N�g
    //LineRenderer linerend;
    //�|�C���^�I�u�W�F�N�g(3����)
    public GameObject pointer3d;
    //�������Ɋւ���萔(�S�ă��[�g��)
    public float displayX; //�𑜓xX
    public float displayY; //�𑜓xY
    public float displayWidth; //�f�B�X�v���C��
    public float displayHeight; //�f�B�X�v���C����
    public float displayDistance; //Leap Motion �ƃf�B�X�v���C�̋���
    //�|�C���^�\���p�̔}��ϐ�
    float t = 0.0f;
    //�����\���p��2�_
    //const float t1 = -100.0f;
    //const float t2 = 100.0f;
    //���ʂ�3�_(Leap Motion ����1���[�g�����ꂽ��������)
    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);
    //���ʂ̕������̒萔
    float a, b, c, d;
    //3D�|�C���^���W
    Vector3 poiPos3d = new Vector3();
    //2D�|�C���^���W
    Vector3[] poiPos2d = 
        new[] { 
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f), 
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f)
        };
    Vector3 poiPos2dMean = new Vector3();
    //3D����2D�ւ̕ϊ��萔
    float sx;
    float sy;

    void Start()
    {
        //linerend = gameObject.GetComponent<LineRenderer>();

        //�������̒萔����
        sx = displayX / displayWidth;
        sy = displayY / displayHeight;
        p1.z = -displayDistance; p2.z = -displayDistance; p3.z = -displayDistance;

        // �x�N�g��AB��AC���v�Z
        Vector3 AB = p2 - p1;
        Vector3 AC = p3 - p1;

        // AB��AC�̊O�ς��v�Z���Ė@���x�N�g���𓾂�
        Vector3 normal = Vector3.Cross(AB, AC);

        // �@���x�N�g���̐�����a, b, c�ɑ��
        a = normal.x;
        b = normal.y;
        c = normal.z;
        d = -(a * p1.x + b * p1.y + c * p1.z);
    }
    void Update()
    {
        // ���݂̃t���[�����擾
        Frame frame = leapProvider.CurrentFrame;
        
        // �t���[�����̊e������[�v
        foreach (Hand hand in frame.Hands)
        {
            // ��U�E��̂�
            if (hand.IsRight)
            {
                // �l�����w���擾
                Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                // ���荜�Ƃ��̏�̊֐߁i�w�̍��{�j
                Bone bone = indexFinger.Bone(0);
                Vector3 pos1 = bone.NextJoint;
                
                // ���ʎw�ߍ��Ƃ��̏�̊֐߁i�w��j
                bone = indexFinger.Bone((Bone.BoneType)3);
                Vector3 pos2 = bone.NextJoint;

                //�����`��
                /*Vector3 linepos1 = new Vector3(pos1.x + t1 * (pos2.x - pos1.x), pos1.y + t1 * (pos2.y - pos1.y), pos1.z + t1 * (pos2.z - pos1.z));
                Vector3 linepos2 = new Vector3(pos1.x + t2 * (pos2.x - pos1.x), pos1.y + t2 * (pos2.y - pos1.y), pos1.z + t2 * (pos2.z - pos1.z));
                linerend.SetPosition(0, linepos1);
                linerend.SetPosition(1, linepos2);*/

                //�X�N���[�����ʂƎw�����̌�_���v�Z
                t = (d - (a * pos1.x + b * pos1.y + c * pos1.z))
                    / (a * (pos2.x - pos1.x) + b * (pos2.y - pos1.y) + c * (pos2.z - pos1.z));

                poiPos3d.x = pos1.x + t * (pos2.x - pos1.x);
                poiPos3d.y = pos1.y + t * (pos2.y - pos1.y);
                poiPos3d.z = pos1.z + t * (pos2.z - pos1.z);

                //3D�|�C���^�̍��W��ύX
                pointer3d.transform.position = poiPos3d;
                //Debug.Log(poiPos);

                //2D�ɍ��W�ϊ�
                poiPos2d[4].x = poiPos3d.x * sx;
                poiPos2d[4].y = poiPos3d.y * sy - displayY / 2;
                poiPos2d[4].z = 0;

                //5��̕��ς��Ƃ�
                poiPos2dMean = poiPos2d[0];
                for(int i = 1; i < 5; i++)
                {
                    poiPos2dMean += poiPos2d[i];
                    poiPos2d[i - 1] = poiPos2d[i];
                }
                poiPos2dMean /= 5;

                //2D�|�C���^�̍��W��ύX
                //pointer2d.transform.localPosition = poiPos2d[4];
                pointer2d.transform.localPosition = poiPos2dMean;

                // ���w���擾
                Finger middleFinger = hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE];
                // ���֐�
                bone = middleFinger.Bone((Bone.BoneType)1);
                Vector3 midFinPos1 = bone.NextJoint;
                // �w��
                bone = middleFinger.Bone((Bone.BoneType)3);
                Vector3 midFinPos2 = bone.NextJoint;

                // �e�w���擾
                Finger thumb = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];
                // �w��
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
        // �X�N���[�����W���烏�[���h���W�ɕϊ�
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));

        // �}�E�X�C�x���g���V�~�����[�g���邽�߂�Raycast
        Ray ray = new Ray(worldPosition, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // �N���b�N�C�x���g�̃f�[�^���쐬
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
                position = screenPosition,
                clickTime = Time.time
            };

            // �N���b�N�C�x���g�𑗐M
            ExecuteEvents.Execute(hit.collider.gameObject, pointerEventData, ExecuteEvents.pointerDownHandler);
        }
    }

    void MouseUp(Vector2 screenPosition)
    {
        // �X�N���[�����W���烏�[���h���W�ɕϊ�
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));

        // �}�E�X�C�x���g���V�~�����[�g���邽�߂�Raycast
        Ray ray = new Ray(worldPosition, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // �N���b�N�C�x���g�̃f�[�^���쐬
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
                position = screenPosition,
                clickTime = Time.time
            };

            // �N���b�N�C�x���g�𑗐M
            ExecuteEvents.Execute(hit.collider.gameObject, pointerEventData, ExecuteEvents.pointerUpHandler);
        }
    }
}