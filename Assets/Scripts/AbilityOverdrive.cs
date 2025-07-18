using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityOverdrive : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] Battle battleManager;
    [SerializeField] int turn;
    [SerializeField] Battler target;

    public void Init(Battle battleMng, Battler target, int stunTurn)
    {
        battleManager = battleMng;
        this.target = target;
        turn = stunTurn;
    }

    public void Trigger()
    {
        // stun the target
        battleManager.AddBuffToBattler(target, BuffType.stun, turn, 0);
        //battleManager.NextTurn(false);

        // SE
        AudioManager.Instance.PlaySFX("Sleep", 0.75f);

        Destroy(this);
    }
}
