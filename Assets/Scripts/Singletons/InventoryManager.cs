using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;
using TMPro;

public class Inventory
{
    // Monobehaviourを継承しないシングルトン
    static Inventory instance = null;
    public static Inventory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Inventory();
            }

            return instance;
        }
    }

    private Inventory()
    {
        // load inventory object
        var origin = Resources.Load("Prefabs/Inventory");
        var gameobject = (GameObject)GameObject.Instantiate(origin);
        obj = gameobject.GetComponent<InventoryManager>();
        
        // Create Canvas
        GameObject canvas = new GameObject("InventoryCanvas(Don't Destroy)");

        canvasObj = canvas.AddComponent<Canvas>();
        var canvasScaler = canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        // Setting Up canvas
        canvasObj.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.vertexColorAlwaysGammaSpace = true;
        canvasObj.sortingOrder = 99;

        // Setting Up Canvas Scaler
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        // Create Alpha Image
        GameObject img = new GameObject("FadeAlpha");

        /// setting Image
        fadeAlpha = img.AddComponent<Image>();
        fadeAlpha.color = new Color(0,0,0,0.0f);
        fadeAlpha.transform.SetParent(canvas.transform);
        fadeAlpha.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
        fadeAlpha.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        fadeAlpha.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        fadeAlpha.raycastTarget = false;

        // Create Tips Text
        origin = Resources.Load("Prefabs/Tips");
        var tip = (GameObject)GameObject.Instantiate(origin);
        tip.name = "Tips";
        tip.transform.SetParent(canvas.transform);
        tips = tip.GetComponent<TMP_Text>();
        tips.rectTransform.anchoredPosition = Vector3.zero;
        tips.raycastTarget = false;
        tips.text = LocalizationManager.Localize("Battle.Cancel");
        tips.color = Color.red;
        tips.alpha = 0.0f;

        // Create Object (Inventory UI)
        obj.transform.SetParent(canvas.transform);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(1320.0f, 580.0f);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        obj.Initialize();

        gameobject.name = "Inventory";
        GameObject.DontDestroyOnLoad(canvas);
    }

    public Image GetAlphaImage()
    {
        return fadeAlpha;
    }

    public void ShowTipsText()
    {
        tips.DOFade(1.0f, 0.25f);
    }
    public void HideTipsText()
    {
        tips.DOFade(0.0f, 0.25f);
    }

    private Image fadeAlpha;
    private TMP_Text tips;
    public Canvas canvasObj = null;
    public InventoryManager obj;
}

