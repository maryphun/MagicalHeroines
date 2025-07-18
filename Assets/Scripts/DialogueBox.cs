using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;



public class DialogueBox : MonoBehaviour
{
    [SerializeField] private RectTransform characterLeft;
    [SerializeField] private RectTransform characterRight;
    [SerializeField] private RectTransform outline;
    [SerializeField] private RectTransform gray;
    [SerializeField] private RectTransform boxHolder;
    [SerializeField] private Image nameBox;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    List<DialoguePaku> registeredDialogue = new List<DialoguePaku>();
    int dialogueCnt = 0;

    //設定
    private float textShowInterval = 0.1f;
    private float tintedCharacterColor = 0.5f;
    private float nameBoxColorAlpha;

    public void Awake()
    {
        dialogueText.text = string.Empty; // clear
        nameText.text = string.Empty; // clear
        characterLeft.localPosition = new Vector3(-1137f, 333.64f, 0f);
        characterRight.localPosition = new Vector3(1137f, 333.64f, 0f);
        boxHolder.localPosition = new Vector3(0.0f, -330.0f, 0.0f);

        nameBoxColorAlpha = nameBox.color.a;
        nameBox.color = new Color(nameBox.color.r, nameBox.color.g, nameBox.color.b, 0.0f); // name box is invisible as default
    }

    public void StartDialogueBox()
    {
        registeredDialogue.Add(new DialoguePaku(DialogueDir.Left, "こんにちは！"));
        registeredDialogue.Add(new DialoguePaku(DialogueDir.Right, "テストテストテスト！　テスト！　てすと"));
        registeredDialogue.Add(new DialoguePaku(DialogueDir.Left, "漢字ひらがなカタナカROMAJIromaji。；："));
        registeredDialogue.Add(new DialoguePaku(DialogueDir.Left, "キャラ A", "名前表示する"));
        registeredDialogue.Add(new DialoguePaku("キャラクターなし"));
        dialogueCnt = 0;

        StartCoroutine(StartDialogueBoxAnim());
    }

    public void StopDialogueBox()
    {
        characterLeft.DOLocalMoveX(-1137f, 0.8f, false);
        characterRight.DOLocalMoveX(1137f, 0.8f, false);
        boxHolder.DOLocalMoveY(-330.0f, 0.8f, false);
    }

    IEnumerator StartDialogueBoxAnim()
    {
        // clear text
        dialogueText.text = string.Empty;
        nameText.text = string.Empty;

        // move parts
        boxHolder.DOLocalMoveY(0.0f, 0.8f, false);
        characterLeft.DOLocalMoveX(-888.0f, 0.8f, false);
        characterLeft.GetComponent<Image>().DOColor(new Color(tintedCharacterColor, tintedCharacterColor, tintedCharacterColor, 1.0f), 0.0f);
        characterRight.DOLocalMoveX(888.0f, 0.8f, false);
        characterRight.GetComponent<Image>().DOColor(new Color(tintedCharacterColor, tintedCharacterColor, tintedCharacterColor, 1.0f), 0.0f);

        // name box is invisible as default
        nameBox.color = new Color(nameBox.color.r, nameBox.color.g, nameBox.color.b, 0.0f); 

        yield return new WaitForSeconds(0.8f);

        while (dialogueCnt < registeredDialogue.Count)
        {
            int dialogueTextCnt = 0;

            // is name box needed?
            if (registeredDialogue[dialogueCnt].name != string.Empty)
            {
                nameBox.DOFade(nameBoxColorAlpha, 0.8f);
                nameText.DOFade(1.0f, 0.8f);
                nameText.text = registeredDialogue[dialogueCnt].name;
            }
            else
            {
                nameBox.DOFade(0.0f, 0.2f);
                nameText.DOFade(0.0f, 0.2f);
            }

            switch (registeredDialogue[dialogueCnt].fromDir)
            {
                case DialogueDir.Left:
                    characterLeft.DOLocalMoveX(-800.0f, 0.8f, false); 
                    characterRight.DOLocalMoveX(888.0f, 0.8f, false);
                    characterLeft.GetComponent<Image>().DOColor(new Color(1f, 1f, 1f, 1.0f), 0.0f);
                    characterRight.GetComponent<Image>().DOColor(new Color(tintedCharacterColor, tintedCharacterColor, tintedCharacterColor, 1.0f), 0.0f);
                    break;
                case DialogueDir.Right:
                    characterLeft.DOLocalMoveX(-888.0f, 0.8f, false);
                    characterRight.DOLocalMoveX(800.0f, 0.8f, false);
                    characterLeft.GetComponent<Image>().DOColor(new Color(tintedCharacterColor, tintedCharacterColor, tintedCharacterColor, 1.0f), 0.0f);
                    characterRight.GetComponent<Image>().DOColor(new Color(1f, 1f, 1f, 1.0f), 0.0f);
                    break;
                case DialogueDir.None:
                    characterLeft.DOLocalMoveX(-888.0f, 0.8f, false);
                    characterLeft.GetComponent<Image>().DOColor(new Color(tintedCharacterColor, tintedCharacterColor, tintedCharacterColor, 1.0f), 0.0f);
                    characterRight.DOLocalMoveX(888.0f, 0.8f, false);
                    characterRight.GetComponent<Image>().DOColor(new Color(tintedCharacterColor, tintedCharacterColor, tintedCharacterColor, 1.0f), 0.0f);
                    break;
            }

            while (dialogueTextCnt < registeredDialogue[dialogueCnt].text.Length)
            {
                dialogueTextCnt++;
                dialogueText.text = registeredDialogue[dialogueCnt].text.Substring(0, dialogueTextCnt);
                yield return new WaitForSeconds(textShowInterval);
            }

            while (!InputManager.Instance.GetIsKeyDown(KeyCode.Space) || !(Input.GetMouseButtonDown(0)))
            {
                yield return null;
            }

            dialogueCnt++;
        }
        
        StopDialogueBox();
    }
}
