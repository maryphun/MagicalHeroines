using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(Image))]
public class TitleLogoLocalizer : MonoBehaviour
{
    [SerializeField] Sprite LOGO_JP;
    [SerializeField] Sprite LOGO_TW;
    [SerializeField] Sprite LOGO_CN;
    [SerializeField] Sprite LOGO_EN;
    [SerializeField] Sprite LOGO_KR;

    [SerializeField] Image LOGO;

    void Start()
    {
        Init();
    }

    public void Init()
    {
#if STEAM
        switch (LocalizationManager.Language)
        {
            case "English":
            case "English_Steam":
                LOGO.sprite = LOGO_EN;
                return;
            case "Simplified Chinese":
            case "Simplified Chinese_Steam":
                LOGO.sprite = LOGO_CN;
                return;
            case "Traditional Chinese":
            case "Traditional Chinese_Steam":
                LOGO.sprite = LOGO_TW;
                return;
            case "Korean":
            case "Korean_Steam":
                LOGO.sprite = LOGO_KR;
                return;
            case "Japanese":
            case "Japanese_Steam":
            default:
                LOGO.sprite = LOGO_JP;
                return;
        }
#endif
    }
}
