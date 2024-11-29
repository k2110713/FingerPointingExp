using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TouchingHandler : MonoBehaviour
{
    public GameObject midAirDisplay;
    public OptitrackStreamingClient optitrackClient;

    public float pixelX = 1920;
    public float pixelY = 1080;

    OptitrackMarkerState marker1State;
    OptitrackMarkerState marker2State;

    private float sx;
    private float sy;
    private Vector2 poiPos2d = Vector2.zero;

    public float difX = 0, difY = 0;

    private float previousZpos = 0.0f;

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

    private void Awake()
    {
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

    void Start()
    {
        this.GetComponent<CanvasRenderer>().cull = true;

        // �ŏ��̃^�[�Q�b�g�ݒ�
        UpdateTargetButton(-1);

        // ��ʃT�C�Y�ƃ|�C���^�[�̃X�P�[�����O��ݒ�
        sx = pixelX / midAirDisplay.transform.localScale.x;
        sy = pixelY / midAirDisplay.transform.localScale.y;
    }

    void Update()
    {
        //�B���p�̃p�l��������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("task start");
            UpdateTargetButton(-1);
            panel.SetActive(false);
        }

        if (optitrackClient == null)
        {
            Debug.LogError("OptitrackStreamingClient is not assigned.");
            return;
        }

        marker1State = optitrackClient.GetLatestMarkerStates()[0];
        marker2State = optitrackClient.GetLatestMarkerStates()[1];
        if (marker1State != null && marker2State != null)
        {
            if (marker1State.Position.z < marker2State.Position.z)
            {
                pushButton(marker2State.Position);
            }
            else
            {
                pushButton(marker1State.Position);
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

    void pushButton(Vector3 fingerPos)
    {
        poiPos2d.x = fingerPos.x * sx - difX;
        poiPos2d.y = (fingerPos.y - midAirDisplay.transform.position.y) * sy - difY;
        if (previousZpos <= midAirDisplay.transform.position.z && fingerPos.z > midAirDisplay.transform.position.z)
        {
            Debug.Log("pushed! " + poiPos2d);
            TriggeredButton(poiPos2d);
        }

        previousZpos = fingerPos.z;
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
