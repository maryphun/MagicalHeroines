using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct HomeSceneDialogue
{
    public int startStage;
    public int endStage;
    public string dialogueID;
    public AudioClip clip;
    public Sprite sprite;
}

[System.Serializable]
[CreateAssetMenu(fileName = "NewHomeCharacter", menuName = "作成/ホーム台詞キャラ")]
public class HomeDialogue : ScriptableObject
{
    [Header("セーブロード対応")]
    [SerializeField] public string pathName;

    [Header("データ")]
    [SerializeField] public bool isDLCCharacter;
    [SerializeField] public Sprite characterSprite;
    [SerializeField] public List<HomeSceneDialogue> dialogueList;
}