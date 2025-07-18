using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;
using UnityEngine.EventSystems;

public class BlackMarketManager : MonoBehaviour
{
    [System.Serializable]
    struct BlackMarketItem
    {
        public ItemDefine item;
        public EquipmentDefine equipment;
        public int cost;
        public int requirementProgress;
        public bool isUnique;
        public bool isEquipment;
    }

    [System.Serializable]
    struct ShopItem
    {
        public Button button;
        public BlackMarketItem data;
        public TMP_Text cost;
        public Vector2 descriptionPosition;
    }

    [Header("Setting")]
    [SerializeField] private Color costTextColorNormal = Color.white;
    [SerializeField] private Color costTextColorNoMoney = Color.red;
    [SerializeField] private BlackMarketItem[] ItemList;


    [Header("References")]
    [SerializeField] private ShopItem[] ShopItemList;
    [SerializeField] private RectTransform itemDescription;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private GameObject inventoryBack;

    [Header("Debug")]
    [SerializeField] private int sellingItemCnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        // PLAY BGM
        AudioManager.Instance.PlayMusicWithFade("DarkHole", 5.0f);
        AlphaFadeManager.Instance.FadeIn(2.0f);

        SetupBlackMarket();
    }

    public void SetupBlackMarket()
    {
        List<BlackMarketItem> itemToSell = new List<BlackMarketItem>();

        itemToSell = ItemList.Where((x) => ProgressManager.Instance.GetCurrentStageProgress() >= x.requirementProgress).ToList();

        sellingItemCnt = 0;
        for (int i = 0; i < itemToSell.Count; i++)
        {
            if (itemToSell[i].isUnique)
            {
                if (itemToSell[i].isEquipment && ProgressManager.Instance.PlayerHasEquipment(itemToSell[i].equipment))
                {
                    continue;
                }
                else if (!itemToSell[i].isEquipment && ProgressManager.Instance.PlayerHasItem(itemToSell[i].item))
                {
                    continue;
                }
            }
            
            ShopItemList[sellingItemCnt].button.gameObject.SetActive(true);
            ShopItemList[sellingItemCnt].data = itemToSell[i];
            ShopItemList[sellingItemCnt].button.image.sprite = itemToSell[i].isEquipment ? itemToSell[i].equipment.Icon : itemToSell[i].item.Icon;
            ShopItemList[sellingItemCnt].cost.text = "$" + itemToSell[i].cost.ToString();

            ShopItem tmp = ShopItemList[sellingItemCnt];
            ShopItemList[sellingItemCnt].button.onClick.RemoveAllListeners();
            ShopItemList[sellingItemCnt].button.onClick.AddListener(delegate { OnClickItem(tmp); });

            var evntTrigger = ShopItemList[sellingItemCnt].button.GetComponent<EventTrigger>();
            evntTrigger.triggers.Clear();

            EventTrigger.Entry hover = new EventTrigger.Entry();
            hover.eventID = EventTriggerType.PointerEnter;
            hover.callback = new EventTrigger.TriggerEvent();
            hover.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(delegate { OnHoverItem(tmp); } ));
            evntTrigger.triggers.Add(hover);

            EventTrigger.Entry unhover = new EventTrigger.Entry();
            unhover.eventID = EventTriggerType.PointerExit;
            unhover.callback = new EventTrigger.TriggerEvent();
            unhover.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>((eventData) => { UnHoverItem(); }));
            evntTrigger.triggers.Add(unhover);

            sellingItemCnt++;
        }

        // ものを販売していない位置は非表示に
        for (int i = sellingItemCnt; i < ShopItemList.Length; i ++)
        {
            ShopItemList[i].button.gameObject.SetActive(false);
        }

        // 値段テキストを更新
        UpdateItemCostText();
    }

    /// <summary>
    /// 資金が足りなく買えないアイテムのコストテキストを赤にする
    /// </summary>
    private void UpdateItemCostText()
    {
        for (int i = 0; i < sellingItemCnt; i++)
        {
            ShopItemList[i].cost.color = ProgressManager.Instance.GetCurrentMoney() >= ShopItemList[i].data.cost ? costTextColorNormal : costTextColorNoMoney;
        }
    }

    public void OnClickBackButton()
    {
        // SE
        AudioManager.Instance.PlaySFX("BattleTransition");

        // Scene Transit
        StartCoroutine(SceneTransition("Home", 1.0f));

        // オートセーブを実行する
        AutoSave.ExecuteAutoSave();
    }


    public IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);
        yield return new WaitForSeconds(animationTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// アイテムを購入しようとしている
    /// </summary>
    private void OnClickItem(ShopItem item)
    {
        // 資金が足りるか
        if (ProgressManager.Instance.GetCurrentMoney() >= item.data.cost
            && (!ProgressManager.Instance.IsItemFull() || item.data.isEquipment)) // アイテム数が上限に達したか
        {
            //SE
            AudioManager.Instance.PlaySFX("Purchase");

            // Decrease money
            ProgressManager.Instance.SetMoney(ProgressManager.Instance.GetCurrentMoney() - item.data.cost);

            // Update
            UpdateItemCostText();

            // Get item
            if (item.data.isEquipment)
            {
                ProgressManager.Instance.AddNewEquipment(item.data.equipment);
            }
            else
            {
                ProgressManager.Instance.AddItemToInventory(item.data.item);
            }

            // 一回きりのアイテム
            if (item.data.isUnique)
            {
                item.button.gameObject.SetActive(false);

                UnHoverItem();
            }

            // Effect
            CreateFadingImage(item.button, 1.5f);
        }
        else
        {
            //SE
            AudioManager.Instance.PlaySFX("SystemFail");
        }
    }

    private void CreateFadingImage(Button item, float fadeTime)
    {
        Vector2 position = item.GetComponent<RectTransform>().position;

        // Create Canvas
        Image img = new GameObject("Using Item Graphic").AddComponent<Image>();
        img.transform.SetParent(item.transform);
        img.sprite = item.image.sprite;
        img.raycastTarget = false;
        img.rectTransform.position = position;
        img.rectTransform.localScale = item.image.rectTransform.localScale;


        img.rectTransform.DOMoveY(position.y + 150.0f, fadeTime);
        img.DOFade(0.0f, fadeTime);

        Destroy(img.gameObject, fadeTime);
    }

    private void OnHoverItem(ShopItem item)
    {
        // set text
        if (item.data.isEquipment)
        {
            itemNameText.text = LocalizationManager.Localize(item.data.equipment.equipNameID);
            itemDescriptionText.text = LocalizationManager.Localize(item.data.equipment.descriptionID);
        }
        else
        {
            itemNameText.text = LocalizationManager.Localize(item.data.item.itemNameID);
            itemDescriptionText.text = LocalizationManager.Localize(item.data.item.descriptionID);
        }

        // update mesh
        itemNameText.ForceMeshUpdate(true, true);

        // resize and reposition
        itemDescription.GetComponent<RectTransform>().anchoredPosition = item.descriptionPosition;

        itemDescriptionText.DOFade(0.0f, 0.0f);
        itemDescription.GetComponent<RectTransform>().DOSizeDelta(new Vector2(Mathf.Max(300.0f, itemNameText.GetRenderedValues().x + 60.0f), itemDescriptionText.GetRenderedValues().y + 130.0f), 0.1f);
        DOTween.Sequence().AppendInterval(0.10f).AppendCallback(() =>
        {
            itemDescriptionText.ForceMeshUpdate(true, true);
            itemDescriptionText.DOFade(1.0f, 0.1f);
            itemDescription.GetComponent<RectTransform>().DOSizeDelta(new Vector2(Mathf.Max(300.0f, itemNameText.GetRenderedValues().x + 60.0f), itemDescriptionText.GetRenderedValues().y + 130.0f), 0.1f);
        });

        // animation
        itemDescription.GetComponent<CanvasGroup>().DOFade(1.0f, 0.25f);
    }

    private void UnHoverItem()
    {
        itemDescription.GetComponent<CanvasGroup>().DOFade(0.0f, 0.25f);
    }

    public void OpenInventory()
    {
        // インベントリメニューを表示
        Inventory.Instance.obj.OpenInventory(false, OnCloseInventory);
        inventoryBack.SetActive(true);
    }

    public void OnCloseInventory()
    {
        // 何もしない
        inventoryBack.SetActive(false);
    }
}
