using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image)), RequireComponent(typeof(RectTransform))]
public class TitleCharacterAnimation : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float delay;
    [SerializeField] private float introTime;
    [SerializeField] private Vector2 offset;

    [Header("References")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image img;

    private Vector2 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        originalPos = rect.position;

        // intro
        rect.position = originalPos + offset;
        img.color = new Color(1, 1, 1, 0);

        StartCoroutine(Delay(this.delay));
    }

    IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);

        rect.DOMove(originalPos, introTime);
        img.DOColor(Color.white, introTime);
    }
}
