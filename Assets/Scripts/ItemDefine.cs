using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum CastType
{
    SelfCast, //< �^�[�Q�b�g����
    Enemy, //< �G�Ɏg��
    Teammate, //< ���ԂɎg��
}

[CreateAssetMenu(fileName = "NewItem", menuName = "�쐬/�A�C�e������")]
public class ItemDefine : ScriptableObject
{
    [Header("�Z�[�u���[�h�Ή�")]
    [SerializeField] public string pathName;

    [Header("��{����")]
    [SerializeField] public string itemNameID;
    [SerializeField] public string descriptionID;
    [SerializeField] public string functionName;
    [SerializeField] public CastType itemType;
    [SerializeField, TextArea()] public string effectText;

    [Header("�A�C�R��")]
    [SerializeField] public Sprite Icon;
}