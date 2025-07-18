using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tutorial4;

    [Header("Setting")]
    [SerializeField] private float sceneTransitionTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        AlphaFadeManager.Instance.FadeIn(sceneTransitionTime);
        if (ProgressManager.Instance.GetCurrentStageProgress() == 1) // チュートリアルバトル前
        {
            AudioManager.Instance.PlayMusicWithFade("Loop 3 (Tutorial)", 6.0f);
            NovelSingletone.Instance.PlayNovel("Tutorial1", true, LookAtMonitor);
        }
        else // バトル終了後
        {
            AudioManager.Instance.PlayMusicWithFade("Loop 3 (Tutorial)", 6.0f);
            NovelSingletone.Instance.PlayNovel("Tutorial4", true, EndTutorial);
            tutorial4.SetActive(true);
        }
    }

    // これは先日発生した私達の部隊の戦闘の様子です。 (Dialog.Tutorial-1-17)
    public void LookAtMonitor()
    {
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(1.0f)
                .AppendCallback(() =>
                {
                    NovelSingletone.Instance.PlayNovel("Tutorial2", true, SceneTransit);
                });
    }

    public void SceneTransit()
    {
        AudioManager.Instance.StopMusicWithFade(sceneTransitionTime);
        StartCoroutine(SceneTransition("Battle", sceneTransitionTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // 敵キャラを設置
        BattleSetup.Reset(true);
        BattleSetup.SetAllowEscape(false);
        BattleSetup.SetEventBattle(true);
        BattleSetup.AddTeammate(PlayerCharacerID.TentacleMan);
        BattleSetup.AddTeammate(PlayerCharacerID.Battler);
        BattleSetup.AddEnemy("Akiho_Enemy");
        BattleSetup.AddEnemy("Rikka_Enemy");
        BattleSetup.SetBattleBGM("BattleTutorial");

        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);
        yield return new WaitForSeconds(animationTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void EndTutorial()
    {
        // シーン遷移
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Home", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        DOTween.Sequence()
            .AppendCallback(() => 
            {
                AudioManager.Instance.StopMusicWithFade(sceneTransitionTime * 0.5f);
                AlphaFadeManager.Instance.FadeOut(sceneTransitionTime);
            })
            .AppendInterval(sceneTransitionTime)
            .AppendCallback(() =>
            {
                asyncLoad.allowSceneActivation = true;
            });
    }
}
