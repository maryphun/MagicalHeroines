using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(Battler))]
public class KeiAndroidManagement : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Battler kei;
    [SerializeField] private Battler[] spawnedAndroid = new Battler[2];
    [SerializeField] private Battle battleManager;
    [SerializeField] private List<EnemyDefine> possibleSpawn;
    [SerializeField] private int spawnedAndroidCnt;

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<Battle>();
        if (battleManager == null) return; // バトル画面ではない

        kei = GetComponent<Battler>();
        kei.onDeathEvent.AddListener(OnDead);

        // 初期化
        for (int i = 0; i < spawnedAndroid.Length; i++)
        {
            spawnedAndroid[i] = null;
        }

        // 京が召喚できるモノをロード
        possibleSpawn = new List<EnemyDefine>();
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "Drone 4"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "Android 1"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "GoldDrone 2"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "GoldAndroid 3"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "DarkAndroid 2"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "DarkAndroid 3"));
    }

    public Battler SpawnNewAndroid()
    {
        // どのキャラを召喚するのか決める
        EnemyDefine targetSummon = possibleSpawn[UnityEngine.Random.Range(0, possibleSpawn.Count)];

        
        // 生成
        GameObject obj = Instantiate<GameObject>(targetSummon.battler, kei.transform.parent);

        // 位置を決める
        Vector3 spawnPosOffset = GetSpawnPosition();
        obj.transform.localPosition = kei.RectTransform.localPosition + spawnPosOffset;
        obj.transform.SetSiblingIndex(GetSpawnSlot() == 0 ? 0 : kei.transform.GetSiblingIndex() + 1);

        // 敵を初期化
        Battler component = obj.GetComponent<Battler>();
        component.InitializeEnemyData(targetSummon);
        component.onDeathEvent.AddListener(SpawnedAndroidDead);

        spawnedAndroid[GetSpawnSlot()] = component;

        // ターン順位の最後尾に置く
        battleManager.AddEnemy(component, targetSummon);

        // 戦闘ログ
        battleManager.AddBattleLog(Assets.SimpleLocalization.Scripts.LocalizationManager.Localize("BattleLog.KeiSpawn").Replace("{0}", component.CharacterNameColored).Replace("{1}", kei.CharacterNameColored));
        
        spawnedAndroidCnt++;

        return component;
    }

    private int GetSpawnSlot()
    {
        for (int i = 0; i < spawnedAndroid.Length; i++)
        {
            if (spawnedAndroid[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    private Vector2 GetSpawnPosition()
    {
        // どの位置に生成するか判断する
        if (GetSpawnSlot() == 0)
        {
            return new Vector2(-150.0f, 100.0f);
        }
        else if (GetSpawnSlot() == 1)
        {
            return new Vector2(150.0f, -100.0f);
        }

        Debug.LogWarning("More than 2 summon from kei");
        return Vector2.zero;
    }

    /// <summary>
    ///  現在の召喚数を獲得
    /// </summary>
    /// <returns></returns>
    public int GetCurrentSummonCount()
    {
        UpdateSummonCount();
        return spawnedAndroidCnt;
    }

    /// <summary>
    ///  現在の召喚数を獲得
    /// </summary>
    /// <returns></returns>
    public Battler GetRandomSummon()
    {
        UpdateSummonCount();
        return spawnedAndroid[Random.Range(0, spawnedAndroid.Length)];
    }

    // 京ちゃんがリタイア
    public void OnDead()
    {
        // けいと一緒にタ倒られる
        for (int i = 0; i < spawnedAndroid.Length; i ++)
        {
            if (spawnedAndroid[i] != null)
            {
                spawnedAndroid[i].KillBattler();
            }
        }
    }

    public void SpawnedAndroidDead()
    {
        UpdateSummonCount();
        for (int i = 0; i < spawnedAndroid.Length; i++)
        {
            if (spawnedAndroid[i] != null && !spawnedAndroid[i].isAlive)
            {
                //Destroy(spawnedAndroid[i].gameObject, 1.3f);
                    spawnedAndroid[i].GetGraphicRectTransform().localScale = Vector3.zero;
                    spawnedAndroid[i] = null;
                    spawnedAndroidCnt--;
                
            }
        }
    }

    public void UpdateSummonCount()
    {
        for (int i = 0; i < spawnedAndroid.Length; i++)
        {
            if (spawnedAndroid[i] != null && spawnedAndroid[i].isAlive)
            {
                if (spawnedAndroid[i].GetComponent<KeiControlledUnit>() != null)
                {
                    // 闇落ち後の京に奪われた
                    spawnedAndroid[i] = null;
                    spawnedAndroidCnt--;
                }
            }
        }
    }
}
