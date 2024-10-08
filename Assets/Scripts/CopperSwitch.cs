using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CopperSwitch : MonoBehaviour
{
    //�V���A���ʐM�n���h���[�i�����j
    public SerialHandler serialHandler;

    //�|�C���^�I�u�W�F�N�g(2����)
    public GameObject pointer2d;

    //�{�^��
    public GameObject buttonObject;

    //�N�[���_�E���^�C��
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        //�����_���z�u
        buttonObject.GetComponent<PlaceButton>().PlaceButtonRandomly();

        //�w�������͕��@�i��������j
        if (Begin.modeStatic == 2 || Begin.modeStatic == 3)
        {
            serialHandler.OnDataReceived += OnDataReceived;
            Begin.stopwatch = Stopwatch.StartNew();
        }
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
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning(e.Message);//�G���[��\��
        }
    }

    // �w�肳�ꂽ���W�Ƀ{�^�������݂��Ă��邩�ǂ������m�F����֐�
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

    // Update is called once per frame
    void Update()
    {
        //�P���s�̏I��
        if (Input.GetKeyUp(KeyCode.Return) && Begin.cnt >= Begin.testNumInOnce)
        {
            if (Begin.currentNum == Begin.practiceNum + Begin.testNum)
            {
                //�S���s�̏I��
                Application.Quit();
            }
            else
            {
                //���̎��s�Ɉڂ�
                Begin.currentNum++;
                serialHandler.Close();
                SceneManager.LoadScene("TestTrial1");
            }
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
