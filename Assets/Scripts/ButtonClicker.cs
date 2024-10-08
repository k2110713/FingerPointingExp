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
        // �}�E�X�̍��N���b�N�����o���ꂽ�ꍇ
        if (handJointPositions.fingersDistance < 2.0)
        {
            // �}�E�X�̃X�N���[�����W���擾
            Vector2 mousePosition = Input.mousePosition;

            // ���C�L���X�g�p�̃f�[�^���쐬
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(handJointPositions.poiPos2dMean20.x, handJointPositions.poiPos2dMean20.y)
            };

            // ���C�L���X�g���ʂ��i�[���郊�X�g���쐬
            List<RaycastResult> results = new List<RaycastResult>();

            // ���C�L���X�g�����s
            EventSystem.current.RaycastAll(pointerData, results);

            // ���C�L���X�g���ʂ��m�F
            foreach (RaycastResult result in results)
            {
                // ���ʂ�Button�R���|�[�l���g�����ꍇ
                Button button = result.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    // �{�^�����N���b�N
                    button.onClick.Invoke();
                    break;
                }
            }
        }
    }
}
