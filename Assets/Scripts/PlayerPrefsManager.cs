using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if STEAM
using Steamworks;
#endif

public static class PlayerPrefsManager
{
    private static int BoolToInt(bool val)
    {
        if (val)
            return 1;
        else
            return 0;
    }
    private static bool IntToBool(int val)
    {
        if (val == 0)
            return false;
        else
            return true;
    }

    public enum PlayerPrefsSave
    {
        IsFullScreen,
        BGM_Volume,
        SE_Volume,
        VOICE_Volume,
        TextSpeed,
        AutoSpeed,
        Language,
        LastestProgress,
        Resolution,
    }

    public static void LoadPlayerPrefs()
    {
        int fullScreenMode = PlayerPrefs.GetInt(PlayerPrefsSave.IsFullScreen.ToString(), (int)(OptionPanel.defaultFullScreenToggle ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed));
        Screen.fullScreenMode = (FullScreenMode)fullScreenMode;

        int resolutionOption = PlayerPrefs.GetInt(PlayerPrefsSave.Resolution.ToString(), OptionPanel.defaultFullScreenToggle ? OptionPanel.defaultResolutionSizeFull : OptionPanel.defaultResolutionSizeWindowed);
        Vector2Int resolution = OptionPanel.resolutionSizeOption[resolutionOption];
        Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreenMode);

        float musicVolume = PlayerPrefs.GetFloat(PlayerPrefsSave.BGM_Volume.ToString(), OptionPanel.defaultBGMVolume);
        AudioManager.Instance.SetMusicVolume(musicVolume);

        float seVolume = PlayerPrefs.GetFloat(PlayerPrefsSave.SE_Volume.ToString(), OptionPanel.defaultSEVolume);
        AudioManager.Instance.SetSEMasterVolume(seVolume);

        float voiceVolume = PlayerPrefs.GetFloat(PlayerPrefsSave.VOICE_Volume.ToString(), OptionPanel.defaultVoiceVolume);
        NovelSingletone.Instance.SetVoiceVolume(voiceVolume);

        int textSpd = PlayerPrefs.GetInt(PlayerPrefsSave.TextSpeed.ToString(), OptionPanel.defaultTextSpeed);
        NovelSingletone.Instance.SetTextSpeed(textSpd);

        float autoSpd = PlayerPrefs.GetFloat(PlayerPrefsSave.AutoSpeed.ToString(), OptionPanel.defaultAutoSpeed);
        NovelSingletone.Instance.SetAutoSpeed(autoSpd);


        SystemLanguage lang = PlayerPrefs.HasKey(PlayerPrefsSave.Language.ToString())
            ? (SystemLanguage)PlayerPrefs.GetInt(PlayerPrefsSave.Language.ToString())
            : GetDefaultLanguage();

        if (!IsSupportedLanguage(lang))
        {
            lang = GetDefaultLanguage();
        }

        ApplyLanguage(lang);
    }

    private static SystemLanguage GetDefaultLanguage()
    {
        if (TryGetSystemLanguage(out SystemLanguage language))
        {
            return language;
        }

#if STEAM
        if (TryGetSteamLanguage(out language))
        {
            return language;
        }
#endif

        return SystemLanguage.EN;
    }

    private static bool TryGetSystemLanguage(out SystemLanguage language)
    {
        switch (Application.systemLanguage)
        {
            case UnityEngine.SystemLanguage.Japanese:
                language = SystemLanguage.JP;
                return true;
            case UnityEngine.SystemLanguage.English:
                language = SystemLanguage.EN;
                return true;
            case UnityEngine.SystemLanguage.ChineseSimplified:
                language = SystemLanguage.SCN;
                return true;
            case UnityEngine.SystemLanguage.ChineseTraditional:
                language = SystemLanguage.TCN;
                return true;
            case UnityEngine.SystemLanguage.Chinese:
                language = SystemLanguage.SCN;
                return true;
            case UnityEngine.SystemLanguage.Korean:
                language = SystemLanguage.KR;
                return true;
            default:
                language = SystemLanguage.EN;
                return false;
        }
    }

