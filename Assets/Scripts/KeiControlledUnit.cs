using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Battler))]
public class KeiControlledUnit : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] Battler master; // ��l�ł��鋞�̃��t�@�����X
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
            // �ꎞ�G�ł͂Ȃ��Ȃ�
            battlerScript.isEnemy = !battlerScript.isEnemy;

            // �����𔽓]
            battlerScript.ReverseFacing();

            // ���̏�ԍX�V
            master.isTargettable = false;
            //master.EnableNormalAttack = false;

            // ���̈ʒu���L�^
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            originalPosition = GetComponent<RectTransform>().position;

            // �ʒu�ړ�
            transform.SetParent(master.transform.parent);
            transform.SetAsLastSibling();

            GetComponent<RectTransform>().DOLocalMove(master.GetComponent<RectTransform>().localPosition + new Vector3(battlerScript.GetCharacterSize().x + 75.0f, 0.0f, 0.0f), 0.5f);

            // ���S��
            battlerScript.onDeathEvent.AddListener(OnDeath);
        }
    }

    public void OnDeath()
    {
        // �����U���ł���悤�ɂ���
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
            // ���̈ʒu�ɖ߂�
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            GetComponent<RectTransform>().DOMove(originalPosition, 0.5f);
        });

        // ���̃X�N���v�g���폜
        Destroy(this, delay + 0.5f);
    }

    public void OnTurnEnd()
    {
        remainingTurn--;
        if (remainingTurn <= 0)
        {
            // �G�ɖ߂�
            battlerScript.isEnemy = !battlerScript.isEnemy;
            // �����𔽓]
            battlerScript.ReverseFacing();

            // ���̈ʒu�ɖ߂�
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            GetComponent<RectTransform>().DOMove(originalPosition, 0.5f);

            // �����U���ł���悤�ɂ���
            master.isTargettable = true;
            //master.EnableNormalAttack = true;

            const float delay = 1.0f;
            // ���̃X�N���v�g���폜
            Destroy(this, delay);
        }
    }
}
