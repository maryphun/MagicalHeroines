using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class TurnBaseInformation : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationSpeed = 0.25f;
    [SerializeField, Range(0.0f, 1.0f)] private float colorBrightness = 0.25f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image informationHover;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text characterSpeed;

    [Header("Debug")]
    [SerializeField] private bool isDisplay = false;
    [SerializeField] private Battler battler;

    // Start is called before the first frame update
    void Awake()
    {
        canvasGroup.alpha = 0.0f;
        isDisplay = false;
    }

    public void Initialize(Color characterColor, string name, Battler target)
    {
        battler = target;
        characterName.text = name;
        characterName.color = new Color(characterColor.r * colorBrightness, characterColor.g * colorBrightness, characterColor.b * colorBrightness, 1.0f);
        characterSpeed.text = LocalizationManager.Localize("System.Speed") + target.speed.ToString();

        // update UI length
        characterName.ForceMeshUpdate();
        characterSpeed.ForceMeshUpdate();
        Vector2 textLengthName = characterName.GetRenderedValues();
        Vector2 textLengthSpeed = characterSpeed.GetRenderedValues();
        float textLength = Mathf.Max(textLengthName.x, textLengthSpeed.x) + (characterName.fontSize * 2);
        informationHover.rectTransform.sizeDelta = new Vector2(textLength, informationHover.rectTransform.sizeDelta.y);
    }

    public void OnHover()
    {
        if (isDisplay) return;
        if (characterSpeed == null) return;

        isDisplay = true;
        canvasGroup.DOComplete();
        canvasGroup.DOFade(1.0f, animationSpeed);

        // update text
        characterSpeed.text = LocalizationManager.Localize("System.Speed") + battler.speed.ToString();
    }

    public void UnHover()
    {
        if (!isDisplay) return;

        isDisplay = false;
        //Å@instant
        canvasGroup.DOComplete();
        canvasGroup.alpha = 0.0f;
    }
}
