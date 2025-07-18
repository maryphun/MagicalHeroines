using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class BattleSceneTutorial : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 0.2f)] private float textInterval = 0.05f;

    [Header("References")]
    [SerializeField] private CanvasGroup tutorialUI;
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject turnPanel, actionPanel, attackBtn, skillBtn, itemBtn, idleBtn;
    [SerializeField] private GameObject autoBattleButton;

    [Header("Debug")]
    [SerializeField] private Tween currentTween;
    [SerializeField] private TutorialStep step;
    [SerializeField] private GameObject lastDisplayingObject = null;
    [SerializeField] private bool isPlayingTutorial = false;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool isSkipping = false;

    public bool IsPlayingTutorial { get { return isPlayingTutorial; } }

    enum TutorialStep
    {
        Basic,
        TurnBase,
        Speed, //順番は行動速度によって決まり、行動速度が高いキャラから行動を選択できる。
        Action, // キャラクターが取れる行動は4種類あります。
        Attack, // 「攻撃」は一番基本の攻撃手段になります。攻撃者と相手のレベルでダメージが影響されます。
        AttackDamage, // 今回は相手の方が圧倒的にレベルが上なので、あまりダメージを与えられないでしょう。
        Skill, // 「特殊技」を使用して、キャラクターの固有スキルを発動できます。
        SkillLearn, // 特殊技はレベルアップなどで習得できます。
        Item, // 「アイテム」はチーム全体で共有されますが、使用するキャラクターのターンを消耗します。
        Idle, // 「待機」何もせずターンを終了することになりますが、SPを回復することができます。
        Hotkey, // ショートカットキーは「Ｚ」「Ｘ」「Ｃ」「Ｖ」となります。
        BattleStart,
        End, // では、実際に戦ってみましょう。
    }

    private void Start()
    {
        tutorialUI.alpha = 0.0f;
        tutorialUI.blocksRaycasts = false;
        tutorialUI.interactable = false;
    }

    // Battle Scene用チュートリアルシーケンス
    public void StartBattleTutorial()
    {
        tutorialUI.DOFade(1.0f, 0.5f);
        tutorialUI.blocksRaycasts = true;
        tutorialUI.interactable = true;
        isPlayingTutorial = true;
        step = TutorialStep.Basic;

        autoBattleButton.SetActive(false);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-1"), 1.0f);
                    StartCoroutine(WaitForInput());
                    step = TutorialStep.Basic;

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
                case TutorialStep.Basic:
                    currentTween = SequenceText("Dialog.Tutorial-3-2");
                    textPanel.DOSizeDelta(new Vector2(1300.0f, textPanel.sizeDelta.y), 1.0f);
                    step = TutorialStep.TurnBase;
                    break;
                case TutorialStep.TurnBase:
                    currentTween = SequenceText("Dialog.Tutorial-3-3");
                    textPanel.DOSizeDelta(new Vector2(1480.0f, 90.0f), 1.0f);
                    textPanel.DOAnchorPosY(375.0f, 1.0f);
                    step = TutorialStep.Speed;
                    DisplayObject(turnPanel);
                    break;
                case TutorialStep.Speed:
                    currentTween = SequenceText("Dialog.Tutorial-3-4");
                    textPanel.DOSizeDelta(new Vector2(880.0f, 90.0f), 1.0f);
                    textPanel.DOAnchorPosY(-250.0f, 1.0f);
                    step = TutorialStep.Action;
                    DisplayObject(actionPanel);
                    break;
                case TutorialStep.Action:
                    currentTween = SequenceText("Dialog.Tutorial-3-5");
                    textPanel.DOSizeDelta(new Vector2(1500, 90.0f), 1.0f);
                    step = TutorialStep.Attack;
                    DisplayObject(attackBtn);
                    break;
                case TutorialStep.Attack:
                    currentTween = SequenceText("Dialog.Tutorial-3-6");
                    textPanel.DOSizeDelta(new Vector2(1250.0f, 130.0f), 1.0f);
                    textPanel.DOAnchorPosY(-220.0f, 1.0f);
                    step = TutorialStep.AttackDamage;
                    break;
                case TutorialStep.AttackDamage:
                    currentTween = SequenceText("Dialog.Tutorial-3-7");
                    textPanel.DOSizeDelta(new Vector2(880.0f, 130.0f), 1.0f);
                    step = TutorialStep.Skill;
                    DisplayObject(skillBtn);
                    break;
                case TutorialStep.Skill:
                    currentTween = SequenceText("Dialog.Tutorial-3-8");
                    textPanel.DOSizeDelta(new Vector2(880.0f, 90.0f), 1.0f);
                    textPanel.DOAnchorPosY(-250.0f, 1.0f);
                    step = TutorialStep.SkillLearn;
                    break;
                case TutorialStep.SkillLearn:
                    currentTween = SequenceText("Dialog.Tutorial-3-9");
                    textPanel.DOSizeDelta(new Vector2(1037.0f, 90.0f), 1.0f);
                    step = TutorialStep.Item;
                    DisplayObject(itemBtn);
                    break;
                case TutorialStep.Item:
                    currentTween = SequenceText("Dialog.Tutorial-3-10");
                    textPanel.DOSizeDelta(new Vector2(615.0f, 90.0f), 1.0f);
                    step = TutorialStep.Idle;
                    DisplayObject(idleBtn);
                    break;
                case TutorialStep.Idle:
                    currentTween = SequenceText("Dialog.Tutorial-3-11");
                    textPanel.DOSizeDelta(new Vector2(1300.0f, 120.0f), 1.0f);
                    step = TutorialStep.Hotkey;
                    DisplayObject(actionPanel);
                    break;
                case TutorialStep.Hotkey:
                    currentTween = SequenceText("Dialog.Tutorial-3-12");
                    textPanel.DOSizeDelta(new Vector2(700.0f, 300.0f), 1.0f);
                    textPanel.DOAnchorPosY(0.0f, 1.0f);
                    step = TutorialStep.BattleStart;
                    DestroyDisplayingObject();
                    break;
                case TutorialStep.BattleStart:
                    {
                        step = TutorialStep.End;
                        // End Battle Tutorial
                        tutorialUI.DOFade(0.0f, 1.0f);
                        tutorialUI.blocksRaycasts = false;
                        tutorialUI.interactable = false;
                        isPlayingTutorial = false;
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

    bool IsButtonDown()
    {
        if (isSkipping || !isPlayingTutorial) return false;

        return (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space));
    }

    private void DisplayObject(GameObject origin)
    {
        if (lastDisplayingObject != null)
        {
            lastDisplayingObject.GetComponent<CanvasGroup>().DOFade(0.0f, 0.5f);
            Destroy(lastDisplayingObject, 0.5f);
            lastDisplayingObject = null;
        }

        var obj = Instantiate(origin, transform);
        obj.transform.position = origin.transform.position;

        obj.transform.SetSiblingIndex(1);
        CanvasGroup canvas;
        if (!obj.TryGetComponent<CanvasGroup>(out canvas))
        {
            canvas = obj.AddComponent<CanvasGroup>();
        }
        
        canvas.alpha = 0.0f;
        canvas.DOFade(1.0f, 0.5f);

        // highlight
        obj.AddComponent<HighlightImage>();

        lastDisplayingObject = obj;

        // いらないコンポネントをすべて削除
        foreach (var component in obj.GetComponentsInChildren<Button>())
        {
            Destroy(component);
        }
    }

    private void DestroyDisplayingObject()
    {
        if (!ReferenceEquals(lastDisplayingObject, null))
        {
            lastDisplayingObject.GetComponent<CanvasGroup>().DOFade(0.0f, 0.5f);
            Destroy(lastDisplayingObject, 0.5f);
            lastDisplayingObject = null;
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

    public void SkipTutorial()
    {
        step = TutorialStep.End;

        // End Battle Tutorial
        tutorialUI.DOFade(0.0f, 1.0f);
        tutorialUI.blocksRaycasts = false;
        tutorialUI.interactable = false;
        isPlayingTutorial = false;

        if (audioSource) audioSource.Stop();
        AudioManager.Instance.PlaySFX("SystemDecide");
    }

    public void MouseOnHoverSkipButton(bool value)
    {
        isSkipping = value;
    }
}
