using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Assets.SimpleLocalization.Scripts;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// 警戒度
/// </summary>
public struct SideQuestData
{
    public int food;
    public int bank;
    public int research;

    public SideQuestData(int foodQuest, int bankQuest, int researchQuest)
    {
        food = foodQuest;
        bank = bankQuest;
        research = researchQuest;
    }
}

[RequireComponent(typeof(CanvasGroup))]
public class SideQuestPanel : MonoBehaviour
{
    [System.Serializable]
    // レベルデザイン用
    public struct SideQuestEnemy
    {
        [SerializeField] public EnemyDefine enemy;
        [SerializeField] public int alertLevel;
    }

    [Header("Setting")]
    [SerializeField] private List<SideQuestEnemy> chapter1Enemies;
    [SerializeField] private List<SideQuestEnemy> chapter2Enemies;
    [SerializeField] private List<SideQuestEnemy> chapter3Enemies;
    [SerializeField] private List<SideQuestEnemy> chapter4Enemies;
    [SerializeField] private List<SideQuestEnemy> chapter5Enemies;

    [SerializeField] private int[] enemyPerAlertLevel = new int[5];

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private TMP_Text rewardFood, rewardBank, rewardResearch;
    [SerializeField] private TMP_Text alertLevelFood, alertLevelBank, alertLevelResearch;
    [SerializeField] private Button foodSideQuestBtn;
    [SerializeField] private GameObject notEnoughItemSlotText;

    public void OpenSideQuestPanel()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGrp.DOFade(1.0f, 1.0f);
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        // Init
        rewardFood.text = LocalizationManager.Localize("System.RewardQuest") + ": " + LocalizationManager.Localize("Battle.Item");
        rewardBank.text = LocalizationManager.Localize("System.RewardQuest") + ": " + LocalizationManager.Localize("System.Money");
        rewardResearch.text = LocalizationManager.Localize("System.RewardQuest") + ": " + LocalizationManager.Localize("System.ResearchPoint");

        alertLevelFood.text = LocalizationManager.Localize("System.AlertLevel") + ": <color=yellow>";
        alertLevelBank.text = LocalizationManager.Localize("System.AlertLevel") + ": <color=yellow>";
        alertLevelResearch.text = LocalizationManager.Localize("System.AlertLevel") + ": <color=yellow>";

        // 警戒度を表す
        string star = LocalizationManager.Localize("System.Star");
        for (int i = 0; i < ProgressManager.Instance.GetSideQuestData().food; i++) alertLevelFood.text = alertLevelFood.text + star;
        for (int i = 0; i < ProgressManager.Instance.GetSideQuestData().bank; i++) alertLevelBank.text = alertLevelBank.text + star;
        for (int i = 0; i < ProgressManager.Instance.GetSideQuestData().research; i++) alertLevelResearch.text = alertLevelResearch.text + star;

