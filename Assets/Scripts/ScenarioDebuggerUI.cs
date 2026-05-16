using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NovelEditor;
using Assets.SimpleLocalization.Scripts;
using TMPro;

public class ScenarioDebuggerUI : MonoBehaviour
{
    [SerializeField] Canvas UICanvas;
    [SerializeField] TMP_Dropdown languageSelection;
    [SerializeField] NovelPlayer player;
    [SerializeField] NovelData data;

    private bool isPlaying = false;
    private int screenshotNumber = 0;

    private void Start()
    {
        PlayerPrefsManager.LoadPlayerPrefs();

#if STEAM
        switch (LocalizationManager.Language)
        {
            case "Japanese_Steam":
            default:
                languageSelection.value = 0;
                break;
            case "English_Steam":
                languageSelection.value = 1;
                break;
            case "Traditional Chinese_Steam":
                languageSelection.value = 2;
                break;
            case "Simplified Chinese_Steam":
                languageSelection.value = 3;
                break;
            case "Korean_Steam":
                languageSelection.value = 4;
                break;
        }
#else
        switch (LocalizationManager.Language)
        {
            case "Japanese":
            default:
                languageSelection.value = 0;
                break;
            case "English":
                languageSelection.value = 1;
                break;
            case "Traditional Chinese":
                languageSelection.value = 2;
                break;
            case "Simplified Chinese":
                languageSelection.value = 3;
                break;
            case "Korean":
                languageSelection.value = 4;
                break;
        }
#endif
    }

    public void PlayScript()
    {
        UICanvas.enabled = false;
        NovelSingletone.Instance.PlayNovel(data, true, End);
        isPlaying = true;
    }

    private void Update()
    {
        //if (isPlaying)
        //{
        //    // 終了チェック
        //    if (NovelSingletone.Instance.IsEnded())
        //    {
        //        // 終了
        //        Debug.Log("End");
        //        isPlaying = false;
        //        UICanvas.enabled = true;
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.F2))
        {
            screenshotNumber++;
            string fileName = screenshotNumber.ToString() + ".png";
            Debug.Log("output " + fileName);
            ScreenCapture.CaptureScreenshot(fileName, 1);
        }
    }

    private void End()
    {
        UICanvas.enabled = true;
        isPlaying = false;
    }

    public void OnLanguageChange()
    {
        string steam = string.Empty;

#if STEAM
        steam = "_Steam";
#endif

        switch (languageSelection.value)
        {
            case 0:
            default:
                LocalizationManager.Language = "Japanese" + steam;
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.JP);
                break;
            case 1:
                LocalizationManager.Language = "English" + steam;
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.EN);
                break;
            case 2:
                LocalizationManager.Language = "Traditional Chinese" + steam;
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.TCN);
                break;
            case 3:
                LocalizationManager.Language = "Simplified Chinese" + steam;
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.SCN);
                break;
            case 4:
                LocalizationManager.Language = "Korean" + steam;
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.KR);
                break;

        }
    }
}
