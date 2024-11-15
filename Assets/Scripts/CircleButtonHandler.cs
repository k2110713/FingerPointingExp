using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CircleButtonHandler : MonoBehaviour
{
    private Image[] buttonImages;
    private Button[] buttons;

    // �F�̐ݒ�
    [SerializeField] private Color defaultColor = Color.white;   // �f�t�H���g�̐F
    [SerializeField] private Color enterColor = Color.green;     // �}�E�X��������Ƃ��̐F
    [SerializeField] private Color clickColor = Color.red;       // �N���b�N���̐F

    bool isInCircle = false;
    private bool[] wasInCircle; // �e�{�^���̏�Ԃ��Ǘ�����t���O

    private void Start()
    {
        // 8�̃{�^����z��Ɋi�[
        buttonImages = new Image[8];
        buttons = new Button[8];
        wasInCircle = new bool[8];

        for (int i = 0; i < 8; i++)
        {
            GameObject buttonObj = GameObject.Find("Button (" + (i + 1) + ")");
            if (buttonObj != null)
            {
                buttonImages[i] = buttonObj.GetComponent<Image>();
                buttons[i] = buttonObj.GetComponent<Button>();
                buttonImages[i].color = defaultColor; // �����F���f�t�H���g�F�ɐݒ�
            }
            else
            {
                Debug.LogWarning("Button (" + (i + 1) + ") not found.");
            }
        }
    }

    private void Update()
    {
        if (CopperSwitch.Instance.isTriggered)//�^�b�v���m
        {
            // �e�{�^���̉~�`�͈͂Ƀ|�C���^�������Ă��邩�`�F�b�N
            for (int i = 0; i < buttonImages.Length; i++)
            {
                if (buttonImages[i] != null)
                {
                    isInCircle = IsPointerInCircle(buttonImages[i]);

                    /*if (!wasInCircle[i] && isInCircle)
                    {
                        // enter���
                        Debug.Log("Enter Button (" + (i + 1) + ")");
                        buttonImages[i].color = enterColor;
                    }
                    else if (wasInCircle[i] && !isInCircle)
                    {
                        // exit���
                        Debug.Log("Exit Button (" + (i + 1) + ")");
                        buttonImages[i].color = defaultColor;
                    }*/

                    if (isInCircle)//�|�C���^���{�^���ォ�ǂ���
                    {
                        //Debug.Log("�g���K�[���������܂����I");
                        Debug.Log("Click Button (" + (i + 1) + ")");
                        buttonImages[i].color = clickColor;
                        buttons[i].onClick.Invoke();
                    }
                    // ��Ԃ��X�V
                    //wasInCircle[i] = isInCircle;
                }
            }
            // �������I�������g���K�[�����Z�b�g
            CopperSwitch.Instance.ResetTrigger();
        }
    }

    private bool IsPointerInCircle(Image buttonImage)
    {
        // �{�^���ƃ|�C���^�̃��[�J���ʒu���v�Z
        RectTransform rectTransform = buttonImage.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition; // �{�^���̒��S�ʒu
        Vector2 localPointerPosition = transform.localPosition; // �|�C���^�i���g�j�̈ʒu

        // �|�C���^����{�^�����S�ւ̑��Έʒu
        Vector2 relativePosition = localPointerPosition - buttonCenter;
        float radius = rectTransform.rect.width * 0.42f;

        // �~�͈͓̔��ɂ��邩�𔻒�
        return relativePosition.magnitude < radius;
    }
}
