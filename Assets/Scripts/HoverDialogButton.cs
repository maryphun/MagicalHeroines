using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class HoverDialogButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text targetDescription;

    private bool isHovering = false;
    private bool isInitialized = false;

    private const float anchoredPosY = -20.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (ReferenceEquals(targetDescription, null))
        {
            Debug.Log("<color=red>References NULL for " + gameObject.name + "!");
            return;
        }

        targetDescription.color = new Color(0, 0, 0, 0);
        isInitialized = true;
        isHovering = false;
    }

    public void OnHover()
    {
        if (!isInitialized) return;
        if (isHovering) return;

        targetDescription.DOFade(1.0f, 0.5f);
        targetDescription.rectTransform.anchoredPosition = new Vector3(0, anchoredPosY, 0);
        targetDescription.rectTransform.DOAnchorPosY(0, 0.5f);
        isHovering = true;
    }

    public void UnHover()
    {
        if (!isInitialized) return;
        if (!isHovering) return;

        targetDescription.DOFade(0.0f, 0.5f);
        targetDescription.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
        targetDescription.rectTransform.DOAnchorPosY(anchoredPosY, 0.5f);
        isHovering = false;
    }
}