#if STEAM
    private static bool TryGetSteamLanguage(out SystemLanguage language)
    {
        language = SystemLanguage.EN;

        if (!SteamManager.Initialized)
        {
            return false;
        }

        if (TryMapSteamLanguage(SteamApps.GetCurrentGameLanguage(), out language))
        {
            return true;
        }

        return TryMapSteamLanguage(SteamUtils.GetSteamUILanguage(), out language);
    }

    private static bool TryMapSteamLanguage(string steamLanguage, out SystemLanguage language)
    {
        switch ((steamLanguage ?? string.Empty).ToLowerInvariant())
        {
            case "japanese":
                language = SystemLanguage.JP;
                return true;
            case "english":
                language = SystemLanguage.EN;
                return true;
            case "schinese":
                language = SystemLanguage.SCN;
                return true;
            case "tchinese":
                language = SystemLanguage.TCN;
                return true;
            case "koreana":
                language = SystemLanguage.KR;
                return true;
            default:
                language = SystemLanguage.EN;
                return false;
        }
    }
#endif

    private static bool IsSupportedLanguage(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.JP:
            case SystemLanguage.EN:
            case SystemLanguage.SCN:
            case SystemLanguage.TCN:
            case SystemLanguage.KR:
                return true;
            default:
                return false;
        }
    }

    private static void ApplyLanguage(SystemLanguage language)
    {
#if STEAM
        switch (language)
        {
            case SystemLanguage.JP:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Japanese_Steam";
                break;
            case SystemLanguage.EN:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "English_Steam";
                break;
            case SystemLanguage.SCN:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Simplified Chinese_Steam";
                break;
            case SystemLanguage.TCN:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Traditional Chinese_Steam";
                break;
            case SystemLanguage.KR:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Korean_Steam";
                break;
            default:
                break;
        }
#else
        switch (language)
        {
            case SystemLanguage.JP:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Japanese";
                break;
            case SystemLanguage.EN:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "English";
                break;
            case SystemLanguage.SCN:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Simplified Chinese";
                break;
            case SystemLanguage.TCN:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Traditional Chinese";
                break;
            case SystemLanguage.KR:
                Assets.SimpleLocalization.Scripts.LocalizationManager.Language = "Korean";
                break;
            default:
                break;
        }
#endif
    }

    public static void SetPlayerPrefs(PlayerPrefsSave name, int value)
    {
        PlayerPrefs.SetInt(name.ToString(), value);
    }
    public static void SetPlayerPrefs(PlayerPrefsSave name, float value)
    {
        PlayerPrefs.SetFloat(name.ToString(), value);
    }

    public static bool GetBool(string name, bool defaultValue = false)
    {
        return IntToBool(PlayerPrefs.GetInt(name, BoolToInt(defaultValue)));
    }

    public static void SetBool(string name, bool value)
    {
        PlayerPrefs.SetInt(name, BoolToInt(value));
    }

    // progress
    public static void UpdateCurrentProgress(int progress)
    {
        PlayerPrefs.SetInt(PlayerPrefsSave.LastestProgress.ToString(), Mathf.Max(progress, GetLatestProgress()));
    }
    public static int GetLatestProgress()
    {
#if DEMO
        return Mathf.Min(PlayerPrefs.GetInt(PlayerPrefsSave.LastestProgress.ToString(), 0), DemoParameter.EndChapter); // Max level at demo
# endif
        return PlayerPrefs.GetInt(PlayerPrefsSave.LastestProgress.ToString(), 0);
    }

    // resolution
    public static int GetResolutionOption()
    {
        return PlayerPrefs.GetInt(PlayerPrefsSave.Resolution.ToString(), OptionPanel.defaultFullScreenToggle ? OptionPanel.defaultResolutionSizeFull : OptionPanel.defaultResolutionSizeWindowed);
    }
}
