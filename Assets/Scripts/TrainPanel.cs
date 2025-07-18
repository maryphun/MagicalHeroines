using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(CanvasGroup))]
public class TrainPanel : MonoBehaviour
{
    // �V�i���I���
    private enum ScenarioType
    {
        Horny, // ������
        Corruption, // �ŗ���
        CoreResearch, // ���j����
    }

    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;
    [SerializeField] EquipmentDefine[] seikakuEquip = new EquipmentDefine[5];

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image characterImg;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text currentMood;
    [SerializeField] private Image darkGaugeFill, holyCoreGaugeFill;
    [SerializeField] private RectTransform previousCharacterBtn, nextCharacterBtn;
    [SerializeField] private Button hornyActionBtn, corruptActionBtn, coreBtn;
    [SerializeField] private TMP_Text hornyActionCost, corruptActionCost, coreCost;
    [SerializeField] private GameObject unavailablePanel;
    [SerializeField] private CanvasGroup underDevelopmentPopUp;
    [SerializeField] private CanvasGroup newBattlerPopup;
    [SerializeField] private TMPro.TMP_Text newBattlerPopupText;
    [SerializeField] private CanvasGroup newCoreEquipmentPopup;
    [SerializeField] private TMPro.TMP_Text newCoreEquipmentText;
    [SerializeField] private Image newCoreEquipmentIcon;
    [SerializeField] private CanvasGroup researchPointPanel;
    [SerializeField] private HomeCharacter homeCharacterScript;
    [SerializeField] private GameObject corruptionGauge, coreResearchGauge;

    [Header("Debug")]
    [SerializeField] private Vector2 previousCharacterBtnPos, nextCharacterBtnPos;
    [SerializeField] private List<Character> characters;
    [SerializeField] private int currentIndex;
    [SerializeField] private int[] cost = new int[3];
    [SerializeField] private ScenarioType currentScenarioType;

    [Header("�����V�[�����e�Ǘ�")]
    // �ŗ����V�[��
    [SerializeField] public Dictionary<int, List<string>> CharacterID_To_HornyNovelNameList = new Dictionary<int, List<string>>();     // ������
    [SerializeField] public Dictionary<int, List<string>> CharacterID_To_BrainwashNovelNameList = new Dictionary<int, List<string>>(); // ���]
    [SerializeField] public Dictionary<int, List<string>> CharacterID_To_CoreNovelNameList = new Dictionary<int, List<string>>(); // ���j����

