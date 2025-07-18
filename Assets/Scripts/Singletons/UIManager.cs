using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DialogueDir
{
    Left,
    Right,
    None,
}

public struct DialoguePaku
{
    public DialoguePaku(DialogueDir dir, string txt)
    {
        fromDir = dir;
        text = txt;
        name = string.Empty;
    }

    public DialoguePaku(string txt)
    {
        fromDir = DialogueDir.None;
        text = txt;
        name = string.Empty;
    }

    public DialoguePaku(string txt, string _name)
    {
        fromDir = DialogueDir.None;
        text = txt;
        name = _name;
    }

    public DialoguePaku(DialogueDir dir, string _name, string txt)
    {
        fromDir = dir;
        text = txt;
        name = _name;
    }

    public DialogueDir fromDir;
    public string text;
    public string name;
}

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    [Header("References")]
    [SerializeField] private DialogueBox prefabOrigin;

    DialogueBox dialogue;

    bool isDialogueBoxSpawned = false;
    bool isDialogueStarted = false;

    public void Initialize()
    {
        Debug.Log("Initialize UI manager");
        prefabOrigin = Resources.Load<DialogueBox>("Prefabs/DialogueBox");
    }

    public void StartDialogueBox(Canvas canvas)
    {
        if (isDialogueStarted)
        {
            Debug.Log("ダイアログは既に表示中です。");
            return;
        }

        if (!isDialogueBoxSpawned)
        {
            isDialogueBoxSpawned = true;
            dialogue = Instantiate(prefabOrigin, canvas.transform);
            Object.DontDestroyOnLoad(dialogue);
        }

        dialogue.StartDialogueBox();
        isDialogueStarted = true;
    }

    public void StopDialogueBox()
    {
        if (!isDialogueStarted || !isDialogueBoxSpawned)
        {
            Debug.Log("ダイアログは存在していません。");
            return;
        }

        if (!Object.ReferenceEquals(dialogue, null) && !(dialogue is null))
        {
            dialogue.StopDialogueBox();
        }

        isDialogueStarted = false;
    }
}