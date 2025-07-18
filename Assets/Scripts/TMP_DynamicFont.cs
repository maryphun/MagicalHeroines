using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(TMP_Text))]
public class TMP_DynamicFont : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private bool isDialogue;

    public void Start()
    {
        UpdateFont();
        LocalizationManager.LocalizationChanged += UpdateFont;
    }

    public void OnDestroy()
    {
        LocalizationManager.LocalizationChanged -= UpdateFont;
    }

    private void UpdateFont()
    {
        if (GetComponent<TMP_Text>().font != DynamicFont.Instance.GetFont())
        {
            GetComponent<TMP_Text>().font = DynamicFont.Instance.GetFont();
        }
    }

    public bool IsDialogue()
    {
        return isDialogue;
    }
}
