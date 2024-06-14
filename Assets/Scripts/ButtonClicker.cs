using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClicker : MonoBehaviour
{
    public HandJointPositions handJointPositions;

    // Update is called once per frame
    void Update()
    {
        // マウスの左クリックが検出された場合
        if (handJointPositions.fingersDistance < 2.0)
        {
            // マウスのスクリーン座標を取得
            Vector2 mousePosition = Input.mousePosition;

            // レイキャスト用のデータを作成
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(handJointPositions.poiPos2dMean20.x, handJointPositions.poiPos2dMean20.y)
            };

            // レイキャスト結果を格納するリストを作成
            List<RaycastResult> results = new List<RaycastResult>();

            // レイキャストを実行
            EventSystem.current.RaycastAll(pointerData, results);

            // レイキャスト結果を確認
            foreach (RaycastResult result in results)
            {
                // 結果がButtonコンポーネントを持つ場合
                Button button = result.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    // ボタンをクリック
                    button.onClick.Invoke();
                    break;
                }
            }
        }
    }
}
