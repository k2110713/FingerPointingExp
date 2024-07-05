using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Begin : MonoBehaviour
{
    //1試行の所要時間と誤操作数
    public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public static int practiceNum = 2;
    public static int testNum = 5;
    public int mode = 0; // 0：タッチ、1：押す動作、2：親指と中指の先端、3：親指と中指の第二関節
    public static int modeStatic = 0;
    public static int cnt = 0;
    public static int currentNum = 1;

    //ボタンを押せた回数
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
