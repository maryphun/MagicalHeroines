using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.SimpleLocalization.Scripts;

public class DynamicFont : SingletonMonoBehaviour<DynamicFont>
{
    [Header("Debug")]
    [SerializeField] private TMP_FontAsset japaneseFont;
    [SerializeField] private TMP_FontAsset englishFont;
    [SerializeField] private TMP_FontAsset tchineseFont;
    [SerializeField] private TMP_FontAsset schineseFont;
    [SerializeField] private TMP_FontAsset japaneseDialogueFont;
    [SerializeField] private TMP_FontAsset englishDialogueFont;
    [SerializeField] private TMP_FontAsset tchineseDialogueFont;
    [SerializeField] private TMP_FontAsset schineseDialogueFont;
    [SerializeField] private bool initiated;

    private void Initiate()
    {
        japaneseFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/JP/ipaexg SDF");
        englishFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/JP/ipaexg SDF");
        tchineseFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/TC/NotoSansTC-VariableFont_wght SDF");
        schineseFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/SC/NotoSansSC-VariableFont_wght SDF");

        japaneseDialogueFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/JP/rounded-l-mplus-1c-heavy SDF");
        englishDialogueFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/JP/rounded-l-mplus-1c-heavy SDF");
        tchineseDialogueFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/TC/NotoSansTC-VariableFont_wght SDF");
        schineseDialogueFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/SC/NotoSansSC-VariableFont_wght SDF");

        initiated = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!initiated)
        {
            Initiate();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically apply fonts after scene change
        // UpdateAllFontsInScene();

        // Debug.Log("---Update all fonts in scene.---");
    }

    public void UpdateAllFontsInScene()
    {
        TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true); // true includes inactive
        foreach (var text in allTexts)
        {
            if (text.TryGetComponent<TMP_DynamicFont>(out TMP_DynamicFont tmp))
            {
                text.font = GetCurrentFont(tmp.IsDialogue());
            }
            else
            {
                text.font = GetCurrentFont(false);
            }
        }
    }

    public TMP_FontAsset GetFont(bool isDialogue = false)
    {
        if (!initiated)
        {
            Initiate();
        }

        return GetCurrentFont(isDialogue);
    }

    private TMP_FontAsset GetCurrentFont(bool isDialogue)
    {
        if (isDialogue)
        {
            switch (LocalizationManager.Language)
            {
                case "English":
                case "English_Steam":
                    return englishDialogueFont;
                case "Simplified Chinese":
                case "Simplified Chinese_Steam":
                    return schineseDialogueFont;
                case "Traditional Chinese":
                case "Traditional Chinese_Steam":
                    return tchineseDialogueFont;
                case "Japanese":
                case "Japanese_Steam":
                default:
                    return japaneseDialogueFont;
            }
        }

        switch (LocalizationManager.Language)
        {
            case "English":
            case "English_Steam":
                return englishFont;
            case "Simplified Chinese":
            case "Simplified Chinese_Steam":
                return schineseFont;
            case "Traditional Chinese":
            case "Traditional Chinese_Steam":
                return tchineseFont;
            case "Japanese":
            case "Japanese_Steam":
            default:
                return japaneseFont;
        }
    }
}
