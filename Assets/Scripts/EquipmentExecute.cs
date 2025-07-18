using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SimpleLocalization.Scripts;
using DG.Tweening;

public class EquipmentExecute : SingletonMonoBehaviour<EquipmentExecute>
{
    public IEnumerator CorruptedGloveStart(Battler battler)
    {
        yield return null;
    }
    public IEnumerator CorruptedGloveEnd(Battler battler)
    {
        yield return null;
    }


    public IEnumerator NiceTshirtStart(Battler battler)
    {
        yield return null;
    }
    public IEnumerator NiceTshirtEnd(Battler battler)
    {
        yield return null;
    }


    public IEnumerator HelmetStart(Battler battler)
    {
        yield return null;
    }
    public IEnumerator HelmetEnd(Battler battler)
    {
        yield return null;
    }

    public IEnumerator StungunStart(Battler battler)
    {
        var ability = Resources.Load<Ability>("AbilityList/Stungun");

        if (!ReferenceEquals(ability, null))
        {
            battler.AddAbilityToCharacter(ability);
        }
        else
        {
            Debug.LogWarning("Ability data 'AbilityList/Stungun' not couldn't be loaded.");
        }
        yield return null;
    }
    public IEnumerator StolenUndieStart(Battler battler)
    {
        battler.onTurnBeginEvent.AddListener(EquipmentMethods.StolenUndieExecute);
        yield return null;
    }
    public IEnumerator StolenUndieEnd(Battler battler)
    {
        battler.onTurnBeginEvent.RemoveListener(EquipmentMethods.StolenUndieExecute);
        yield return null;
    }

    public IEnumerator Equip_AkihoStart(Battler battler)
    {
        //int addValue = battler.max_hp / 2;
        //Debug.Log("add value: " + addValue.ToString());
        //battler.max_hp += addValue;
        //battler.current_hp += addValue;

        battler.onTurnBeginEvent.AddListener(EquipmentMethods.AkihoSeikakuExecute);
        yield return null;
    }
    public IEnumerator Equip_AkihoEnd(Battler battler)
    {
        battler.onTurnBeginEvent.RemoveListener(EquipmentMethods.AkihoSeikakuExecute);
        yield return null;
    }

    public IEnumerator Equip_RikkaStart(Battler battler)
    {
        battler.onAttackedEvent.AddListener(EquipmentMethods.RikkaSeikakuExecute);
        yield return null;
    }
    public IEnumerator Equip_RikkaEnd(Battler battler)
    {
        battler.onAttackedEvent.RemoveListener(EquipmentMethods.RikkaSeikakuExecute);
        yield return null;
    }
    public IEnumerator Equip_KeiStart(Battler battler)
    {
        var teammates = FindObjectOfType<Battle>().GetAllTeammate();

        Debug.Log("[Equip_KeiStart] teammate count = " + teammates.Count.ToString());
        foreach (var teammate in teammates)
        {
            if (teammate != battler) EquipmentMethods.KeiSeikakuStart(teammate);
        }
        yield return null;
    }

    public IEnumerator Equip_NayutaStart(Battler battler)
    {
        battler.onTurnBeginEvent.AddListener(EquipmentMethods.NayutaSeikakuExecute);
        yield return null;
    }
    public IEnumerator Equip_NayutaEnd(Battler battler)
    {
        battler.onTurnBeginEvent.RemoveListener(EquipmentMethods.NayutaSeikakuExecute);
        yield return null;
    }
}

public static class EquipmentMethods
{
    private static Battle battleManager;
    private static GameObject floatingTextOrigin;

    public static void Initialization(Battle reference)
    {
        battleManager = reference;
        floatingTextOrigin = Resources.Load<GameObject>("Prefabs/FloatingNumber");
    }

    public static void Finalize()
    {
        battleManager = null;
        floatingTextOrigin = null;
    }

    public static void CreateFloatingText(string text, Color color, float size, Battler target)
    {
        if (floatingTextOrigin == null) return;

        // create floating text
        var floatingText = UnityEngine.GameObject.Instantiate(floatingTextOrigin, target.transform);
        floatingText.GetComponent<FloatingText>().Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.35f), new Vector2(0.0f, 100.0f), text, size, color);
    }

    public static void StolenUndieExecute()
    {
        if (battleManager == null) return;
        Battler battler = battleManager.GetCurrentBattler();

        // SP ‚Ì—Ê
        const float percentage = 0.12f;

        int healAmount = Mathf.FloorToInt((float)battler.max_mp * percentage);

        battler.AddSP(healAmount);
        CreateFloatingText(healAmount.ToString(), CustomColor.heal(), 32, battler);
        AudioManager.Instance.PlaySFX("Heal2");
    }

    public static void CushionExecute(int healAmount)
    {
        if (battleManager == null) return;
        Battler battler = battleManager.GetCurrentBattler();

        battler.Heal(healAmount);
    }

    public static void AkihoSeikakuExecute()
    {
        if (battleManager == null) return;
        Battler battler = battleManager.GetCurrentBattler();

        // ‰ñ•œ—Ê
        const float percentage = 0.02f;
        int amountHP = Mathf.FloorToInt((float)battler.max_hp * percentage);
        int amountSP = Mathf.FloorToInt((float)battler.max_mp * percentage);

        battler.AddSP(amountSP);
        CreateFloatingText(amountSP.ToString(), CustomColor.SP(), 32, battler);
        AudioManager.Instance.PlaySFX("Heal2", 0.5f);

        DOTween.Sequence().AppendInterval(0.25f).AppendCallback(() =>
        {
            battler.Heal(amountHP);
            CreateFloatingText(amountHP.ToString(), CustomColor.heal(), 32, battler);
            AudioManager.Instance.PlaySFX("Heal2", 0.5f);
        });
    }
    public static void RikkaSeikakuExecute(Battler attacked, Battler attacker, int value)
    {
        attacked.DeductHP(attacker, value, true);

        // ”½Œ‚
        attacked.PlayAnimation(BattlerAnimationType.attack);

        int damage = Battle.CalculateDamage(attacked, attacker, true);
        attacker.DeductHP(attacked, damage, true);
        CreateFloatingText(damage.ToString(), CustomColor.damage(), 32, attacker);
    }
    public static void ErenaSeikakuExecute(Battler target)
    {
        if (battleManager == null) return;
        const float chance = 0.2f;

        if (UnityEngine.Random.Range(0.0f, 1.0f) <= chance)
        {
            battleManager.AddBuffToBattler(target, BuffType.stun, 1, 0);
        }
    }
    public static void KeiSeikakuStart(Battler battler)
    {
        if (battleManager == null) return;

        const int amount = 15;
        Debug.Log("[KeiSeikakuStart] " + battler.character_name + " + " + amount.ToString() + "dmg");
        battler.attack += amount;
    }
    public static void NayutaSeikakuExecute()
    {
        if (battleManager == null) return;
        Battler battler = battleManager.GetCurrentBattler();

        int amount = UnityEngine.Random.Range(5, 16);
        battler.attack += amount;

        string log = string.Format(LocalizationManager.Localize("BattleLog.NayutaEquip"), LocalizationManager.Localize(battler.equipment.equipNameID), battler.CharacterNameColored, CustomColor.AddColor(amount, CustomColor.damage()));
        battleManager.AddBattleLog(log);
    }
}
