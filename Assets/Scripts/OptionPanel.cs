using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NovelEditor;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public enum SystemLanguage
{
    JP,
    EN,
    SCN,
    TCN,
}

[RequireComponent(typeof(CanvasGroup))]
public class OptionPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    [SerializeField] private Slider textSpeedSlider;
    [SerializeField] private Slider autoSpeedSlider;
    [SerializeField] private TMP_Text bgmVolumeValue;
    [SerializeField] private TMP_Text seVolumeValue;
    [SerializeField] private TMP_Text voiceVolumeValue;
    [SerializeField] private TMP_Text textSpeedValue;
    [SerializeField] private TMP_Text autoSpeedValue;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle windowScreenToggle;
    [SerializeField] private Toggle JPToggle, ENToggle, SCNToggle, TCNToggle;
    [SerializeField] private TMP_Dropdown resolutionOption;
    [SerializeField] private Button backButton;

    public static float defaultBGMVolume = 0.2f;
    public static float defaultSEVolume = 0.45f;
    public static float defaultVoiceVolume = 0.75f;
    public static int defaultTextSpeed = 6;
    public static float defaultAutoSpeed = 1.0f;
    public static bool defaultFullScreenToggle = false;
    public static bool defaultWindowScreenToggle = true;
    public static int defaultResolutionSizeWindowed = 0;
    public static int defaultResolutionSizeFull = 1;
    public static Vector2Int[] resolutionSizeOption = { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160) };
    public static SystemLanguage defaultLanguage = SystemLanguage.JP;


    private float tempBGMVolume = 0.5f;
    private float tempSEVolume = 0.5f;
    private float tempVoiceVolume = 0.5f;
    private int tempTextSpeed = 6;
    private float tempAutoSpeed = 1.0f;
    private int tempResolutionOption;

    private bool isChangingLanguge = false;
    private bool isOpen = false;
    
    public bool IsOpen()
    {
        return isOpen;
    }

    public void OpenOptionPanel()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        tempBGMVolume = AudioManager.Instance.GetMusicVolume();
        tempSEVolume = AudioManager.Instance.GetSEMasterVolume();
        tempVoiceVolume = NovelSingletone.Instance.GetVoiceVolume();
        tempTextSpeed = NovelSingletone.Instance.GetTextSpeed();
        tempAutoSpeed = NovelSingletone.Instance.GetAutoSpeed();
        tempResolutionOption = PlayerPrefsManager.GetResolutionOption();

        bgmVolumeSlider.value = tempBGMVolume;
        seVolumeSlider.value = tempSEVolume;
        voiceVolumeSlider.value = tempVoiceVolume;
        textSpeedSlider.value = tempTextSpeed;
        autoSpeedSlider.value = tempAutoSpeed;
        fullScreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
        windowScreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.Windowed;
#if STEAM
        JPToggle.isOn = LocalizationManager.Language == "Japanese_Steam";
        ENToggle.isOn = LocalizationManager.Language == "English_Steam";
        SCNToggle.isOn = LocalizationManager.Language == "Simplified Chinese_Steam";
        TCNToggle.isOn = LocalizationManager.Language == "Traditional Chinese_Steam";
#else
        JPToggle.isOn = LocalizationManager.Language == "Japanese";
        ENToggle.isOn = LocalizationManager.Language == "English";
        SCNToggle.isOn = LocalizationManager.Language == "Simplified Chinese";
        TCNToggle.isOn = LocalizationManager.Language == "Traditional Chinese";
