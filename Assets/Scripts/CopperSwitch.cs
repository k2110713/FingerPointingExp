using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CopperSwitch : MonoBehaviour
{
    //�V���A���ʐM�n���h���[�i�����j
    public SerialHandler serialHandler;

    //�|�C���^�I�u�W�F�N�g(2����)
    public GameObject pointer2d;

    public TextMeshProUGUI PushedOrNot;

    //�{�^������������
    public static int correctCount = 0;

    public RectTransform canvasRectTransform; // Canvas �� RectTransform
    public RectTransform buttonRectTransform; // Button �� RectTransform

    // Start is called before the first frame update
    void Start()
    {
        //�����_���z�u
        PlaceButtonRandomly();
        //�M������M�����Ƃ��ɁA���̃��b�Z�[�W�̏������s��
        serialHandler.OnDataReceived += OnDataReceived;
        PushedOrNot.text = "Not Pushed";
        Begin.stopwatch.Start();
    }

    //��M�����M��(message)�ɑ΂��鏈��
    void OnDataReceived(string message)
    {
        var data = message.Split(
                new string[] { "\n" }, System.StringSplitOptions.None);
        try
        {
            Debug.Log(data[0]);//Unity�̃R���\�[���Ɏ�M�f�[�^��\��
            if (data[0] == "1")
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
                    Debug.Log(result.ToString());
                    // ���ʂ�Button�R���|�[�l���g�����ꍇ
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        PushedOrNot.text = "Pushed";
                        Debug.Log("Pushed");
                        correctCount++;
                        break;
                    }
                }
                Debug.Log(correctCount);
                PlaceButtonRandomly();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);//�G���[��\��
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlaceButtonRandomly()
    {
        // Canvas �̃T�C�Y���擾

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        // Button �̃T�C�Y���擾
        float buttonWidth = buttonRectTransform.rect.width;
        float buttonHeight = buttonRectTransform.rect.height;

        // �����_���Ȉʒu���v�Z (�{�^������ʊO�ɏo�Ȃ��悤��)
        float randomX = Random.Range(buttonWidth / 2, canvasWidth - buttonWidth / 2);
        float randomY = Random.Range(buttonHeight / 2, canvasHeight - buttonHeight / 2);

        // �{�^���̈ʒu��ݒ�
        buttonRectTransform.position = new Vector3(randomX, randomY, 1);
    }
}
