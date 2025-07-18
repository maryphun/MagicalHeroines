using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;
using TMPro;
using System.Linq;

public class CharacterUpgradePanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0f, 1f)] private float resourcesPanelAnimationTime = 0.5f;
    [SerializeField] private Gradient upgradeGradient;
    [SerializeField, Range(0f, 1f)] private float upgradeAnimationTime = 0.5f;

    [Header("References")]
    [SerializeField] private Image characterSprite;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text levelValue;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private TMP_Text mpValue;
    [SerializeField] private TMP_Text attackValue;
    [SerializeField] private TMP_Text defenseValue;
    [SerializeField] private TMP_Text speedValue;
    [SerializeField] private TMP_Text levelUpText;
    [SerializeField] private TMP_Text newAbility;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private CanvasGroup resourcesPanel;

    [Header("Debug")]
    Character currentCharacter;
    Sequence newAbilitySequence;

    const int MaxLevel = 20;

    public void ResetAnimation()
    {
        // Reset
        newAbilitySequence.Complete();
        newAbility.alpha = 0.0f;
    }

    /// <summary>
    /// 表示するキャラクターを更新
    /// </summary>
    private void UpdateCharacterData(Character character)
    {
        characterSprite.sprite = character.GetCurrentStatus().character;
        characterName.text = character.localizedName;
        levelValue.text = character.current_level.ToString();
        hpValue.text = character.current_maxHp.ToString();
        mpValue.text = character.current_maxMp.ToString();
        attackValue.text = character.current_attack.ToString();
        defenseValue.text = character.current_defense.ToString();
        speedValue.text = character.current_speed.ToString();
    }

    private int CalculateLevelUpCost(Character character, int level)
    {
        return character.characterData.levelUp_base_cost + (character.characterData.levelUp_cost_increment * level);
    }

    private void UpdateLevelUpButton(Character character)
    {
        if (character.current_level >= MaxLevel)
        {
            levelUpButton.interactable = false;
            levelUpText.text = LocalizationManager.Localize("System.LevelMax");
            return;
        }

        //　レベルアップコストを計算
        int goldCost = CalculateLevelUpCost(character, character.current_level);

        levelUpText.text = LocalizationManager.Localize("System.LevelUp") + "\n(" + LocalizationManager.Localize("System.Cost") + ": " + goldCost.ToString() + ")";

        // 資金足りるか
        levelUpButton.interactable = (goldCost <= ProgressManager.Instance.GetCurrentMoney());
    }

    public void InitializeUpgradePanel(Character character)
    {
        UpdateCharacterData(character);
        UpdateLevelUpButton(character);

        currentCharacter = character;
    }

    public void HoverLevelUpButton()
    {
        if (currentCharacter == null) return;

        levelValue.text = currentCharacter.current_level.ToString() + "<color=green>(+1)</color>";
        hpValue.text = currentCharacter.current_maxHp.ToString() + "<color=green>(+" + currentCharacter.characterData.hp_growth + ")</color>";
        mpValue.text = currentCharacter.current_maxMp.ToString() + "<color=green>(+" + currentCharacter.characterData.mp_growth + ")</color>";
        attackValue.text = currentCharacter.current_attack.ToString() + "<color=green>(+" + currentCharacter.characterData.attack_growth + ")</color>";
        defenseValue.text = currentCharacter.current_defense.ToString() + "<color=green>(+" + currentCharacter.characterData.defense_growth + ")</color>";
        speedValue.text = currentCharacter.current_speed.ToString() + "<color=green>(+" + currentCharacter.characterData.speed_growth + ")</color>";

        resourcesPanel.DOFade(1.0f, resourcesPanelAnimationTime);
    }
    public void UnhoverLevelUpButton()
    {
        if (currentCharacter == null) return;

        UpdateCharacterData(currentCharacter);

        resourcesPanel.DOFade(0.0f, resourcesPanelAnimationTime);
    }

    public void OnClickLevelUpButton()
    {
        // SE再生
        AudioManager.Instance.PlaySFX("SystemLevelUp");

        // 資金更新
        ProgressManager.Instance.SetMoney(ProgressManager.Instance.GetCurrentMoney() - CalculateLevelUpCost(currentCharacter, currentCharacter.current_level));

        // キャラクターデータ更新
        var data = ProgressManager.Instance.GetAllCharacter(true, true);
        int index = CharacterIDToIndex(currentCharacter.characterData.characterID);

        data[index].current_level++;
        data[index].current_maxHp += data[index].characterData.hp_growth;
        data[index].current_maxMp += data[index].characterData.mp_growth;
        data[index].current_hp += data[index].characterData.hp_growth;
        data[index].current_mp += data[index].characterData.mp_growth;
        data[index].current_attack += data[index].characterData.attack_growth;
        data[index].current_defense += data[index].characterData.defense_growth;
        data[index].current_speed += data[index].characterData.speed_growth;

        // 新しく習得した技があるか
        if (data[index].characterData.abilities.Any(item => item.requiredLevel == data[index].current_level))
        {
            newAbility.rectTransform.localScale = new Vector3(2.2f, 2.2f, 1f);
            newAbility.alpha = 0.0f;

            newAbilitySequence = DOTween.Sequence()
                .AppendCallback(() => {   
                    newAbility.DOFade(1.0f, 0.2f);
                    newAbility.rectTransform.DOScale(1.0f, 0.2f);
                                      })
                .AppendInterval(0.2f)
                .AppendCallback(() => { newAbility.rectTransform.DOShakePosition(0.5f, 3); })
                .AppendInterval(1.0f)
                .AppendCallback(() => { newAbility.DOFade(0.0f, 0.2f); });

            // SE
            AudioManager.Instance.PlaySFX("NewAbility");
        }
        
        // 再度データをコピー
        currentCharacter = ProgressManager.Instance.GetAllCharacter(false, true)[index];

        // 表示データ更新
        InitializeUpgradePanel(currentCharacter);
        HoverLevelUpButton();

        // エフェクト
        characterSprite.DOGradientColor(upgradeGradient, upgradeAnimationTime);
        ShakeManager.Instance.ShakeObject(characterSprite.GetComponent<RectTransform>(), upgradeAnimationTime, 2);
    }

    public int CharacterIDToIndex(int characterID)
    {
        int[] index = { 0, 1, 2, 3, 4, 5, 6, 7, -1, -1, -1, 9, 8, -1 };

        return index[characterID];
    }
}
