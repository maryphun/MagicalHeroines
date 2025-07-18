using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class CharacterBuildingPanelTutorial : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Image img;
    [SerializeField] public RectTransform textPanel;
    [SerializeField] public TMP_Text tutorialText;
    [SerializeField] private GameObject equipmentButton, abilityButtons;

    [Header("Debug")]
    [SerializeField] private GameObject displayingObj;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TutorialStep step;
    [SerializeField] private bool isPlayingTutorial = false;
    [SerializeField] private Tween currentTween;

    enum TutorialStep
    {
        Start,          // {0}はキャラクターのステータスの確認、レベルアップ、装備を行うところです。
        Equipment,      // ここで装備を確認・変更でます。
        Ability,        // キャラの特殊技も確認できます！
        Regenerate,     // 戦闘後に資金を使用してキャラクターを治療できます。
        End,
    }

    const float textInterval = 0.05f;

    public void StartTutorial()
    {
        gameObject.SetActive(true);
        img.DOFade(0.95f, 1.0f);
        step = TutorialStep.Start;
        isPlayingTutorial = true;

        textPanel.anchoredPosition = new Vector3(0.0f, 0f, 0.0f);
        textPanel.sizeDelta = new Vector2(1250.0f, 200.0f);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.75f)
                .AppendCallback(() =>
                {
                    currentTween = tutorialText.DOText(string.Format(LocalizationManager.Localize("Tutorial.CharacterBuildingPanel-1"), LocalizationManager.Localize("System.CharacterBuilding")), 2.0f);
                    StartCoroutine(WaitForInput());
                    step = TutorialStep.Start;

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
                case TutorialStep.Start:
                    currentTween = SequenceText("Tutorial.CharacterBuildingPanel-2");
                    textPanel.DOSizeDelta(new Vector2(458.1858f, 150.0f), 1.0f);
                    textPanel.DOAnchorPos(new Vector3(200f, -449f, 0.0f), 1.0f);
                    step = TutorialStep.Equipment;

                    displayingObj = Instantiate(equipmentButton, transform, true);
                    displayingObj.transform.SetSiblingIndex(0);
                    break;
                case TutorialStep.Equipment:
                    currentTween = SequenceText("Tutorial.CharacterBuildingPanel-3");
                    textPanel.DOSizeDelta(new Vector2(470.0f, 150.0f), 1.0f);
                    textPanel.DOAnchorPosX(550.0f, 1.0f);
                    step = TutorialStep.Ability;

                    Destroy(displayingObj);
                    displayingObj = Instantiate(abilityButtons, transform, true);
                    displayingObj.transform.SetSiblingIndex(0);
                    break;
                case TutorialStep.Ability:
                    {
                        step = TutorialStep.End;

                        Destroy(displayingObj);

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
