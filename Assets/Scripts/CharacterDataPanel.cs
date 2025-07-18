using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class CharacterDataPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image characterSprite;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text levelValue;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private TMP_Text mpValue;
    [SerializeField] private TMP_Text attackValue;
    [SerializeField] private TMP_Text defenseValue;
    [SerializeField] private TMP_Text speedValue;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Image equipment;
    [SerializeField] private TMP_Text skillList;
    [SerializeField] private Button[] skillButtonList = new Button[4];
    [SerializeField] private Image[] skillImageList = new Image[4];
    [SerializeField] private bool[] skillAvailable = new bool[4];
    [SerializeField] private SkillRequirement[] requirement = new SkillRequirement[4];
    [SerializeField] private CharacterBuildingPanel mainPanel;
    [SerializeField] private EquipmentPanel equipmentPanel;

    // abilities
    [SerializeField] private CanvasGroup abilityRequirement;
    [SerializeField] private TMP_Text abilityRequirementText;

    // pop up
    [SerializeField] private CanvasGroup popup;
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text abilitySPCost;
    [SerializeField] private TMP_Text abilityType;
    [SerializeField] private TMP_Text abilityCooldown;
    [SerializeField] private TMP_Text abilityCastType;
    [SerializeField] private TMP_Text abilityDescription;

    [Header("Setting")]
    [SerializeField] private Sprite lockIcon;

    /// <summary>
    /// 表示するキャラクターを更新
    /// </summary>
    public void InitializeCharacterData(Character character)
    {
        characterSprite.sprite = character.GetCurrentStatus().character;
        characterName.text = character.localizedName;
        levelValue.text = character.current_level.ToString();
        hpValue.text = character.current_maxHp.ToString();
        mpValue.text = character.current_maxMp.ToString();
        attackValue.text = character.current_attack.ToString();
        defenseValue.text = character.current_defense.ToString();
        speedValue.text = character.current_speed.ToString();

        // get equipment bonus
        EquipmentDefine equipmentData = new EquipmentDefine();
        bool isEquiped = ProgressManager.Instance.GetCharacterEquipment(character.characterData.characterID, ref equipmentData);
        if (isEquiped)
        {
            var data = ProgressManager.Instance.GetEquipmentData().FirstOrDefault(x => x.equipingCharacterID == mainPanel.CurrentCheckingSlot).data;

            // 明穂の聖核装備特殊処理
            if (data.pathName == "Equip_Akiho") data.hp = character.current_maxHp / 2;

            if (data.hp != 0) hpValue.text = hpValue.text + "<size=75%><color=" + (data.hp > 0 ? "green>(+" : "red>(") + data.hp + ")";
            if (data.sp != 0) mpValue.text = mpValue.text + "<size=75%><color=" + (data.sp > 0 ? "green>(+" : "red>(") + data.sp + ")";
            if (data.atk != 0) attackValue.text = attackValue.text + "<size=75%><color=" + (data.atk > 0 ? "green>(+" : "red>(") + data.atk + ")";
            if (data.def != 0) defenseValue.text = defenseValue.text + "<size=75%><color=" + (data.def > 0 ? "green>(+" : "red>(") + data.def + ")";
            if (data.spd != 0) speedValue.text = speedValue.text + "<size=75%><color=" + (data.spd > 0 ? "green>(+" : "red>(") + data.spd + ")";
        }


        // setup ability
        List<Ability> abilities = new List<Ability>(character.characterData.abilities);

        // 順番並べ
        if (abilities.Count > 0)
        {
            abilities.Sort((x, y) =>
            {
                int powerComparison = x.requiredHornyness.CompareTo(y.requiredHornyness);

                if (powerComparison == 0) // If requiredHornyness is equal, compare by requiredLevel
                {
                    return x.requiredLevel.CompareTo(y.requiredLevel);
                }

                return powerComparison;
            });

            for (int i = 0; i < abilities.Count; i++)
            {
                skillAvailable[i] = (character.current_level >= abilities[i].requiredLevel
                                    && character.hornyEpisode >= abilities[i].requiredHornyness);
            }
        }
        else
        {
            skillAvailable = skillAvailable.Select(_ => false).ToArray();
        }

        // 残りのアイコンはいらない
        for (int i = abilities.Count; i < skillAvailable.Length; i++)
        {
            skillAvailable[i] = false;
        }

        // ボタンを初期化
        for (int i = 0; i < skillButtonList.Length; i++)
        {
            if (skillAvailable[i])
            {
                skillButtonList[i].image.color = new Color(1, 1, 1, 1);
                skillButtonList[i].interactable = true;
                skillButtonList[i].onClick.RemoveAllListeners();
                Ability abilityInfo = abilities[i];
                skillButtonList[i].onClick.AddListener(delegate { OnClickAbility(abilityInfo); });
                skillImageList[i].color = new Color(1, 1, 1, 1);
                skillImageList[i].sprite = abilities[i].icon;
                requirement[i].enabled = false;
            }
            else
            {
                skillButtonList[i].interactable = false;

                if (abilities.Count > i)
                {
                    // 技が存在しているが開放条件まだ満たしていない
                    skillButtonList[i].image.color = new Color(1, 1, 1, 1);
                    skillImageList[i].color = new Color(1, 1, 1, 1);

                    skillImageList[i].sprite = lockIcon;
                    requirement[i].enabled = true;
                    requirement[i].Initialization(abilities[i].requiredLevel, abilities[i].requiredHornyness);
                }
                else
                {
                    // そもそも技存在していない
                    skillButtonList[i].image.color = new Color(1, 1, 1, 0);
                    skillImageList[i].color = new Color(1, 1, 1, 0);
                    requirement[i].enabled = false;
                }
            }
        }

        // 技説明pop upを初期化
        popup.alpha = 0.0f;
        popup.interactable = false;
        popup.blocksRaycasts = false;

        UpdateEquipmentIcon();
    }

    public void OnClickAbility(Ability ability)
    {
        // 技説明pop upを初期化
        popup.DOFade(1.0f, 0.2f);
        popup.interactable = true;
        popup.blocksRaycasts = true;

        abilityName.text = LocalizationManager.Localize(ability.abilityNameID);
        abilitySPCost.text = LocalizationManager.Localize("System.SPCost") + ability.consumeSP;
        abilityType.text = LocalizationManager.Localize("System.AbilityType") + "：" + AbilityTypeToString(ability.abilityType);
        abilityCooldown.text = LocalizationManager.Localize("System.Cooldown") + "：" + ability.cooldown + LocalizationManager.Localize("System.Turn");
        abilityCooldown.alpha = ability.cooldown == 0 ? 0 : 1; // only show cooldown if cooldown is longer than 0
        abilityCastType.text = LocalizationManager.Localize("System.EffectTarget") + CastTypeToString(ability.castType);
        abilityDescription.text = LocalizationManager.Localize(ability.descriptionID);

        // SE 再生
        AudioManager.Instance.PlaySFX("SystemOpen");
    }

    public void CloseAbilityInfoPopup()
    {
        popup.DOFade(0.0f, 0.2f).OnComplete(() => {
            popup.interactable = false;
            popup.blocksRaycasts = false;
        });

        // SE 再生
        AudioManager.Instance.PlaySFX("SystemCancel");
    }

    private string CastTypeToString(CastType castType)
    {
        switch (castType)
        {
            case CastType.SelfCast:
                return LocalizationManager.Localize("System.EffectSelf");
            case CastType.Teammate:
                return LocalizationManager.Localize("System.EffectTeam");
            case CastType.Enemy:
                return LocalizationManager.Localize("System.EffectEnemy");
            default:
                return string.Empty;
        }
    }

    private string AbilityTypeToString(AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityType.Attack:
                return LocalizationManager.Localize("System.AbilityAttack");
            case AbilityType.Buff:
                return LocalizationManager.Localize("System.AbilityBuff");
            case AbilityType.Heal:
                return LocalizationManager.Localize("System.AbilityHeal");
            case AbilityType.Special:
                return LocalizationManager.Localize("System.AbilitySpecial");
            default:
                return string.Empty;
        }
    }

    public void UpdateEquipmentIcon()
    {
        var EquipmentData = ProgressManager.Instance.GetEquipmentData();
        bool isEquiped = EquipmentData.Any(x => x.equipingCharacterID == mainPanel.CurrentCheckingSlot);
        if (isEquiped)
        {
            var data = ProgressManager.Instance.GetEquipmentData().FirstOrDefault(x => x.equipingCharacterID == mainPanel.CurrentCheckingSlot).data;
            equipment.sprite = data.Icon;
            equipment.color = Color.white;
            equipmentButton.image.color = equipmentPanel.GetColorByEquipmentType(data.equipmentType);
        }
        else
        {
            equipment.color = new Color(1, 1, 1, 0);
            equipmentButton.image.color = Color.white;
        }
    }
}
