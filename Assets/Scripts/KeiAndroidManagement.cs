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
        if (battleManager == null) return; // �o�g����ʂł͂Ȃ�

        kei = GetComponent<Battler>();
        kei.onDeathEvent.AddListener(OnDead);

        // ������
        for (int i = 0; i < spawnedAndroid.Length; i++)
        {
            spawnedAndroid[i] = null;
        }

        // ���������ł��郂�m�����[�h
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
        // �ǂ̃L��������������̂����߂�
        EnemyDefine targetSummon = possibleSpawn[UnityEngine.Random.Range(0, possibleSpawn.Count)];

        
        // ����
        GameObject obj = Instantiate<GameObject>(targetSummon.battler, kei.transform.parent);

        // �ʒu�����߂�
        Vector3 spawnPosOffset = GetSpawnPosition();
        obj.transform.localPosition = kei.RectTransform.localPosition + spawnPosOffset;
        obj.transform.SetSiblingIndex(GetSpawnSlot() == 0 ? 0 : kei.transform.GetSiblingIndex() + 1);

        // �G��������
        Battler component = obj.GetComponent<Battler>();
        component.InitializeEnemyData(targetSummon);
        component.onDeathEvent.AddListener(SpawnedAndroidDead);

        spawnedAndroid[GetSpawnSlot()] = component;

        // �^�[�����ʂ̍Ō���ɒu��
        battleManager.AddEnemy(component, targetSummon);

        // �퓬���O
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
        // �ǂ̈ʒu�ɐ������邩���f����
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
    ///  ���݂̏��������l��
    /// </summary>
    /// <returns></returns>
    public int GetCurrentSummonCount()
    {
        UpdateSummonCount();
        return spawnedAndroidCnt;
    }

    /// <summary>
    ///  ���݂̏��������l��
    /// </summary>
    /// <returns></returns>
    public Battler GetRandomSummon()
    {
        UpdateSummonCount();
        return spawnedAndroid[Random.Range(0, spawnedAndroid.Length)];
    }

    // ������񂪃��^�C�A
    public void OnDead()
    {
        // �����ƈꏏ�Ƀ^�|����
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
                    // �ŗ�����̋��ɒD��ꂽ
                    spawnedAndroid[i] = null;
                    spawnedAndroidCnt--;
                }
            }
        }
    }
}