        if (ProgressManager.Instance.GetInventorySlotLeft() < ProgressManager.Instance.GetSideQuestData().food)
        {
            // インベントリの空欄が足りない
            foodSideQuestBtn.interactable = false;
            notEnoughItemSlotText.SetActive(true);
        }
        else
        {
            foodSideQuestBtn.interactable = true;
            notEnoughItemSlotText.SetActive(false);
        }
    }

    public void CloseSideQuestPanel()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGrp.DOFade(0.0f, 1.0f);
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;
    }

    public void OnClickSideQuestFood()
    {
        // SE
        AudioManager.Instance.PauseMusic();
        AudioManager.Instance.PlaySFX("SystemAlert2");

        // 敵キャラを設置
        BattleSetup.Reset(false);
        BattleSetup.SetAllowEscape(true);
        GenerateEnemy(ProgressManager.Instance.GetSideQuestData().food);
        BattleSetup.SetBattleBGM("BattleTutorial");
        BattleSetup.SetSideQuestIncrement(1, -1, -1);
        BattleSetup.SetReward(Random.Range(1, 50), Random.Range(1, 10));
        CheckEquipmentDrop();
        
        BattleSetup.AddItemReward("食パン");
        if (ProgressManager.Instance.GetSideQuestData().food >= 2) BattleSetup.AddItemReward("クロワッサン");
        if (ProgressManager.Instance.GetSideQuestData().food >= 3) BattleSetup.AddItemReward("救急箱");
        if (ProgressManager.Instance.GetSideQuestData().food >= 4) BattleSetup.AddItemReward("食パン");
        if (ProgressManager.Instance.GetSideQuestData().food >= 5) BattleSetup.AddItemReward("救急箱");
        
        const float animationTime = 1.0f;

        // シーン遷移
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Battle", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        AlphaFadeManager.Instance.FadeOut(animationTime);

        DOTween.Sequence()
               .AppendInterval(animationTime)
               .AppendCallback(() => { asyncLoad.allowSceneActivation = true; });
    }

    public void OnClickSideQuestBank()
    {
        // SE
        AudioManager.Instance.PauseMusic();
        AudioManager.Instance.PlaySFX("SystemAlert2");
        
        // 敵キャラを設置
        BattleSetup.Reset(false);
        BattleSetup.SetBattleBack(BattleBack.CentreTower);
        BattleSetup.SetAllowEscape(true);
        GenerateEnemy(ProgressManager.Instance.GetSideQuestData().bank);
        BattleSetup.SetBattleBGM("BattleTutorial");
        BattleSetup.SetSideQuestIncrement(-1, 1, -1);
        BattleSetup.SetReward(Random.Range(300 + (75 * ProgressManager.Instance.GetSideQuestData().bank), 900 + (75 * ProgressManager.Instance.GetSideQuestData().bank)), Random.Range(5, 10));
        CheckEquipmentDrop();

        const float animationTime = 1.0f;

        // シーン遷移
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Battle", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        AlphaFadeManager.Instance.FadeOut(animationTime);

        DOTween.Sequence()
               .AppendInterval(animationTime)
               .AppendCallback(() => { asyncLoad.allowSceneActivation = true; });
    }

    public void OnClickSideQuestResearch()
    {
        // SE
        AudioManager.Instance.PauseMusic();
        AudioManager.Instance.PlaySFX("SystemAlert2");
        
        // 敵キャラを設置
        BattleSetup.Reset(false);
        BattleSetup.SetBattleBack(BattleBack.CentreTower);
        BattleSetup.SetAllowEscape(true);
        GenerateEnemy(ProgressManager.Instance.GetSideQuestData().research);
        BattleSetup.SetBattleBGM("BattleTutorial");
        BattleSetup.SetSideQuestIncrement(-1, -1, 1);
        BattleSetup.SetReward(Random.Range(50, 150), Random.Range(100 + (30 * ProgressManager.Instance.GetSideQuestData().research), 75 + (30 * ProgressManager.Instance.GetSideQuestData().research)));
        CheckEquipmentDrop();

        const float animationTime = 1.0f;

        // シーン遷移
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Battle", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        AlphaFadeManager.Instance.FadeOut(animationTime);

        DOTween.Sequence()
               .AppendInterval(animationTime)
               .AppendCallback(() => { asyncLoad.allowSceneActivation = true; });
    }

    private void CheckEquipmentDrop()
    {
        var alertPoint = ProgressManager.Instance.GetSideQuestData();
        if (alertPoint.food == 1 && alertPoint.bank == 1 && alertPoint.research == 1)
        {
            BattleSetup.AddEquipmentReward("Stick");
        }
        if (ProgressManager.Instance.GetCurrentStageProgress() >= 4) // chapter 2
        {
            BattleSetup.AddEquipmentReward("Cushion");
        }
        if (ProgressManager.Instance.GetCurrentStageProgress() >= 7) // chapter 3
        {
            BattleSetup.AddEquipmentReward("Collar");
        }
    }

    private List<SideQuestEnemy> GetEnemyList()
    {
        int currentStage = ProgressManager.Instance.GetCurrentStageProgress();
        int currentChapter = (((currentStage - 1) / 3) + 1);

        switch (currentChapter)
        {
            case 1:
                return chapter1Enemies;
            case 2:
                return chapter2Enemies;
            case 3:
                return chapter3Enemies;
            case 4:
                return chapter4Enemies;
            case 5:
                return chapter5Enemies;
            default:
                Debug.LogWarning("No enemy list available for chapter " + currentChapter.ToString());
                return chapter1Enemies;
        }
    }

    private void GenerateEnemy(int alertLevel)
    {
        var possibleEnemy = GetEnemyList().Where(x => x.alertLevel <= alertLevel).ToArray();
        int enemyNumber = Mathf.Clamp(enemyPerAlertLevel[alertLevel - 1] + Random.Range(-1, 2), 1, 5);

        var enemies = new List<EnemyDefine>();
        for (int i = 0; i < enemyNumber; i ++)
        {
            var random = new System.Random();
            int index = random.Next(possibleEnemy.Count());
            enemies.Add(possibleEnemy[index].enemy);
        }

        BattleSetup.SetEnemy(enemies);
    }
}
