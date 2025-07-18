using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ボタンの子であるテキストをボタンとともに色変化させる
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
                // ボタンが存在していない
                enabled = false;
                Debug.LogWarning("ボタンが存在していない");
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