#endif
        resolutionOption.SetValueWithoutNotify(tempResolutionOption);

        // Update value texts
        ChangeBGMVolume();
        ChangeSEVolume();
        ChangeVoiceVolume();
        TextSpeedVolume();
        AutoSpeedVolume();

        isOpen = true;
    }

    public void QuitOptionPanel()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        AudioManager.Instance.SetMusicVolume(tempBGMVolume);
        AudioManager.Instance.SetSEMasterVolume(tempSEVolume);
        NovelSingletone.Instance.SetVoiceVolume(tempVoiceVolume);
        NovelSingletone.Instance.SetTextSpeed(tempTextSpeed);
        NovelSingletone.Instance.SetAutoSpeed(tempAutoSpeed);

        UpdateScreenResolution(tempResolutionOption);

        isOpen = false;
    }

    public void ChangeBGMVolume()
    {
        AudioManager.Instance.SetMusicVolume(bgmVolumeSlider.value);
        NovelSingletone.Instance.SetBGMVolume(bgmVolumeSlider.value);

        string percentValue = (bgmVolumeSlider.value * 100).ToString("0") + "%";
        bgmVolumeValue.SetText(percentValue);
    }
    public void ChangeSEVolume()
    {
        AudioManager.Instance.SetSEMasterVolume(seVolumeSlider.value);
        NovelSingletone.Instance.SetSEVolume(seVolumeSlider.value);

        string percentValue = (seVolumeSlider.value * 100).ToString("0") + "%";
        seVolumeValue.SetText(percentValue);
    }
    public void ChangeVoiceVolume()
    {
        NovelSingletone.Instance.SetVoiceVolume(voiceVolumeSlider.value);

        string percentValue = (voiceVolumeSlider.value * 100).ToString("0") + "%";
        voiceVolumeValue.SetText(percentValue);
    }
    public void EndSEVolumeDrag()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemAlert");
    }
    public void TextSpeedVolume()
    {
        NovelSingletone.Instance.SetTextSpeed((int)(textSpeedSlider.value));

        textSpeedValue.SetText(((int)textSpeedSlider.value).ToString());
    }
    public void AutoSpeedVolume()
    {
        NovelSingletone.Instance.SetAutoSpeed(autoSpeedSlider.value);

        string percentValue = (autoSpeedSlider.value).ToString("0.00") + LocalizationManager.Localize("Option.Second");
        autoSpeedValue.SetText(percentValue);
    }

    public void SetToDefault()
    {
        bgmVolumeSlider.DOValue(defaultBGMVolume, 0.25f);
        seVolumeSlider.DOValue(defaultSEVolume, 0.25f);
        voiceVolumeSlider.DOValue(defaultVoiceVolume, 0.25f);
        textSpeedSlider.DOValue(defaultTextSpeed, 0.25f);
        autoSpeedSlider.DOValue(defaultAutoSpeed, 0.25f);
        fullScreenToggle.isOn = defaultFullScreenToggle;
        windowScreenToggle.isOn = defaultWindowScreenToggle;
        resolutionOption.value = defaultFullScreenToggle ? defaultResolutionSizeFull : defaultResolutionSizeWindowed;

        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemSelect");
    }

    public void ApplySetting()
    {
        tempBGMVolume = AudioManager.Instance.GetMusicVolume();
        PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.BGM_Volume, tempBGMVolume);
        tempSEVolume = AudioManager.Instance.GetSEMasterVolume();
        PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.SE_Volume, tempSEVolume);
        tempVoiceVolume = NovelSingletone.Instance.GetVoiceVolume();
        PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.VOICE_Volume, tempVoiceVolume);
        tempTextSpeed = NovelSingletone.Instance.GetTextSpeed();
        PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.TextSpeed, tempTextSpeed);
        tempAutoSpeed = NovelSingletone.Instance.GetAutoSpeed();
        PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.AutoSpeed, tempAutoSpeed);

        // apply resolution option
        tempResolutionOption = resolutionOption.value;
        PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Resolution, tempResolutionOption);
        UpdateScreenResolution(tempResolutionOption);
        Vector2Int resolution = OptionPanel.resolutionSizeOption[tempResolutionOption];

        // apply to change full screen mode
        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            if (windowScreenToggle.isOn)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.Windowed);
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.IsFullScreen, (int)FullScreenMode.Windowed);
            }
        }
        else
        {
            if (fullScreenToggle.isOn)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.ExclusiveFullScreen);
                PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.IsFullScreen, (int)FullScreenMode.ExclusiveFullScreen);
            }
        }

