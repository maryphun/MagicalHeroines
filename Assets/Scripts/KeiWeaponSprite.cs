using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class KeiWeaponSprite : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Vector2 moveValue = new Vector2(0.0f, 10.0f);
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite attack;

    [Header("References")]
    [SerializeField] private Battler mainScript;

    [Header("Debug")]
    [SerializeField] private RectTransform graphic;
    [SerializeField] private Vector2 origin;
    [SerializeField] private bool isActive = false;

    [HideInInspector] public RectTransform Rect { get { return graphic; } }

    public void Init()
    {
        graphic = GetComponent<RectTransform>();
        origin = graphic.localPosition;
        SetAnimationNormal();
    }

    private void Update()
    {
        if (!isActive) return;

        float ease = mainScript.Ease; 
        
        Mathf.PingPong(ease, 1.0f);

        float value = EaseInOutSine(ease);

        graphic.localPosition = origin + (moveValue * value);
    }

    private float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1.0f) / 2.0f;
    }

    public void SetEnableMovement(bool boolean)
    {
        isActive = boolean;
    }

    public void SetAnimationNormal()
    {
        GetComponent<Image>().sprite = normal;
    }
    public void SetAnimationAttack()
    {
        GetComponent<Image>().sprite = attack;
    }

    public void FadeOut()
    {
        GetComponent<Image>().DOFade(0.0f, 1.0f);
    }
}
