using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HideDialogueUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NovelEditor.NovelPlayer novelPlayer;

    [Header("Debug")]
    [SerializeField] private bool isHiding = false;

    private void Start()
    {
        isHiding = false;
        enabled = false;
    }

    public void Hide()
    {
        isHiding = true;
        enabled = true;
    }

    private void Update()
    {
        if (!isHiding)
        {
            enabled = false;
            return;

        }

        if (Input.anyKeyDown)
        {
            enabled = false;
            isHiding = false;

            // 会話がスキップされないように待っておく
            DOTween.Sequence().AppendInterval(Time.deltaTime).AppendCallback(() => { novelPlayer.DisplayUI(); });
        }
    }
}
