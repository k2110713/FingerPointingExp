using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public class SimulateClick : MonoBehaviour
{
    public Vector2 clickPosition; // クリックする座標を指定する

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) // Spaceキーを押すとクリックをシミュレート
        {
            SimulateMouseClick(clickPosition);
        }
    }

    void SimulateMouseClick(Vector2 position)
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            // マウス座標を設定
            InputState.Change(mouse.position, position);
            InputSystem.Update();

            // 左クリックのシミュレーション (押す)
            InputSystem.QueueStateEvent(mouse, new MouseState
            {
                position = position,
                buttons = 1 << (int)MouseButton.Left
            });
            InputSystem.Update();

            // フレームを待つ
            InputSystem.Update();

            // 左クリックのシミュレーション (離す)
            InputSystem.QueueStateEvent(mouse, new MouseState
            {
                position = position,
                buttons = 0
            });
            InputSystem.Update();
        }
    }
}
