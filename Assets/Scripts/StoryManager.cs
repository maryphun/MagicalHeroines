using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private bool isAfterBattle = false;

    [Header("References")]
    [SerializeField] private GameObject underdevelopment;

    // Start is called before the first frame update
    void Start()
    {
        // 画面遷移
        AlphaFadeManager.Instance.FadeIn(1.0f);

        if (isAfterBattle)
        {
            AfterStory();
        }
        else
        {
            Story();
        }
    }

    /// <summary>
    /// 戦闘前用
    /// </summary>
    void Story()
    {
        BattleSetup.Reset(true);
        BattleSetup.SetAllowEscape(true);

        if (DLCManager.isEnterDLCStage)
        {
            BattleSetup.SetBattleDLC(true);
            switch (ProgressManager.Instance.GetCurrentDLCStageProgress())
            {
                case 1:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/Prologue", true, GoToHomeScreen);
                        ProgressManager.Instance.DLCStageProgress();
                        HomeDialogue kei_dlc = Resources.Load<HomeDialogue>("HomeDialogue/Kei(DLC)");
                        ProgressManager.Instance.AddHomeCharacter(kei_dlc);

                        // 侵食記録開放【明穂＆クローン戦闘員】
                        ProgressManager.Instance.AddNewRecord("Record.AkihoDLC", "AkihoDLC");

                        AutoSave.ExecuteAutoSave();
                    }
                    break;

                case 2:
                    {
                        BattleSetup.AddEnemy("SeikakuSenki 5");
                        BattleSetup.AddEnemy("SeikakuSenki 6");
                        BattleSetup.SetBattleBGM("Loop 3 (Tutorial)");
                        BattleSetup.SetReward(1000, 200);
                        StartCoroutine(SceneTransition("Battle", 0));
                    }
                    break;

                case 3:
                    {
                        BattleSetup.AddEnemy("RogueTank");
                        BattleSetup.SetBattleBGM("Loop 12 (Battle2)");
                        BattleSetup.SetReward(1200, 200);
                        StartCoroutine(SceneTransition("Battle", 0));
                    }
                    break;

                case 4:
                    {
                        BattleSetup.AddEnemy("PrivateDrone");
                        BattleSetup.AddEnemy("PrivateDrone");
                        BattleSetup.AddEnemy("PrivateAndroid");
                        BattleSetup.AddEnemy("PrivateDrone");
                        BattleSetup.AddEnemy("PrivateDrone");
                        BattleSetup.SetBattleBGM("Loop 12 (Battle2)");
                        BattleSetup.SetReward(1400, 200);
                        StartCoroutine(SceneTransition("Battle", 0));
                    }
                    break;

                case 5:
                    {
                        BattleSetup.AddEnemy("Daiya_Enemy");
                        BattleSetup.SetBattleBGM("zensen he totugekiseyo (ErenaBattle)");
                        BattleSetup.SetBattleBack(BattleBack.Basement);
                        BattleSetup.SetReward(3000, 500);
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 2-0 PreBattle", true, GoToBattle);
                    }
                    break;

                case 6:
                    {
                        BattleSetup.AddEnemy("SeikakuSenki 7");
                        BattleSetup.AddEnemy("SeikakuSenki 8");
                        BattleSetup.SetBattleBGM("Loop (Battle3)");
                        BattleSetup.SetReward(1600, 200);
                        StartCoroutine(SceneTransition("Battle", 0));

                        // 侵食記録開放【明穂＆クローン戦闘員】
                        ProgressManager.Instance.AddNewRecord("Record.ErenaDLC", "ErenaDLC");
                    }
                    break;

                case 7:
                    {
                        BattleSetup.AddEnemy("PrivateAndroid");
                        BattleSetup.AddEnemy("RogueTank");
                        BattleSetup.AddEnemy("PrivateAndroid");
                        BattleSetup.SetBattleBGM("Loop (Battle3)");
                        BattleSetup.SetReward(1800, 400);
                        StartCoroutine(SceneTransition("Battle", 0));
                    }
                    break;

                case 8:
                    {
                        BattleSetup.AddEnemy("Hisui_Enemy_Final");
                        BattleSetup.AddEnemy("Kei_Enemy_Final");
                        BattleSetup.SetBattleBGM("apoptosis");
                        BattleSetup.SetBattleBack(BattleBack.CentreTower);
                        BattleSetup.SetReward(0, 10000);
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 2-3 PreBattle", true, GoToBattle);
                    }
                    break;

                default:
                    // 未開発地帯
                    underdevelopment.SetActive(true);
                    break;
            }
            return;
        }

        switch (ProgressManager.Instance.GetCurrentStageProgress())
        {
            case 2:
                {
                    BattleSetup.AddEnemy("Drone");
                    BattleSetup.AddEnemy("Android");
                    BattleSetup.SetBattleBGM("BattleTutorial");
                    BattleSetup.SetReward(350, 25);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 3:
                {
                    BattleSetup.AddEnemy("Akiho_Enemy");
                    BattleSetup.SetBattleBGM("AkihoBattle");
                    BattleSetup.SetReward(1250, 150);
                    BattleSetup.SetBattleBack(BattleBack.Basement);
                    NovelSingletone.Instance.PlayNovel("Chapter1-2 Prebattle", true, GoToBattle);
                }
                break;
            case 4:
                {
                    BattleSetup.AddEnemy("Drone");
                    BattleSetup.AddEnemy("GoldAndroid");
                    BattleSetup.SetBattleBGM("AkihoBattle");
                    BattleSetup.SetReward(400, 50);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 5:
                {
                    BattleSetup.AddEnemy("GoldAndroid");
                    BattleSetup.AddEnemy("GoldAndroid 2");
                    BattleSetup.SetBattleBGM("AkihoBattle");
                    BattleSetup.SetReward(450, 50);
                    BattleSetup.AddEquipmentReward("NiceTshirt");
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 6:
                {
                    BattleSetup.AddEnemy("Rikka_Enemy");
                    BattleSetup.SetBattleBGM("Loop 37 (RikkaBattle)");
                    BattleSetup.SetReward(1500, 300);
                    BattleSetup.SetBattleBack(BattleBack.Basement);
                    NovelSingletone.Instance.PlayNovel("Chapter2-3 Prebattle", true, GoToBattle);
                }
                break;
            case 7:
                {
                    BattleSetup.AddEnemy("Tank");
                    BattleSetup.SetBattleBGM("Loop 12 (Battle2)");
                    BattleSetup.SetReward(500, 75);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 8:
                {
                    BattleSetup.AddEnemy("GoldDrone");
                    BattleSetup.AddEnemy("DarkAndroid");
                    BattleSetup.AddEnemy("GoldDrone");
                    BattleSetup.SetBattleBGM("Loop 12 (Battle2)");
                    BattleSetup.AddEquipmentReward("Glove");
                    BattleSetup.SetReward(550, 75);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 9:
                {
                    BattleSetup.AddEnemy("Erena_Enemy");
                    BattleSetup.SetBattleBGM("zensen he totugekiseyo (ErenaBattle)");
                    BattleSetup.SetReward(1750, 300);
                    BattleSetup.SetBattleBack(BattleBack.Council);
                    NovelSingletone.Instance.PlayNovel("Chapter3-3 Prebattle", true, GoToBattle);
                }
                break;
            case 10:
                {
                    BattleSetup.AddEnemy("DarkTank");
                    BattleSetup.SetBattleBGM("Loop (Battle3)");
                    BattleSetup.SetReward(600, 100);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 11:
                {
                    BattleSetup.AddEnemy("Drone 4");
                    BattleSetup.AddEnemy("DarkAndroid");
                    BattleSetup.AddEnemy("DarkAndroid 2");
                    BattleSetup.AddEnemy("Drone 4");
                    BattleSetup.SetBattleBGM("Loop (Battle3)");
                    BattleSetup.AddEquipmentReward("Shoes");
                    BattleSetup.SetReward(600, 100);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 12:
                {
                    BattleSetup.AddEnemy("Kei_Enemy");
                    BattleSetup.SetBattleBGM("Mystic Edge (KeiBattle)");
                    BattleSetup.SetBattleBack(BattleBack.CentreTower);
                    NovelSingletone.Instance.PlayNovel("Chapter4-3 Prebattle", true, GoToBattle);
                }
                break;
            case 13:
                {
                    // 戦闘なし
                    BattleSetup.SetReward(2000, 300);
                    NovelSingletone.Instance.PlayNovel("Chapter5-1", true, GoToRewardScreen);
                    ProgressManager.Instance.StageProgress();

                    PlayerCharacterDefine Kei = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/7.Kei");
                    ProgressManager.Instance.AddPlayerCharacter(Kei);
                }
                break;
            case 14:
                {
                    BattleSetup.AddEnemy("GoldDrone");
                    BattleSetup.AddEnemy("DarkAndroid");
                    BattleSetup.AddEnemy("GoldDrone");
                    BattleSetup.SetBattleBGM("Loop 12 (Battle2)");
                    BattleSetup.SetReward(650, 120);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 15:
                {
                    BattleSetup.AddEnemy("Nayuta_Enemy");
                    BattleSetup.SetBattleBGM("apoptosis");
                    BattleSetup.SetBattleBack(BattleBack.CentreTower);
                    BattleSetup.SetReward(2000, 400);
                    NovelSingletone.Instance.PlayNovel("Chapter5-3 Prebattle", true, GoToBattle);
                }
                break;
            case 16:
                {
                    BattleSetup.AddEnemy("SeikakuSenki 1");
                    BattleSetup.AddEnemy("SeikakuSenki 2");
                    BattleSetup.AddEnemy("SeikakuSenki 3");
                    BattleSetup.AddEnemy("SeikakuSenki 4");
                    BattleSetup.SetBattleBGM("finalbattle");
                    BattleSetup.SetBattleBack(BattleBack.Council);
                    NovelSingletone.Instance.PlayNovel("ChapterFinal Prebattle", true, GoToBattle);
                }
                break;
            case 17: // エンドゲームコンテンツ　1
                {
                    BattleSetup.AddEnemy("SeikakuSenki 1");
                    BattleSetup.SetBattleBGM("Loop 12 (Battle2)");
                    BattleSetup.SetReward(1000, 250);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 18: // エンドゲームコンテンツ　2
                {
                    BattleSetup.AddEnemy("Akiho_Enemy");
                    BattleSetup.AddEnemy("Rikka_Enemy");
                    BattleSetup.SetBattleBGM("AkihoBattle");
                    BattleSetup.SetReward(1000, 250);
                    BattleSetup.SetBattleBack(BattleBack.Basement);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 19: // エンドゲームコンテンツ　3
                {
                    BattleSetup.AddEnemy("Kei_Enemy 2");
                    BattleSetup.SetBattleBGM("Mystic Edge (KeiBattle)");
                    BattleSetup.SetBattleBack(BattleBack.CentreTower);
                    BattleSetup.SetReward(1000, 250);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            case 20: // エンドゲームコンテンツ　4
                {
                    BattleSetup.AddEnemy("Erena_Enemy");
                    BattleSetup.AddEnemy("Nayuta_Enemy");
                    BattleSetup.SetBattleBGM("apoptosis");
                    BattleSetup.SetReward(1000, 250);
                    StartCoroutine(SceneTransition("Battle", 0));
                }
                break;
            default:
                // 未開発地帯
                underdevelopment.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 戦闘後用
    /// </summary>
    void AfterStory()
    {
        if (DLCManager.isEnterDLCStage)
        {
            switch (ProgressManager.Instance.GetCurrentDLCStageProgress())
            {
                case 1:
                    {

                    }
                    break;
                case 2:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 1-1", true, GoToRewardScreen);
                    }
                    break;
                case 3:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 1-2", true, GoToRewardScreen);

                        // ホーム画面にヒスイを登場させる
                        HomeDialogue hisui = Resources.Load<HomeDialogue>("HomeDialogue/Hisui");
                        ProgressManager.Instance.AddHomeCharacter(hisui);
                        ProgressManager.Instance.RemoveHomeCharacter("Kei(DLC)");

                        AutoSave.ExecuteAutoSave();
                    }
                    break;
                case 4:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 1-3", true, GoToRewardScreen);

                        // ホーム画面にヒスイを登場させる
                        HomeDialogue kei_dlc = Resources.Load<HomeDialogue>("HomeDialogue/Kei(DLC)");
                        ProgressManager.Instance.AddHomeCharacter(kei_dlc);
                        ProgressManager.Instance.RemoveHomeCharacter("Hisui");
                    }
                    break;
                case 5:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 2-0 AfterBattle", true, GoToRewardScreen);

                        PlayerCharacterDefine Daiya = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/13.Daiya");
                        ProgressManager.Instance.AddPlayerCharacter(Daiya);
                    }
                    break;
                case 6:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 2-1", true, GoToRewardScreen);
                    }
                    break;
                case 7:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 2-2 PreBattle", true, GoToBattle);

                        // setup event battle
                        BattleSetup.Reset(false);
                        BattleSetup.SetBattleDLC(true);
                        BattleSetup.SetAllowEscape(false);
                        BattleSetup.SetEventBattle(true);
                        BattleSetup.SetReward(0, 0);
                        BattleSetup.AddTeammate("8.Nayuta");
                        BattleSetup.SetBattleBack(BattleBack.Default);
                        BattleSetup.AddEnemy("Hisui_Enemy");
                        BattleSetup.SetBattleBGM("zensen he totugekiseyo (ErenaBattle)");
                    }
                    break;
                case 8:
                    {
                        NovelSingletone.Instance.PlayNovel("DLC/DLC 2-3 AfterBattle", true, GoToRewardScreen);

                        // 京のホームキャラはこれで終了
                        ProgressManager.Instance.RemoveHomeCharacter("Kei(DLC)");

                        // キャラ追加
                        PlayerCharacterDefine Hisui = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/12.Hisui");
                        ProgressManager.Instance.AddPlayerCharacter(Hisui);
                        PlayerCharacterDefine K22 = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/14.K22");
                        ProgressManager.Instance.AddPlayerCharacter(K22);
                    }
                    break;
                default:
                    // 未開発地帯 WARNING (もうここにこないはず)
                    underdevelopment.SetActive(true);
                    break;
            }
            ProgressManager.Instance.DLCStageProgress();
            return;
        }

        switch (ProgressManager.Instance.GetCurrentStageProgress())
        {
            case 2:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter1-1", true, GoToRewardScreen);
                }
                break;
            case 3:
                {
                    // 明穂敗北
                    NovelSingletone.Instance.PlayNovel("Chapter1-2 AfterBattle", true, GoToRewardScreen);

                    PlayerCharacterDefine Akiho = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/4.Akiho");
                    ProgressManager.Instance.AddPlayerCharacter(Akiho);
                }
                break;
            case 4:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter2-1", true, GoToRewardScreen);
                }
                break;
            case 5:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter2-2", true, GoToRewardScreen);
                }
                break;
            case 6:
                {
                    // 立花敗北
                    NovelSingletone.Instance.PlayNovel("Chapter2-3 AfterBattle", true, GoToRewardScreen);

                    PlayerCharacterDefine Rikka = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/5.Rikka");
                    ProgressManager.Instance.AddPlayerCharacter(Rikka);

                    // 侵食記録開放【爆乳怪人明穂の淫靡な日常】
                    ProgressManager.Instance.AddNewRecord("Record.AkihoPaizuri", "AkihoPaizuri");
                }
                break;
            case 7:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter3-1", true, GoToRewardScreen);
                }
                break;
            case 8:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter3-2", true, GoToRewardScreen);
                }
                break;
            case 9:
                {
                    // エレナ敗北
                    NovelSingletone.Instance.PlayNovel("Chapter3-3 AfterBattle", true, GoToRewardScreen);

                    PlayerCharacterDefine Erena = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/6.Erena");
                    ProgressManager.Instance.AddPlayerCharacter(Erena);

                    // 侵食記録開放【クローンの通常業務】
                    ProgressManager.Instance.AddNewRecord("Record.CloneTask", "CloneTask");
                }
                break;
            case 10:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter4-1", true, GoToRewardScreen);
                }
                break;
            case 11:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter4-2", true, GoToRewardScreen);
                }
                break;
            case 12:
                {
                    // 京敗北
                    NovelSingletone.Instance.PlayNovel("Chapter4-3 AfterBattle", true, GoToBattle);

                    // setup event battle
                    BattleSetup.Reset(false);
                    BattleSetup.SetAllowEscape(false);
                    BattleSetup.SetEventBattle(true);
                    BattleSetup.SetReward(0, 0);
                    BattleSetup.AddTeammate("10.No5(Event)");
                    BattleSetup.AddTeammate("11.No72(Event)");
                    BattleSetup.SetBattleBack(BattleBack.CentreTower);
                    BattleSetup.AddEnemy("Nayuta_Enemy");
                    BattleSetup.SetBattleBGM("zensen he totugekiseyo (ErenaBattle)");
                }
                break;
            case 13:
                {
                    // 5-1は戦闘がないのでここに辿らない
                }
                break;
            case 14:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter5-2", true, GoToRewardScreen);
                }
                break;
            case 15:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter5-3 AfterBattle", true, GoToRewardScreen);

                    // 那由多捕獲
                    PlayerCharacterDefine Nayuta = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/8.Nayuta");
                    ProgressManager.Instance.AddPlayerCharacter(Nayuta);
                }
                break;
            case 16:
                {
                    NovelSingletone.Instance.PlayNovel("ChapterFinal AfterBattle", true, GoToEndCredit);
                }
                break;
            case 17: // エンドゲームコンテンツ　1
                {
                    // 侵食記録開放【苗床へ堕ちる聖核戦姫】
                    ProgressManager.Instance.AddNewRecord("Record.Nursery", "Nursery");

                    GoToRewardScreen();
                }
                break;
            case 18: // エンドゲームコンテンツ　2
                {
                    // 侵食記録開放【最後の二人の聖核戦姫】
                    ProgressManager.Instance.AddNewRecord("Record.Final", "Final");

                    GoToRewardScreen();
                }
                break;
            case 19: // エンドゲームコンテンツ　3
                {
                    // 侵食記録開放【小悪魔の日常】
                    ProgressManager.Instance.AddNewRecord("Record.Kei", "Kei");

                    GoToRewardScreen();
                }
                break;
            case 20: // エンドゲームコンテンツ　4
                {
                    // 侵食記録開放【最強の二人】
                    ProgressManager.Instance.AddNewRecord("Record.Erenayuta", "Erenayuta");

                    GoToRewardScreen();
                }
                break;
            default:
                // 未開発地帯 WARNING (もうここにこないはず)
                underdevelopment.SetActive(true);
                break;
        }
        ProgressManager.Instance.StageProgress();
    }

    public void GoToBattle()
    {
        StartCoroutine(SceneTransition("Battle", 0));
    }

    public void GoToHomeScreen()
    {
        StartCoroutine(SceneTransition("Home", 0.5f));
    }

    public void GoToRewardScreen()
    {
        StartCoroutine(SceneTransition("Reward", 0.5f));
    }

    public void GoToEndCredit()
    {
        StartCoroutine(SceneTransition("EndCredit", 0.5f));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        if (animationTime > 0)
        {
            yield return new WaitForSeconds(animationTime);
        }
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 
        asyncLoad.allowSceneActivation = true;
    }
}