    void InitList()
    {
        // �������V�i���I���X�g
        CharacterID_To_HornyNovelNameList.Add(3, new List<string> { "Akiho/A_Horny_1", "Akiho/A_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(4, new List<string> { "Rikka/R_Horny_1", "Rikka/R_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(5, new List<string> { "Erena/E_Horny_1", "Erena/E_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(6, new List<string> { "Kei/K_Horny_1", "Kei/K_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(7, new List<string> { "Nayuta/N_Horny_1", "Nayuta/N_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(11, new List<string> { "DLC/H_Horny_1", "DLC/H_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(12, new List<string> { "DLC/D_Horny_1", "DLC/D_Horny_2" });
        CharacterID_To_HornyNovelNameList.Add(13, new List<string> { });
        // ���]�V�i���I���X�g
        CharacterID_To_BrainwashNovelNameList.Add(3, new List<string> { "Akiho/A_BrainWash_1", "Akiho/A_BrainWash_2", "Akiho/A_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(4, new List<string> { "Rikka/R_BrainWash_1", "Rikka/R_BrainWash_2", "Rikka/R_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(5, new List<string> { "Erena/E_BrainWash_1", "Erena/E_BrainWash_2", "Erena/E_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(6, new List<string> { "Kei/K_BrainWash_1", "Kei/K_BrainWash_2", "Kei/K_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(7, new List<string> { "Nayuta/N_BrainWash_1", "Nayuta/N_BrainWash_2", "Nayuta/N_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(11, new List<string> { "DLC/H_BrainWash_1", "DLC/H_BrainWash_2", "DLC/H_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(12, new List<string> { "DLC/D_BrainWash_1", "DLC/D_BrainWash_2", "DLC/D_BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(13, new List<string> { "DLC/Kei_DLC_BrainWash_1", "DLC/Kei_DLC_BrainWash_2" });
        // ���j�����V�i���I���X�g
        CharacterID_To_CoreNovelNameList.Add(3, new List<string> { "Akiho/A_Core_1", "Akiho/A_Core_2" });
        CharacterID_To_CoreNovelNameList.Add(4, new List<string> { "Rikka/R_Core_1", "Rikka/R_Core_2" });
        CharacterID_To_CoreNovelNameList.Add(5, new List<string> { "Erena/E_Core_1", "Erena/E_Core_2" });
        CharacterID_To_CoreNovelNameList.Add(6, new List<string> { "Kei/K_Core_1", "Kei/K_Core_2" });
        CharacterID_To_CoreNovelNameList.Add(7, new List<string> { "Nayuta/N_Core_1", "Nayuta/N_Core_2" });
        CharacterID_To_CoreNovelNameList.Add(11, new List<string> { "DLC/H_Core_1" });
        CharacterID_To_CoreNovelNameList.Add(12, new List<string> { "DLC/D_Core_1" });
        CharacterID_To_CoreNovelNameList.Add(13, new List<string> { });
    }

    private void Awake()
    {
        InitList();
    }

    public void OpenTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        // UI �t�F�C�h
        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // ������
        previousCharacterBtn.gameObject.SetActive(true);
        nextCharacterBtn.gameObject.SetActive(true);
        previousCharacterBtnPos = previousCharacterBtn.localPosition;
        nextCharacterBtnPos = nextCharacterBtn.localPosition;

        // �f�[�^�����[�h
        characters = ProgressManager.Instance.GetAllCharacter(false, true);

        // �q���C������Ȃ��L������r��
        currentIndex = PlayerPrefs.GetInt("TrainPanelIndex", 0);
        characters.RemoveAll(s => !s.characterData.is_heroin);

        if (characters.Count <= 0)
        {
            // �ߊl�����q���C�������Ȃ�
            unavailablePanel.SetActive(true);
            return;
        }
        else
        {
            characterImg.gameObject.SetActive(true);

            if (characters.Count == 1)
            {
                // 1�l�������Ȃ�
                previousCharacterBtn.gameObject.SetActive(false);
                nextCharacterBtn.gameObject.SetActive(false);
            }
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, characters.Count-1);
        UpdateCharacterData();
    }

    public void QuitTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        // ��ʋL�^
        PlayerPrefs.SetInt("TrainPanelIndex", currentIndex);

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // �f�[�^���N���A
        characters.Clear();
        characters = null;
    }

    public void NextCharacter()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemSwitch");

        // UI Animation
        const float animTime = 0.2f;
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                nextCharacterBtn.DOLocalMoveX(nextCharacterBtnPos.x + 10.0f, animTime * 0.5f);
            })
            .AppendInterval(animTime * 0.5f)
            .AppendCallback(() =>
            {
                nextCharacterBtn.DOLocalMoveX(nextCharacterBtnPos.x, animTime * 0.5f);
            });

        // Calculate Index
        currentIndex++;
        if (currentIndex >= characters.Count)
        {
            currentIndex = 0;
        }

        // Change character
        UpdateCharacterData();
    }

    public void PreviousCharacter()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemSwitch");

        // UI Animation
        const float animTime = 0.2f;
        DOTween.Sequence()
            .AppendCallback(() => 
            { 
                previousCharacterBtn.DOLocalMoveX(previousCharacterBtnPos.x - 10.0f, animTime * 0.5f); 
            })
            .AppendInterval(animTime * 0.5f)
            .AppendCallback(() => 
            {
                previousCharacterBtn.DOLocalMoveX(previousCharacterBtnPos.x, animTime * 0.5f);
            });

        // Calculate Index
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = characters.Count-1;
        }

        // Change character
        UpdateCharacterData();
    }

    public void UpdateCharacterData()
    {
        // ���O
        characterName.text = characters[currentIndex].localizedName;

        // �����𖞂����Ă���u�S��v
        var characterStatus = characters[currentIndex].GetCurrentStatus();
        currentMood.text = LocalizationManager.Localize(characterStatus.moodNameID);
        characterImg.sprite = characterStatus.character;

        darkGaugeFill.fillAmount = ((float)characters[currentIndex].corruptionEpisode) / (float)CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID].Count;
        holyCoreGaugeFill.fillAmount = ((float)characters[currentIndex].holyCoreEpisode) / (float)CharacterID_To_CoreNovelNameList[characters[currentIndex].characterData.characterID].Count;

        // �{�^�����X�V (Has next scenario?)
        hornyActionBtn.interactable = characters[currentIndex].hornyEpisode < CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID].Count;
        corruptActionBtn.interactable = characters[currentIndex].corruptionEpisode < CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID].Count;
        coreBtn.interactable = characters[currentIndex].holyCoreEpisode < CharacterID_To_CoreNovelNameList[characters[currentIndex].characterData.characterID].Count;

        // ������
        if (!hornyActionBtn.interactable)
        {
            // (��������)
            hornyActionCost.text = "<color=#d400ff><size=25>" + LocalizationManager.Localize("System.ResearchComplete");
        }
        else
        {
            cost[0] = (characters[currentIndex].hornyEpisode * 15) + 50;
            hornyActionCost.text = LocalizationManager.Localize("System.ResearchPointCost") + "\n<size=25><color=#ed94ff>" + cost[0];

            // �|�C���g�s��
            if (ProgressManager.Instance.GetCurrentResearchPoint() < cost[0]) hornyActionBtn.interactable = false;
        }

        // �ŗ���
        if (!corruptActionBtn.interactable)
        {
            // (��������)
            corruptActionCost.text = "<color=#d400ff><size=25>" + LocalizationManager.Localize("System.ResearchComplete");
        }
        else
        {
            cost[1] = (characters[currentIndex].corruptionEpisode * 10) + 50;
            corruptActionCost.text = LocalizationManager.Localize("System.ResearchPointCost") + "\n<size=25><color=#ed94ff>" + cost[1];

            // �|�C���g�s��
            if (ProgressManager.Instance.GetCurrentResearchPoint() < cost[1]) corruptActionBtn.interactable = false;
        }

        // ���j����
        if (!coreBtn.interactable)
        {
            // (��������)
            coreCost.text = "<color=#d400ff><size=25>" + LocalizationManager.Localize("System.ResearchComplete");
        }
        else
        {
            cost[2] = (characters[currentIndex].holyCoreEpisode * 60) + 60;
            coreCost.text = LocalizationManager.Localize("System.ResearchPointCost") + "\n<size=25><color=#ed94ff>" + cost[2];

            // �|�C���g�s��
            if (ProgressManager.Instance.GetCurrentResearchPoint() < cost[2]) coreBtn.interactable = false;
        }

        // DLC���ꏈ��
        if (DLCManager.isDLCEnabled)
        {
            if (characters[currentIndex].characterData.characterID == 12) // �_�C��
            {
                if (corruptActionBtn.interactable)
                {
                    // �ŗ����V�i���I�܂��I����ĂȂ�
                    hornyActionBtn.interactable = false;
                    coreBtn.interactable = false;

                    hornyActionCost.text = LocalizationManager.Localize("System.ResearchConditionDaiya");
                    coreCost.text = LocalizationManager.Localize("System.ResearchConditionDaiya");
                }
            }
            
            if (characters[currentIndex].characterData.characterID == 13) // K22
            {
                // K22�̏ꍇ����UI���ς��
                hornyActionBtn.gameObject.SetActive(false);
                coreBtn.gameObject.SetActive(false);

                corruptionGauge.gameObject.SetActive(false);
                coreResearchGauge.gameObject.SetActive(false);
            }
            else
            {
                hornyActionBtn.gameObject.SetActive(true);
                coreBtn.gameObject.SetActive(true);

                corruptionGauge.gameObject.SetActive(true);
                coreResearchGauge.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void HornyTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");
        homeCharacterScript.StopVoice();

        // BGM ��~
        AudioManager.Instance.PauseMusic();

        List<string> episodeList = CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID];
        currentScenarioType = ScenarioType.Horny;

        // �V�i���I�Đ�
        canvasGroup.interactable = false;

        // �|�C���g����
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() - cost[0]);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() => 
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);

            string novelpath = episodeList[characters[currentIndex].hornyEpisode];
            // not dlc characters
            if (!characters[currentIndex].characterData.isDLCCharacter)
            {
                novelpath = "TrainScene/" + novelpath;
            }
            NovelSingletone.Instance.PlayNovel(novelpath, true, ReturnFromEpisode);
            characters[currentIndex].hornyEpisode++;
        });
    }

    /// <summary>
    /// ���]����
    /// </summary>
    public void BrainwashTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");
        homeCharacterScript.StopVoice();

        // BGM ��~
        AudioManager.Instance.PauseMusic();

        List<string> episodeList = CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID];
        currentScenarioType = ScenarioType.Corruption;

