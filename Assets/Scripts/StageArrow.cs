using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class StageArrow : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float moveDistance = 10.0f;

    [Header("Debug")]
    [SerializeField] private Image img;
    [SerializeField] private RectTransform rect;
    [SerializeField] private float count;
    [SerializeField] private float originalPositionY;
    [SerializeField] private bool isLocked = false;


    private void Awake()
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        count = 0.0f;
        originalPositionY = rect.position.y;
        img.color = new Color(1, 1, 1, 0);
        img.raycastTarget = false;
        img.maskable = false;
        isLocked = false;
    }

    public void SetStage(RectTransform stageObj, float offsetY)
    {
        transform.SetParent(stageObj);
        originalPositionY = offsetY + (stageObj.sizeDelta.y * 0.5f);
        rect.localPosition = new Vector3(0.0f, originalPositionY, 0.0f);
        img.color = Color.white;
    }

    private void ResetLock()
    {
        isLocked = false;
    }

    private void Update()
    {
        if (isLocked) return;

        count = (count + Time.deltaTime);

        Mathf.PingPong(count, 1.0f);

        float value = (EaseInOutSine(count) * moveDistance);

        rect.localPosition = new Vector3(0.0f, originalPositionY + value, 0.0f);
    }

    private float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1.0f) / 2.0f;
    }
}
