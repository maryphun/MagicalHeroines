using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.SimpleLocalization.Scripts;
using DG.Tweening;
using System.Linq;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class EquipmentPanel : MonoBehaviour
{
    const int totalSlotNumber = 15;

    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animTime = 0.5f;
    [SerializeField] private Color normalItemColor = Color.white;
    [SerializeField] private Color rareItemColor = Color.blue;
    [SerializeField] private Color holyItemColor = Color.yellow;

    [Header("References")]
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private Button[] equipmentSlot = new Button[totalSlotNumber];
    [SerializeField] private CharacterBuildingPanel mainPanel;
    [SerializeField] private CharacterDataPanel dataPanel;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Image sprite;
    [SerializeField] private TMP_Text equipmentName_Text;
    [SerializeField] private TMP_Text equipmentType_Text;
    [SerializeField] private TMP_Text description;

    [Header("Debug")]
    [SerializeField] private int characterID = -1;
    [SerializeField] private List<EquipmentData> equips;

    public void OpenEquipmentPanel()
    {
        // アニメション
        panel.DOFade(1.0f, animTime);
        panel.interactable = true;
        panel.blocksRaycasts = true;
        
        // カーソルを初期化する
        cursor.SetParent(transform);
        cursor.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        var cursorImg = cursor.GetComponent<Image>();
        cursorImg.color = new Color(cursorImg.color.r, cursorImg.color.g, cursorImg.color.b, 0);

        // データを初期化
        ResetData();
        InitEquipmentSlots(mainPanel.CurrentCheckingSlot);

        // SE 再生
        AudioManager.Instance.PlaySFX("SystemOpen");
    }

    public void CloseEquipmentPanel()
    {
        panel.DOFade(0.0f, animTime);
        DOTween.Sequence().AppendInterval(animTime * 0.5f).AppendCallback(() => {
            panel.interactable = false;
            panel.blocksRaycasts = false;
        });

        // SE 再生
        AudioManager.Instance.PlaySFX("SystemCancel");
    }

    private void InitEquipmentSlots(int characterID)
    {
        this.characterID = characterID;

        var data = ProgressManager.Instance.GetEquipmentData();

        // arrange
        equips = new List<EquipmentData>();
        equips.AddRange(data.Where(x => x.data.equipmentType == EquipmentType.Holy).ToList());
        equips.AddRange(data.Where(x => x.data.equipmentType == EquipmentType.Rare).ToList());
        equips.AddRange(data.Where(x => x.data.equipmentType == EquipmentType.Normal).ToList());

        for (int i = 0; i < totalSlotNumber; i++)
        {
            var icon = equipmentSlot[i].transform.GetChild(0).GetComponent<Image>();
            if (i < equips.Count)
            {
                // 装備を表示
                equipmentSlot[i].interactable = true;

                if (equips[i].equipingCharacterID >= 0 && equips[i].equipingCharacterID != characterID)
                {
                    // 別のキャラに装備されている装備
                    equipmentSlot[i].interactable = false;
                    icon.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }
                else
                {
                    icon.color = Color.white;
                }
                icon.sprite = equips[i].data.Icon;

                switch (equips[i].data.equipmentType)
                {
                    case EquipmentType.Normal:
                        equipmentSlot[i].GetComponent<Image>().color = normalItemColor;
                        break;
                    case EquipmentType.Rare:
                        equipmentSlot[i].GetComponent<Image>().color = rareItemColor;
                        break;
                    case EquipmentType.Holy:
                        equipmentSlot[i].GetComponent<Image>().color = holyItemColor;
                        break;
                }
                EquipmentDefine item = equips[i].data;
                int slotIndex = i;
                equipmentSlot[i].onClick.RemoveAllListeners();
                equipmentSlot[i].onClick.AddListener(delegate { OnClickEquipItem(item, slotIndex); });

                // event triggers
                var trigger = equipmentSlot[i].GetComponent<EventTrigger>();
                EventTrigger.Entry enter = new EventTrigger.Entry();
                enter.eventID = EventTriggerType.PointerEnter;
                enter.callback.AddListener((a) => { OnPointerEnter(item); });
                trigger.triggers.Add(enter);
                
                EventTrigger.Entry exit = new EventTrigger.Entry();
                exit.eventID = EventTriggerType.PointerExit;
                exit.callback.AddListener((a) => { ResetData(); });
                trigger.triggers.Add(exit);

                // すでに装備している
                if (equips[i].equipingCharacterID == characterID)
                {
                    cursor.SetParent(equipmentSlot[slotIndex].transform);
                    cursor.localPosition = new Vector2(50, -50);
                    cursor.localScale = new Vector3(1, 1, 1);
                    var cursorImg = cursor.GetComponent<Image>();
                    cursorImg.color = new Color(cursorImg.color.r, cursorImg.color.g, cursorImg.color.b, 1.0f);
                }
            }
            else
            {
                // 装備がない
                equipmentSlot[i].interactable = false;
                equipmentSlot[i].onClick.RemoveAllListeners();
                icon.color = new Color(1, 1, 1, 0);

                // event triggers
                var trigger = equipmentSlot[i].GetComponent<EventTrigger>();
                trigger.triggers.Clear();
            }
        }
    }

    private void OnClickEquipItem(EquipmentDefine item, int slotIndex)
    {
        ResetCursor();
        var cursorImg = cursor.GetComponent<Image>();

        // このキャラが装備しているアイテムがあるかをチェック
        var EquipmentData = ProgressManager.Instance.GetEquipmentData();
        bool isEquiped = EquipmentData.Any(x => x.equipingCharacterID == characterID);
        string equiped = string.Empty;
        if (isEquiped)
        {
            equiped = ProgressManager.Instance.GetEquipmentData().FirstOrDefault(x => x.equipingCharacterID == characterID).data.pathName;
        }
        if (!isEquiped)
        {
            // このキャラはアイテムを装備していない
            ProgressManager.Instance.ApplyEquipmentToCharacter(item, characterID);
            cursor.SetParent(equipmentSlot[slotIndex].transform);
            cursor.localPosition = new Vector2(50, -50);
            cursor.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            cursor.DOScale(1.0f, 0.05f);
            cursorImg.DOFade(1.0f, 0.0f);
            AudioManager.Instance.PlaySFX("SystemEquip");
        }
        else if (equiped == item.pathName)
        {
            // 装備を解除する
            ProgressManager.Instance.UnapplyEquipment(item.pathName);
            cursor.SetParent(transform);
            cursor.DOScale(1.3f, 0.05f);
            cursorImg.DOFade(0.0f, 0.05f);
            AudioManager.Instance.PlaySFX("Unequip");
        }
        else
        {
            // 別の装備を解除してこっちに入れ替わる
            ProgressManager.Instance.UnapplyEquipment(equiped);
            ProgressManager.Instance.ApplyEquipmentToCharacter(item, characterID);
            cursor.SetParent(equipmentSlot[slotIndex].transform);
            cursor.localPosition = new Vector2(50, -50);
            cursor.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            cursor.DOScale(1.0f, 0.05f);
            cursorImg.DOFade(1.0f, 0.0f);
            AudioManager.Instance.PlaySFX("SystemEquip");
        }

        // 外部のアイコンも更新
        dataPanel.InitializeCharacterData(ProgressManager.Instance.GetCharacterByID(characterID));
    }

    public void OnPointerEnter(EquipmentDefine itemdefine)
    {
        // Reset
        ResetData();

        sprite.sprite = itemdefine.Icon;
        sprite.DOFade(1.0f, 0.1f);
        description.DOText(LocalizationManager.Localize(itemdefine.descriptionID) +"\n\n"+ itemdefine.effectText, 0.5f).SetEase(Ease.Linear);
        equipmentName_Text.DOText(LocalizationManager.Localize(itemdefine.equipNameID), 0.2f).SetEase(Ease.Linear);

        equipmentType_Text.color = GetColorByEquipmentType(itemdefine.equipmentType);
        string label = LocalizationManager.Localize("System.EquipmentType") + ": ";
        switch (itemdefine.equipmentType)
        {
            case EquipmentType.Normal:
                equipmentType_Text.DOText(label + LocalizationManager.Localize("System.EquipmentNormal"), 0.2f).SetEase(Ease.Linear);
                break;
            case EquipmentType.Rare:
                equipmentType_Text.DOText(label + LocalizationManager.Localize("System.EquipmentRare"), 0.2f).SetEase(Ease.Linear);
                break;
            case EquipmentType.Holy:
                equipmentType_Text.DOText(label + LocalizationManager.Localize("System.EquipmentHoly"), 0.2f).SetEase(Ease.Linear);
                break;
        }
    }
    public void ResetData()
    {
        // end all animation
        sprite.DOKill();
        description.DOKill();
        equipmentType_Text.DOKill();
        equipmentName_Text.DOKill();

        sprite.color = new Color(1, 1, 1, 0);
        description.text = string.Empty;
        equipmentType_Text.text = string.Empty;
        equipmentName_Text.text = string.Empty;
    }

    public void ResetCursor()
    {
        cursor.DOKill(true);
        cursor.GetComponent<Image>().DOKill(true);
    }

    public Color GetColorByEquipmentType(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Normal:
                return normalItemColor;
            case EquipmentType.Rare:
                return rareItemColor;
            case EquipmentType.Holy:
                return holyItemColor;
        }
        return Color.white;
    }
}
