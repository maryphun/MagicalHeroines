using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class WorldMapUI : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private bool isEndGameContent = false;
    [SerializeField] private string BGM = "Specification";

    [Header("References")]
    [SerializeField] private StageHandler stagehandler;
    [SerializeField] private TMPro.TMP_Text chapterName;
    [SerializeField] private Button startButton;
    [SerializeField] private Button dlcButton;

    private void Start()
    {
        // BGM�Đ�
        AudioManager.Instance.PlayMusicWithFade(BGM, 6.0f);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeIn(1.0f);

        if (chapterName != null)
        {
            chapterName.text = GetChapterName(ProgressManager.Instance.GetCurrentStageProgress());
        }

        if (isEndGameContent && ProgressManager.Instance.IsGameEnded())
        {
            if (stagehandler != null) stagehandler.gameObject.SetActive(false);
            if (chapterName != null) chapterName.gameObject.SetActive(false);
            if (startButton != null) startButton.gameObject.SetActive(false);
        }

        // dlc content
        if (dlcButton != null)
        {
            if (DLCManager.isDLCEnabled)
            {
                dlcButton.GetComponentInChildren<TMPro.TMP_Text>().color = Color.white;
                dlcButton.interactable = true;
            }
        }
    }

    public void ToHomeScene()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        const float animationTime = 1.0f;
        StartCoroutine(SceneTransition("Home", animationTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // BGM��~
        AudioManager.Instance.StopMusicWithFade(1.0f);

        // �V�[���J��
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to

        AlphaFadeManager.Instance.FadeOut(animationTime);

        yield return new WaitForSeconds(animationTime);
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 

        asyncLoad.allowSceneActivation = true;
    }

    public void DebugNextStage()
    {
        ProgressManager.Instance.StageProgress();
        stagehandler.Init();
    }

    public void NextStory()
    {
        if (!CheckCondition()) return;

        const float animationTime = 1.0f;

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("Story", LoadSceneMode.Single); });

        // SE
        AudioManager.Instance.PlaySFX("QuestStart");

        // Switch BGM
        AudioManager.Instance.StopMusicWithFade(animationTime);

        // �I�[�g�Z�[�u�����s����
        AutoSave.ExecuteAutoSave();
    }

    public void EnterDLCScene()
    {
        if (!DLCManager.isDLCEnabled) return;

        const float animationTime = 0.5f;

        DLCManager.isEnterDLCStage = false;

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("DLCWorldMap", LoadSceneMode.Single); });

        // SE
        AudioManager.Instance.PlaySFX("QuestStart");

        // Switch BGM
        AudioManager.Instance.StopMusicWithFade(animationTime);
    }

    private string GetChapterName(int progress)
    {
        return "Chapter " + (((progress - 1) / 3) + 1).ToString() + "-" + (((progress - 1) % 3) + 1).ToString();
    }

    private bool CheckCondition()
    {
        // ��������������Ă��Ȃ�?
        if (ProgressManager.Instance.GetCurrentStageProgress() == 6) // 2�3(�Z�ԕҍŏI�b)������ő��ŏI�i�K���K�v
        {
            if (!ProgressManager.Instance.HasCharacter(3, true))
            {
                NovelSingletone.Instance.PlayNovel("Condition Chapter2-3", true);
                return false;
            }
        }
        else if (ProgressManager.Instance.GetCurrentStageProgress() == 9) // 3-3(�G���i�ҍŏI�b)���Z�Ԉő��ŏI�i�K���K�v
        {
            if (!ProgressManager.Instance.HasCharacter(4, true))
            {
                NovelSingletone.Instance.PlayNovel("Condition Chapter3-3", true);
                return false;
            }
        }
        else if (ProgressManager.Instance.GetCurrentStageProgress() == 15) // 5�]3(�ߗR���ҍŏI�b)���G���i�ő��ŏI�i�K���K�v
        {
            if (!ProgressManager.Instance.HasCharacter(5, true))
            {
                NovelSingletone.Instance.PlayNovel("Condition Chapter5-3", true);
                return false;
            }
        }
        else if (ProgressManager.Instance.GetCurrentStageProgress() == 16) // 6�]1(�ŏI�b)���ߗR���ő��ŏI�i�K���K�v
        {
            if (!ProgressManager.Instance.HasCharacter(6, true) || !ProgressManager.Instance.HasCharacter(7, true))
            {
                NovelSingletone.Instance.PlayNovel("Condition Chapter6-1", true);
                return false;
            }
        }

        return true;
    }
}
