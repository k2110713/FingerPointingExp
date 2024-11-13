using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class CircleButton : MonoBehaviour, IPointerClickHandler
{
    private Image image;
    private bool wasInCircle = false;

    // �F�̐ݒ�
    [SerializeField] private Color defaultColor = Color.white;      // �f�t�H���g�̐F
    [SerializeField] private Color enterColor = Color.green;         // �}�E�X��������Ƃ��̐F
    [SerializeField] private Color clickColor = Color.red;           // �N���b�N���̐F

    private void Awake()
    {
        image = GetComponent<Image>();
        image.color = defaultColor; // �����F���f�t�H���g�F�ɐݒ�
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsPointerInCircle())
        {
            GetComponent<Button>().onClick.Invoke();
            Debug.Log("click");

            // �N���b�N���̐F�ɕύX
            image.color = clickColor;
        }
    }

    private void Update()
    {
        // ���݂̃}�E�X�ʒu���擾���A�~�`�͈͓̔��ɂ��邩�ǂ������m�F
        bool isInCircle = IsPointerInCircle();

        if (!wasInCircle && isInCircle)
        {
            // �O�̃t���[���ł͔͈͊O���������A���݂̃t���[���ł͔͈͓��̏ꍇ
            Debug.Log("enter");

            // �}�E�X���{�^���̏�ɗ����Ƃ��̐F�ɕύX
            image.color = enterColor;
        }
        else if (wasInCircle && !isInCircle)
        {
            // �O�̃t���[���ł͔͈͓����������A���݂̃t���[���ł͔͈͊O�̏ꍇ
            Debug.Log("exit");

            // �}�E�X���{�^�����痣�ꂽ��f�t�H���g�F�ɖ߂�
            image.color = defaultColor;
        }

        // ��Ԃ��X�V
        wasInCircle = isInCircle;
    }

    private bool IsPointerInCircle()
    {
        // �}�E�X�ʒu���X�N���[�����W���烏�[���h���W�ɕϊ����A�{�^���̃��[�J����Ԃł̈ʒu���擾
        RectTransform rectTransform = image.rectTransform;
        Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        float radius = rectTransform.rect.width * 0.42f;

        // �~�͈͓̔��ɂ��邩�𔻒�
        return localMousePosition.magnitude < radius;
    }
}
