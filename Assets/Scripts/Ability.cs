using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AbilityType
{
    Attack, //< �U��
    Buff,   //< �o�t/�f�o�t
    Heal,   //< ��
    Special,//< ����
}

[System.Serializable]
[CreateAssetMenu(fileName = "NewAbility", menuName = "�쐬/����Z����")]
public class Ability : ScriptableObject
{
    [Header("��{����")]
    public Sprite icon;
    public string abilityNameID;
    public string descriptionID;
    public string functionName;
    public int consumeSP;
    public int cooldown;
    public int requiredLevel;
    public int requiredHornyness; // �����x�v��
    public CastType castType;
    public AbilityType abilityType;
    public bool isAOE;
    public bool canTargetSelf;
    public bool disableOnDefault;
}
