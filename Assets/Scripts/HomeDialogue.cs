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
[CreateAssetMenu(fileName = "NewHomeCharacter", menuName = "�쐬/�z�[���䎌�L����")]
public class HomeDialogue : ScriptableObject
{
    [Header("�Z�[�u���[�h�Ή�")]
    [SerializeField] public string pathName;

    [Header("�f�[�^")]
    [SerializeField] public bool isDLCCharacter;
    [SerializeField] public Sprite characterSprite;
    [SerializeField] public List<HomeSceneDialogue> dialogueList;
}