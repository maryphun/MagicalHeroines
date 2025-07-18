using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class CharacterArrow : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float moveDistance = 10.0f;
    [SerializeField] private float changeCharacterSpeed = 0.2f;

    [Header("Debug")]
    [SerializeField] private Image img;
    [SerializeField] private RectTransform rect;
    [SerializeField] private float count;
    [SerializeField] private float originalPositionY;
    [SerializeField] private bool isFirstTurn = true;
    [SerializeField] private bool isSetCharacter = false;
    [SerializeField] private bool isLocked = false;

    public float ChangeCharacterSpeed { get { return changeCharacterSpeed; } }

    private void Awake()
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        count = 0.0f;
        originalPositionY = rect.position.y;
        img.color = new Color(1, 1, 1, 0);
        img.raycastTarget = false;
        img.maskable = false;
        isSetCharacter = false;
        isFirstTurn = true;
        isLocked = false;
    }

    public void SetCharacter(Battler battler, float offsetY)
    {
        transform.SetParent(battler.transform);
        offsetY /= CanvasReferencer.Instance.GetScaleFactor();
        originalPositionY = offsetY;
        if (isFirstTurn)
        {
            rect.localPosition = new Vector3(0.0f, offsetY, 0.0f);
            isFirstTurn = false;
        }
        else
        {
            isLocked = true;
            rect.DOLocalMove(new Vector3(0.0f, offsetY, 0.0f), changeCharacterSpeed).onComplete = ResetLock;
        }
        isSetCharacter = true;
        img.color = Color.white;
    }

    private void ResetLock()
    {
        isLocked = false;
    }
    
    private void Update()
    {
        if (!isSetCharacter || isLocked) return;

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
