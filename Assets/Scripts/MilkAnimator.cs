using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image)), RequireComponent(typeof(RectTransform))]
public class MilkAnimator : MonoBehaviour
{
    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        rect.DOShakeScale(1.0f, 0.25f, 1).SetLoops(10);
        rect.DOLocalRotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(10);
    }

    private void OnDestroy()
    {
        rect.DOKill(false);
    }
}
