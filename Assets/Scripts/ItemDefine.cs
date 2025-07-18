using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum CastType
{
    SelfCast, //< ターゲット無し
    Enemy, //< 敵に使う
    Teammate, //< 仲間に使う
}

[CreateAssetMenu(fileName = "NewItem", menuName = "作成/アイテム生成")]
public class ItemDefine : ScriptableObject
{
    [Header("セーブロード対応")]
    [SerializeField] public string pathName;

    [Header("基本資料")]
    [SerializeField] public string itemNameID;
    [SerializeField] public string descriptionID;
    [SerializeField] public string functionName;
    [SerializeField] public CastType itemType;
    [SerializeField, TextArea()] public string effectText;

    [Header("アイコン")]
    [SerializeField] public Sprite Icon;
}