using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HomeMenuUI : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] Vector2 offset;
    [SerializeField] float animationTime = 1.0f;
    [SerializeField] float startDelay = 0.0f;
    private void Awake()
    {
        var rect = GetComponent<RectTransform>();
        var img = GetComponent<Image>();
        var canvasGrp = GetComponent<CanvasGroup>();
        var originPos = rect.anchoredPosition;

        rect.anchoredPosition = originPos + offset;
        if (canvasGrp != null)
        {
            var originalAlpha = canvasGrp.alpha;
            var isInteractable = canvasGrp.interactable;

            canvasGrp.interactable = false;
            canvasGrp.alpha = 0.0f;
            canvasGrp.DOFade(originalAlpha, animationTime).SetDelay(startDelay).SetEase(Ease.Linear).OnComplete(() => { canvasGrp.interactable = isInteractable; });
        }

        if (img != null)
        {
            var originalAlpha = img.color.a;

            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.0f);
            img.DOFade(originalAlpha, animationTime).SetDelay(startDelay).SetEase(Ease.Linear);
        }

        rect.DOAnchorPos(originPos, animationTime).SetDelay(startDelay).SetEase(Ease.OutBack);
    }
}
