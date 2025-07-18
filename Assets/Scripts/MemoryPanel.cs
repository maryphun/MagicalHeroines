using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MemoryPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;
    [SerializeField] private Sprite lockedMemorySprite;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private Transform memoryHandle;
    [SerializeField] private CanvasGroup demoOnlyPanel;
    [SerializeField] private Image verticalScrollbar;
    [SerializeField] private Image verticalScrollbarHandle;

    [Header("Debug")]
    [SerializeField] private bool isOpen;

    public bool IsOpen { get { return isOpen; } }

    public void OpenMemoryPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        // UI �t�F�C�h
        canvasGrp.DOFade(1.0f, animationTime);
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        // ������
        SetupMemories();

#if DEMO
        demoOnlyPanel.alpha = 1.0f;
        demoOnlyPanel.interactable = true;
        demoOnlyPanel.blocksRaycasts = true;

        verticalScrollbar.color = CustomColor.invisible();
        verticalScrollbarHandle.color = CustomColor.invisible();
#endif

        isOpen = true;
    }

    public void CloseMemoryPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGrp.DOFade(0.0f, animationTime);
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        isOpen = false;
    }

    /// <summary>
    /// ������
    /// </summary>
    private void SetupMemories()
    {
        for (int i = 0; i < memoryHandle.childCount; i++)
        {
            if (memoryHandle.GetChild(i).gameObject.activeSelf)
            {
                memoryHandle.GetChild(i).GetComponent<MemorySlot>().Initialization(lockedMemorySprite);
            }
        }
    }
}
