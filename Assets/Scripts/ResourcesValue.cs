using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
public class ResourcesValue : MonoBehaviour
{
    [System.Serializable]
    enum ResourcesType
    {
        Money,
        ResourcesPoint,
    }

    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;
    [SerializeField] private ResourcesType type;

    [HideInInspector] private TMP_Text text;
    [HideInInspector] private int currentDisplayingValue = 0;

    public void Start()
    {
        text = GetComponent<TMP_Text>();
        UpdateValue();
    }

    private void Update()
    {
        UpdateValue();
    }

    private void UpdateValue()
    {
        int realValue = type == ResourcesType.Money ? ProgressManager.Instance.GetCurrentMoney() : ProgressManager.Instance.GetCurrentResearchPoint();

        if (realValue != currentDisplayingValue)
        {
            this.text.DOCounter(currentDisplayingValue, realValue, animationTime);
            currentDisplayingValue = realValue;
        }
    }
}
