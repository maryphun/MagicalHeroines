using UnityEngine;
using Assets.SimpleLocalization.Scripts;

public class Nayuta_Powermode : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private int extraDamage = 0;
    [SerializeField] private int chargedDamage = 0;
    [SerializeField] private int chargedStack = 0;
    [SerializeField] private bool isEnabled = false;
    [SerializeField] private Battler nayuta;

    public int ExtraDamage { get { return extraDamage; } }
    public int ChargedDamage { get { return chargedDamage; } }
    public bool IsActive { get { return isEnabled; } }

    private void Start()
    {
        isEnabled = false;
        extraDamage = 0;
        chargedDamage = 0;
        chargedStack = 0;
        nayuta = GetComponent<Battler>();

        nayuta.afterAttackEvent.AddListener(ResetChargedDamage);
    }

    public void SetActive(bool value)
    {
        if (isEnabled == value) return;

        isEnabled = value;

        if (isEnabled)
        {
            extraDamage = Mathf.FloorToInt((float)nayuta.attack * 0.70f);
            nayuta.attack += extraDamage;
            nayuta.onTurnEndEvent.AddListener(DamageNayuta);
        }
        else
        {
            // disable
            nayuta.attack -= extraDamage;
            extraDamage = 0;
            nayuta.onTurnEndEvent.RemoveListener(DamageNayuta);
        }
    }

    public void DamageNayuta()
    {
        const int damage = 20;
        nayuta.DeductHP(nayuta, damage, true);

        // text
        var floatingText = AbilityExecute.CreateFloatingText(nayuta.transform);
        floatingText.Init(2.0f, nayuta.GetMiddleGlobalPosition(), nayuta.GetMiddleGlobalPosition() + new Vector2(0.0f, 100.0f), damage.ToString(), 64, CustomColor.damage());

        // SFX
        AudioManager.Instance.PlaySFX("Attacked", 0.8f);

        // êÌì¨ÉçÉO
        FindObjectOfType<Battle>().AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.PowerMode"), nayuta.CharacterNameColored, CustomColor.AddColor(damage, CustomColor.damage())));
    }

    public void ChargeAttack(int addDamage)
    {
        nayuta.attack += addDamage;
        chargedDamage += addDamage;

        chargedStack++;

        // êÌì¨ÉçÉO
        FindObjectOfType<Battle>().AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.ChargeAttack"), CustomColor.AddColor(addDamage, CustomColor.damage()), chargedStack));
    }

    public void ResetChargedDamage()
    {
        nayuta.attack -= chargedDamage;
        chargedDamage = 0;
        chargedStack = 0;
    }
}
