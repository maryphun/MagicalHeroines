using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using Assets.SimpleLocalization.Scripts;

public class AbilityPanel : MonoBehaviour
{
    [Header("Header")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.25f;

    [Header("References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Button abilityButton;
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform closeButton;
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private CanvasGroup descriptionPanel;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Battle battleManager;
    [SerializeField] private ActionPanel actionPanel;
    [SerializeField] private TMP_Text description_Name;   //<�@�Z��
    [SerializeField] private TMP_Text description_SPCost; //<�@SP����
    [SerializeField] private TMP_Text description_Cooldown; //<�@SP����
    [SerializeField] private TMP_Text description_Target; //<�@���ʑΏ�
    [SerializeField] private TMP_Text description_Type;   //<�@�@�\
    [SerializeField] private TMP_Text description_Info;   //<�@�Z����

    [Header("Debug")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isHiding = false;
    [SerializeField] private Action onCloseCallback;
    [SerializeField] private Tuple<Ability, ActionPanelAbilityButton>[] buttonList;
    [SerializeField] private bool isDescriptionShowing = false;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, -(rectTransform.sizeDelta.y+closeButton.sizeDelta.y), 0.0f);
        isOpen = false;
        isHiding = false;
        isDescriptionShowing = false;
        abilityButton.gameObject.SetActive(false);

        canvasGrp.alpha = 0.0f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;
    }

    public void SetupAbilityData(List<Ability> abilities)
    {
        if (abilities.Count == 0) // ����Z��������Ă��Ȃ��ꍇ
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 100.0f);
            text.gameObject.SetActive(true);
            text.rectTransform.anchoredPosition = Vector3.zero;
            
            // setup close button
            closeButton.anchoredPosition = new Vector3(closeButton.anchoredPosition.x, (rectTransform.sizeDelta.y * 0.5f) + (closeButton.sizeDelta.y * 0.5f), 0.0f);

            return;
        }
        text.gameObject.SetActive(false);

        const float spacePerAbility = 105.0f;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (abilities.Count+1) * spacePerAbility);
        
