using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CharacterBuildingPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CharacterDataPanel characterDataPanel;
    [SerializeField] private CharacterUpgradePanel characterUpgradePanel;
    [SerializeField] private GameObject characterDataButton;
    [SerializeField] private GameObject characterUpgradeButton;
    [SerializeField] private Image[] characterIconSlots;
    [SerializeField] private RectTransform pinkPanel;
    [SerializeField] private CharacterBuildingPanelTutorial tutorial;
    [SerializeField] private RectTransform characterSelectionScrollContent;
    [SerializeField] private GameObject[] DLCcharactersIcon;

    [Header("Debug")]
    [SerializeField] List<Character> characters;
    [SerializeField, HideInInspector] private float tabLocalPosY;
    [SerializeField] private int currentCheckingSlot = 0;

    private Color _darkenedTabColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
    const float _pinkPanelShakeTime = 0.1f;
    const float _pinkPanelShakeMagnitude = 2.5f;
    public int CurrentCheckingSlot { get { return IndexToCharacterID(currentCheckingSlot); } }

    private void Start()
    {
        tabLocalPosY = characterDataButton.GetComponent<RectTransform>().localPosition.y;

        if (DLCManager.isDLCEnabled)
        {
            characterSelectionScrollContent.sizeDelta = new Vector2(characterSelectionScrollContent.sizeDelta.x, 1100.0f);
            foreach (GameObject icon in DLCcharactersIcon)
            {
                icon.SetActive(true);
            }
        }
    }

    public void OpenCharacterBuildingPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // ������
        currentCheckingSlot = 0;

        // �L�����N�^�[�������擾���ĕ\������
        characters = ProgressManager.Instance.GetAllCharacter(false, true);
        for (int i = 0; i < characters.Count; i++)
        {
            int index = CharacterIDToIndex(characters[i].characterData.characterID);

            if (characters[i].is_corrupted || !characters[i].characterData.is_heroin) // �q���C���L�����͈ŗ���������̂ݎg�p�䗗�ł���
            {
                // �L�����N�^�[�����݂��Ă���Ȃ�A�C�R���𔒂�����
                characterIconSlots[index].transform.Find("Character").GetComponent<Image>().color = Color.white;
            }
        }

        for (int i = 0; i < characterIconSlots.Length; i++)
        {
            Color tmp = (i == currentCheckingSlot) ? Color.white : new Color(1, 1, 1, 0);
            characterIconSlots[i].transform.Find("Selection Highlight").GetComponent<Image>().color = tmp;
        }
        SwitchToCharacterDataTab(false);

        // Enter tutorial
        var tutorialData = ProgressManager.Instance.GetTutorialData();
        if (tutorialData.characterbuildingPanel == false)
        {
            tutorialData.characterbuildingPanel = true;
            ProgressManager.Instance.SetTutorialData(tutorialData);

            tutorial.StartTutorial();
        }
        else
        {
            tutorial.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void QuitCharacterBuildingPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // COPY�������̂��폜
        this.characters.Clear();
        this.characters = null;
    }

    /// <summary>
    /// �����^�u
    /// </summary>
    public void SwitchToCharacterDataTab(bool playSE = true)
    {
        characterDataButton.GetComponent<Image>().color = Color.white;
        characterDataButton.GetComponent<Button>().interactable = false;
        characterDataButton.GetComponent<RectTransform>().localPosition = new Vector3(characterDataButton.GetComponent<RectTransform>().localPosition.x, 
                                                                                      tabLocalPosY, 
                                                                                      characterDataButton.GetComponent<RectTransform>().localPosition.z);
        characterUpgradeButton.GetComponent<Image>().color = _darkenedTabColor;
        characterUpgradeButton.GetComponent<Button>().interactable = true;
        characterUpgradeButton.GetComponent<RectTransform>().localPosition = new Vector3(characterUpgradeButton.GetComponent<RectTransform>().localPosition.x,
                                                                                         tabLocalPosY - 20f,
                                                                                         characterUpgradeButton.GetComponent<RectTransform>().localPosition.z);

        characterDataPanel.gameObject.SetActive(true);
        characterUpgradePanel.gameObject.SetActive(false);

        if (playSE)
        {
            // SE
            AudioManager.Instance.PlaySFX("SystemTab");
        }

        // �����X�V
        characterDataPanel.InitializeCharacterData(characters[currentCheckingSlot]);
    }

    /// <summary>
    /// �琬�^�u
    /// </summary>
    public void SwitchToCharacterUpgradeTab(bool playSE = true)
    {
        characterUpgradeButton.GetComponent<Image>().color = Color.white;
        characterUpgradeButton.GetComponent<Button>().interactable = false;
        characterUpgradeButton.GetComponent<RectTransform>().localPosition = new Vector3(characterUpgradeButton.GetComponent<RectTransform>().localPosition.x,
                                                                                        tabLocalPosY,
                                                                                        characterUpgradeButton.GetComponent<RectTransform>().localPosition.z);
        characterDataButton.GetComponent<Image>().color = _darkenedTabColor;
        characterDataButton.GetComponent<Button>().interactable = true;
        characterDataButton.GetComponent<RectTransform>().localPosition = new Vector3(characterDataButton.GetComponent<RectTransform>().localPosition.x,
                                                                                      tabLocalPosY - 20f,
                                                                                      characterDataButton.GetComponent<RectTransform>().localPosition.z);

        characterUpgradePanel.gameObject.SetActive(true);
        characterDataPanel.gameObject.SetActive(false);

        if (playSE)
        {
            // SE
            AudioManager.Instance.PlaySFX("SystemTab");
        }

        // �����X�V
        characterUpgradePanel.InitializeUpgradePanel(characters[currentCheckingSlot]);
        characterUpgradePanel.ResetAnimation();
    }

    /// <summary>
    /// SE�Đ�
    /// </summary>
    public void OnSwitchPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTab");
    }

    /// <summary>
    /// �L�����ύX
    /// </summary>
    public void ChangeCharacterSlot(int characterID)
    {
        if (!ProgressManager.Instance.HasCharacter(characterID, true))
        {
            // ���̃L�����͂܂��ғ�����ĂȂ�
            Debug.Log("Character not available. CharacterID : " + characterID.ToString());
            return;
        }

        var character = ProgressManager.Instance.GetCharacterByID(characterID);

        if (character.characterData.is_heroin && !character.is_corrupted) return; // �܂��ŗ�������Ă��Ȃ��q���C���L�����͊m�F�ł��Ȃ�

        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemSelect");

        characterIconSlots[currentCheckingSlot].transform.Find("Selection Highlight").GetComponent<Image>().DOFade(0.0f, 0.1f);
        currentCheckingSlot = CharacterIDToIndex(characterID);
        characterIconSlots[currentCheckingSlot].transform.Find("Selection Highlight").GetComponent<Image>().DOFade(1.0f, 0.1f);
        characterDataPanel.InitializeCharacterData(character);
        characterUpgradePanel.InitializeUpgradePanel(character);
        characterUpgradePanel.ResetAnimation();
    }

    public int CharacterIDToIndex(int characterID)
    {
        int[] index = { 0, 1, 2, 3, 4, 5, 6, 7, -1, -1, -1, 9, 8, -1 };

        return index[characterID];
    }
    public int IndexToCharacterID(int index)
    {
        int[] characterID = { 0, 1, 2, 3, 4, 5, 6, 7, 12, 11 };

        return characterID[index];
    }
}

