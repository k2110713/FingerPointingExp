using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Begin : MonoBehaviour
{
    //1���s�̏��v���Ԃƌ둀�쐔
    public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public static int practiceNum = 2;
    public static int testNum = 5;
    public int mode = 0; // 0�F�^�b�`�A1�F��������A2�F�e�w�ƒ��w�̐�[�A3�F�e�w�ƒ��w�̑��֐�
    public static int modeStatic = 0;
    public static int cnt = 0;
    public static int currentNum = 1;

    //�{�^������������
    public static int correctCount = 0;

    public static int testNumInOnce = 10;

    // Start is called before the first frame update
    void Start()
    {
        modeStatic = mode;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            SceneManager.LoadScene("TestTrial1");
        }
    }
}
