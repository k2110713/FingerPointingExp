using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public class SimulateClick : MonoBehaviour
{
    public Vector2 clickPosition; // �N���b�N������W���w�肷��

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) // Space�L�[�������ƃN���b�N���V�~�����[�g
        {
            SimulateMouseClick(clickPosition);
        }
    }

    void SimulateMouseClick(Vector2 position)
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            // �}�E�X���W��ݒ�
            InputState.Change(mouse.position, position);
            InputSystem.Update();

            // ���N���b�N�̃V�~�����[�V���� (����)
            InputSystem.QueueStateEvent(mouse, new MouseState
            {
                position = position,
                buttons = 1 << (int)MouseButton.Left
            });
            InputSystem.Update();

            // �t���[����҂�
            InputSystem.Update();

            // ���N���b�N�̃V�~�����[�V���� (����)
            InputSystem.QueueStateEvent(mouse, new MouseState
            {
                position = position,
                buttons = 0
            });
            InputSystem.Update();
        }
    }
}
