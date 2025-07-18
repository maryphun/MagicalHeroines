using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCharacter", menuName = "作成/アクター生成")]
public class PlayerCharacterDefine : ScriptableObject
{
    [SerializeField] public PlayerCharacter detail;
    [SerializeField] public GameObject battler;
}