        // �V�i���I�Đ�
        AlphaFadeManager.Instance.FadeOut(1.0f);

        // �|�C���g����
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() - cost[1]);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() =>
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);

            string novelpath = episodeList[characters[currentIndex].corruptionEpisode];
            // not dlc characters
            if (!characters[currentIndex].characterData.isDLCCharacter)
            {
                novelpath = "TrainScene/" + novelpath;
            }
            NovelSingletone.Instance.PlayNovel(novelpath, true, ReturnFromEpisode);
            characters[currentIndex].corruptionEpisode++;
        });

        // �L�����l��?
        if (characters[currentIndex].corruptionEpisode >= episodeList.Count - 1)
        {
            // �ŗ���
            characters[currentIndex].is_corrupted = true;

            // DLC�d�l
            if (DLCManager.isDLCEnabled)
            {
                if ( characters[currentIndex].characterData.characterID == 13 )// K22
                {
                    characters[currentIndex].is_corrupted = false;
                }
            }
        }
    }

    /// <summary>
    /// ���j����
    /// </summary>
    public void HolyCoreResearch()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");
        homeCharacterScript.StopVoice();

        // BGM ��~
        AudioManager.Instance.PauseMusic();

        List<string> episodeList = CharacterID_To_CoreNovelNameList[characters[currentIndex].characterData.characterID];
        currentScenarioType = ScenarioType.CoreResearch;

        // �V�i���I�Đ�
        AlphaFadeManager.Instance.FadeOut(1.0f);

        // �|�C���g����
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() - cost[2]);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() =>
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);

            string novelpath = episodeList[characters[currentIndex].holyCoreEpisode];
            // not dlc characters
            if (!characters[currentIndex].characterData.isDLCCharacter)
            {
                novelpath = "TrainScene/" + novelpath;
            }
            NovelSingletone.Instance.PlayNovel(novelpath, true, ReturnFromEpisode);
            characters[currentIndex].holyCoreEpisode++;
        });
    }

    private void ReturnFromEpisode()
    {
        switch (currentScenarioType)
        {
            case ScenarioType.Horny:
                // ������
                if (characters[currentIndex].hornyEpisode >= CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID].Count)
                {
                    // ����Z�l��
                    CallNewSpecialAbilityPopUp();
                }
                break;
            case ScenarioType.Corruption:
                // update heroin data
                if (characters[currentIndex].is_corrupted)
                {
                    CallNewBattlerPopUp();
                    AddNewHomeCharacter(characters[currentIndex].characterData.characterID);

                    // update character name
                    characters[currentIndex].localizedName = LocalizationManager.Localize(characters[currentIndex].characterData.corruptedName);
                }
                break;
            case ScenarioType.CoreResearch:
                // ������
                if (characters[currentIndex].holyCoreEpisode >= CharacterID_To_CoreNovelNameList[characters[currentIndex].characterData.characterID].Count)
                {
                    // ���j�����l��
                    EquipmentDefine newEquipment = characters[currentIndex].characterData.coreEquipment;
                    ProgressManager.Instance.AddNewEquipment(newEquipment);
                    CallNewCoreEquipmentPopup(newEquipment);
                }
                break;
            default:
                break;
        }

        canvasGroup.interactable = true;
        UpdateCharacterData();

        // Play home music
        AudioManager.Instance.PlayMusicWithFade("Loop 32 (HomeScene)", 4.0f);

        // �I�[�g�Z�[�u�����s����
        AutoSave.ExecuteAutoSave();
    }

    public void CloseUnderDevelopmentPopup()
    {
        underDevelopmentPopUp.DOKill(false);
        underDevelopmentPopUp.DOFade(0.0f, 0.1f).OnComplete(() =>
        { 
            underDevelopmentPopUp.interactable = false;
            underDevelopmentPopUp.blocksRaycasts = false;
        });
    }

    public void CallNewSpecialAbilityPopUp()
    {
        // SE
        AudioManager.Instance.PlaySFX("NewAbility");

        // Update Text
        newBattlerPopupText.text = HornynessMessage(characters[currentIndex].characterData.characterID);

        // UI
        newBattlerPopup.DOKill(false);
        newBattlerPopup.DOFade(1.0f, 0.5f);
        newBattlerPopup.interactable = true;
        newBattlerPopup.blocksRaycasts = true;
    }
    public void CallNewBattlerPopUp()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemNewHeroin");

        // Update Text
        newBattlerPopupText.text = CorruptedMessage(characters[currentIndex].characterData.characterID);

        // UI
        newBattlerPopup.DOKill(false);
        newBattlerPopup.DOFade(1.0f, 0.5f);
        newBattlerPopup.interactable = true;
        newBattlerPopup.blocksRaycasts = true;
    }
    public void CloseNewBattlerPopup()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemButton");

        newBattlerPopup.DOKill(false);
        newBattlerPopup.DOFade(0.0f, 0.1f).OnComplete(() =>
        {
            newBattlerPopup.interactable = false;
            newBattlerPopup.blocksRaycasts = false;
        });
    }

    public void CallNewCoreEquipmentPopup(EquipmentDefine newEquipment)
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemButton");

        // Update Text
        newCoreEquipmentText.text = CoreEquipmentMessage(characters[currentIndex].characterData.characterID, newEquipment);
        newCoreEquipmentIcon.sprite = newEquipment.Icon;

        // UI
        newCoreEquipmentPopup.DOKill(false);
        newCoreEquipmentPopup.DOFade(1.0f, 0.5f);
        newCoreEquipmentPopup.interactable = true;
        newCoreEquipmentPopup.blocksRaycasts = true;

        // Check Record
        if (   ProgressManager.Instance.PlayerHasEquipment(seikakuEquip[0])
            && ProgressManager.Instance.PlayerHasEquipment(seikakuEquip[1])
            && ProgressManager.Instance.PlayerHasEquipment(seikakuEquip[2])
            && ProgressManager.Instance.PlayerHasEquipment(seikakuEquip[3])
            && ProgressManager.Instance.PlayerHasEquipment(seikakuEquip[4]))
        {
            ProgressManager.Instance.AddNewRecord("Record.SeikakuEquip", "SeikakuEquip");
        }
    }
    public void CloseNewCoreEquipmentPopup()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemEquip");

        newCoreEquipmentPopup.DOKill(false);
        newCoreEquipmentPopup.DOFade(0.0f, 0.1f).OnComplete(() =>
        {
            newCoreEquipmentPopup.interactable = false;
            newCoreEquipmentPopup.blocksRaycasts = false;
        });
    }

    /// <summary>
    /// �������V�i���I�@����Z�l�����b�Z�[�W
    /// </summary>
    public string HornynessMessage(int characterID)
    {
        string s = string.Empty;
        switch ((PlayerCharacerID)characterID)
        {
            case PlayerCharacerID.Akiho: // ����
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Akiho"), CustomColor.akiho());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            case PlayerCharacerID.Rikka: // ����
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Rikka"), CustomColor.rikka());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            case PlayerCharacerID.Erena: // �G���i
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Erena"), CustomColor.erena());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            case PlayerCharacerID.Kei: // ��
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Kei"), CustomColor.kei());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            case PlayerCharacerID.Nayuta: // �ߗR��
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Nayuta"), CustomColor.nayuta());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            case PlayerCharacerID.Hisui: // �q�X�C
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Hisui"), CustomColor.hisui());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            case PlayerCharacerID.Daiya: // �_�C��
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Diamond"), CustomColor.daiya());
                return LocalizationManager.Localize("System.NewAbilityTraining").Replace("{s}", s);
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// �ŗ��������̃V�X�e�����b�Z�[�W
    /// </summary>
    public string CorruptedMessage(int characterID)
    {
        string s = string.Empty;
        switch ((PlayerCharacerID)characterID)
        {
            case PlayerCharacerID.Akiho: // ����
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Akiho"), CustomColor.akiho());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Rikka: // ����
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Rikka"), CustomColor.rikka());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Erena: // �G���i
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Erena"), CustomColor.erena());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Kei: // ��
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Kei"), CustomColor.kei());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Nayuta: // �ߗR��
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Nayuta"), CustomColor.nayuta());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Hisui: // �q�X�C
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Hisui"), CustomColor.hisui());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Daiya: // �_�C��
                s = CustomColor.AddColor(LocalizationManager.Localize("Name.Diamond"), CustomColor.daiya());
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// ���j�����l���V�X�e�����b�Z�[�W
    /// </summary>
    public string CoreEquipmentMessage(int characterID, EquipmentDefine equipment)
    {
        string s = string.Empty;
        switch ((PlayerCharacerID)characterID)
        {
            case PlayerCharacerID.Akiho: // ����
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.akiho());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            case PlayerCharacerID.Rikka: // ����
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.rikka());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            case PlayerCharacerID.Erena: // �G���i
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.erena());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            case PlayerCharacerID.Kei: // ��
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.kei());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            case PlayerCharacerID.Nayuta: // �ߗR��
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.nayuta());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            case PlayerCharacerID.Hisui: // �q�X�C
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.hisui());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            case PlayerCharacerID.Daiya: // �_�C��
                s = CustomColor.AddColor(LocalizationManager.Localize(equipment.equipNameID), CustomColor.daiya());
                return LocalizationManager.Localize("System.NewCoreEquipment").Replace("{s}", s);
            default:
                return string.Empty;
        }
    }

    // �z�[���䎌�L�����ǉ�
    public void AddNewHomeCharacter(int characterID)
    {
        switch (characterID)
        {
            case 3: // ����
                HomeDialogue akiho = Resources.Load<HomeDialogue>("HomeDialogue/Akiho");
                ProgressManager.Instance.AddHomeCharacter(akiho);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 4: // ����
                HomeDialogue rikka = Resources.Load<HomeDialogue>("HomeDialogue/Rikka");
                ProgressManager.Instance.AddHomeCharacter(rikka);
                ProgressManager.Instance.RemoveHomeCharacter("No5"); // ���Ԃ��ŗ���������No5���z�[���L��������r��
                homeCharacterScript.SetToLastCharacter();
                return;
            case 5: // �G���i
                HomeDialogue erena = Resources.Load<HomeDialogue>("HomeDialogue/Erena");
                ProgressManager.Instance.AddHomeCharacter(erena);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 6: // ��
                HomeDialogue kei = Resources.Load<HomeDialogue>("HomeDialogue/Kei");
                ProgressManager.Instance.AddHomeCharacter(kei);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 7: // �ߗR��
                HomeDialogue nayuta = Resources.Load<HomeDialogue>("HomeDialogue/Nayuta");
                ProgressManager.Instance.AddHomeCharacter(nayuta);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 11: // �q�X�C
                HomeDialogue hisui = Resources.Load<HomeDialogue>("HomeDialogue/Hisui");
                ProgressManager.Instance.AddHomeCharacter(hisui);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 12: // �_�C��
                HomeDialogue daiya = Resources.Load<HomeDialogue>("HomeDialogue/Daiya");
                ProgressManager.Instance.AddHomeCharacter(daiya);
                homeCharacterScript.SetToLastCharacter();
                return;
            default:
                return;
        }
    }

    public void DisplayResearchPointPanel(bool display)
    {
        researchPointPanel.DOFade(display ? 1.0f: 0.0f, 0.5f);
    }
}
