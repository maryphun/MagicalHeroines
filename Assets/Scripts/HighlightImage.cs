using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class HighlightImage : MonoBehaviour
{
    private Image img;
    private Color color;

    private void Start()
    {
        img = GetComponent<Image>();
        color = img.color;
        Loop();
    }

    private void Loop()
    {
        const float animTime = 1.2f;
        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            img.DOComplete();
            img.DOColor(color * 0.75f, animTime);
        })
        .AppendInterval(animTime)
        .AppendCallback(() =>
        {
            img.DOComplete();
            img.DOColor(color, animTime);
        })
        .AppendInterval(animTime)
        .AppendCallback(() =>
        {
            if (enabled)
            {
                Loop();
            }
        });
    }

    private void OnEnable()
    {
        img.DOPlay();
    }

    private void OnDisable()
    {
        img.DOPause();
    }
}
