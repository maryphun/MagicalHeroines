using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[System.Serializable]
enum CommandType
{
    Waiting,    // �R�}���h�҂�
    Attack,
    Item,
    Ability,
}

public class ActionPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animTime = 0.5f;
    [SerializeField] private Texture2D cursorTexture;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private Battle battleManager;
    [SerializeField] private TMPro.TMP_Text tipsText;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private AbilityPanel abilityPanel;
    [SerializeField] private Button attackBtn;
    [SerializeField] private TMPro.TMP_Text cannotAttackText; // �U���ł��Ȃ��e�L�X�g
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Button escapeBtn;

    [Header("Debug")]
    [SerializeField] private bool isSelectingTarget;
    [SerializeField] private bool isSelectedTarget;
    [SerializeField] private CommandType commandType;
    [SerializeField] private bool isCannotAttackTextShowing;

    public bool IsEnabled { get { return canvasGrp.interactable; } }

    private void Awake()
    {
        canvasGrp.alpha = 0.0f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        isSelectingTarget = false;
        isCannotAttackTextShowing = false;
        cannotAttackText.gameObject.SetActive(false);
    }

    public void SetEnablePanel(bool boolean)
    {
        canvasGrp.DOFade(boolean ? 1.0f:0.0f, animTime);
        canvasGrp.interactable = boolean;
        canvasGrp.blocksRaycasts = boolean;

        escapeBtn.interactable = boolean;

        isSelectingTarget = false;

        if (boolean) // ���͑҂�
        {
            commandType = CommandType.Waiting;

            // ���ʍU�����o���Ȃ�Battler�Ȃ�{�^���𖳌��ɂ���
            attackBtn.interactable = battleManager.GetCurrentBattler().EnableNormalAttack;
            cannotAttackText.gameObject.SetActive(!attackBtn.interactable);
            cannotAttackText.color = CustomColor.invisible();
            cannotAttackText.rectTransform.anchoredPosition = Vector3.zero;
            isCannotAttackTextShowing = false;
        }
        else if (!attackBtn.interactable)
        {
            cannotAttackText.DOFade(0.0f, animTime);
        }
    }

    public void OnClickAttack()
    {
        // �U�������I��
        isSelectingTarget = true;

        canvasGrp.alpha = 0.25f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        escapeBtn.interactable = false;

        // �J�[�\����ύX
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f), CursorMode.Auto);

        // tips�\��
        tipsText.DOFade(1.0f, 0.25f);

        // �U��
        commandType = CommandType.Attack;

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemActionPanel");
    }

    public void OnClickItem()
    {
        // �C���x���g�����j���[��\��
        Inventory.Instance.obj.OpenInventory(true, CloseInventory);

        // ActionPanel����֎~
        canvasGrp.alpha = 0.25f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        escapeBtn.interactable = false;

        // �A�C�e��
        commandType = CommandType.Item;

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemActionPanel");
    }

    /// <summary>
    /// Callback: �C���x���g�����j���[������ꂽ�玩���I�ɌĂ΂��
    /// </summary>
    public void CloseInventory()
    {
        // ActionPanel����ɂ��ǂ�
        canvasGrp.alpha = 1.0f;
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        escapeBtn.interactable = true;

        commandType = CommandType.Waiting;

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemActionCancel");
    }

    public void OnClickSkill()
    {
        // ����Z���X�g��\��
        abilityPanel.SetupAbilityData(battleManager.GetCurrentBattler().abilities);
        abilityPanel.OpenPanel(OnCloseSkillPanel);

        // ActionPanel����֎~
        canvasGrp.DOFade(0.5f, abilityPanel.GetAnimTime());
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        escapeBtn.interactable = false;

        // ����Z
        commandType = CommandType.Ability;

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemActionPanel");
    }
    
    /// <summary>
    /// Callback: ����Z���X�g������ꂽ�玩���I�ɌĂ΂��
    /// </summary>
    public void OnCloseSkillPanel()
    {
        // ActionPanel����ɂ��ǂ�
        canvasGrp.DOFade(1.0f, abilityPanel.GetAnimTime());
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        escapeBtn.interactable = true;

        commandType = CommandType.Waiting;

        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemActionCancel");
    }

    public void OnClickIdle()
    {
        // ActionPanel����֎~
        canvasGrp.DOFade(0.5f, abilityPanel.GetAnimTime());
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        // �X�L�b�v�^�[��
        battleManager.IdleCommand();
    }

    private void Update()
    {
        if (commandType == CommandType.Item && !isSelectingTarget) return; // �A�C�e���I��

        if (commandType == CommandType.Ability && !isSelectingTarget) return; // ����Z�I��

        if (isSelectingTarget)
        {
            // �E�N���b�N
            if (Input.GetMouseButtonDown(1))
            {
                // SE�Đ�
                AudioManager.Instance.PlaySFX("SystemActionCancel");

                CancelAttack();
            }
            else
            {
                // arrow that follow the mouse
                Vector3 mousePosition = Input.mousePosition / mainCanvas.scaleFactor;

                var targetBattler = battleManager.GetBattlerByPosition(mousePosition, false, true, true);

                if (!ReferenceEquals(targetBattler, null))
                {
                    isSelectedTarget = true;
                    battleManager.PointTargetWithArrow(targetBattler, 0.25f);

                    if (Input.GetMouseButtonDown(0))
                    {
                        switch (commandType)
                        {
                            case CommandType.Attack: // �U��
                                battleManager.AttackCommand(targetBattler);
                                CancelAttack();
                                SetEnablePanel(false);
                                break;
                            case CommandType.Item: // �A�C�e��

                                break;
                            case CommandType.Ability: // ����Z

                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (isSelectedTarget)
                {
                    battleManager.UnPointArrow(0.25f);
                }
            }
        }

        // �U���ł��Ȃ����b�Z�[�W
        if (!attackBtn.interactable)
        {
            var evtSystem = (CustomStandaloneInputModule)eventSystem.currentInputModule;
            var hovering = evtSystem.GetPointerData().hovered;
            const float animationTime = 0.5f;
            if (hovering.Contains(attackBtn.gameObject) && !isCannotAttackTextShowing)
            {
                isCannotAttackTextShowing = true;
                cannotAttackText.DOColor(Color.white, animationTime);
                cannotAttackText.rectTransform.DOAnchorPosY(cannotAttackText.fontSize * 2.0f, animationTime);
                cannotAttackText.text = Assets.SimpleLocalization.Scripts.LocalizationManager.Localize("Battle.CannotAttack");
            }
            else if (!hovering.Contains(attackBtn.gameObject) && isCannotAttackTextShowing)
            {
                isCannotAttackTextShowing = false;
                cannotAttackText.DOColor(CustomColor.invisible(), animationTime);
                cannotAttackText.rectTransform.DOAnchorPosY(0.0f, animationTime);
            }
        }
    }

    private void CancelAttack()
    {
        if (commandType != CommandType.Attack) return;

        // ������
        canvasGrp.alpha = 1f;
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;
        
        escapeBtn.interactable = true;
        
        isSelectingTarget = false;

        // �J�[�\����߂�
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // tips������
        tipsText.DOFade(0.0f, 0.25f);

        if (isSelectedTarget)
        {
            battleManager.UnPointArrow(0.25f);
        }
    }
}
