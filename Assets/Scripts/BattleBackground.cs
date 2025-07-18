using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class BattleBackground : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Vector2 movement;
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private Sprite basementSprite, centreTowerSprite, councilSprite;

    [Header("References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Image battleback;
    [SerializeField] private Image battlemid;
    [SerializeField] private Image battlefront;

    private Vector2 originalPosition;
    private RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalPosition = rect.anchoredPosition;
        rect.sizeDelta = rect.sizeDelta + movement;

        switch (BattleSetup.background)
        {
            case BattleBack.Basement:
                battleback.sprite = basementSprite;
                battlemid.gameObject.SetActive(false);
                battlefront.gameObject.SetActive(false);
                break;
            case BattleBack.CentreTower:
                battleback.sprite = centreTowerSprite;
                battlemid.gameObject.SetActive(false);
                battlefront.gameObject.SetActive(false);
                break;
            case BattleBack.Council:
                battleback.sprite = councilSprite;
                battlemid.gameObject.SetActive(false);
                battlefront.gameObject.SetActive(false);
                break;
            default:
            case BattleBack.Default:

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = (Input.mousePosition / mainCanvas.scaleFactor) / new Vector2(Screen.width, Screen.height) - new Vector2(0.5f, 0.5f);
        mousePosition = new Vector2(Mathf.Clamp(mousePosition.x, -0.5f, 0.5f), Mathf.Clamp(mousePosition.y, -0.5f, 0.5f));

        rect.DOAnchorPos(originalPosition + (mousePosition * movement), moveSpeed);
    }
}
