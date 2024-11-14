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

    //�N�[���_�E���^�C��
    private bool isCooldown = false;
    private float cooldownTime = 1.0f;

    // �V���O���g���C���X�^���X
    public static CopperSwitch Instance { get; private set; }

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
    }

    // �g���K�[�𔭐������郁�\�b�h
    public void ActivateTrigger()
    {
        isTriggered = true;
    }

    // �g���K�[�����Z�b�g���郁�\�b�h
    public void ResetTrigger()
    {
        isTriggered = false;
    }

    // Start is called before the first frame update
    void Start()
    {
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
                isTriggered = true;
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
