using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class FormationTutorial : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Image img;
    [SerializeField] public RectTransform textPanel;
    [SerializeField] public TMP_Text tutorialText;
    [SerializeField] private GameObject formationSlots, unlockButton, regenButton;

    [Header("Debug")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TutorialStep step;
    [SerializeField] private bool isPlayingTutorial = false;
    [SerializeField] private GameObject displayingObj;
    [SerializeField] private Tween currentTween;

    enum TutorialStep
    {
        FormationPanel, // キャラクター編成画面は出征前の準備を行うところです。
        SwapCharacter,  // スロットを選択してフォーメーションを変えることができます。
        UnlockSlot,     // 資金があれば、パーティ人数を増やせます。
        Regenerate,     // 戦闘後に資金を使用してキャラクターを治療できます。
        End, 
    }

    const float textInterval = 0.05f;

    public void StartTutorial()
    {
        gameObject.SetActive(true);
        img.DOFade(0.95f, 1.0f);
        step = TutorialStep.FormationPanel;
        isPlayingTutorial = true;

        textPanel.anchoredPosition = new Vector3(0.0f, -55f, 0.0f);
        textPanel.sizeDelta = new Vector2(1100.0f, 200.0f);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    currentTween = tutorialText.DOText(string.Format(LocalizationManager.Localize("Tutorial.FormationPanel-1"), LocalizationManager.Localize("System.Formation")), 1.0f);
                    StartCoroutine(WaitForInput());
                    step = TutorialStep.FormationPanel;

                    // SE
                    audioSource = AudioManager.Instance.PlaySFX("TextDisplay");
                });
    }

    IEnumerator WaitForInput()
    {
        yield return new WaitUntil(IsButtonDown);

        if (currentTween.IsPlaying())
        {
            currentTween.Complete();
            tutorialText.DOComplete();
            yield return null;
            // 再帰
            StartCoroutine(WaitForInput());
        }
        else
        {
            // 音声演出
            AudioManager.Instance.PlaySFX("SystemCursor");

            // next
            switch (step)
            {
                case TutorialStep.FormationPanel:
                    currentTween = SequenceText("Tutorial.FormationPanel-2");
                    textPanel.DOSizeDelta(new Vector2(1300.0f, 220.0f), 1.0f);
                    textPanel.DOAnchorPosY(-165f, 1.0f);
                    displayingObj = Instantiate(formationSlots, transform);
                    //displayingObj.transform.SetParent(transform);
                    displayingObj.transform.SetAsFirstSibling();
                    step = TutorialStep.SwapCharacter;
                    break;
                case TutorialStep.SwapCharacter:
                    currentTween = SequenceText("Tutorial.FormationPanel-3");
                    textPanel.DOAnchorPosY(-230f, 1.0f);
                    Destroy(displayingObj);
                    displayingObj = null;
                    unlockButton.SetActive(true);
                    step = TutorialStep.UnlockSlot;
                    break;
                case TutorialStep.UnlockSlot:
                    currentTween = SequenceText("Tutorial.FormationPanel-4");
                    unlockButton.SetActive(false);
                    regenButton.SetActive(true);
                    step = TutorialStep.Regenerate;
                    break;
                case TutorialStep.Regenerate:
                    {
                        regenButton.SetActive(false);
                        step = TutorialStep.End;

                        // End Battle Tutorial
                        img.DOFade(0.0f, 0.5f);
                        textPanel.GetComponent<Image>().DOFade(0.0f, 0.5f);
                        tutorialText.DOFade(0.0f, 0.5f);

                        yield return new WaitForSeconds(0.5f + Time.deltaTime);

                        img.raycastTarget = false;
                        isPlayingTutorial = false;
                        gameObject.SetActive(false);
                        break;
                    }
            }

            yield return null;

            // 再帰
            if (step != TutorialStep.End)
            {
                StartCoroutine(WaitForInput());
            }
        }
    }

    private Tween SequenceText(string localizeID)
    {
        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            tutorialText.DOFade(0.0f, 0.25f);
        })
        .AppendInterval(0.25f)
        .AppendCallback(() =>
        {
            if (audioSource) audioSource.Stop();

            tutorialText.DOComplete();
            tutorialText.alpha = 1.0f;
            tutorialText.text = string.Empty;

            string newText = LocalizationManager.Localize(localizeID);
            tutorialText.DOText(newText, newText.Length * textInterval, true).SetEase(Ease.Linear).OnComplete(() => { if (audioSource) audioSource.Stop(); });

            // SE
            audioSource = AudioManager.Instance.PlaySFX("TextDisplay");
        });

        return sequence;
    }

    bool IsButtonDown()
    {
        if (!isPlayingTutorial) return false;

        return (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space));
    }
}
