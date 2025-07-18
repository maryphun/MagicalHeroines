using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class FormationPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;
    [SerializeField, Range(0.0f, 1.0f)] private float formationSelectionPanelAnimationTime = 0.15f;
    [SerializeField] private int[] moneyCostForSlot = new int[5];
    [SerializeField] private float iconGap = 200.0f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup FormationSelectionPanel;
    [SerializeField] private FormationSlot[] slots = new FormationSlot[5];
    [SerializeField] private Button[] formationSelectIcon = new Button[8];
    [SerializeField] private FormationTutorial tutorial;
    [SerializeField] private GameObject DLC_FormationSelectionPanel; // �ǉ��L����
    [SerializeField] private DLCFormationIcon[] DLC_formationSelectIcon = new DLCFormationIcon[2];

    [Header("Debug")]
    [SerializeField] private int formationSelectionPanelIndex = 0;  // �ҏW���̃L�����ʒu�ԍ�
    [SerializeField] public bool isFormationSelecting;  // �L�����Ґ���

    public void OpenFormationPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        FormationSelectionPanel.interactable = false;
        FormationSelectionPanel.blocksRaycasts = false;
        FormationSelectionPanel.alpha = 0.0f;
        isFormationSelecting = false;

        formationSelectionPanelIndex = -1;

        InitializeFormation();

        // Enter tutorial
        var tutorialData = ProgressManager.Instance.GetTutorialData();
        if (tutorialData.formationPanel == false)
        {
            tutorialData.formationPanel = true;
            ProgressManager.Instance.SetTutorialData(tutorialData);

            tutorial.StartTutorial();
        }
        else
        {
            tutorial.gameObject.SetActive(false);
        }
    }

    public void QuitFormationPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        FormationSelectionPanel.interactable = false;
        FormationSelectionPanel.blocksRaycasts = false;
        FormationSelectionPanel.alpha = 0.0f;
        isFormationSelecting = false;

        formationSelectionPanelIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ResetData(animationTime);
        }
    }

    public void InitializeFormation()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool isLocked = ProgressManager.Instance.GetUnlockedFormationCount() <= i;
            slots[i].Initialize(isLocked, moneyCostForSlot[i], ProgressManager.Instance.GetUnlockedFormationCount() == i, i);

            var party = ProgressManager.Instance.GetFormationParty(false);
            if (party[i].isFilled)
            {
                slots[i].SetBattler(ProgressManager.Instance.GetCharacterByID(party[i].characterID));
            }
        }
    }

    public void UpdateFormation(int slot, int characterID, bool isNull, bool UpdateUI)
    {
        var originalData = ProgressManager.Instance.GetFormationParty(true);
        originalData[slot].characterID = characterID;
        originalData[slot].isFilled = !isNull;

        if (UpdateUI)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].ResetData();
            }
            InitializeFormation();
        }
    }

    public void UnlockSlot()
    {
        // �������X���b�g
        int slotIndex = ProgressManager.Instance.GetUnlockedFormationCount();

        // �����R�X�g
        ProgressManager.Instance.SetMoney(ProgressManager.Instance.GetCurrentMoney() - moneyCostForSlot[slotIndex]);

        // ���
        ProgressManager.Instance.UnlockedFormationCount();

        // UI�X�V
        slots[slotIndex].Initialize(false, moneyCostForSlot[slotIndex], false, slotIndex);

        if (slotIndex < slots.Length-1)
        {
            slots[slotIndex + 1].Initialize(true, moneyCostForSlot[slotIndex + 1], true, slotIndex + 1);
        }

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemUnlock", 1.5f);

        // ������\����
        slots[slotIndex + 1].HideResourcesPanel(1.5f);
    }

    public void OpenFormationSelectionPanel(int targetSlotIndex)
    {
        isFormationSelecting = true;
        FormationSelectionPanel.interactable = true;
        FormationSelectionPanel.blocksRaycasts = true;
        FormationSelectionPanel.DOFade(1.0f, formationSelectionPanelAnimationTime);

        var allCharacters = ProgressManager.Instance.GetAllCharacter(false, false);
        var usableCharacters = ProgressManager.Instance.GetAllUsableCharacter(false); // �g�p�ł���L�����N�^�[������

        float totalGap = iconGap * (usableCharacters.Count-1);
        float firstPosition = -totalGap * 0.5f;
        float nextposition = 0f;
        for (int i = 0; i < formationSelectIcon.Length; i++)
        {
            // �ғ��ł������
            if (!usableCharacters.Any(x => x.characterData.characterID == i)) continue; // ���̃L�����͂܂������Ă��Ȃ�
            if (allCharacters[i].characterData.is_heroin && !allCharacters[i].is_corrupted) continue; // �܂��ŗ����ł��Ă��Ȃ�

            // �ғ��ł���L����
            {
                // �����Ă���
                formationSelectIcon[i].gameObject.SetActive(true);

                // �{�^���z�u
                float buttonPosition = firstPosition + nextposition;
                nextposition += iconGap;
                formationSelectIcon[i].GetComponent<RectTransform>().localPosition = new Vector3(buttonPosition, 0.0f, 0.0f);

                // ���łɔz�u����Ă��邩
                bool isAlreadyInFormation = IsCharacterInFormation(allCharacters[i]);
                formationSelectIcon[i].interactable = !isAlreadyInFormation;

                // �L��������\��
                formationSelectIcon[i].GetComponentInChildren<TMP_Text>().text = allCharacters[i].localizedName;
            }
        }

        // DLC�ǉ��L����
        if (DLCManager.isDLCEnabled)
        {
            allCharacters = ProgressManager.Instance.GetAllCharacter(false, true);
            usableCharacters = ProgressManager.Instance.GetAllUsableCharacter(true); // �g�p�ł���L�����N�^�[������

            if (usableCharacters.Any(x => x.characterData.isDLCCharacter))
            {
                DLC_FormationSelectionPanel.SetActive(true);
                List<Character> dlc_members = usableCharacters.Where(x => x.characterData.isDLCCharacter).ToList();

                // �A�C�R���ʒu�v�Z
                totalGap = iconGap * (dlc_members.Count - 1);
                firstPosition = -totalGap * 0.5f;
                nextposition = 0f;

                for (int i = 0; i < DLC_formationSelectIcon.Length; i++)
                {
                    int targetCharacterID = DLC_formationSelectIcon[i].characterID;
                    if (!ProgressManager.Instance.HasCharacter(targetCharacterID)) continue; // �����ĂȂ�

                    // �����Ă���
                    DLC_formationSelectIcon[i].gameObject.SetActive(true);

                    // ���łɔz�u����Ă��邩
                    var targetCharacter = ProgressManager.Instance.GetCharacterByID(targetCharacterID);
                    bool isAlreadyInFormation = IsCharacterInFormation(targetCharacter);
                    DLC_formationSelectIcon[i].button.interactable = !isAlreadyInFormation;

                    // �{�^���z�u
                    float buttonPosition = firstPosition + nextposition;
                    nextposition += iconGap;
                    DLC_formationSelectIcon[i].GetComponent<RectTransform>().localPosition = new Vector3(buttonPosition, 0.0f, 0.0f);

                    // �L��������\��
                    DLC_formationSelectIcon[i].name.text = targetCharacter.localizedName;
                }
            }
            else
            {
                DLC_FormationSelectionPanel.SetActive(false);
            }
        }

        formationSelectionPanelIndex = targetSlotIndex;
    }

    public void CloseFormationSelectionPanel(bool isPlaySE)
    {
        isFormationSelecting = false;
        FormationSelectionPanel.interactable = false;
        FormationSelectionPanel.blocksRaycasts = false;
        FormationSelectionPanel.DOFade(0.0f, formationSelectionPanelAnimationTime);

        formationSelectionPanelIndex = -1;

        // SE�Đ�
        if (isPlaySE) AudioManager.Instance.PlaySFX("SystemCancel", 0.5f);
    }

    public void SelectFormationCharacter(int characterID)
    {
        UpdateFormation(formationSelectionPanelIndex, characterID, false, true);

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemEquip", 1.5f);

        CloseFormationSelectionPanel(false);
    }

    private bool IsCharacterInFormation(Character character)
    {
        var party = ProgressManager.Instance.GetFormationParty(false);
        foreach (FormationSlotData data in party)
        {
            if (data.isFilled && data.characterID == character.characterData.characterID)
            {
                return true;
            }
        }
        return false;
    }

    public int GetUnlockCost(int slotIndex)
    {
        return moneyCostForSlot[slotIndex];
    }
}
