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
        if (ProgressManager.Instance.GetCurrentStageProgress() == 1) // �`���[�g���A���o�g���O
        {
            AudioManager.Instance.PlayMusicWithFade("Loop 3 (Tutorial)", 6.0f);
            NovelSingletone.Instance.PlayNovel("Tutorial1", true, LookAtMonitor);
        }
        else // �o�g���I����
        {
            AudioManager.Instance.PlayMusicWithFade("Loop 3 (Tutorial)", 6.0f);
            NovelSingletone.Instance.PlayNovel("Tutorial4", true, EndTutorial);
            tutorial4.SetActive(true);
        }
    }

    // ����͐�������������B�̕����̐퓬�̗l�q�ł��B (Dialog.Tutorial-1-17)
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
        // �G�L������ݒu
        BattleSetup.Reset(true);
        BattleSetup.SetAllowEscape(false);
        BattleSetup.SetEventBattle(true);
        BattleSetup.AddTeammate(PlayerCharacerID.TentacleMan);
        BattleSetup.AddTeammate(PlayerCharacerID.Battler);
        BattleSetup.AddEnemy("Akiho_Enemy");
        BattleSetup.AddEnemy("Rikka_Enemy");
        BattleSetup.SetBattleBGM("BattleTutorial");

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        yield return new WaitForSeconds(animationTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void EndTutorial()
    {
        // �V�[���J��
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