        buttonList = new Tuple<Ability, ActionPanelAbilityButton>[abilities.Count];
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i] = new Tuple<Ability, ActionPanelAbilityButton>(abilities[i], GameObject.Instantiate(abilityButton.gameObject, abilityButton.transform.parent).GetComponent<ActionPanelAbilityButton>());

            string abilityName = LocalizationManager.Localize(abilities[i].abilityNameID);
            buttonList[i].Item2.gameObject.name = abilityName;
            buttonList[i].Item2.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, spacePerAbility + (i * spacePerAbility), 0.0f);
            buttonList[i].Item2.abilityName.text = abilityName;
            buttonList[i].Item2.abilityIcon.sprite = abilities[i].icon;
            if (abilities[i].icon == null) buttonList[i].Item2.abilityIcon.color = CustomColor.invisible();
            buttonList[i].Item2.gameObject.SetActive(true);
            Ability ability = buttonList[i].Item1;
            buttonList[i].Item2.abilityButton.onClick.RemoveAllListeners();
            buttonList[i].Item2.abilityButton.onClick.AddListener(delegate { OnClickAbility(ability); });

            // �g�p�ł��邩���`�F�b�N
            buttonList[i].Item2.abilityButton.interactable = true;
            if (battleManager.GetCurrentBattler().current_mp < buttonList[i].Item1.consumeSP)
            {
                // SP �s��
                buttonList[i].Item2.abilityButton.interactable = false;
                buttonList[i].Item2.abilityContextText.color = Color.red;
                buttonList[i].Item2.abilityContextText.text = System.String.Format(LocalizationManager.Localize("Battle.NotEnoughSP"), buttonList[i].Item1.consumeSP);
            }
            if (battleManager.GetCurrentBattler().IsAbilityOnCooldown(abilities[i]) > 0)
            {
                // �`���[�W��
                buttonList[i].Item2.abilityButton.interactable = false;
                buttonList[i].Item2.abilityContextText.color = Color.red;
                buttonList[i].Item2.abilityContextText.text = System.String.Format(LocalizationManager.Localize("Battle.OnCooldown"), battleManager.GetCurrentBattler().IsAbilityOnCooldown(abilities[i]));
            }
            if (!battleManager.GetCurrentBattler().IsAbilityActive(abilities[i]))
            {
                // �g�p�s��
                buttonList[i].Item2.abilityButton.interactable = false;
                buttonList[i].Item2.abilityContextText.color = Color.red;
                buttonList[i].Item2.abilityContextText.text = LocalizationManager.Localize("Battle.AbilityDisabled");
            }
            // �ǉ�����
            if (buttonList[i].Item2.abilityButton.interactable)
            {
                buttonList[i].Item2.abilityContextText.color = Color.black;
                string context = string.Empty;
                if (buttonList[i].Item1.consumeSP > 0) context = context + LocalizationManager.Localize("System.SPCost") + buttonList[i].Item1.consumeSP.ToString() + "�@�@";
                if (buttonList[i].Item1.cooldown > 0) context = context + LocalizationManager.Localize("System.Cooldown") + "�F" + buttonList[i].Item1.cooldown.ToString() + LocalizationManager.Localize("System.Turn");
                buttonList[i].Item2.abilityContextText.text = context;

                // �ǉ��������Ȃ��ꍇ�͋Z����^�񒆂Ɉړ�
                if (buttonList[i].Item2.abilityContextText.text == string.Empty) 
                    buttonList[i].Item2.abilityName.rectTransform.anchoredPosition = new Vector2(buttonList[i].Item2.abilityName.rectTransform.anchoredPosition.x, -22f);
            }
        }

        // setup close button
        closeButton.anchoredPosition = new Vector3(closeButton.anchoredPosition.x, (rectTransform.sizeDelta.y * 0.5f) + (closeButton.sizeDelta.y * 0.5f), 0.0f);
    }

    public void OnClickAbility(Ability ability)
    {
        HideDescription();

        switch (ability.castType)
        {
            case CastType.SelfCast:
                AbilityExecute.Instance.Invoke(ability.functionName, 0);
                battleManager.GetCurrentBattler().DeductSP(ability.consumeSP); // SP����
                if (ability.cooldown > 0) battleManager.GetCurrentBattler().SetAbilityOnCooldown(ability, ability.cooldown); // �`���[�W
                ClosePanel();
                actionPanel.SetEnablePanel(false);
                break;
            case CastType.Teammate:
                StartCoroutine(SelectingTarget(ability, true, false, true));
                HidePanel();
                break;
            case CastType.Enemy:
                StartCoroutine(SelectingTarget(ability, false, true, true));
                HidePanel();
                break;
            default:
                break;
        }
    }

    public void OpenPanel(Action callbackWhenClose)
    {
        if (isOpen) return;

        onCloseCallback = callbackWhenClose;
        rectTransform.DOAnchorPosY(0, animationTime);
        DOTween.Sequence().AppendInterval(animationTime * 0.75f).AppendCallback(() => { isOpen = true; }); // ���S�ɊJ���Ă��玟�̂𑀍�󂯕t����
        isHiding = false;

        canvasGrp.DOFade(1.0f, animationTime);
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;
    }

    public void ClosePanel()
    {
        if (!isOpen) return;

        rectTransform.DOAnchorPosY(-(rectTransform.sizeDelta.y+closeButton.sizeDelta.y), animationTime);
        isOpen = false;
        isHiding = false;

        if (!ReferenceEquals(buttonList, null))
        {
            // �{�^�����폜
            foreach (var btn in buttonList)
            {
                Destroy(btn.Item2.gameObject, animationTime);
            }

            // For the garbage collector to truly find it
            buttonList = null;
        }
        
        canvasGrp.DOFade(0.0f, animationTime);
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        onCloseCallback?.Invoke();
    }


    private void HidePanel()
    {
        if (!isOpen) return;
        if (isHiding) return;

        isHiding = true;

        rectTransform.DOAnchorPosY(-(rectTransform.sizeDelta.y + closeButton.sizeDelta.y), animationTime);

        canvasGrp.DOFade(0.0f, animationTime);
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;
    }

    private void UnhidePanel()
    {
        if (!isOpen) return;
        if (!isHiding) return;

        isHiding = false;

        rectTransform.DOAnchorPosY(0.0f, animationTime);

        canvasGrp.DOFade(1.0f, animationTime);
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;
    }

    private void Update()
    {
        if (!isOpen) return;
        if (isHiding) return;

        if (Input.GetMouseButtonDown(1))
        {
            ClosePanel();
        }

        if (ReferenceEquals(buttonList, null)) return;

        // �}�E�X�N���b�N�����m
        Vector3 mousePosition = Input.mousePosition / canvas.scaleFactor;
        bool isMouseOnItem = false;
        foreach (var ability in buttonList)
        {
            RectTransform buttonRect = ability.Item2.GetComponent<RectTransform>();
            if (   mousePosition.x > (buttonRect.position.x / canvas.scaleFactor - buttonRect.sizeDelta.x * 0.5f)
                && mousePosition.x < (buttonRect.position.x / canvas.scaleFactor + buttonRect.sizeDelta.x * 0.5f)
                && mousePosition.y > (buttonRect.position.y / canvas.scaleFactor - buttonRect.sizeDelta.y * 0.5f)
                && mousePosition.y < (buttonRect.position.y / canvas.scaleFactor + buttonRect.sizeDelta.y * 0.5f))
            {
                // �t���O
                isMouseOnItem = true;

                // �����\��
                ShowDescription(ability);

                // �c��̃��[�v���ȗ�
                return;
            }
        }

        if (!isMouseOnItem && isDescriptionShowing)
        {
            HideDescription();
        }
    }

    private void ShowDescription(Tuple<Ability, ActionPanelAbilityButton> ability)
    {
        // ���t�@�����X����
        RectTransform buttonRect = ability.Item2.GetComponent<RectTransform>();
        RectTransform descriptionPanelRect = descriptionPanel.GetComponent<RectTransform>();

        //�t���O
        isDescriptionShowing = true;

        // UI
        descriptionPanel.GetComponent<CanvasGroup>().DOFade(1.0f, 0.1f);
        descriptionPanelRect.position = new Vector2(buttonRect.position.x, buttonRect.position.y + (buttonRect.sizeDelta.y * 0.5f) + 10.0f);

        // �f�[�^�ǂݍ���
        description_SPCost.text = LocalizationManager.Localize("System.SPCost") + ability.Item1.consumeSP + "/" + battleManager.GetCurrentBattler().current_mp;
        string cooldown = ability.Item1.cooldown > 0 ? ability.Item1.cooldown + LocalizationManager.Localize("System.Turn") : LocalizationManager.Localize("System.None");
        description_Cooldown.text = LocalizationManager.Localize("System.Cooldown") + "�F" + cooldown;

        description_Name.text = LocalizationManager.Localize(ability.Item1.abilityNameID);
        string effectTargetText = string.Empty;
        switch (ability.Item1.castType)
        {
            case CastType.SelfCast:
                effectTargetText = LocalizationManager.Localize("System.EffectSelf");
                if (ability.Item1.isAOE) effectTargetText = LocalizationManager.Localize("System.EffectSelfAoE");
                break;
            case CastType.Teammate:
                effectTargetText = LocalizationManager.Localize("System.EffectTeam");
                break;
            case CastType.Enemy:
                effectTargetText = LocalizationManager.Localize("System.EffectEnemy");
                if (ability.Item1.isAOE) effectTargetText = LocalizationManager.Localize("System.EffectEnemyAoE");
                break;
            default:
                break;
        }
        description_Target.text = LocalizationManager.Localize("System.EffectTarget") + effectTargetText;

        string abilityTypeText = string.Empty;
        switch (ability.Item1.abilityType)
        {
            case AbilityType.Attack:
                abilityTypeText = LocalizationManager.Localize("System.AbilityAttack");
                break;
            case AbilityType.Buff:
                abilityTypeText = LocalizationManager.Localize("System.AbilityBuff");
                break;
            case AbilityType.Heal:
                abilityTypeText = LocalizationManager.Localize("System.AbilityHeal");
                break;
            case AbilityType.Special:
                abilityTypeText = LocalizationManager.Localize("System.AbilitySpecial");
                break;
            default:
                break;
        }
        description_Type.text = LocalizationManager.Localize("System.AbilityType") + "�F" + abilityTypeText;

        description_Info.text = LocalizationManager.Localize(ability.Item1.descriptionID);

        // �����X�V
        description_Name.ForceMeshUpdate();
        description_SPCost.ForceMeshUpdate();
        description_Cooldown.ForceMeshUpdate();
        description_Target.ForceMeshUpdate();
        description_Type.ForceMeshUpdate();
        description_Info.ForceMeshUpdate();

        // Resize UI
        descriptionPanelRect.sizeDelta = new Vector2(descriptionPanelRect.sizeDelta.x,
            (description_Name.rectTransform.rect.height +
             description_SPCost.rectTransform.rect.height +
             description_Cooldown.rectTransform.rect.height + 
             description_Target.rectTransform.rect.height + 
             description_Type.rectTransform.rect.height + 
            (description_Info.GetRenderedValues(false).y) + description_Info.fontSize));
    }

    private void HideDescription()
    {
        isDescriptionShowing = false;
        descriptionPanel.DOFade(0.0f, 0.1f);
    }

    private IEnumerator SelectingTarget(Ability ability, bool isTeammateAllowed, bool isEnemyAllowed, bool isAliveOnly)
    {
        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemActionPanel");

        // �J�[�\����ύX
        var texture = Resources.Load<Texture2D>("Icon/focus");
        Cursor.SetCursor(texture, new Vector2(texture.width * 0.5f, texture.height * 0.5f), CursorMode.Auto);
        bool isSelectingTarget = false;
        bool isFinished = false;

        // Tips
        Inventory.Instance.ShowTipsText();

        do
        {
            // arrow that follow the mouse
            Vector3 mousePosition = Input.mousePosition / canvas.scaleFactor;
            var targetBattler = battleManager.GetBattlerByPosition(mousePosition, isTeammateAllowed, isEnemyAllowed, isAliveOnly);

            if (!ReferenceEquals(targetBattler, null) && (ability.canTargetSelf || targetBattler != battleManager.GetCurrentBattler()))
            {
                isSelectingTarget = true;
                battleManager.PointTargetWithArrow(targetBattler, 0.25f);
                if (Input.GetMouseButtonDown(0))
                {
                    isFinished = true;

                    // �I�������G
                    if (ability.isAOE)
                    {
                        // �S��
                        AbilityExecute.Instance.SetTargetBattlers(battleManager.GetAllEnemy());
                    }
                    else
                    {
                        // �P��
                        AbilityExecute.Instance.SetTargetBattler(targetBattler);
                    }

                    // �Z���g�p
                    AbilityExecute.Instance.Invoke(ability.functionName, 0);
                    battleManager.GetCurrentBattler().DeductSP(ability.consumeSP);
                    ClosePanel();
                    actionPanel.SetEnablePanel(false);

                    // �J�[�\����߂�
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    battleManager.UnPointArrow(0.25f);

                    // �`���[�W
                    if (ability.cooldown > 0)
                    {
                        battleManager.GetCurrentBattler().SetAbilityOnCooldown(ability, ability.cooldown);
                    }
                }
            }
            else if (isSelectingTarget)
            {
                isSelectingTarget = false;
                battleManager.UnPointArrow(0.25f);
            }

            if (Input.GetMouseButtonDown(1))
            {
                // SE�Đ�
                AudioManager.Instance.PlaySFX("SystemActionCancel");

                // �L�����Z��
                isFinished = true;
                battleManager.UnPointArrow(0.25f);
                UnhidePanel();

                // �J�[�\����߂�
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            yield return null;
        } while (!isFinished);

        // Hide Tips (�K�v�Ȃ̂Ńo�O�ł͂Ȃ��I)
        Inventory.Instance.HideTipsText();
    }

    public float GetAnimTime()
    {
        return animationTime;
    }
}
