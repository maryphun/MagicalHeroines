using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public struct BattlerAnimation
{
    public Sprite idle, attack, magic, item, attacked, retire;
}

[System.Serializable]
public struct BattlerSoundEffect
{
    public List<AudioClip> attack, attacked, retire;
}

public enum BattlerSoundEffectType
{
    Attack, Attacked, Retire,
}

[System.Serializable]
public enum BattlerAnimationType
{
    idle, attack, magic, item, attacked, retire,
}

[System.Serializable]
public struct PlayerCharacter
{
    // system
    [Header("システム")]
    public int characterID;
    public bool isDLCCharacter;

    // graphic
    [Header("画像")]
    public Sprite icon;
    public Sprite sprite;
    public BattlerAnimation animations;
    public Color color;

    [Header("戦闘関連")]
    // battle related
    public string nameID;
    public int starting_level;
    public int base_hp;
    public int base_mp;
    public int base_attack;
    public int base_defense;
    public int base_speed;
    public int hp_growth;
    public int mp_growth;
    public int attack_growth;
    public int defense_growth;
    public int speed_growth;

    [Header("育成")]
    public int levelUp_base_cost;
    public int levelUp_cost_increment;

    [Header("ステータス")]
    public bool is_heroin;      // ヒロイン
    public List<CharacterStatus> characterStatus;
    public string corruptedName; // 闇落ち後の名前
    public EquipmentDefine coreEquipment;

    [Header("特殊技リスト")]
    public List<Ability> abilities;
}

[System.Serializable]
public class Character
{
    [Header("セーブロード対応")]
    [SerializeField] public string pathName;

    [Header("資料")]
    public PlayerCharacter characterData; // 基本資料
    public GameObject battler; // 戦闘像
    public int corruptionEpisode;
    public int hornyEpisode;
    public int holyCoreEpisode;

    public string localizedName;
    public int current_level;
    public int current_maxHp;
    public int current_maxMp;
    public int current_hp;
    public int current_mp;
    public int current_attack;
    public int current_defense;
    public int current_speed;

    public bool is_corrupted;    // 闇落ちしたか

    public CharacterStatus GetCurrentStatus()
    {
        if (!characterData.is_heroin)
        {
            // 普通戦闘員
            return new CharacterStatus(characterData.sprite);
        }

        for (int i = characterData.characterStatus.Count-1; i >= 0; i--)
        {
            if (corruptionEpisode >= characterData.characterStatus[i].requiredCorruptionEpisode
             && hornyEpisode >= characterData.characterStatus[i].requiredHornyEpisode)
            {
                return characterData.characterStatus[i];
            }
        }

        // どれも満たさない場合は初期値を返す
        return characterData.characterStatus[0];
    }
}

[System.Serializable]
public struct CharacterStatus
{
    [SerializeField] public string moodNameID;
    [SerializeField] public Color textColor;
    [SerializeField] public Sprite character;
    [SerializeField] public int requiredCorruptionEpisode;
    [SerializeField] public int requiredHornyEpisode;

    public CharacterStatus(Sprite sprite)
    {
        moodNameID = string.Empty;
        textColor = Color.white;
        character = sprite;
        requiredCorruptionEpisode = 0;
        requiredHornyEpisode = 0;
    }
}

/// <summary>
/// パーティー管理用
/// </summary>
[System.Serializable]
public class FormationSlotData
{
    public int characterID;
    public bool isFilled;

    public FormationSlotData(int ID, bool flag)
    {
        characterID = ID;
        isFilled = flag;
    }
}

[System.Serializable]
public enum PlayerCharacerID
{
    Battler = 0,
    TentacleMan = 1,
    Clone = 2,
    Akiho = 3,
    Rikka = 4,
    Erena = 5,
    Kei = 6,
    Nayuta = 7,
    BattlerTemp = 8,
    No5 = 9,
    No72 = 10,
    Hisui = 11,
    Daiya = 12,
}