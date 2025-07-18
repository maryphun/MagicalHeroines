using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class ItemExecute : SingletonMonoBehaviour<ItemExecute>
{
    [Header("References")]
    [SerializeField] private Battle battleManager;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private List<Battler> targetBattlers;

    /// <summary>
    /// Send references
    /// </summary>
    /// <param name="battleManagerScript"></param>
    public void Initialize(Battle battleManagerScript)
    {
        battleManager = battleManagerScript;
    }

    #region common methods
    public void SetTargetBattlers(List<Battler> targets)
    {
        targetBattlers = targets;
    }

    public void SetTargetBattler(Battler target)
    {
        targetBattlers = new List<Battler>();
        targetBattlers.Add(target);
    }

    public void SetItemIcon(Sprite icon)
    {
        itemSprite = icon;
    }

    private FloatingText CreateFloatingText(Transform parent)
    {
        GameObject origin = Resources.Load<GameObject>("Prefabs/FloatingNumber");
        var obj = Instantiate(origin);
        obj.transform.SetParent(parent);
        var floatingTextComponent = obj.GetComponent<FloatingText>();

        return floatingTextComponent;
    }

    private void CreateFadingImage(Sprite sprite, float fadeTime)
    {
        Vector2 position = battleManager.GetCurrentBattler().GetMiddleGlobalPosition();

        // Create Canvas
        Image img = new GameObject("Using Item Graphic").AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.rectTransform.position = position;

        img.transform.SetParent(battleManager.GetCurrentBattler().transform.parent.parent);

        img.rectTransform.DOMoveY(position.y + 150.0f, fadeTime);
        img.DOFade(0.0f, fadeTime);

        Destroy(img.gameObject, fadeTime);
    }


    private void CreateMovingImage(Sprite sprite, Vector3 targetPosition, float animTime)
    {
        Vector2 position = battleManager.GetCurrentBattler().GetMiddleGlobalPosition();

        // Create Canvas
        Image img = new GameObject("Using Item Graphic").AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.rectTransform.position = position;

        img.transform.SetParent(battleManager.GetCurrentBattler().transform.parent.parent);

        img.rectTransform.DOMove(targetPosition, animTime, true).SetEase(Ease.Linear);

        Destroy(img.gameObject, animTime);
    }
    #endregion common methods

    #region items
    public void OnUseCroissant()
    {
        const int SPAmount = 50;
        
        var self = battleManager.GetCurrentBattler();
        self.PlayAnimation(BattlerAnimationType.item);

        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            // create image
            CreateFadingImage(itemSprite, 1.0f);
        })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // text
                    var floatingText = CreateFloatingText(self.transform);
                    floatingText.Init(2.0f, self.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + SPAmount.ToString(), 64, CustomColor.SP());

                    // effect
                    self.AddSP(SPAmount);

                    // play SE
                    AudioManager.Instance.PlaySFX("Heal");

                    // animation
                    self.PlayAnimation(BattlerAnimationType.idle);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealSP"), self.CharacterNameColored, CustomColor.AddColor(SPAmount, CustomColor.SP())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    public void OnUseBread()
    {
        // hp+40
        const int healAmount = 40;

        var target = battleManager.GetCurrentBattler();
        target.PlayAnimation(BattlerAnimationType.item);

        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
                {
                    // create image
                    CreateFadingImage(itemSprite, 1.0f);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {

                    // text
                    var floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+"+healAmount.ToString(), 64, CustomColor.heal());

                    // effect
                    target.Heal(healAmount);

                    // play SE
                    AudioManager.Instance.PlaySFX("Heal");

                    // animation
                    target.PlayAnimation(BattlerAnimationType.idle);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.heal())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    public void OnUseM24()
    {
        const int damage = 50;
        var target = targetBattlers[0];
        var self = battleManager.GetCurrentBattler();
        self.PlayAnimation(BattlerAnimationType.item);

        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            // play SE
            AudioManager.Instance.PlaySFX("Throw", 0.5f);

            // create image
            CreateMovingImage(itemSprite, targetBattlers[0].GetMiddleGlobalPosition(), 0.8f);
        })
                .AppendInterval(0.8f)
                .AppendCallback(() =>
                {
                    // text
                    var floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), damage.ToString(), 64, CustomColor.damage());

                    // effect
                    target.DeductHP(self, damage);

                    // animation
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    // shake enemy
                    target.Shake(1f);

                    // play SE
                    AudioManager.Instance.PlaySFX("Explode");

                    // animation
                    self.PlayAnimation(BattlerAnimationType.idle);

                    // VFX
                    VFXSpawner.SpawnVFX("Explode", target.transform, target.GetMiddleGlobalPosition());

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(damage, CustomColor.damage())));
                })
                .AppendInterval(0.2f)
                .AppendCallback(() =>
                {
                    // animation
                    target.PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(0.3f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    public void OnUseFirstAid()
    {
        // HP 10~100
        int healAmount = SeriouslyRandom.Next(10, 100);
        var target = targetBattlers[0];

        battleManager.GetCurrentBattler().PlayAnimation(BattlerAnimationType.item);

        float animTime = 0.8f;

        // 自身に使った
        if (target == battleManager.GetCurrentBattler())
        {
            animTime = 0.5f;
        }

        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            // play SE
            AudioManager.Instance.PlaySFX("Air", 0.5f);

            // create image
            if (target != battleManager.GetCurrentBattler())
            {
                CreateMovingImage(itemSprite, targetBattlers[0].GetMiddleGlobalPosition(), animTime);
            }
            else
            {
                CreateFadingImage(itemSprite, 1.0f);
            }
        })
                .AppendInterval(animTime)
                .AppendCallback(() =>
                {
                    // text
                    var floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());

                    // effect
                    target.Heal(healAmount);

                    // play SE
                    AudioManager.Instance.PlaySFX("Heal");

                    // animation
                    battleManager.GetCurrentBattler().PlayAnimation(BattlerAnimationType.idle);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.heal())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }
    #endregion items
}
