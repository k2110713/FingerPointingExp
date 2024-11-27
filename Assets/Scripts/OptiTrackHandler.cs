using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptiTrackHandler : MonoBehaviour
{
    public GameObject midAirDisplay;
    public OptitrackStreamingClient optitrackClient;

    public float pixelX = 1920;
    public float pixelY = 1080;

    OptitrackMarkerState marker1State;
    OptitrackMarkerState marker2State;

    private const int BufferSize = 8;
    private Queue<Vector3> pointerPosHistory = new Queue<Vector3>();
    private Vector3 filteredPointerPos = Vector3.zero;

    private float sx;
    private float sy;
    private Vector3 poiPos2d = Vector3.zero;

    public float difX = 0, difY = 0;

    private float previousZpos = 0.0f;

    float t = 0.0f;

    Vector3 p1 = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 p2 = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 p3 = new Vector3(0.0f, 1.0f, 0.0f);

    float a, b, c, d;

    public int marker1ID = 1;
    public int marker2ID = 2;

    // �{�^���̃r�W���A���\����ێ����邽�߂̔z��
    [SerializeField] private Image[] buttonImages;
    // �{�^���R���|�[�l���g��ێ����邽�߂̔z��
    [SerializeField] private Button[] buttons;
    // �{�^���̊Ǘ��ƃ^�X�N�i�s��S��ButtonsManager�̎Q��
    [SerializeField] private ButtonsManager buttonsManager;

    // �{�^���̃f�t�H���g�F
    [SerializeField] private Color defaultColor = Color.white;
    // ���݂̃^�[�Q�b�g�{�^���������F
    [SerializeField] private Color targetColor = Color.green;

    public GameObject panel;

    // ���݂̃^�[�Q�b�g�{�^���̃C���f�b�N�X
    private int currentTargetIndex = -1;

    // �V���O���g���C���X�^���X
    public static OptiTrackHandler Instance { get; private set; }

    // �g���K�[�t���O
    public bool isTriggered { get; private set; } = false;

    private void Awake()
    {
        // �C���X�^���X�����ɑ��݂���ꍇ�͔j�����A���݂��Ȃ��ꍇ�͂��̃C���X�^���X��ݒ�
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����؂�ւ���Ă��j������Ȃ��悤�ɐݒ�
        }

        // �S�{�^���̏������Ɣz��ւ̓o�^
        buttonImages = new Image[9];
        buttons = new Button[9];
        for (int i = 0; i < 9; i++)
        {
            string buttonName = "Button (" + (i + 1) + ")";
            GameObject buttonObj = GameObject.Find(buttonName);
            if (buttonObj)
            {
                buttonImages[i] = buttonObj.GetComponent<Image>();
                buttons[i] = buttonObj.GetComponent<Button>();
                buttonImages[i].color = defaultColor;
            }
            else
            {
                Debug.LogError(buttonName + " not found.");
            }
        }
    }

    // �g���K�[�𔭐������郁�\�b�h
    public void SetTrigger()
    {
        isTriggered = true;
    }

    // �g���K�[�����Z�b�g���郁�\�b�h
    public void ResetTrigger()
    {
        isTriggered = false;
    }

    void Start()
    {
        // �ŏ��̃^�[�Q�b�g�ݒ�
        UpdateTargetButton(-1);

        // ��ʃT�C�Y�ƃ|�C���^�[�̃X�P�[�����O��ݒ�
        sx = pixelX / midAirDisplay.transform.localScale.x;
        sy = pixelY / midAirDisplay.transform.localScale.y;

        // OptiTrack�̕��ʌv�Z�p
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
        if ((isTriggered || Input.GetKeyDown(KeyCode.Return)) && !panel.activeSelf) //�w�����̃g���K�[������
        {
            TriggeredButton(transform.localPosition);
            ResetTrigger();
        }

        //�B���p�̃p�l��������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("task start");
            UpdateTargetButton(-1);
            panel.SetActive(false);
        }

        /* OptiTrack����}�[�J�����擾���A�w�������͂̃|�C���^��\�� */
        if (optitrackClient == null)
        {
            Debug.LogError("OptitrackStreamingClient is not assigned.");
            return;
        }

        marker1State = optitrackClient.GetLatestMarkerStates()[0];
        marker2State = optitrackClient.GetLatestMarkerStates()[1];
        if (marker1State != null && marker2State != null)
        {
            CalculatePointerPosition(marker1State.Position, marker2State.Position);

            if (Input.GetKey(KeyCode.R))
            {
                difX = poiPos2d.x;
                difY = poiPos2d.y;
            }

            filteredPointerPos = FilteringPosition(pointerPosHistory);
            transform.localPosition = new Vector3(filteredPointerPos.x - difX, filteredPointerPos.y - difY, filteredPointerPos.z);

            if (marker1State.Position.z < marker2State.Position.z)
            {
                pushButton(marker2State.Position.z);
            }
            else
            {
                pushButton(marker1State.Position.z);
            }
        }
        else
        {
            Debug.LogWarning("Marker data not available.");
        }//*/

        /* �f�o�b�O�p */
        /*// ���L�[�Ń|�C���^�𓮂���
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

        // �|�C���^�̈ʒu���X�V
        transform.localPosition = new Vector3(poiPos2d.x - difX, poiPos2d.y - difY, poiPos2d.z);//*/
    }

    void pushButton(float zPos)
    {
        Vector2 pushPos = new Vector2(
            pointerPosHistory.ToArray()[pointerPosHistory.Count - 1].x - difX,
            pointerPosHistory.ToArray()[pointerPosHistory.Count - 1].y - difY
            );
        if (previousZpos <= midAirDisplay.transform.position.z && zPos > midAirDisplay.transform.position.z)
        {
            Debug.Log("pushed!");
            TriggeredButton(pushPos);
        }

        previousZpos = zPos;
    }

    void AddToHistory(Vector3 pos, Queue<Vector3> history)
    {
        if (history.Count >= BufferSize)
        {
            history.Dequeue();
        }
        history.Enqueue(pos);
    }

    Vector3 FilteringPosition(Queue<Vector3> history)
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

        AddToHistory(poiPos2d, pointerPosHistory);
    }

    // �{�^���N���b�N�̃`�F�b�N�Ə���
    private void TriggeredButton(Vector2 triggeredPosition)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] != null && IsPointerInCircle(buttonImages[i], triggeredPosition))
            {
                if (i == currentTargetIndex)
                {
                    Debug.Log("Correct button clicked: Button (" + (i + 1) + ")");
                    int previousTargetIndex = buttonsManager.GetNextTargetButton();
                    if (!buttonsManager.SetNextTask()) // �^�[�Q�b�g�E�^�X�N���X�V���A�S�^�[�Q�b�g�E�^�X�N���I�����Ă���Ȃ�I��
                    {
                        Debug.Log("All tasks completed.");
                        Application.Quit();
                    }
                    else
                    {
                        UpdateTargetButton(previousTargetIndex); //�^�[�Q�b�g���X�V
                    }
                }
                else
                {
                    Debug.LogWarning("Incorrect button clicked: Button (" + (i + 1) + ")");
                }
            }
        }
    }

    // ���݂̃^�[�Q�b�g�{�^���̍X�V
    private bool UpdateTargetButton(int prev)
    {
        currentTargetIndex = buttonsManager.GetNextTargetButton();
        if (currentTargetIndex != -1)
        {
            //Debug.Log("Current target is Button (" + (currentTargetIndex + 1) + ")");
            buttonImages[currentTargetIndex].color = targetColor;
            if (prev != -1)
            {
                buttonImages[prev].color = defaultColor;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    // �{�^���ƃ|�C���^�ʒu���~�`�͈͓��ɂ��邩�𔻒�
    private bool IsPointerInCircle(Image buttonImage, Vector2 pointerPosition)
    {
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition;
        Vector2 relativePosition = pointerPosition - buttonCenter;

        return relativePosition.magnitude < (rectTransform.rect.width * 0.42f);
    }
}