public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform originItemBox;
    [SerializeField] private float animTime;
    [SerializeField] private float fadeAlpha = 0.785f;
    [SerializeField] private RectTransform itemDescription;
    [SerializeField] private TMP_Text itemDescription_Name;
    [SerializeField] private TMP_Text itemDescription_Target;
    [SerializeField] private TMP_Text itemDescription_Effect;

    [Header("Debug")]
    [SerializeField] private Canvas parent;
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private bool isInitialized = false;
    [SerializeField] private bool isDescriptionShowing = false;
    [SerializeField] private bool isOpened = false;
    [SerializeField] private bool isHiding = false;
    [SerializeField] private bool canUseItem = false;
    [SerializeField] private ItemDefine selectingItem;
    [SerializeField] private RectTransform[] itemSlots;
    [SerializeField] private Action onCloseCallback;
    [SerializeField] private List<Tuple<RectTransform, ItemDefine>> itemsInInventory;

    // 設定
    const int ItemColumnCount = 12;
    const int ItemRowCount = 4;
    private Vector2 ItemSlotSize = new Vector2(100.0f, 100.0f);
    private Vector2 SlotGap = new Vector2(5.0f, 5.0f);
    private Vector2 MarginGap = new Vector2(0.0f, 35.0f);

    public void Initialize()
    {
        if (this.isInitialized) return;

        itemSlots = new RectTransform[ItemColumnCount * ItemRowCount];

        int index = 0;
        for (int y = ItemRowCount - 1; y >= 0; y--)
        {
            for (int x = 0; x < ItemColumnCount; x++)
            {
                var obj = Instantiate(originItemBox.gameObject, originItemBox.transform.parent);

                obj.SetActive(true);

                itemSlots[index] = obj.GetComponent<RectTransform>();
                itemSlots[index].sizeDelta = ItemSlotSize;
                float positionX = (-((ItemColumnCount-1) * (ItemSlotSize.x + SlotGap.x)) * 0.5f) + x * (ItemSlotSize.x + SlotGap.x) + MarginGap.x;
                float positionY = (-((ItemRowCount-1   ) * (ItemSlotSize.y + SlotGap.y)) * 0.5f) + y * (ItemSlotSize.y + SlotGap.y) - MarginGap.y;
                itemSlots[index].localPosition = new Vector2(positionX, positionY);
                itemSlots[index].name = "Slot [" + index.ToString() + "]";
                itemSlots[index].GetComponent<Image>().raycastTarget = false;
                index++;
            }
        }

        panel = gameObject.AddComponent<CanvasGroup>();
        panel.alpha = 0.0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        parent = transform.parent.GetComponent<Canvas>();
        itemDescription.SetSiblingIndex(itemDescription.transform.parent.childCount);

        isOpened = false;
        isHiding = false;
        isDescriptionShowing = false;
        isInitialized = true;
    }

    public void ConfigureItem()
    {
        this.itemsInInventory = new List<Tuple<RectTransform, ItemDefine>>();
        var itemList = ProgressManager.Instance.GetItemList(false);

        const int MaxItemSlot = 48;
        int displayItemCount = Mathf.Min(itemList.Count, MaxItemSlot);
        for (int i = 0; i < displayItemCount; i++)
        {
            var itemImg = new GameObject().AddComponent<Image>();
            string itemName = LocalizationManager.Localize(itemList[i].itemNameID);
            itemImg.name = "Item [" + itemName + "]";
            itemImg.sprite = itemList[i].Icon;
            itemImg.transform.SetParent(itemSlots[i].transform);
            itemImg.rectTransform.localScale = Vector3.one;

            var itemRect = itemImg.GetComponent<RectTransform>();
            itemRect.sizeDelta = ItemSlotSize;
            itemRect.localPosition = Vector2.zero;

            itemSlots[i].GetComponent<Image>().color = Color.white;

            itemsInInventory.Add(new Tuple<RectTransform, ItemDefine>(itemRect, itemList[i]));
        }

        for (int i = itemList.Count; i < itemSlots.Length; i++)
        {
            itemSlots[i].GetComponent<Image>().color = new Color(0.63f, 0.63f, 0.63f, 1.0f);
        }
    }

    /// <summary>
    /// インベントリUIを表示
    /// </summary>
    /// <param name="canUseItem">道具を使うシーンであるかどうか</param>
    /// <param name="callbackWhenClose">UIが閉じるときのコールバック</param>
    public void OpenInventory(bool canUseItem, Action callbackWhenClose)
    {
        if (!isInitialized) Initialize();
        if (isOpened) return;
        ConfigureItem();

        onCloseCallback = callbackWhenClose;
        panel.DOFade(1.0f, animTime);
        panel.interactable = true;
        panel.blocksRaycasts = true;

        Inventory.Instance.GetAlphaImage().DOFade(fadeAlpha, animTime);
        itemDescription.GetComponent<CanvasGroup>().alpha = 0.0f;
        isOpened = true;
        isHiding = false;
        this.canUseItem = canUseItem;
    }

    public void CloseInventory(bool isUsingItem)
    {
        if (!isOpened) return;
        isOpened = false;
        isHiding = false;

        if (!isUsingItem) onCloseCallback?.Invoke();
        panel.DOFade(0.0f, animTime);
        panel.interactable = false;
        panel.blocksRaycasts = false;

        Inventory.Instance.GetAlphaImage().DOFade(0.0f, animTime);

        // remove all item
        foreach (Tuple<RectTransform, ItemDefine> item in itemsInInventory)
        {
            Destroy(item.Item1.gameObject);
        }
        itemsInInventory.Clear();
        itemsInInventory = null;
    }

    public void HideInventory()
    {
        if (!isOpened) return;
        if (isHiding) return;

        isHiding = true;
        Inventory.Instance.GetAlphaImage().DOFade(0.0f, animTime);
        panel.DOFade(0.0f, animTime);
        panel.interactable = false;
        panel.blocksRaycasts = false;
    }

    public void UnhideInventory()
    {
        if (!isOpened) return;
        if (!isHiding) return;

        isHiding = false;
        Inventory.Instance.GetAlphaImage().DOFade(fadeAlpha, animTime);
        panel.DOFade(1.0f, animTime);
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    /// <summary>
    /// 所持アイテム数の最大値を取得
    /// </summary>
    public int GetMaxItemCount()
    {
        return ItemColumnCount * ItemRowCount;
    }

    private void Update()
    {
        if (!isOpened || !isInitialized || isHiding) return;

        // マウスクリックを検知
        Vector3 mousePosition = Input.mousePosition / parent.scaleFactor;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) // 消し
        {
            CloseInventory(false);
            return;
        }

        bool isMouseOnItem = false;
        foreach (Tuple<RectTransform, ItemDefine> item in itemsInInventory)
        {
            if (   mousePosition.x > (item.Item1.position.x / parent.scaleFactor - ItemSlotSize.x * 0.5f)
                && mousePosition.x < (item.Item1.position.x / parent.scaleFactor + ItemSlotSize.x * 0.5f)
                && mousePosition.y > (item.Item1.position.y / parent.scaleFactor - ItemSlotSize.y * 0.5f)
                && mousePosition.y < (item.Item1.position.y / parent.scaleFactor + ItemSlotSize.y * 0.5f))
            {
                // フラグ
                isMouseOnItem = true;

                // 資料表示
                ShowDescription(item);

                // 選択・使用
                if (Input.GetMouseButtonDown(0))
                {
                    // effect
                    UseItem(item);
                }

                // 残りのループを省略
                return;
            }
        }

        if (!isMouseOnItem && isDescriptionShowing )
        {
            HideDescription();
        }
    }

    private IEnumerator SelectingTarget(bool isTeammateAllowed, bool isEnemyAllowed, bool isAliveOnly)
    {
        // SE再生
        AudioManager.Instance.PlaySFX("SystemActionPanel");

        var battleManager = FindObjectOfType<Battle>(); // lazy implementation...

        // カーソルを変更
        var texture = Resources.Load<Texture2D>("Icon/focus");
        Cursor.SetCursor(texture, new Vector2(texture.width * 0.5f, texture.height * 0.5f), CursorMode.Auto);
        bool isSelectingTarget = false;
        bool isFinished = false;

        // Tips
        Inventory.Instance.ShowTipsText();

        do
        {
            // arrow that follow the mouse
            Vector3 mousePosition = Input.mousePosition / parent.scaleFactor;
            var targetBattler = battleManager.GetBattlerByPosition(mousePosition, isTeammateAllowed, isEnemyAllowed, isAliveOnly);

            if (!ReferenceEquals(targetBattler, null))
            {
                isSelectingTarget = true;
                battleManager.PointTargetWithArrow(targetBattler, 0.25f);
                if (Input.GetMouseButtonDown(0))
                {
                    isFinished = true;

                    // 選択した敵
                    ItemExecute.Instance.SetTargetBattler(targetBattler);

                    // アイテムを使用
                    ItemExecute.Instance.Invoke(selectingItem.functionName, 0);
                    ProgressManager.Instance.RemoveItemFromInventory(selectingItem);
                    CloseInventory(true);

                    // カーソルを戻す
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    battleManager.UnPointArrow(0.25f);

                    // ログ出力
                    GenerateBattleLog(CustomColor.AddColor(LocalizationManager.Localize(selectingItem.itemNameID), CustomColor.itemName()), targetBattler.CharacterNameColored);
                }
            }
            else if (isSelectingTarget)
            {
                isSelectingTarget = false;
                battleManager.UnPointArrow(0.25f);
            }

            if (Input.GetMouseButtonDown(1))
            {
                // SE再生
                AudioManager.Instance.PlaySFX("SystemActionCancel");

                // キャンセル
                isFinished = true;
                battleManager.UnPointArrow(0.25f);
                UnhideInventory();

                // カーソルを戻す
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            yield return null;
        } while (!isFinished);

        // Hide Tips
        Inventory.Instance.HideTipsText();
    }

    private void UseItem(Tuple<RectTransform, ItemDefine> item)
    {
        if (!canUseItem) return;

        ItemExecute.Instance.SetItemIcon(item.Item2.Icon);
        switch (item.Item2.itemType)
        {
            case CastType.SelfCast:
                ItemExecute.Instance.Invoke(item.Item2.functionName, 0);
                ProgressManager.Instance.RemoveItemFromInventory(item.Item2);
                CloseInventory(true);
                GenerateBattleLog(CustomColor.AddColor(LocalizationManager.Localize(item.Item2.itemNameID), CustomColor.itemName()), string.Empty);
                break;
            case CastType.Teammate:
                HideInventory();
                selectingItem = item.Item2;
                StartCoroutine(SelectingTarget(true, false, true));
                break;
            case CastType.Enemy:
                HideInventory();
                selectingItem = item.Item2;
                StartCoroutine(SelectingTarget(false, true, true));
                break;
            default:
                break;
        }
    }

    private void ShowDescription(Tuple<RectTransform, ItemDefine> item)
    {
        //フラグ
        isDescriptionShowing = true;

        // UI
        itemDescription.GetComponent<CanvasGroup>().DOFade(1.0f, 0.1f);
        itemDescription.position = new Vector2(item.Item1.position.x, item.Item1.position.y + (ItemSlotSize.y * 0.5f) + 10.0f);

        // データ読み込み
        itemDescription_Name.text = LocalizationManager.Localize(item.Item2.itemNameID);
        string effectTargetText = string.Empty;
        switch (item.Item2.itemType)
        {
            case CastType.SelfCast:
                effectTargetText = LocalizationManager.Localize("System.EffectSelf");
                break;
            case CastType.Teammate:
                effectTargetText = LocalizationManager.Localize("System.EffectTeam");
                break;
            case CastType.Enemy:
                effectTargetText = LocalizationManager.Localize("System.EffectEnemy");
                break;
            default:
                break;
        }
        itemDescription_Target.text = LocalizationManager.Localize("System.EffectTarget") + effectTargetText;
        itemDescription_Effect.text = item.Item2.effectText + "\n\n" + LocalizationManager.Localize(item.Item2.descriptionID);

        // 強制更新
        itemDescription_Name.ForceMeshUpdate();
        itemDescription_Target.ForceMeshUpdate();
        itemDescription_Effect.ForceMeshUpdate();
        
        // Resize UI
        itemDescription.sizeDelta = new Vector2(itemDescription.sizeDelta.x, 
            (itemDescription_Name.rectTransform.rect.height + itemDescription_Target.rectTransform.rect.height + (itemDescription_Effect.GetRenderedValues(false).y) + itemDescription_Effect.fontSize));
    }

    private void HideDescription()
    {
        isDescriptionShowing = false;
        itemDescription.GetComponent<CanvasGroup>().DOFade(0.0f, 0.1f);
    }

    /// <summary>
    /// 戦闘ログを出力
    /// </summary>
    private void GenerateBattleLog(string itemName, string targetName)
    {
        var battleManager = FindObjectOfType<Battle>(); // lazy implementation...
        if (ReferenceEquals(battleManager, null)) return;
        string self = battleManager.GetCurrentBattler().CharacterNameColored;
        string output = string.Empty;
        if (targetName == string.Empty) // 自身に使う系
        {
            output = String.Format(LocalizationManager.Localize("BattleLog.ItemSelf"), self, itemName);
        }
        else if (targetName == self) // 他人に使えるアイテムだが自分に使ったパターン
        {
            output = String.Format(LocalizationManager.Localize("BattleLog.ItemSelfToSelf"), self, itemName);
        }
        else if (targetName != self) // 他人に使った
        {
            output = String.Format(LocalizationManager.Localize("BattleLog.ItemOther"), self, targetName, itemName);
        }
        if (output != string.Empty)
        {
            battleManager.AddBattleLog(output);
        }
    }
}
