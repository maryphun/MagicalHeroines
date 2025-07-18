using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NovelEditor;
using System.Linq;
using System;

public class NovelSingletone : SingletonMonoBehaviour<NovelSingletone>
{
    NovelPlayer novelplayer;
    bool isNovelCreated = false;

    List<GraphicRaycaster> disabledCanvas = new List<GraphicRaycaster>();
    Action callbackWhenFinish;
    public float defaultHidePlayTime;

    public void CreateNovelPlayer()
    {
        if (isNovelCreated) return;

        GameObject origin = Resources.Load<GameObject>("Prefabs/NovelPlayer");

        if (ReferenceEquals(origin, null))
        {
            Debug.LogWarning("Failed to load NovelPlayer Prefabs!");
        }

        GameObject spawnedObj = Instantiate(origin);
        spawnedObj.name = "NovelPlayerCanvas";
        spawnedObj.transform.SetParent(this.transform);

        novelplayer = spawnedObj.GetComponentInChildren<NovelPlayer>();
        defaultHidePlayTime = novelplayer._hideFadeTime;
        novelplayer.textSpeed = OptionPanel.defaultTextSpeed;
        novelplayer.VOICEVolume = OptionPanel.defaultVoiceVolume;

        if (ReferenceEquals(novelplayer, null))
        {
            Debug.LogWarning("NovelPlayer component not found!");
        }

        DontDestroyOnLoad(spawnedObj);

        isNovelCreated = true;
    }

    public void PlayNovel(string dataName, bool hideAfterPlay, Action callback = null)
    {
        Debug.Log("Start playing " + dataName);
        if (!isNovelCreated) CreateNovelPlayer();
        NovelData data = Resources.Load<NovelData>(dataName);
        this.PlayNovel(data, hideAfterPlay, callback);
    }

    public void PlayNovel(NovelData data, bool hideAfterPlay, Action callback = null)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer.Play(data, hideAfterPlay);

        // Raycast‚ðŽ×–‚‚·‚é‚à‚Ì‚ðˆê’U–³Œø‚É‚·‚é
        var raycasters = FindObjectsOfType<GraphicRaycaster>();
        disabledCanvas = raycasters.ToList();

        for (int i = 0; i < disabledCanvas.Count; i++)
        {
            if (disabledCanvas[i].gameObject.name != "NovelPlayerCanvas" && disabledCanvas[i].enabled)
            {
                disabledCanvas[i].enabled = false;
            }
            else
            {
                disabledCanvas.RemoveAt(i);
                i--;
            }
        }

        // I‚í‚Á‚½Œã‚ÌCallback
        callbackWhenFinish = callback;
        enabled = true;

        // ‰ñ‘zŠJ•ú
        PlayerPrefsManager.SetBool("memoryunlock:" + data.name, true);
    }

    public void SetHidePlayTime(float time)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer._hideFadeTime = time;
    }

    public bool IsPlaying()
    {
        if (!isNovelCreated) CreateNovelPlayer();

        return novelplayer.IsPlaying;
    }

    public bool IsPaused()
    {
        if (!isNovelCreated) CreateNovelPlayer();

        return novelplayer.IsStop;
    }

    public bool IsEnded()
    {
        if (!isNovelCreated) CreateNovelPlayer();

        return novelplayer.IsEnded && !novelplayer.IsImageChanging;
    }

    // Set
    public void SetBGMVolume(float value)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer.BGMVolume = value;
    }
    public void SetSEVolume(float value)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer.SEVolume = value;
    }
    public void SetTextSpeed(int value)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer.textSpeed = Mathf.Clamp(value, 1, 10) * 2;
    }
    public void SetAutoSpeed(float value)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer.autoSpeed = value;
    }
    public void SetVoiceVolume(float value)
    {
        if (!isNovelCreated) CreateNovelPlayer();

        novelplayer.VOICEVolume = value;
    }

    // Get
    public int GetTextSpeed()
    {
        if (!isNovelCreated) CreateNovelPlayer();

        return novelplayer.textSpeed / 2;
    }
    public float GetAutoSpeed()
    {
        if (!isNovelCreated) CreateNovelPlayer();

        return novelplayer.autoSpeed;
    }
    public float GetVoiceVolume()
    {
        if (!isNovelCreated) CreateNovelPlayer();

        return novelplayer.VOICEVolume;
    }

    private void Update()
    {
        if (IsEnded())
        {
            callbackWhenFinish?.Invoke();

            foreach (GraphicRaycaster canvas in disabledCanvas)
            {
                canvas.enabled = true;
            }
            disabledCanvas.Clear();
            novelplayer._hideFadeTime = defaultHidePlayTime; // return to normal
            enabled = false;
        }
    }
}
