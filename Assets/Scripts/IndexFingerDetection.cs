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

    //�{�^������������
    private int pushCount = 0;

    //�|�C���^�I�u�W�F�N�g(2����)
    public GameObject pointer2d;

    //�e�L�X�g
    //public TextMeshProUGUI distanceText;
    //public TextMeshProUGUI accelerationText;

    //�������Ɋւ���萔(�S�ă��[�g��)
    public float displayX; //�𑜓xX
    public float displayY; //�𑜓xY
    public float displayWidth; //�f�B�X�v���C��
    public float displayHeight; //�f�B�X�v���C����
    public float sensorPosY; //�f�B�X�v���C���ӂ̒��_����Z���T�̑��΋���(����)
    public float sensorPosZ; //�f�B�X�v���C���ӂ̒��_����Z���T�̑��΋���(���s)

    //�|�C���^�\���p�̔}��ϐ�
    float t = 0.0f;

    //���ʂ�3�_(Leap Motion ���� displayDistance ���[�g�����ꂽ��������)
    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    //���ʂ̕������̒萔
    float a, b, c, d;

    //3D�|�C���^���W
    Vector3 poiPos3d = new Vector3();

    //2D�|�C���^���W(5����)
    Vector3[] poiPos2d =
        new[] {
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 0f)
        };
    Vector3 poiPos2dMean = new Vector3();

    //2D�|�C���^���S�����p
    float difX = 0, difY = 0;

    //3D����2D�ւ̕ϊ��萔
    float sx;
    float sy;

    //�O��̎��ԁi�����p�j
    private float previousTime = 0.0f;
    // �Z���T�f�[�^�̍X�V�Ԋu
    public float updateInterval = 0.1f;
    //�����x��臒l
    private float[] thresholdAcceleration = { 100, 100, 200 };
    //2������p�f�[�^
    private float previousDistance1 = 0.0f;
    private float previousDistance2 = 0.0f;

    //�`���^�����O�h�~�̂��߂̃N�[���_�E���^�C��
    private bool isCooldown = false;
    private float cooldownTime = 0.5f;

    //���蓮��̃��[�h(0�F�����X�C�b�`�A1�F�l�����w�̓���)
    public int mode = 0;

    void Start()
    {
        //�������̒萔����
        sx = displayX / displayWidth;
        sy = displayY / displayHeight;
        p1.z = sensorPosZ; p2.z = sensorPosZ; p3.z = sensorPosZ;

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

        // ������
        previousTime = Time.time;
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
                Bone bone = indexFinger.Bone((Bone.BoneType)2);
                Vector3 pos1 = bone.NextJoint;
                //pos1 = hand.PalmPosition;

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

                //2D�ɍ��W�ϊ�
                poiPos2d[4].x = poiPos3d.x * sx;
                poiPos2d[4].y = (poiPos3d.y + sensorPosY) * sy - displayY / 2;
                poiPos2d[4].z = 0;

                //5��̕��ς��Ƃ�
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

    //�l�����w�Ń{�^���������������������
    void pushImaginaryButton(float currentDistance)
    {
        //2�K�������ĉ����x�Őe�w�̓��������m����
        // ���Ԋu�Ńf�[�^���X�V
        if (Time.time - previousTime >= updateInterval)
        {
            // 1������i���x�j���v�Z
            float velocity1 = (currentDistance - previousDistance1) / updateInterval;
            float velocity2 = (previousDistance1 - previousDistance2) / updateInterval;

            // 2������i�����x�j���v�Z
            float acceleration = (velocity1 - velocity2) / updateInterval;
            //accelerationText.text = acceleration.ToString();

            // �����x��臒l�𒴂����ꍇ�Ƀ{�^������������
            if (acceleration > thresholdAcceleration[mode])
            //if (currentDistance < 1.5 && velocity1 < 0 && velocity2 > 0)
            {
                // ���C�L���X�g�p�̃f�[�^���쐬
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = pointer2d.transform.position
                };
                //Debug.Log(pointerData.position);

                // ���C�L���X�g���ʂ��i�[���郊�X�g���쐬
                List<RaycastResult> results = new List<RaycastResult>();

                // ���C�L���X�g�����s
                EventSystem.current.RaycastAll(pointerData, results);

                // ���C�L���X�g���ʂ��m�F
                foreach (RaycastResult result in results)
                {
                    UnityEngine.Debug.Log(result.ToString());
                    // ���ʂ�Button�R���|�[�l���g�����ꍇ
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

            // �O��̋����f�[�^���X�V
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
