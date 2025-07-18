using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Assets.SimpleLocalization.Scripts;
using UnityEngine.SceneManagement;

public class RewardPanel : MonoBehaviour
{
    [SerializeField] private bool isDebug = false;

    [Header("Setting")]
    [SerializeField] private float displayPositionY = -150.0f;
    [SerializeField] private float animTime = 0.25f;
    [SerializeField] private float removeAnimTime = 1f;
    [SerializeField] private Sprite[] heroinSprite = new Sprite[5];

    [Header("References")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private CanvasGroup panelCanvas;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private Image itemSprite;

    [SerializeField] private RectTransform moneyRewardPanel;
    [SerializeField] private TMP_Text researchPointValue;
    [SerializeField] private TMP_Text moneyValue;

    [SerializeField] private Button nextButton;

    [SerializeField] private CanvasGroup heroinPanel;
    [SerializeField] private Image newHeroin;
    [SerializeField] private TMP_Text newHeroinText;

    [Header("Debug")]
    [SerializeField] private bool isFirst = true;
    [SerializeField] private bool isHeroinDisplayed = false;
    [SerializeField] private bool isGetHisui = false;
    [SerializeField] private bool isGetKei = false;

    private void Start()
    {
#if DEBUG_MODE
        if (isDebug)
        {
            ProgressManager.Instance.InitializeProgress();
            BattleSetup.Reset(true);
            BattleSetup.SetBattleDLC(true);
            BattleSetup.SetReward(0, 10000);
            ProgressManager.Instance.DLCStageProgress(8);
        }
#endif

        DisplayMoneyAndResearchPointReward();
        AlphaFadeManager.Instance.FadeIn(0.5f);

        // 警戒度を更新
        ProgressManager.Instance.SetSideQuestData(ProgressManager.Instance.GetSideQuestData().food + BattleSetup.sideQuestIncrement.food,
                                                  ProgressManager.Instance.GetSideQuestData().bank + BattleSetup.sideQuestIncrement.bank,
                                                  ProgressManager.Instance.GetSideQuestData().research + BattleSetup.sideQuestIncrement.research);
    }

    private void PlayAnimation()
    {
        panel.DOComplete();
        panelCanvas.DOComplete();

        if (isFirst)
        {
            HidePanel(moneyRewardPanel.GetComponent<CanvasGroup>(), moneyRewardPanel);
        }
        else
        {
            var copy = Instantiate(panel.gameObject, panel.transform.parent);
            copy.transform.SetSiblingIndex(0);
            HidePanel(copy.GetComponent<CanvasGroup>(), copy.GetComponent<RectTransform>());
            Destroy(copy, animTime);
        }

        panelCanvas.alpha = 0.0f;
        panel.anchoredPosition = new Vector2(0.0f, 0.0f);
        panel.DOAnchorPosY(displayPositionY, animTime);
        panelCanvas.DOFade(1.0f, animTime);

        isFirst = false;

        // SE
        AudioManager.Instance.PlaySFX("BattleTransition");
    }

    private void UpdateInformation()
    {
        // determine item or equipment to display, item first
        if (BattleSetup.itemReward != null && BattleSetup.itemReward.Count > 0)
        {
            var item = BattleSetup.itemReward[BattleSetup.itemReward.Count - 1];
            itemName.text = LocalizationManager.Localize(item.itemNameID);
            itemSprite.sprite = item.Icon;

            ProgressManager.Instance.AddItemToInventory(item);
            BattleSetup.itemReward.Remove(item);
        }
        else if (BattleSetup.equipmentReward != null && BattleSetup.equipmentReward.Count > 0)
        {
            var equipment = BattleSetup.equipmentReward[BattleSetup.equipmentReward.Count - 1];
            itemName.text = LocalizationManager.Localize(equipment.equipNameID);
            itemSprite.sprite = equipment.Icon;

            ProgressManager.Instance.AddNewEquipment(equipment);
            BattleSetup.equipmentReward.Remove(equipment);
        }
        else if (IsNewHeroinGet())
        {
            heroinPanel.DOFade(1.0f, 1.0f);

            isHeroinDisplayed = true;

            // DLC特殊処理
            if (true || DLCManager.isDLCEnabled)
            {
                if (isGetHisui && !isGetKei)
                {
                    isHeroinDisplayed = false;
                }
            }

            panelCanvas.DOKill(false);
            panelCanvas.alpha = 0.0f;

            // SE
            AudioManager.Instance.PlaySFX("SystemNewHeroin");
        }
        else if (IsSpecialEvent())
        {

        }
        else　// reward end
        {
            panelCanvas.DOKill(false);
            panelCanvas.alpha = 0.0f;

            nextButton.interactable = false;

            // SE
            AudioManager.Instance.PlaySFX("RewardEnd");

            // scene transition
            AlphaFadeManager.Instance.FadeOut(1.0f);
            string targetMap = (ProgressManager.Instance.GetCurrentStageProgress() <= 16) ? "WorldMap" : "EndGameContent";
            string nextScene = BattleSetup.isStoryMode ? "Home" : targetMap;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
            asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
            DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() => { asyncLoad.allowSceneActivation = true; });

            // オートセーブを実行する
            AutoSave.ExecuteAutoSave();
        }
    }

