using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AbilityType
{
    Attack, //< 攻撃
    Buff,   //< バフ/デバフ
    Heal,   //< 回復
    Special,//< 特殊
}

[System.Serializable]
[CreateAssetMenu(fileName = "NewAbility", menuName = "作成/特殊技生成")]
public class Ability : ScriptableObject
{
    [Header("基本資料")]
    public Sprite icon;
    public string abilityNameID;
    public string descriptionID;
    public string functionName;
    public int consumeSP;
    public int cooldown;
    public int requiredLevel;
    public int requiredHornyness; // 淫乱度要求
    public CastType castType;
    public AbilityType abilityType;
    public bool isAOE;
    public bool canTargetSelf;
    public bool disableOnDefault;
}
