using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �{�^���̎q�ł���e�L�X�g���{�^���ƂƂ��ɐF�ω�������
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ActionPanelText : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] Color enabledColor = Color.white;
    [SerializeField] Color disabledColor = Color.white;

    [Header("References")]
    [SerializeField] Button button;

    [Header("Debug")]
    [SerializeField] bool isEnabled = true;

    private void Awake()
    {
        if (button == null)
        {
            if (!transform.parent.gameObject.TryGetComponent<Button>(out button))
            {
                // �{�^�������݂��Ă��Ȃ�
                enabled = false;
                Debug.LogWarning("�{�^�������݂��Ă��Ȃ�");
            }
        }

        isEnabled = true;
        UpdateColor();
    }

    private void Update()
    {
        if (isEnabled != button.IsInteractable())
        {
            isEnabled = button.IsInteractable();
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        GetComponent<TMP_Text>().color = isEnabled ? enabledColor : disabledColor;
    }
}
