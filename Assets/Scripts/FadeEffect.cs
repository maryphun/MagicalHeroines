using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 残像を生成するコンポネント
/// </summary>
public class FadeEffect : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool isInitialized = false;
    [SerializeField] private float time = 0.0f;
    [SerializeField] private float interval = 0.0f;

    // hidden internal variable
    private float timeCnt = 0.0f;
    private float intervalCnt = 0.0f;
    private Image targetImgComponent;
    private Sprite targetSprite;

    // constant
    const float fadeTime = 1f;

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized)
        {
            enabled = false;
        }

        intervalCnt += Time.deltaTime;
        timeCnt += Time.deltaTime;
        if (intervalCnt >= interval)
        {
            intervalCnt = 0.0f;

            // 残像を作成
            Image img = new GameObject("FadingImage["+gameObject.name+"]").AddComponent<Image>();
            img.transform.SetParent(transform.parent);
            img.transform.SetSiblingIndex(transform.GetSiblingIndex());
            img.sprite = targetSprite;
            img.raycastTarget = false;
            img.rectTransform.pivot = targetImgComponent.rectTransform.pivot;
            img.rectTransform.position = targetImgComponent.rectTransform.position;
            img.rectTransform.localScale = targetImgComponent.rectTransform.localScale;
            img.rectTransform.sizeDelta = targetImgComponent.rectTransform.sizeDelta;
            img.rectTransform.rotation = targetImgComponent.rectTransform.rotation;

            img.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            img.DOFade(0.0f, fadeTime);
            Destroy(img.gameObject, fadeTime + Time.deltaTime);
        }
        if (timeCnt >= time)
        {
            enabled = false;
            Destroy(this);
        }
    }

    /// <summary>
    ///  初期化されるまでスクリプトは動かない
    /// </summary>
    public void Initialize(float time, float interval, Image target)
    {
        isInitialized = true;
        this.time = time;
        this.interval = interval;
        this.targetImgComponent = target;
        this.targetSprite = targetImgComponent.sprite;
    }
}
