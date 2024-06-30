using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Begin : MonoBehaviour
{
    //1試行の所要時間と誤操作数
    public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public int practiceNum = 1;
    public int testNum = 5;
    public static int[] modes = { 0, 1, 2, 3 };
    public static int cnt = 0;

    //ボタンを押せた回数
    public static int correctCount = 0;

    public static int testNumInOnce = 10;

    // Start is called before the first frame update
    void Start()
    {
        //modesをシャッフル
        System.Random random = new System.Random();
        for (int i = modes.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            int temp = modes[i];
            modes[i] = modes[j];
            modes[j] = temp;
        }
        Debug.Log(modes[0].ToString() + modes[1].ToString() + modes[2].ToString() + modes[3].ToString());
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
