using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class ButtonPAnel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float originalPosition; 
    [SerializeField] private float displayDestination; 
    [SerializeField,Range(0.0f, 1.0f)] private float animationTime = 0.25f;

    [Header("References")]
    [SerializeField] private Image arrowIcon; 
    [SerializeField] private HomeCharacter homeCharacterScript; 

    private RectTransform rect;
    private bool isDisplaying;
    private bool isEnabled;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        isDisplaying = false;
        isEnabled = true;

#if DEMO
        if (ProgressManager.Instance.GetCurrentStageProgress() == DemoParameter.EndChapter)
        {
            this.SetEnabled(false);
        }
#endif
    }

    public void StartDisplay()
    {
        if (isDisplaying) return;

        arrowIcon.DOFade(0.0f, animationTime);
        rect.DOLocalMoveX(displayDestination, animationTime);
        isDisplaying = true;
    }

    public void EndDisplay()
    {
        if (!isDisplaying) return;

        arrowIcon.DOFade(1.0f, animationTime);
        rect.DOLocalMoveX(originalPosition, animationTime);
        isDisplaying = false;
    }

    private void Update()
    {
        if (!isEnabled) return;

        // カーソル位置を取得
        Vector3 mousePosition = Input.mousePosition; // no need to calculate scale factor as Screen.width does change the resolution.
        if (mousePosition.x > Screen.width * 0.75f)
        {
            StartDisplay();
        }
        else
        {
            EndDisplay();
        }
    }

    public void SetEnabled(bool enable)
    {
        isEnabled = enable;

        if (!isEnabled) 
        {
            EndDisplay();
        }
        else
        {
            // 台詞
            homeCharacterScript.TriggerDialogue(); // なんかしゃべらせる
        }
    }
}
