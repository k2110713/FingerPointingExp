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
    // �������̂��߂̃o�b�t�@�T�C�Y
    private const int BufferSize = 10;

    // pos1��pos2�̍��W�̗���
    private Queue<Vector3> pos1History = new Queue<Vector3>();
    private Queue<Vector3> pos2History = new Queue<Vector3>();

    // pos1��pos2�̕��������ꂽ���W
    private Vector3 smoothedPos1 = Vector3.zero;
    private Vector3 smoothedPos2 = Vector3.zero;

    public Camera mainCamera;

    public LeapServiceProvider leapProvider;

    public TextMeshProUGUI accelerationText;

    //�|�C���^�I�u�W�F�N�g(2����)
    public GameObject pointer2d;
    public GameObject midAirButton;

    //�{�^��
    public GameObject buttonObject;

    public RectTransform canvasRectTransform; // Canvas �� RectTransform
    public RectTransform buttonRectTransform; // Button �� RectTransform

    //�e�L�X�g
    //public TextMeshProUGUI distanceText;
    //public TextMeshProUGUI accelerationText;

    //�������Ɋւ���萔(�S�ă��[�g��)
    public float pixelX = 1920; //�𑜓xX
    public float pixelY = 1080; //�𑜓xY

    //�󒆑��f�B�X�v���C�̈ʒu�擾�̂���
    public GameObject display;

    //�|�C���^�\���p�̔}��ϐ�
    float t = 0.0f;

    //���ʂ�3�_�i�����j
    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    //���ʂ̕������̒萔
    float a, b, c, d;

    //3D�|�C���^���W
    Vector3 poiPos3d = new Vector3(0f, 0f, 0f);

    //2D�|�C���^���W
    Vector3 poiPos2d = new Vector3(0f, 0f, 0f);

    //2D�|�C���^���S�����p
    public float difX = 0, difY = 0;

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

    //���蓮��̃��[�h(0�F�����X�C�b�`�A1�F�l�����w�̓���)
    public int mode = 0;

    //�^�b�`���͂�F�����邽�߂̕ϐ�
    private float previousZpos = 0.0f;

    //�N�[���_�E���^�C��
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    void Start()
    {
        //�������̒萔����
        sx = pixelX / display.transform.localScale.x;
        sy = pixelY / display.transform.localScale.y;
        p1.z = -display.transform.position.z; p2.z = -display.transform.position.z; p3.z = -display.transform.position.z;

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

        Begin.cnt = 0;
        Begin.correctCount = 0;

        //�����Ҋm�F�p
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
                //�l�����w
                Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
                //�l�����w�̍��{
                Bone boneBase = indexFinger.Bone((Bone.BoneType)0);
                Vector3 pos1 = boneBase.NextJoint;
                //�l�����w�̐�[
                Bone boneTip = indexFinger.Bone((Bone.BoneType)3);
                Vector3 pos2 = boneTip.NextJoint;

                // �����ɍ��W��ǉ����ĕ��������s��
                AddToHistory(pos1, pos1History);
                AddToHistory(pos2, pos2History);

                smoothedPos1 = CalculateSmoothedPosition(pos1History);
                smoothedPos2 = CalculateSmoothedPosition(pos2History);

                // ���������ꂽ���W�ňȍ~�̌v�Z�����s
                CalculatePointerPosition(smoothedPos1, smoothedPos2);

                if (Input.GetKey(KeyCode.Space))
                {
                    difX = poiPos2d.x;
                    difY = poiPos2d.y;
                }

                //UnityEngine.Debug.Log("pointer: " + poiPos2d);
                pointer2d.transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);

                //�^�b�`���͕��@
                if (Begin.modeStatic == 0)
                {
                    pushButton(smoothedPos2);
                }

                //�w�������͕��@�i��������j
                if (Begin.modeStatic == 1)
                {
                    pushImaginaryButton(-smoothedPos2.z);
                }
            }
        }
    }

    //�l�����w�Ń{�^���������������������
    void pushImaginaryButton(float zpos)
    {
        //2�K�������ĉ����x�Őe�w�̓��������m����
        // ���Ԋu�Ńf�[�^���X�V
        if (Time.time - previousTime >= updateInterval)
        {
            // 1������i���x�j���v�Z
            float velocity1 = (zpos - previousDistance1) / updateInterval;
            float velocity2 = (previousDistance1 - previousDistance2) / updateInterval;

            // 2������i�����x�j���v�Z
            float acceleration = (velocity1 - velocity2) / updateInterval;
            accelerationText.text = acceleration.ToString();

            // �����x��臒l�𒴂����ꍇ�Ƀ{�^������������
            if (acceleration > 4)
            //if (Input.GetKeyUp(KeyCode.Return))
            //if (currentDistance < 1.5 && velocity1 < 0 && velocity2 > 0)
            {
                //�{�^���ォ�ǂ�������
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

            // �O��̋����f�[�^���X�V
            previousDistance2 = previousDistance1;
            previousDistance1 = zpos;
            previousTime = Time.time;
        }
    }

    void pushButton(Vector3 pos)
    {
        // 1�O�̃t���[���ł͒����Ă��Ȃ����A���݂̃t���[���ł͒������ꍇ
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

        // ���݂�pos.z�����̃t���[���̂��߂ɕۑ�
        previousZpos = pos.z;
    }

    bool IsButtonAtPositionRay(Vector3 position)
    {
        // ���C�L���X�g�p�̃f�[�^���쐬
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        // ���C�L���X�g���ʂ��i�[���郊�X�g���쐬
        List<RaycastResult> results = new List<RaycastResult>();

        // ���C�L���X�g�����s
        EventSystem.current.RaycastAll(pointerData, results);

        // ���C�L���X�g���ʂ��m�F
        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                // �w�肳�ꂽ���W�Ƀ{�^�������݂���ꍇ
                return true;
            }
        }

        // �w�肳�ꂽ���W�Ƀ{�^�������݂��Ȃ��ꍇ
        return false;
    }

    // �w�肳�ꂽ���W�Ƀ{�^�������݂��Ă��邩�ǂ������m�F����֐�
    bool IsButtonAtPosition(Vector3 position)
    {
        // �{�^����RectTransform���擾
        RectTransform buttonRectTransform = midAirButton.GetComponent<RectTransform>();

        // �{�^���̒��S���W�ƃT�C�Y���擾
        Vector3 buttonPosition = midAirButton.transform.localPosition;
        Vector2 buttonScale = midAirButton.transform.localScale;

        // �{�^���̗̈���v�Z
        float buttonMinX = buttonPosition.x - buttonScale.x / 2;
        float buttonMaxX = buttonPosition.x + buttonScale.x / 2;
        float buttonMinY = buttonPosition.y - buttonScale.y / 2;
        float buttonMaxY = buttonPosition.y + buttonScale.y / 2;
        // �f�o�b�O���O�ɕϐ����o��
        /*UnityEngine.Debug.Log("Button Position: " + buttonPosition);
        UnityEngine.Debug.Log("Button Scale: " + buttonScale);
        UnityEngine.Debug.Log("Button MinX: " + buttonMinX);
        UnityEngine.Debug.Log("Button MaxX: " + buttonMaxX);
        UnityEngine.Debug.Log("Button MinY: " + buttonMinY);
        UnityEngine.Debug.Log("Button MaxY: " + buttonMaxY);
        UnityEngine.Debug.Log("Pointer Position: " + position);*/

        // �w�肳�ꂽ���W���{�^���̗̈���ɂ��邩���m�F
        if (position.x >= buttonMinX && position.x <= buttonMaxX &&
            position.y >= buttonMinY && position.y <= buttonMaxY)
        {
            return true;
        }

        // �w�肳�ꂽ���W���{�^���̗̈�O�ɂ���ꍇ
        return false;
    }

    void AddToHistory(Vector3 pos, Queue<Vector3> history)
    {
        if (history.Count >= BufferSize)
        {
            history.Dequeue(); // �Â��f�[�^���폜
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
        // �X�N���[�����ʂƎw�����̌�_���v�Z��������̃��W�b�N�������ɓK�p
        // t, poiPos3d.x, poiPos3d.y, poiPos3d.z �̌v�Z�Ȃ�
        // ���̕����̃R�[�h�͏�L�̍X�V�O�̃��W�b�N�Ɋ�Â�
        //�X�N���[�����ʂƎw�����̌�_���v�Z
        t = (d - (a * pos1.x + b * pos1.y + c * pos1.z))
            / (a * (pos2.x - pos1.x) + b * (pos2.y - pos1.y) + c * (pos2.z - pos1.z));

        poiPos3d.x = pos1.x + t * (pos2.x - pos1.x);
        poiPos3d.y = pos1.y + t * (pos2.y - pos1.y);
        poiPos3d.z = pos1.z + t * (pos2.z - pos1.z);

        //2D�ɍ��W�ϊ�
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