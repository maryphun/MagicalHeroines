using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EquipmentType
{
    Normal,
    Rare,
    Holy,
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "�쐬/��������")]
public class EquipmentDefine : ScriptableObject
{
    [Header("�Z�[�u���[�h�Ή�")]
    [SerializeField] public string pathName;

    [Header("��{����")]
    [SerializeField] public string equipNameID;
    [SerializeField] public string descriptionID;
    [SerializeField] public EquipmentType equipmentType;
    [SerializeField, TextArea()] public string effectText;
    [SerializeField] public string battleStartFunctionName;
    [SerializeField] public string battleEndFunctionName;
    [SerializeField] public int hp;
    [SerializeField] public int sp;
    [SerializeField] public int atk;
    [SerializeField] public int def;
    [SerializeField] public int spd;

    [Header("�A�C�R��")]
    [SerializeField] public Sprite Icon;
}

[System.Serializable]
public class EquipmentData
{
    public EquipmentDefine data;
    public int equipingCharacterID;

    public EquipmentData(EquipmentDefine define)
    {
        data = define;
        equipingCharacterID = -1;
    }

    public void SetEquipCharacter(int characterID)
    {
        equipingCharacterID = characterID;
    }

    public void Unequip()
    {
        equipingCharacterID = -1;
    }
}