    /// <summary>
    /// 新しく聖核戦姫を捕獲できたか
    /// </summary>
    private bool IsNewHeroinGet()
    {
        if (!BattleSetup.isStoryMode) return false; // 資源調達クエストでヒロインを捉えることはない
        if (BattleSetup.isDLCBattle) return IsNewDLCHeroinGet();
        if (isHeroinDisplayed)
        {
            // UIを非表示にする
            heroinPanel.DOFade(0.0f, 1.0f);
            return false;
        }

        int stage = (ProgressManager.Instance.GetCurrentStageProgress() - 1); // ステージ番号はすでに更新されているので-1で見る
        string s = string.Empty;
        switch (stage)
        {
            case 3: // 明穂
                newHeroin.sprite = heroinSprite[0];
                s = "<color=#FFC0CB>" + LocalizationManager.Localize("Name.Akiho") + "</color>";
                newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                return true;
            case 6: // 立花
                newHeroin.sprite = heroinSprite[1];
                s = "<color=#ADD8E6>" + LocalizationManager.Localize("Name.Rikka") + "</color>";
                newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                return true;
            case 9: // エレナ
                newHeroin.sprite = heroinSprite[2];
                s = "<color=#F1E5AC>" + LocalizationManager.Localize("Name.Erena") + "</color>";
                newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                return true;
            case 13: // 京 (京の捕獲は5-1)
                newHeroin.sprite = heroinSprite[3];
                s = "<color=#ADD8E6>" + LocalizationManager.Localize("Name.Kei") + "</color>";
                newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                return true;
            case 15: // 那由多
                newHeroin.sprite = heroinSprite[4];
                s = "<color=#8b0000>" + LocalizationManager.Localize("Name.Nayuta") + "</color>";
                newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 新しく聖核戦姫を捕獲できたか(D_LC用)
    /// </summary>
    private bool IsNewDLCHeroinGet()
    {
        if (isHeroinDisplayed)
        {
            // UIを非表示にする
            heroinPanel.DOFade(0.0f, 1.0f);
            return false;
        }

        int stage = (ProgressManager.Instance.GetCurrentDLCStageProgress() - 1); // ステージ番号はすでに更新されているので-1で見る
        string s = string.Empty;
        switch (stage)
        {
            case 5: // ダイヤ
                newHeroin.sprite = heroinSprite[5];
                s = "<color=#D4BEE4>" + LocalizationManager.Localize("Name.Diamond") + "</color>";
                newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                return true;
            case 8: // ヒスイと京
                if (!isGetHisui)
                {
                    newHeroin.sprite = heroinSprite[6];
                    s = "<color=#C9E9D2>" + LocalizationManager.Localize("Name.Hisui") + "</color>";
                    newHeroinText.text = LocalizationManager.Localize("System.Trapped").Replace("{s}", s);
                    isGetHisui = true;
                    isHeroinDisplayed = false;
                }
                else
                {
                    newHeroin.sprite = heroinSprite[7];
                    s = "<color=#C9E9D2>" + LocalizationManager.Localize("Name.Kei_Battler") + "</color>";
                    newHeroinText.text = System.String.Format(LocalizationManager.Localize("System.KeiDowngrade"), LocalizationManager.Localize("Name.Kei_Corrupted"), s);
                    newHeroinText.rectTransform.localPosition = new Vector3(newHeroinText.rectTransform.localPosition.x, newHeroinText.rectTransform.localPosition.y + 20.0f, newHeroinText.rectTransform.localPosition.z);
                    isGetKei = true;
                    isGetHisui = true;
                }
                return true;
            default:
                return false;
        }
    }

    public void Next()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemSwitch");

        PlayAnimation();
        UpdateInformation();
    }

    private void DisplayMoneyAndResearchPointReward()
    {
        // SE
        AudioManager.Instance.PlaySFX("Reward");

        // UI
        researchPointValue.DOCounter(0, BattleSetup.researchPointReward, 2.0f).SetEase(Ease.Linear);
        moneyValue.DOCounter(0, BattleSetup.moneyReward, 2.0f).SetEase(Ease.Linear);

        // Add resources
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() + BattleSetup.researchPointReward);
        ProgressManager.Instance.SetMoney(ProgressManager.Instance.GetCurrentMoney() + BattleSetup.moneyReward);

        // Reset
        BattleSetup.SetReward(0, 0);
    }

    private void HidePanel(CanvasGroup fade, RectTransform rect)
    {
        rect.DOScale(0.5f, removeAnimTime);
        fade.DOFade(0.0f, removeAnimTime);
    }

    // 特殊イベント発生
    private bool IsSpecialEvent()
    {
        int stage = (ProgressManager.Instance.GetCurrentStageProgress() - 1); // ステージ番号はすでに更新されているので-1で見る
        if (BattleSetup.isStoryMode && stage == 6)
        {
            panelCanvas.DOKill(false);
            panelCanvas.alpha = 0.0f;

            nextButton.interactable = false;
            nextButton.gameObject.SetActive(false);

            // SE
            AudioManager.Instance.PlaySFX("RewardEnd");

            // scene transition
            AlphaFadeManager.Instance.FadeOut(1.0f);

            // setup event battle
            BattleSetup.Reset(false);
            BattleSetup.SetAllowEscape(false);
            BattleSetup.SetEventBattle(true);
            BattleSetup.SetReward(0, 0);
            BattleSetup.AddTeammate("9.Battler(Event)");
            BattleSetup.AddTeammate("9.Battler(Event)");
            BattleSetup.AddEnemy("Erena_Enemy");
            BattleSetup.SetBattleBGM("zensen he totugekiseyo (ErenaBattle)");

            DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() => {
                AlphaFadeManager.Instance.FadeIn(0.25f);
                NovelSingletone.Instance.PlayNovel("Chapter2-3 PreEvent", true, StartEventBattle);
            });
            return true;
        }
        return false;
    }

    public void StartEventBattle()
    {
        // scene transition
        string nextScene = "Battle";
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { asyncLoad.allowSceneActivation = true; });
    }
}