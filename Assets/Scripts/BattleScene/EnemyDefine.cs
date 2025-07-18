using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable]
public struct EnemyActionPattern
{
    [SerializeField] public EnemyActionType actionType;
    [SerializeField] public Ability ability;
    [SerializeField] public float chance;
    [SerializeField] public bool hasThresholdCondition;
    [SerializeField] public float HpThreshold; // only take this action when HP is lower than a certain percentage.
    [SerializeField] public float SpThreshold; // only take this action when SP is lower than a certain percentage.
}

public enum EnemyActionType
{
    NormalAttack,
    SpecialAbility,
    Idle,
}

[CreateAssetMenu(fileName = "Enemy", menuName = "�쐬/�G�L��������")]
public class EnemyDefine : ScriptableObject
{
    [Header("��{����")]
    [SerializeField] public string enemyName = "�X���C��";
    [SerializeField] public int maxHP = 100;
    [SerializeField] public int maxMP = 100;
    [SerializeField] public int attack = 10;
    [SerializeField] public int defense = 10;
    [SerializeField] public int speed = 10;
    [SerializeField] public int level = 1;
    [SerializeField] public Color character_color = new Color(1,1,1,1);

    [Header("�s���p�^�[���ݒ�")]
    [SerializeField] public List<EnemyActionPattern> actionPattern = new List<EnemyActionPattern>();

    [Header("�A�C�R��")]
    [SerializeField] public Sprite icon;

    [Header("���t�@�����X")]
    [SerializeField] public GameObject battler;
}
