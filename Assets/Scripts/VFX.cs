using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class VFX : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] bool fadeOut = false; // ìßñæÇ…Ç»Ç¡ÇƒÇ©ÇÁè¡Ç¶ÇÈ
    [SerializeField] bool fadeIn = false; // ìßñæÇ©ÇÁì¸ÇÈ
    [SerializeField] float delaySeconds = 0.0f;
    [SerializeField] float extraFrameBeforeDestruction = 0.0f;

    private void Start()
    {
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        var animator = GetComponent<Animator>();
        var img = GetComponent<Image>();

        if (ReferenceEquals(animator.runtimeAnimatorController, null))
        {
            Debug.LogError("Animator controller of " + gameObject.name + " doesn't exist!");
            Destroy(gameObject);
            yield break;
        }

        if (delaySeconds > 0.0f)
        {
            img.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(delaySeconds);
            img.color = new Color(1, 1, 1, 1);
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0.0f);
        }
        
        if (fadeIn)
        {
            img.color = new Color(1, 1, 1, 0);
            img.DOFade(1.0f, animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * 0.25f).SetEase(Ease.Linear);
        }
        
        if (fadeOut)
        {
            img.DOFade(0.0f, animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * 0.5f)
                .SetDelay(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length)
                .SetEase(Ease.Linear);

            Object.Destroy(gameObject, (animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * 1.5f) + extraFrameBeforeDestruction);
        }
        else
        {
            Object.Destroy(gameObject, animator.GetCurrentAnimatorClipInfo(0)[0].clip.length + extraFrameBeforeDestruction);
        }
    }
}
