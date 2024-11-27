using UnityEngine;
using UnityEngine.UI;

public class PointingHandler : MonoBehaviour
{
    // �{�^���̃r�W���A���\����ێ����邽�߂̔z��
    [SerializeField] private Image[] buttonImages;
    // �{�^���R���|�[�l���g��ێ����邽�߂̔z��
    [SerializeField] private Button[] buttons;
    // �{�^���̊Ǘ��ƃ^�X�N�i�s��S��ButtonsManager�̎Q��
    [SerializeField] private ButtonsManager buttonsManager;

    // �{�^���̃f�t�H���g�F
    [SerializeField] private Color defaultColor = Color.white;
    // ���݂̃^�[�Q�b�g�{�^���������F
    [SerializeField] private Color targetColor = Color.red;
    // �{�^�����N���b�N���ꂽ�ۂ̐F
    [SerializeField] private Color clickColor = Color.green;

    public GameObject panel;

    // ���݂̃^�[�Q�b�g�{�^���̃C���f�b�N�X
    private int currentTargetIndex = -1;

    // �V���O���g���C���X�^���X
    public static PointingHandler Instance { get; private set; }

    // �g���K�[�t���O
    public int isTriggered { get; private set; } = 0;

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
    }

    // �g���K�[�𔭐������郁�\�b�h
    public void TouchingTrigger()
    {
        isTriggered = 1;
    }

    // �g���K�[�𔭐������郁�\�b�h
    public void PointingTrigger()
    {
        isTriggered = 2;
    }

    // �g���K�[�����Z�b�g���郁�\�b�h
    public void ResetTrigger()
    {
        isTriggered = 0;
    }

    void Start()
    {
        // �{�^���̏������ƍŏ��̃^�[�Q�b�g�ݒ�
        InitializeButtons();
        UpdateTargetButton(-1);
    }

    void Update()
    {
        if ((isTriggered == 1 || Input.GetKeyDown(KeyCode.Return)) && !panel.activeSelf) //�^�b�v���o�i�f�o�b�O���j
        {
            CheckButtonTouching();
            ResetTrigger();
        }
        if ((isTriggered == 2 || Input.GetKeyDown(KeyCode.Return)) && !panel.activeSelf) //�^�b�v���o�i�f�o�b�O���j
        {
            //CheckButtonPointing();
            ResetTrigger();
        }

        //�B���p�̃p�l��������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("task start");
            panel.SetActive(false);
        }
    }

    // �S�{�^���̏������Ɣz��ւ̓o�^
    private void InitializeButtons()
    {
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

    // �{�^���N���b�N�̃`�F�b�N�Ə���
    private void CheckButtonTouching()
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] != null && IsPointerInCircle(buttonImages[i]))
            {
                if (i == currentTargetIndex)
                {
                    Debug.Log("Correct button clicked: Button (" + (i + 1) + ")");
                    int previousTargetIndex = buttonsManager.GetNextTargetButton();
                    if (!buttonsManager.SetNextTask()) // �^�[�Q�b�g�E�^�X�N���X�V���A�S�^�[�Q�b�g�E�^�X�N���I�����Ă���Ȃ�I��
                    {
                        Debug.Log("All tasks completed. Restarting...");
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
            Debug.Log("Current target is Button (" + (currentTargetIndex + 1) + ")");
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
    private bool IsPointerInCircle(Image buttonImage)
    {
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition;
        Vector2 localPointerPosition = transform.localPosition;
        Vector2 relativePosition = localPointerPosition - buttonCenter;

        return relativePosition.magnitude < (rectTransform.rect.width * 0.42f);
    }
}
