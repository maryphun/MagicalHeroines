using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class SkipDialogueUI : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float interval = 0.05f;
    [SerializeField] private Sprite toggleONButton;
    [SerializeField] private Sprite toggleOFFButton;

    [Header("References")]
    [SerializeField] private NovelEditor.NovelPlayer novelPlayer;
    [SerializeField] private Image buttonIcon;

    [Header("Debug")]
    [SerializeField] private bool isSkipping = false;
    [SerializeField] private bool isFlashing = false;
    [SerializeField] private float intervalcnt;
    [SerializeField] private bool isCooldown; // ˆê’èŽžŠÔ‚¢‚È‚¢ƒ{ƒ^ƒ“‚ð–³Œø‚Æ‚·‚é

    private void Start()
    {
        isSkipping = false;
        enabled = false;

        buttonIcon.sprite = toggleOFFButton;
    }

    public void StartSkipping()
    {
        if (isCooldown) return;

        intervalcnt = interval;
        isSkipping = true;
        isFlashing = false;
        enabled = true;

        buttonIcon.sprite = toggleONButton;
    }

    public void StopSkipping()
    {
        intervalcnt = interval;
        isSkipping = false;
        isFlashing = false;
        enabled = false;
        isCooldown = true;
        DOTween.Sequence().AppendInterval(0.2f).AppendCallback(() => { isCooldown = false; });

        buttonIcon.sprite = toggleOFFButton;
    }

    public void ForceStopSkipping()
    {
        if (isSkipping)
        {
            StopSkipping();
        }
    }

    private void Update()
    {
        if (isSkipping)
        {
            if (Input.anyKeyDown)
            {
                if (!novelPlayer.IsChoicing)
                {
                    StopSkipping();
                    return;
                }
            }

            if (novelPlayer.IsImageChanging)
            {
                return;
            }

            if (novelPlayer.IsChoicing)
            {
                return;
            }

            intervalcnt -= Time.deltaTime;
            if (intervalcnt <= 0.0f)
            {
                intervalcnt = interval;

                if (isFlashing)
                {
                    isFlashing = false;
                    novelPlayer.FlashText();
                }
                else
                {
                    isFlashing = true;
                    novelPlayer.GoNext();
                }
            }
        }
        else
        {
            enabled = false;
        }
    }
}
