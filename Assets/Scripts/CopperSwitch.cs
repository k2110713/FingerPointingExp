using System.Collections.Generic;
using System.Diagnostics;
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

    //�{�^��
    public GameObject buttonObject;

    public TextMeshProUGUI PushedOrNot;

    // Start is called before the first frame update
    void Start()
    {
        //�����_���z�u
        buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
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
            UnityEngine.Debug.Log(data[0]);//Unity�̃R���\�[���Ɏ�M�f�[�^��\��
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
                Begin.cnt++;
                UnityEngine.Debug.Log(Begin.correctCount.ToString() + Begin.cnt.ToString());
                //if (Begin.cnt < Begin.testNumInOnce)
                if (true)
                {
                    buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();
                }
                else
                {
                    Begin.stopwatch.Stop();
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning(e.Message);//�G���[��\��
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
