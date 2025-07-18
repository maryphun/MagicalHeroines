using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using Assets.SimpleLocalization.Scripts;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class BattleSceneTransition : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float textAnimationTime;
    [SerializeField] private float blackOpenTime;
    [SerializeField] private float bannerCloseTime;
    [SerializeField] private float endTransitionTime;

    [Header("References")]
    [SerializeField] private RectTransform leftPiece;
    [SerializeField] private RectTransform rightPiece;
    [SerializeField] private RectTransform banner;
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private Battle battleManager;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        canvasGrp.alpha = 1.0f;
        canvasGrp.blocksRaycasts = true;
        canvasGrp.interactable = true;

        text.color = new Color(1, 1, 1, 0);
        text.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);

        banner.sizeDelta = new Vector2(0.0f, 0.0f);
        text.color = new Color(1, 1, 1, 0);
    }

    public void StartScene(Action<bool> callback)
    {
        StartCoroutine(StartSceneTransition(callback));
    }

    IEnumerator StartSceneTransition(Action<bool> callback)
    {
        banner.sizeDelta = new Vector2(0.0f, 0.0f);
        text.text = LocalizationManager.Localize("Battle.Start"); // 戦闘開始
        text.DOFade(1.0f, textAnimationTime);
        text.GetComponent<RectTransform>().DOScale(0.2f, textAnimationTime);

        // 音声演出
        AudioManager.Instance.PlaySFXDelay("BattleTransition", textAnimationTime * 0.5f);

        yield return new WaitForSeconds(textAnimationTime);

        banner.sizeDelta = new Vector2(0.0f, -900.0f);
        ShakeManager.Instance.ShakeObject(text.GetComponent<RectTransform>(), blackOpenTime * 0.1f, 5.0f);

        leftPiece.DOLocalMoveX(leftPiece.localPosition.x - leftPiece.rect.width, blackOpenTime);
        rightPiece.DOLocalMoveX(rightPiece.localPosition.x + rightPiece.rect.width, blackOpenTime);

        // 音声演出
        AudioManager.Instance.PlaySFX("SystemCollide");

        yield return new WaitForSeconds(blackOpenTime);

        banner.DOSizeDelta(new Vector2(banner.sizeDelta.x, -1125.0f), bannerCloseTime);

        yield return new WaitForSeconds(bannerCloseTime);

        callback?.Invoke(true);

        // end
        canvasGrp.alpha = 0.0f;
        canvasGrp.blocksRaycasts = false;
        canvasGrp.interactable = false;
    }

    public void EndScene(bool isVictory, Action<string> callback)
    {
        text.text = LocalizationManager.Localize(isVictory ? "Battle.Victory" : "Battle.Defeat");

        if (BattleSetup.isStoryMode) // ストーリーモード
        {
            if (isVictory)
            {
                // ストーリーに突入
                StartCoroutine(EndSceneTransition(callback, "AfterStory"));
            }
            else
            {
                // ゲームオーバー
                StartCoroutine(EndSceneTransition(callback, "GameOver"));
            }
        }
        else // 資源調達クエスト
        {
            if (isVictory)
            {
                // 報酬を表示
                StartCoroutine(EndSceneTransition(callback, "Reward"));
            }
            else
            {
                // 失敗
                string targetMap = (ProgressManager.Instance.GetCurrentStageProgress() <= 16) ? "WorldMap" : "EndGameContent";
                StartCoroutine(EndSceneTransition(callback, targetMap));
            }
        }
    }

    public void EndTutorial()
    {
        text.text = LocalizationManager.Localize("Battle.Defeat");
        ProgressManager.Instance.StageProgress(); // チュートリアル終了
        StartCoroutine(EndSceneTransition(battleManager.ChangeScene, "Tutorial"));
    }
    public void EndScene()
    {
        text.text = LocalizationManager.Localize("Battle.Defeat");
        StartCoroutine(EndSceneTransition(battleManager.ChangeScene, "Home"));
    }

    IEnumerator EndSceneTransition(Action<string> callback, string sceneName)
    {
        // 音声演出
        AudioManager.Instance.StopMusicWithFade(endTransitionTime);

        Init();
        text.color = Color.white;
        text.rectTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        banner.sizeDelta = new Vector2(banner.sizeDelta.x, -1125.0f);
        banner.DOSizeDelta(new Vector2(banner.sizeDelta.x, -900.0f), bannerCloseTime);

        yield return new WaitForSeconds(endTransitionTime);

        // 音声演出
        AudioManager.Instance.PlaySFX("Defeat");

        leftPiece.DOLocalMoveX(leftPiece.localPosition.x + leftPiece.rect.width, blackOpenTime);
        rightPiece.DOLocalMoveX(rightPiece.localPosition.x - rightPiece.rect.width, blackOpenTime);

        yield return new WaitForSeconds(blackOpenTime);

        AlphaFadeManager.Instance.FadeOut(textAnimationTime);

        yield return new WaitForSeconds(textAnimationTime);

        callback?.Invoke(sceneName);
    }
}
