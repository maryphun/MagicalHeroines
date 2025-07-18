using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class ErenaShield : MonoBehaviour
{
    private Battler battler;
    private RectTransform rect;
    private Image img;
    private EventType type;
    private Battle battleManager;

    public enum EventType
    {
        DivineShield,
        StunShield,
    }

    public void Init(Battle battleManager, Battler battler, RectTransform rect, EventType type)
    {
        this.battler = battler;
        this.rect = rect;
        img = rect.GetComponent<Image>();
        this.type = type;
        this.battleManager = battleManager;

        if (this.type == EventType.DivineShield)
        {
            battler.onAttackedEvent.AddListener(OnAttackDivineShield);
        }
        if (this.type == EventType.StunShield)
        {
            battler.onAttackedEvent.AddListener(OnAttackStunShield);
        }
    }

    public void OnAttackDivineShield(Battler attacked, Battler source, int damage)
    {
        battler.onAttackedEvent.RemoveListener(OnAttackDivineShield);
        AudioManager.Instance.PlaySFX("NewAbility"); // shield break

        if (battler.name == "Erena_Enemy(Clone)")
        {
            AudioManager.Instance.PlaySFXDelay("Erena_Attacked" + Random.Range(2, 4), 0.5f);
        }

        const float animTime = 0.5f;
        rect.DOScale(3.0f, animTime);
        img.DOFade(0.0f, animTime);
        Destroy(gameObject, animTime + Time.deltaTime);
    }

    public void OnAttackStunShield(Battler attacked, Battler source, int damage)
    {
        battler.onAttackedEvent.RemoveListener(OnAttackStunShield);
        AudioManager.Instance.PlaySFX("NewAbility"); // shield break

        if (battler.name == "Erena_Enemy(Clone)")
        {
            AudioManager.Instance.PlaySFXDelay("Erena_Attacked" + Random.Range(2, 4), 0.5f);
        }

        if (battleManager != null)
        {
            battleManager.AddBuffToBattler(source, BuffType.stun, 2, 0);
        }

        // É_ÉÅÅ[ÉWÇÕÇ∑ÇÈ
        battler.DeductHP(source, damage, true);

        const float animTime = 0.5f;
        rect.DOScale(3.0f, animTime);
        img.DOFade(0.0f, animTime);
        Destroy(gameObject, animTime + Time.deltaTime);
    }

    public void Destroy()
    {
        if (this.type == EventType.DivineShield)
        {
            battler.onAttackedEvent.RemoveListener(OnAttackDivineShield);
        }
        if (this.type == EventType.StunShield)
        {
            battler.onAttackedEvent.RemoveListener(OnAttackStunShield);
        }

        Destroy(gameObject);
    }
}
