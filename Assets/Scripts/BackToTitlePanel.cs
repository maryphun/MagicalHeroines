using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BackToTitlePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private HomeSceneUI homesceneUI;

    public void OpenPanel()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, 1.0f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void ClosePanel()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, 1.0f);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void BackToTitle()
    {
        // SEçƒê∂
        AudioManager.Instance.PlaySFX("SystemPrebattle");
        StartCoroutine(homesceneUI.SceneTransition("Title", 1.5f));
    }

}
