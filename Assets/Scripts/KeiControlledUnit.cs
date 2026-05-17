using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(Battler))]
public class KeiControlledUnit : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] Battler master; // 主人である京のレファレンス
    [SerializeField] Battler battlerScript;
    [SerializeField] int remainingTurn;
    [SerializeField] Battle battleManager;
    [SerializeField] Transform originalParent;
    [SerializeField] int originalSiblingIndex;
    [SerializeField] Vector3 originalPosition;

    public void StartControl(Battler kei, int turn, Battle battleManager)
    {
        master = kei;
        remainingTurn = turn;
        this.battleManager = battleManager;
        battlerScript = GetComponent<Battler>();

        if (battlerScript == null)
        {
            Debug.LogWarning("Can't control non battler object");
            return;
        }
        else
        {
            // 一時敵ではなくなる
            battlerScript.isEnemy = !battlerScript.isEnemy;
            battleManager.AddRemoveTeammate(battlerScript, !master.isEnemy);

            // 向きを反転
            battlerScript.ReverseFacing();

            // 京の状態更新
            master.isTargettable = false;
            //master.EnableNormalAttack = false;

            // 元の位置を記録
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            originalPosition = GetComponent<RectTransform>().position;

            // 位置移動
            transform.SetParent(master.transform.parent);
            transform.SetAsLastSibling();

            RectTransform selfRect = GetComponent<RectTransform>();
            RectTransform masterRect = master.GetComponent<RectTransform>();

            Vector2 targetPos = masterRect.anchoredPosition;
            targetPos.x += battlerScript.GetCharacterRectSize().x;

            selfRect.DOAnchorPos(targetPos, 0.5f);

            // 死亡時
            battlerScript.onDeathEvent.AddListener(OnDeath);
            battlerScript.onTurnEndEvent.AddListener(OnTurnEnd);
        }
    }

    public void OnDeath()
    {
        battleManager.AddRemoveTeammate(battlerScript, master.isEnemy);

        // 京を攻撃できるようにする
        master.isTargettable = true;
        //master.EnableNormalAttack = true;
        master.GetComponent<KeiWeaponController>().ResetControlledUnit();

        master.SetAbilityActive("Hacking", true);
        master.SetAbilityOnCooldown(master.GetAbility("Hacking"), master.GetAbility("Hacking").cooldown);
        master.SetAbilityActive("SuicideAttack", false);
        master.SetAbilityActive("Reprogram", false);
        master.SetAbilityActive("EffeciencyBoost", false);

        const float delay = 1.0f;
        DOTween.Sequence().AppendInterval(delay).AppendCallback(() =>
        {
            // 元の位置に戻す
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            GetComponent<RectTransform>().DOMove(originalPosition, 0.5f);
        });


        battlerScript.onDeathEvent.RemoveListener(OnDeath);
        battlerScript.onTurnEndEvent.RemoveListener(OnTurnEnd);

        // このスクリプトを削除
        Destroy(this, delay + 0.5f);
    }

    public void OnTurnEnd()
    {
        remainingTurn--;
        if (remainingTurn <= 0)
        {
            // 敵に戻す
            battlerScript.isEnemy = !battlerScript.isEnemy;
            battleManager.AddRemoveTeammate(battlerScript, master.isEnemy);

            // 向きを反転
            battlerScript.ReverseFacing();

            // 元の位置に戻す
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            GetComponent<RectTransform>().DOMove(originalPosition, 0.5f);

            // 京を攻撃できるようにする
            master.isTargettable = true;
            //master.EnableNormalAttack = true;
            master.GetComponent<KeiWeaponController>().ResetControlledUnit();

            master.SetAbilityActive("Hacking", true);
            master.SetAbilityOnCooldown(master.GetAbility("Hacking"), master.GetAbility("Hacking").cooldown);
            master.SetAbilityActive("SuicideAttack", false);
            master.SetAbilityActive("Reprogram", false);
            master.SetAbilityActive("EffeciencyBoost", false);

            // Battle log
            battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking_End"), battlerScript.CharacterNameColored));

            battlerScript.onDeathEvent.RemoveListener(OnDeath);
            battlerScript.onTurnEndEvent.RemoveListener(OnTurnEnd);

            const float delay = 1.0f;
            // このスクリプトを削除
            Destroy(this, delay);
        }
    }
}
