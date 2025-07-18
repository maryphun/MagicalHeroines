using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class SkillRequirement : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float animTime = 0.15f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform rect;

    [Header("Debug")]
    [SerializeField] private int requiredLevel;
    [SerializeField] private int requiredHornyness;
    [SerializeField] private bool isHovering;
    [SerializeField] private Vector3 rectPosition;
    [SerializeField] private Vector3 sizeDelta;

    public void Initialization(int requiredLevel, int requireHornyness)
    {
        this.requiredLevel = requiredLevel;
        this.requiredHornyness = requireHornyness;
        isHovering = false;
        sizeDelta = rect.sizeDelta * mainCanvas.scaleFactor;
        rectPosition = rect.position + new Vector3(sizeDelta.x * 0.5f, -sizeDelta.y * 0.5f, 0.0f);
    }

    private void Update()
    {
        Vector3 mousePosition = Input.mousePosition / mainCanvas.scaleFactor;
        bool isHover = (   mousePosition.x > (rectPosition.x / mainCanvas.scaleFactor - sizeDelta.x * 0.5f)
                        && mousePosition.x < (rectPosition.x / mainCanvas.scaleFactor + sizeDelta.x * 0.5f)
                        && mousePosition.y > (rectPosition.y / mainCanvas.scaleFactor - sizeDelta.y * 0.5f)
                        && mousePosition.y < (rectPosition.y / mainCanvas.scaleFactor + sizeDelta.y * 0.5f));
        
        if (isHovering)
        {
            if (!isHover)
            {
                OnUnhover();
            }
        }
        else
        {
            if (isHover)
            {
                OnHover();
            }
        }
    }

    private void OnHover()
    {
        isHovering = true;
        canvasGroup.DOFade(1, animTime);

        string requirement = requiredLevel > requiredHornyness ? "Lv" + requiredLevel : LocalizationManager.Localize("System.HornyTrained");
        text.text = LocalizationManager.Localize("SysteÇç.Requirement") + "\n" + requirement;
        var canvasRect = canvasGroup.GetComponent<RectTransform>();
        canvasRect.position = new Vector3(rectPosition.x, canvasRect.position.y, canvasRect.position.z);
    }

    public void OnUnhover()
    {
        isHovering = false;
        canvasGroup.DOFade(0, animTime);
    }
}
