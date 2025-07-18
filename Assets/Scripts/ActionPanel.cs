using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[System.Serializable]
enum CommandType
{
    Waiting,    // コマンド待ち
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
    [SerializeField] private TMPro.TMP_Text cannotAttackText; // 攻撃できないテキスト
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

        if (boolean) // 入力待ち
        {
            commandType = CommandType.Waiting;

            // 普通攻撃が出来ないBattlerならボタンを無効にする
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
        // 攻撃相手を選ぶ
        isSelectingTarget = true;

        canvasGrp.alpha = 0.25f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        escapeBtn.interactable = false;

        // カーソルを変更
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f), CursorMode.Auto);

        // tips表示
        tipsText.DOFade(1.0f, 0.25f);

        // 攻撃
        commandType = CommandType.Attack;

        // SE再生
        AudioManager.Instance.PlaySFX("SystemActionPanel");
    }

    public void OnClickItem()
    {
        // インベントリメニューを表示
        Inventory.Instance.obj.OpenInventory(true, CloseInventory);

        // ActionPanel操作禁止
        canvasGrp.alpha = 0.25f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        escapeBtn.interactable = false;

        // アイテム
        commandType = CommandType.Item;

        // SE再生
        AudioManager.Instance.PlaySFX("SystemActionPanel");
    }

    /// <summary>
    /// Callback: インベントリメニューが閉じられたら自動的に呼ばれる
    /// </summary>
    public void CloseInventory()
    {
        // ActionPanel操作にもどる
        canvasGrp.alpha = 1.0f;
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        escapeBtn.interactable = true;

        commandType = CommandType.Waiting;

        // SE再生
        AudioManager.Instance.PlaySFX("SystemActionCancel");
    }

    public void OnClickSkill()
    {
        // 特殊技リストを表示
        abilityPanel.SetupAbilityData(battleManager.GetCurrentBattler().abilities);
        abilityPanel.OpenPanel(OnCloseSkillPanel);

        // ActionPanel操作禁止
        canvasGrp.DOFade(0.5f, abilityPanel.GetAnimTime());
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        escapeBtn.interactable = false;

        // 特殊技
        commandType = CommandType.Ability;

        // SE再生
        AudioManager.Instance.PlaySFX("SystemActionPanel");
    }
    
    /// <summary>
    /// Callback: 特殊技リストが閉じられたら自動的に呼ばれる
    /// </summary>
    public void OnCloseSkillPanel()
    {
        // ActionPanel操作にもどる
        canvasGrp.DOFade(1.0f, abilityPanel.GetAnimTime());
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        escapeBtn.interactable = true;

        commandType = CommandType.Waiting;

        // SE再生
        AudioManager.Instance.PlaySFX("SystemActionCancel");
    }

    public void OnClickIdle()
    {
        // ActionPanel操作禁止
        canvasGrp.DOFade(0.5f, abilityPanel.GetAnimTime());
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        // スキップターン
        battleManager.IdleCommand();
    }

    private void Update()
    {
        if (commandType == CommandType.Item && !isSelectingTarget) return; // アイテム選択中

        if (commandType == CommandType.Ability && !isSelectingTarget) return; // 特殊技選択中

        if (isSelectingTarget)
        {
            // 右クリック
            if (Input.GetMouseButtonDown(1))
            {
                // SE再生
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
                            case CommandType.Attack: // 攻撃
                                battleManager.AttackCommand(targetBattler);
                                CancelAttack();
                                SetEnablePanel(false);
                                break;
                            case CommandType.Item: // アイテム

                                break;
                            case CommandType.Ability: // 特殊技

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

        // 攻撃できないメッセージ
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

        // 取り消し
        canvasGrp.alpha = 1f;
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;
        
        escapeBtn.interactable = true;
        
        isSelectingTarget = false;

        // カーソルを戻す
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // tipsを消す
        tipsText.DOFade(0.0f, 0.25f);

        if (isSelectedTarget)
        {
            battleManager.UnPointArrow(0.25f);
        }
    }
}