#if STEAM
        string lastLanguage = LocalizationManager.Language;
        if (JPToggle.isOn && lastLanguage != "Japanese_Steam")
        {
            LocalizationManager.Language = "Japanese_Steam";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.JP);
        }
        else if (ENToggle.isOn && lastLanguage != "English_Steam")
        {
            LocalizationManager.Language = "English_Steam";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.EN);
        }
        else if (SCNToggle.isOn && lastLanguage != "Simplified Chinese_Steam")
        {
            LocalizationManager.Language = "Simplified Chinese_Steam";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.SCN);
        }
        else if (TCNToggle.isOn && lastLanguage != "Traditional Chinese_Steam")
        {
            LocalizationManager.Language = "Traditional Chinese_Steam";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.TCN);
        }
#else
        string lastLanguage = LocalizationManager.Language;
        if (JPToggle.isOn && lastLanguage != "Japanese")
        {
            LocalizationManager.Language = "Japanese";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.JP);
        }
        else if (ENToggle.isOn && lastLanguage != "English")
        {
            LocalizationManager.Language = "English";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.EN);
        }
        else if (SCNToggle.isOn && lastLanguage != "Simplified Chinese")
        {
            LocalizationManager.Language = "Simplified Chinese";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.SCN);
        }
        else if (TCNToggle.isOn && lastLanguage != "Traditional Chinese")
        {
            LocalizationManager.Language = "Traditional Chinese";
            PlayerPrefsManager.SetPlayerPrefs(PlayerPrefsManager.PlayerPrefsSave.Language, (int)SystemLanguage.TCN);
        }
#endif

        // åæåÍïœçX
        if (lastLanguage != LocalizationManager.Language)
        {
            ProgressManager.Instance.RelocalizeCharactersName();
            AudioManager.Instance.PlaySFX("SystemButton");
            DynamicFont.Instance.UpdateAllFontsInScene();
        }

        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemSelect");
    }

    public void OnToggleFullScreen()
    {
        AudioManager.Instance.PlaySFX("SystemCursor");
        windowScreenToggle.isOn = !fullScreenToggle.isOn;
    }
    public void OnToggleWindoedScreen()
    {
        AudioManager.Instance.PlaySFX("SystemCursor");
        fullScreenToggle.isOn = !windowScreenToggle.isOn;
    }

    public void OnToggleLanguage(int lang)
    {
        if (isChangingLanguge) return;
        AudioManager.Instance.PlaySFX("SystemCursor");
        isChangingLanguge = true;
        switch ((SystemLanguage)lang)
        {
            case SystemLanguage.JP:
                ENToggle.isOn = false;
                SCNToggle.isOn = false;
                TCNToggle.isOn = false;
                break;
            case SystemLanguage.EN:
                JPToggle.isOn = false;
                SCNToggle.isOn = false;
                TCNToggle.isOn = false;
                break;
            case SystemLanguage.SCN:
                JPToggle.isOn = false;
                ENToggle.isOn = false;
                TCNToggle.isOn = false;
                break;
            case SystemLanguage.TCN:
                JPToggle.isOn = false;
                ENToggle.isOn = false;
                SCNToggle.isOn = false;
                break;
            default:
                break;
        }
        isChangingLanguge = false;
    }

    public void OnChangeResolution()
    {
        AudioManager.Instance.PlaySFX("SystemCursor");

        this.UpdateScreenResolution(resolutionOption.value);
    }

    public void UpdateScreenResolution(int option)
    {
        Vector2Int resolution = OptionPanel.resolutionSizeOption[option];
        Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreenMode);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            backButton.onClick.Invoke();
        }
    }
}
