using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;

// どのキャラクターのターンになるのか管理するクラス
public class TurnBase : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 2.0f)] private float rearrangeAnimationTime = 1.0f;
    [SerializeField] private float startPosition = -350.0f;
    [SerializeField] private float gapSpace = 5.0f;

    [Header("References")]
    [SerializeField] private Image originIcon;

    [Header("Debug")]
    [SerializeField] private List<Battler> battlerList;
    [SerializeField] private List<Battler> deathBattler;
    [SerializeField] private List<Image> iconList;
    [SerializeField] private List<Tuple<Battler, Image>> characterInOrder;

    [HideInInspector] public List<Battler> DeathBattler { get { return deathBattler; } }

    public void Initialization(List<Battler> playerCharacters, List<Battler> enemies)
    {
        // 初期化
        characterInOrder = new List<Tuple<Battler, Image>>();

        // Listを合成する
        battlerList = new List<Battler>(playerCharacters); // 元のデータを影響しないためにコピーを作っとく
        deathBattler = new List<Battler>();
        battlerList = battlerList.Concat(enemies).ToList();

        // 死亡したものを排除
        battlerList.RemoveAll(s => s.isAlive == false);

        // 人数分のアイコンを生成する
        for (int i = 0; i < battlerList.Count; i++)
        {
            iconList.Add(Instantiate(originIcon, transform));
            iconList[i].color = Color.white;
            iconList[i].sprite = battlerList[i].icon;
            iconList[i].GetComponent<TurnBaseInformation>().Initialize(battlerList[i].character_color, battlerList[i].character_name, battlerList[i]);
            characterInOrder.Add(new Tuple<Battler, Image>(battlerList[i], iconList[i]));
        }

        // 行動順番を計算
        characterInOrder.Sort((a, b) =>
        {
            // 「素早さ」を比較する
           int speedComparison = b.Item1.speed.CompareTo(a.Item1.speed);
            if (speedComparison != 0)
            {
                return speedComparison;
            }
            else
            {
                // 「素早さ」が同じ場合、プレイヤーキャラクターを優先する
                return a.Item1.isEnemy.CompareTo(b.Item1.isEnemy);
            }
        });

        // DLC 特殊処理
        if (DLCManager.isDLCEnabled)
        {
            // ヒスイの聖核装備をそうびしてる者がいたら無理やり先端に持っていく
            var conditionCharacter = characterInOrder.FirstOrDefault(c => c.Item1.equipment != null && c.Item1.equipment.pathName == "Equip_Hisui");
            if (conditionCharacter != null)
            {
                // 条件を満たしたキャラクターをリストから削除して、先頭に追加
                characterInOrder.Remove(conditionCharacter);
                characterInOrder.Insert(0, conditionCharacter);
            }
        }

        // アイコンを並ぶ
        IconArrangeInstant();
    }

    // バトル途中から新規敵キャラを追加
    public void AddEnemy(Battler newEnemy, EnemyDefine data)
    {
        iconList.Add(Instantiate(originIcon, transform));
        iconList[iconList.Count - 1].color = Color.white;
        iconList[iconList.Count - 1].sprite = data.icon;
        iconList[iconList.Count - 1].GetComponent<TurnBaseInformation>().Initialize(data.character_color, newEnemy.character_name, newEnemy);

        characterInOrder.Add(new Tuple<Battler, Image>(newEnemy, iconList[iconList.Count - 1]));
        IconArrangeInstant();
    }

    /// <summary>
    /// 最初から更新
    /// </summary>
    public void UpdateTurn(bool rearrange)
    {
        // リタイアのキャラがいるかを確認
        for (int i = 0; i < characterInOrder.Count; i++)
        {
            if (!characterInOrder[i].Item1.isAlive)
            {
                // アイコンを非表示に
                characterInOrder[i].Item2.DOColor(new Color(0,0,0,0), rearrangeAnimationTime);

                // 戦闘ログ用　再起不能の告知
                deathBattler.Add(characterInOrder[i].Item1);

                characterInOrder.RemoveAt(i);
                i--;
            }
        }

        if (rearrange)
        {
            IconArrangeInstant();
        }
    }

    public Battler GetCurrentTurnBattler()
    {
        return characterInOrder.First().Item1;
    }

    // 次に順番が回ってくるプレイヤーキャラクターを獲得
    public Battler GetNextPlayerCharacter()
    {
        for (int i = 0; i < characterInOrder.Count; i++) 
        {
            if (!characterInOrder[i].Item1.isEnemy)
            {
                return characterInOrder[i].Item1;
            }
        }

        return null;
    }

    public void SetNextCharacter(Battler character)
    {
        for (int i = 0; i < characterInOrder.Count; i++)
        {
            if (characterInOrder[i].Item1 == character)
            {
                Tuple<Battler, Image> item = characterInOrder[i];
                characterInOrder.RemoveAt(i);
                characterInOrder.Insert(1, item);
                return;
            }
        }
    }

    // 全プレイヤーキャラを取得
    public List<Battler> GetAllPlayerCharacters()
    {
        var rtn = new List<Battler>();
        foreach (var character in characterInOrder)
        {
            if (!character.Item1.isEnemy)
            {
                rtn.Add(character.Item1);
            }
        }

        return rtn;
    }

    // 全敵を取得
    public List<Battler> GetAllEnemyCharacters()
    {
        var rtn = new List<Battler>();
        foreach (var character in characterInOrder)
        {
            if (character.Item1.isEnemy)
            {
                rtn.Add(character.Item1);
            }
        }

        return rtn;
    }

    // プレイヤーキャラクターをランダムに獲得
    public Battler GetRandomPlayerCharacter(bool mustBeTargettable)
    {
        Battler randomPlayer = characterInOrder[UnityEngine.Random.Range(0, characterInOrder.Count)].Item1;
        if (!randomPlayer.isEnemy && (randomPlayer.isTargettable || !mustBeTargettable))
        {
            return randomPlayer;
        }

        return GetRandomPlayerCharacter(mustBeTargettable);
    }

    // 敵キャラをランダムに取得
    public Battler GetRandomEnemyCharacter(bool mustBeTargettable)
    {
        Battler randomEnemy = characterInOrder[UnityEngine.Random.Range(0, characterInOrder.Count)].Item1;
        if (randomEnemy.isEnemy && (randomEnemy.isTargettable || !mustBeTargettable))
        {
            return randomEnemy;
        }

        return GetRandomEnemyCharacter(mustBeTargettable);
    }


    // 敵キャラをランダムに取得 (HP低い方を優先)
    public Battler GetEnemyCharacterWithLowestHP()
    {
        // Get the enemy with the lowest current_hp
        var lowestHpEnemy = characterInOrder
            .Where(tuple => tuple.Item1.isEnemy) // Filter out only enemy battlers
            .OrderBy(tuple => tuple.Item1.current_hp) // Order by current_hp in ascending order
            .FirstOrDefault(); // Take the first (lowest) enemy battler or null if the list is empty

        if (lowestHpEnemy == null)
        {
            // enemy list is empty
            Debug.Log("Enemy list is empty");
            return null;
        }

        return lowestHpEnemy.Item1;
    }

    /// アイコンを並ぶ
    private void IconArrangeInstant()
    {
        float iconPosition = startPosition;
        for (int i = 0; i < characterInOrder.Count; i++)
        {
            var iconRect = characterInOrder[i].Item2.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector3(iconPosition, iconRect.localPosition.y, iconRect.localPosition.z);
            iconPosition += iconRect.rect.width + gapSpace;
        }
    }

    public void NextBattler()
    {
        // Move the first character to the last position
        var firstElement = characterInOrder.First();
        characterInOrder.RemoveAt(0);
        characterInOrder.Add(firstElement);

        // UI
        firstElement.Item2.DOFade(0.0f, 0.5f);
        firstElement.Item2.GetComponent<RectTransform>().DOScale(2.0f, 0.5f);

        StartCoroutine(IconArrangeAnimation());
    }

    IEnumerator IconArrangeAnimation()
    {
        yield return new WaitForSeconds(rearrangeAnimationTime * 0.5f);

        // アイコンを再並ぶ
        float iconPosition = startPosition;
        for (int i = 0; i < characterInOrder.Count -1; i++)
        {
            var iconRect = characterInOrder[i].Item2.GetComponent<RectTransform>();
            iconRect.DOAnchorPosX(iconPosition, 0.5f);
            iconPosition += iconRect.rect.width + gapSpace;
        }

        yield return new WaitForSeconds(rearrangeAnimationTime * 0.5f);

        int lastIndex = characterInOrder.Count - 1;
        var lastIconRect = characterInOrder[lastIndex].Item2.GetComponent<RectTransform>();
        lastIconRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        lastIconRect.anchoredPosition = new Vector3(iconPosition + 200.0f, lastIconRect.localPosition.y, lastIconRect.localPosition.z);

        lastIconRect.DOAnchorPosX(iconPosition, 0.5f);
        characterInOrder[lastIndex].Item2.DOFade(1.0f, 0.5f);
    }
}
