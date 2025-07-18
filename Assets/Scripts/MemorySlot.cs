using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NovelEditor;
using TMPro;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(Button)), RequireComponent(typeof(Image))]
public class MemorySlot : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private NovelData data;
    [SerializeField] private string memoryNameID;

    [Header("References")]
    [SerializeField] private Image btnImg;
    [SerializeField] private Button btn;
    [SerializeField] private TMP_Text memoryNameText;

    [Header("Debug")]
    [SerializeField] private bool isUnlocked;

    public void Initialization(Sprite disableSprite)
    {
        if (!ReferenceEquals(data, null)) // データがない場合は未開放ということにする
        {
            isUnlocked = PlayerPrefsManager.GetBool("memoryunlock:" + data.name, false);
        }

        btn.interactable = isUnlocked;
        if (!isUnlocked)
        {
            memoryNameText.text = LocalizationManager.Localize("System.Locked");
            btnImg.sprite = disableSprite;
        }
        else
        {
            btn.onClick.AddListener(OnClickButton);
            memoryNameText.text = LocalizationManager.Localize(memoryNameID);
        }
    }

    public void OnClickButton()
    {
        btn.interactable = false;
        AudioManager.Instance.PauseMusic();
        AudioManager.Instance.PlaySFX("SystemDecide");
        NovelSingletone.Instance.PlayNovel(data, true, ReturnFromMemory);
    }

    public void OnClosePanel()
    {
        btn.onClick.RemoveAllListeners();
    }

    public void ReturnFromMemory()
    {
        btn.interactable = true;
        AudioManager.Instance.UnpauseMusic();
    }
}
