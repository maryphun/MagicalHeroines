using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.SimpleLocalization.Scripts;
using DG.Tweening;

public class FormationSlot : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float nameTextSize = 35.0f;
    [SerializeField] private float nonNameTextSize = 27.0f;
    [SerializeField] private Vector2 hoverCollisionSize = new Vector2(100.0f, 200.0f);
    [SerializeField] private float resourcesPanelAnimationTime = 0.5f;

    [Header("References")]
    [SerializeField] private Image lockIcon; 
    [SerializeField] private TMP_Text slotName;
    [SerializeField] private GameObject HPStatus;
    [SerializeField] private GameObject MPStatus;
    [SerializeField] private Image HPFill;
    [SerializeField] private Image MPFill;
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private TMP_Text MPText;
    [SerializeField] private CanvasGroup resourcesPanel;
    [SerializeField] private Button unlockSlotButton;
    [SerializeField] private Button regenerateButton;
    [SerializeField] private TMP_Text regenerateCostText;
    [SerializeField] private FormationPanel formationPanel;
    [SerializeField] private GameObject effect;

    [Header("Debug")]
    [SerializeField] private GameObject battler;
    [SerializeField] private Battler battlerComponent;
    [SerializeField] private Vector3 objectPosition;
    [SerializeField] private bool isDisplaying;
    [SerializeField] private int slotIndex;
    [SerializeField] private bool isRegenerating = false;
    [SerializeField] private bool isSlotFilled = false;
    [SerializeField] private AudioSource audio;

    public void Initialize(bool isLocked, int moneyCost, bool displayCost, int slotIndex)
    {
        lockIcon.color = isLocked ? Color.white : new Color(1, 1, 1, 0);
        this.slotName.fontSize = nonNameTextSize;
        this.slotName.text = string.Empty;
        HPStatus.SetActive(false);
        MPStatus.SetActive(false);
        unlockSlotButton.gameObject.SetActive(false);
        isSlotFilled = false;
        SetupRegenerateButton();

        if (displayCost)
        {
            unlockSlotButton.gameObject.SetActive(true);

            // お金が足りるか
            bool isEnoughMoney = ProgressManager.Instance.GetCurrentMoney() >= moneyCost;
            unlockSlotButton.interactable = isEnoughMoney;
            unlockSlotButton.GetComponentInChildren<TMP_Text>().alpha = isEnoughMoney ? 1.0f: 0.25f;
            this.slotName.text = LocalizationManager.Localize("System.Cost") + ": ";
            this.slotName.text = this.slotName.text + (isEnoughMoney ? "<color=yellow>" : "<color=#FF000088>") + moneyCost.ToString();
        }
        else if (isLocked)
        {
            this.slotName.text = LocalizationManager.Localize("System.Locked");
        }

        // System
        objectPosition = GetComponent<RectTransform>().position;
        this.slotIndex = slotIndex;
    }

    public void SetBattler(Character unit)
    {
        isSlotFilled = true;
        battler = Instantiate(unit.battler, transform);
        battler.transform.localPosition = Vector3.zero;

        battlerComponent = battler.GetComponent<Battler>();
        battlerComponent.InitializeCharacterData(unit);
        battlerComponent.SetupFormationPanelMode();

        // 名前とレベル
        slotName.text = LocalizationManager.Localize("Battle.Level") + battlerComponent.currentLevel + " " + unit.localizedName;
        slotName.fontSize = nameTextSize;
        slotName.color = Color.white;
        
        // ステータス表示
        if (battlerComponent.max_hp > 0)
        {
            HPStatus.SetActive(true);
            HPFill.fillAmount = (float)battlerComponent.current_hp / (float)battlerComponent.max_hp;
        }
        HPText.text = LocalizationManager.Localize("Battle.HP") + "：" + battlerComponent.current_hp.ToString() + "/" + battlerComponent.max_hp.ToString();

        if (unit.current_maxMp > 0)
        {
            MPStatus.SetActive(true);
            MPFill.fillAmount = (float)battlerComponent.current_mp / (float)battlerComponent.max_mp;
            MPText.text = LocalizationManager.Localize("Battle.MP") + "：" + battlerComponent.current_mp.ToString() + "/" + battlerComponent.max_mp.ToString();
        }

        // 表示しないものを非表示に
        battlerComponent.HideBars();
        SetupRegenerateButton();
    }

    public void ResetData(float delay)
    {
        if (delay <= 0.0)
        {
            ResetData();
            return;
        }
        StartCoroutine(ResetDataDelay(delay));
    }

    IEnumerator ResetDataDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetData();
    }

    public void ResetData()
    {
        if (!ReferenceEquals(battler, null) && isSlotFilled)
        {
            Object.Destroy(battler.gameObject);
            battler = null;
            battlerComponent = null;
            isSlotFilled = false;
        }

        lockIcon.color = Color.white;
        this.slotName.text = string.Empty;
        HPStatus.SetActive(false);
        MPStatus.SetActive(false);
        unlockSlotButton.gameObject.SetActive(false);
        SetupRegenerateButton();
    }

    public void OnClickSlot()
    {
        if (!ReferenceEquals(battler, null) && isSlotFilled)
        {
            // 外す
            // UpdateFormationで全スロットに対してResetDataをかけるのでObject.BattlerをDestroyする必要がない
            formationPanel.UpdateFormation(slotIndex, -1, true, true);

            // エフェクトを表示
            var vfx = Instantiate(effect, transform);
            vfx.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 80.0f, 0.0f);
            vfx.GetComponent<Animator>().speed = 1.0f;

            // SE再生
            AudioManager.Instance.PlaySFX("Flee", 0.5f);
        }
        else if (ProgressManager.Instance.GetUnlockedFormationCount() > slotIndex) // 解放済み
        {
            formationPanel.OpenFormationSelectionPanel(slotIndex);

            // SE再生
            AudioManager.Instance.PlaySFX("SystemSelect");
        }
    }

    private void FixedUpdate()
    {
        // 回復
        if (isRegenerating)
        {
            ProgressManager.Instance.SetMoney(ProgressManager.Instance.GetCurrentMoney() - 1);

            // ステータス更新
            if (battlerComponent.max_hp > 0)
            {
                // heal
                if (battlerComponent.current_hp < battlerComponent.max_hp)
                {
                    battlerComponent.current_hp = Mathf.Min(battlerComponent.current_hp + 2, battlerComponent.max_hp);
                    HPFill.fillAmount = (float)battlerComponent.current_hp / (float)battlerComponent.max_hp;
                    HPText.text = LocalizationManager.Localize("Battle.HP") + "：" + battlerComponent.current_hp.ToString() + "/" + battlerComponent.max_hp.ToString();
                }
            }

            if (battlerComponent.max_mp > 0)
            {
                // heal
                if (battlerComponent.current_mp < battlerComponent.max_mp)
                {
                    battlerComponent.current_mp = Mathf.Min(battlerComponent.current_mp + 2, battlerComponent.max_mp);
                    MPFill.fillAmount = (float)battlerComponent.current_mp / (float)battlerComponent.max_mp;
                    MPText.text = LocalizationManager.Localize("Battle.MP") + "：" + battlerComponent.current_mp.ToString() + "/" + battlerComponent.max_mp.ToString();
                }
            }

            // effect
            battlerComponent.ColorTint(Color.yellow, 0.5f);

            var hpToHeal = battlerComponent.max_hp - battlerComponent.current_hp;
            var mpToHeal = battlerComponent.max_mp - battlerComponent.current_mp;

            // cost
            int cost = (hpToHeal + mpToHeal) / 2;
            regenerateCostText.text = LocalizationManager.Localize("System.RegenerateCost") + ":" + cost.ToString();

            if (ProgressManager.Instance.GetCurrentMoney() < 1 || (hpToHeal + mpToHeal <= 0))
            {
                OnStopRegen();
            }
        }
        else
        {
            // check enough cost for button
            if (ProgressManager.Instance.GetCurrentMoney() < 1)
            {
                regenerateButton.interactable = false;
            }
        }

        // 別のところでお金を使ってしまってないかを常時チェック
        if (!isSlotFilled && unlockSlotButton.IsInteractable() && unlockSlotButton.gameObject.activeInHierarchy)
        {
            if (ProgressManager.Instance.GetCurrentMoney() < formationPanel.GetUnlockCost(slotIndex))
            {
                // 資金が足りなくなった
                unlockSlotButton.interactable = false;
                this.slotName.text = LocalizationManager.Localize("System.Cost") + ": <color=#FF000088>" + formationPanel.GetUnlockCost(slotIndex).ToString();
            }
        }
    }

    public void OnHover()
    {
        if (isDisplaying) return;
        if (formationPanel.isFormationSelecting) return;
        isDisplaying = true;
        resourcesPanel.DOKill(false);
        resourcesPanel.DOFade(1.0f, resourcesPanelAnimationTime);
    }

    public void OnUnHover()
    {
        if (!isDisplaying) return;
        isDisplaying = false;
        resourcesPanel.DOKill(false);
        resourcesPanel.DOFade(0.0f, resourcesPanelAnimationTime);
    }

    public void SetupRegenerateButton()
    {
        if (isSlotFilled)
        {
            // calculation
            var hpToHeal = battlerComponent.max_hp - battlerComponent.current_hp;
            var mpToHeal = battlerComponent.max_mp - battlerComponent.current_mp;
            if (hpToHeal + mpToHeal <= 0)
            {
                // nothing to heal
                regenerateButton.gameObject.SetActive(false);
                regenerateCostText.gameObject.SetActive(false);
                return;
            }

            regenerateButton.gameObject.SetActive(true);
            regenerateCostText.gameObject.SetActive(true);
            regenerateButton.interactable = true;

            // cost
            int cost = (hpToHeal + mpToHeal) / 2;
            regenerateCostText.text = LocalizationManager.Localize("System.RegenerateCost") + ":" + cost.ToString();

            if (ProgressManager.Instance.GetCurrentMoney() < 1)
            {
                regenerateButton.interactable = false;
            }
        }
        else
        {
            // slot is empty. 
            regenerateButton.gameObject.SetActive(false);
            regenerateCostText.gameObject.SetActive(false);
        }
    }

    public void OnStartRegen()
    {
        if (isRegenerating || !regenerateButton.IsInteractable()) return;
        isRegenerating = true;

        audio = AudioManager.Instance.PlaySFX("MagicCharge");
    }

    public void OnStopRegen()
    {
        if (!isRegenerating) return;
        isRegenerating = false;

        // update character status
        ProgressManager.Instance.UpdateCharacterByBattler(battlerComponent.characterID, battlerComponent);

        SetupRegenerateButton();

        // SE 停止
        audio.Stop();

        // 全回した
        var hpToHeal = battlerComponent.max_hp - battlerComponent.current_hp;
        var mpToHeal = battlerComponent.max_mp - battlerComponent.current_mp;
        if (hpToHeal + mpToHeal <= 0)
        {
            battlerComponent.Graphic.DOComplete();
            battlerComponent.CreateGlowEffect(1.2f, 0.4f);

            AudioManager.Instance.PlaySFX("SystemLevelUp");
        }
        else
        {
            AudioManager.Instance.PlaySFX("Miss");
        }

        // 資金非表示に
        HideResourcesPanel(1.5f);
    }

    public void HideResourcesPanel(float delay)
    {
        isDisplaying = false;
        resourcesPanel.DOFade(0.0f, resourcesPanelAnimationTime).SetDelay(delay);
    }
}
