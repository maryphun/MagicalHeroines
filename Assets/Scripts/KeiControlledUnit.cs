using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    public void StartControl(Battler kei, int turn)
    {
        master = kei;
        remainingTurn = turn;
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

            GetComponent<RectTransform>().DOLocalMove(master.GetComponent<RectTransform>().localPosition + new Vector3(battlerScript.GetCharacterSize().x + 75.0f, 0.0f, 0.0f), 0.5f);

            // 死亡時
            battlerScript.onDeathEvent.AddListener(OnDeath);
        }
    }

    public void OnDeath()
    {
        // 京を攻撃できるようにする
        master.isTargettable = true;
        master.EnableNormalAttack = true;
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
            // 向きを反転
            battlerScript.ReverseFacing();

            // 元の位置に戻す
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            GetComponent<RectTransform>().DOMove(originalPosition, 0.5f);

            // 京を攻撃できるようにする
            master.isTargettable = true;
            //master.EnableNormalAttack = true;

            const float delay = 1.0f;
            // このスクリプトを削除
            Destroy(this, delay);
        }
    }
}
