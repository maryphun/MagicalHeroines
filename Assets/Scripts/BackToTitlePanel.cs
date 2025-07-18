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
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, 1.0f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void ClosePanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, 1.0f);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void BackToTitle()
    {
        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemPrebattle");
        StartCoroutine(homesceneUI.SceneTransition("Title", 1.5f));
    }

}
