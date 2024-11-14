using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class CircleButton : MonoBehaviour, IPointerClickHandler
{
    private Image image;
    private bool wasInCircle = false;
    public GameObject pointer;

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
        // �{�^���ƃ|�C���^�̃��[�J���ʒu���v�Z
        RectTransform rectTransform = image.rectTransform;
        Vector2 buttonCenter = rectTransform.localPosition; // �{�^���̒��S�ʒu
        Vector2 localPointerPosition = pointer.transform.localPosition; // �|�C���^�̈ʒu

        // �|�C���^����{�^�����S�ւ̑��Έʒu
        Vector2 relativePosition = localPointerPosition - buttonCenter;
        float radius = rectTransform.rect.width * 0.42f;

        Debug.Log("Pointer: " + localPointerPosition + ", RelativePosition: " + relativePosition.magnitude + ", Radius: " + radius);

        // �~�͈͓̔��ɂ��邩�𔻒�
        return relativePosition.magnitude < radius;
    }
}
