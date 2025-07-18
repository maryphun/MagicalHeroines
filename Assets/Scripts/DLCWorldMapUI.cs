using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;

public class DLCWorldMapUI : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private string BGM = "Town 1";

    [Header("References")]
    [SerializeField] private StageHandler stagehandler;
    [SerializeField] private TMPro.TMP_Text chapterName;
    [SerializeField] private Button startButton;

    private void Start()
    {
        // BGM再生
        AudioManager.Instance.PlayMusicWithCrossFade(BGM, 6.0f);

        // 画面遷移
        AlphaFadeManager.Instance.FadeIn(1.0f);

        if (chapterName != null)
        {
            chapterName.text = GetChapterName(ProgressManager.Instance.GetCurrentDLCStageProgress());
        }

        if (ProgressManager.Instance.IsDLCEnded())
        {
            if (stagehandler != null) stagehandler.gameObject.SetActive(false);
            if (chapterName != null) chapterName.gameObject.SetActive(false);
            if (startButton != null) startButton.gameObject.SetActive(false);
        }
    }

    public void BackButton()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        const float animationTime = 1.0f;

        string targetMap = "EndGameContent";
        if (ProgressManager.Instance.GetCurrentStageProgress() == 21)
        {
            targetMap = "Home";
        }

        StartCoroutine(SceneTransition(targetMap, animationTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // BGM停止
        AudioManager.Instance.StopMusicWithFade(1.0f);

        // シーン遷移
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to

        AlphaFadeManager.Instance.FadeOut(animationTime);

        yield return new WaitForSeconds(animationTime);
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 

        asyncLoad.allowSceneActivation = true;
    }

    public void NextStory()
    {
        if (!CheckCondition()) return;

        const float animationTime = 1.0f;

        DLCManager.isEnterDLCStage = true;

        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);
        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("Story", LoadSceneMode.Single); });

        // SE
        AudioManager.Instance.PlaySFX("QuestStart");

        // Switch BGM
        AudioManager.Instance.StopMusicWithFade(animationTime);

        // オートセーブを実行する
        AutoSave.ExecuteAutoSave();
    }

    // DLC_TODO
    private string GetChapterName(int progress)
    {
        return LocalizationManager.Localize("DLCStage-" + progress);
    }

    private bool CheckCondition()
    {
        // 条件が満たされていない?
        if (ProgressManager.Instance.GetCurrentDLCStageProgress() > 1) // 2キャラまでしかフォーメーション編成できない
        {
            if (ProgressManager.Instance.GetCharacterNumberInFormationParty() >= 3)
            {
                NovelSingletone.Instance.PlayNovel("DLC/FormationCondition", true);
                return false;
            }
        }
        
        if (ProgressManager.Instance.GetCurrentDLCStageProgress() >= 8) // Finalステージ
        {
            if (!ProgressManager.Instance.HasCharacter(12, true)) // ダイヤ闇落ち
            {
                NovelSingletone.Instance.PlayNovel("DLC/Condition Final", true);
                return false;
            }

            if (ProgressManager.Instance.IsCharacterInFormationParty(6)) // 京編入禁止
            {
                NovelSingletone.Instance.PlayNovel("DLC/Condition Final 2", true);
                return false;
            }
        }

        return true;
    }
}
