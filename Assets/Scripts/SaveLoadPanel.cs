using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Assets.SimpleLocalization.Scripts;
using UnityEngine.SceneManagement;

public class SaveLoadPanel : MonoBehaviour
{
    // スロット数を設定
    const int totalSlotNum = 10;

    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;

    [Header("References")]
    [SerializeField] private SaveLoadSlotButton[] saveSlot = new SaveLoadSlotButton[totalSlotNum];
    [SerializeField] private Button saveTab;
    [SerializeField] private Button loadTab;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup comfirmationPopUp;
    [SerializeField] private GameObject saveComfirm;
    [SerializeField] private GameObject loadComfirm;
    [SerializeField] private TMP_Text saveComfirmText;
    [SerializeField] private TMP_Text loadComfirmText;
    [SerializeField] private TMP_InputField saveCommentInput;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private HomeSceneUI homeSceneUI;

    [Header("Debug")]
    [SerializeField] private bool isSaving = false;
    [SerializeField] private int currentSelectingSlotIndex = -1;
    [SerializeField] private string[] slotComment = new string[totalSlotNum];
    [SerializeField] private bool[] isDataAvailable = new bool[totalSlotNum];
    [SerializeField] private bool isMainMenu = false;
    [SerializeField] private bool isOpen = false;

    public bool IsOpen { get { return isOpen; } }
    private Color _darkenedTabColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);

    private void Start()
    {
        for (int i = 0; i < totalSlotNum; i ++)
        {
            int slotIndex = i;
            saveSlot[i].Button.onClick.AddListener(delegate { OnClickSlot(slotIndex); });
            slotComment[i] = string.Empty;
        }
    }

    public void OpenSaveLoadPanel(bool isMainMenu)
    {
        // タイトルメニューからは仕様が違う
        this.isMainMenu = isMainMenu;
        isOpen = true;

        // SE 再生
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        comfirmationPopUp.alpha = 0.0f;
        comfirmationPopUp.interactable = false;
        comfirmationPopUp.blocksRaycasts = false;

        // データ初期化
        currentSelectingSlotIndex = -1;
        UpdateSlotInfo();

        // デフォルトタブ
        if (isMainMenu)
        {
            SwapToLoadTab(false);
        }
        else
        {
            SwapToSaveTab(false);
        }
    }

    public void CloseSaveLoadPanel()
    {
        isOpen = false;

        // SE 再生
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void SwapToSaveTab(bool playSE)
    {
        if (!isMainMenu)
        {
            saveTab.interactable = false;
            loadTab.interactable = true;

            saveTab.image.color = Color.white;
            loadTab.image.color = _darkenedTabColor;

            saveTab.GetComponent<RectTransform>().anchoredPosition = new Vector3(saveTab.GetComponent<RectTransform>().anchoredPosition.x, -33.0f, 0.0f);
            loadTab.GetComponent<RectTransform>().anchoredPosition = new Vector3(saveTab.GetComponent<RectTransform>().anchoredPosition.x, -50.0f, 0.0f);
        }

        if (playSE)
        {
            // SE
            AudioManager.Instance.PlaySFX("SystemTab");
        }

        // reset scroll rect
        scrollRect.verticalNormalizedPosition = 1.0f;

        // flag
        isSaving = true;

        // enable all slot
        for (int i = 1; i < totalSlotNum; i++)
        {
            saveSlot[i].Button.interactable = true;
        }

        // slot 1はオートセーブ用のスロットとして確保。マニュアルでセーブ不能
        saveSlot[AutoSave.AutoSaveSlot].Button.interactable = false;
    }

    public void SwapToLoadTab(bool playSE)
    {
        if (!isMainMenu)
        {
            saveTab.interactable = true;
            loadTab.interactable = false;

            saveTab.image.color = _darkenedTabColor;
            loadTab.image.color = Color.white;

            saveTab.GetComponent<RectTransform>().anchoredPosition = new Vector3(saveTab.GetComponent<RectTransform>().anchoredPosition.x, -50.0f, 0.0f);
            loadTab.GetComponent<RectTransform>().anchoredPosition = new Vector3(saveTab.GetComponent<RectTransform>().anchoredPosition.x, -33.0f, 0.0f);
        }

        if (playSE)
        {
            // SE
            AudioManager.Instance.PlaySFX("SystemTab");
        }

        // reset scroll rect
        scrollRect.verticalNormalizedPosition = 1.0f;

        // flag
        isSaving = false;

        // disable slot with no data
        for (int i = 0; i < totalSlotNum; i++)
        {
            saveSlot[i].Button.interactable = isDataAvailable[i];

#if DEMO
            if (SaveDataManager.GetDataProgress(i) > DemoParameter.EndChapter)
            {
                saveSlot[i].Button.interactable = false;
            }
# endif
        }
    }

    public void OnClickSlot(int slotIndex)
    {
        currentSelectingSlotIndex = slotIndex;

        if (isSaving)
        {
            saveComfirm.SetActive(true);
            loadComfirm.SetActive(false);

            if (SaveDataManager.IsDataExist(slotIndex))
            {
                saveComfirmText.text = string.Format(LocalizationManager.Localize("System.ConfirmReplace"), slotIndex + 1);
                saveCommentInput.text = slotComment[slotIndex];

                // SE
                AudioManager.Instance.PlaySFX("SystemWarning");
            }
            else
            {
                saveComfirmText.text = string.Format(LocalizationManager.Localize("System.ConfirmSave"), slotIndex + 1);
                saveCommentInput.text = string.Empty;

                // SE
                AudioManager.Instance.PlaySFX("SystemAlert");
            }
        }
        else
        {
            saveComfirm.SetActive(false);
            loadComfirm.SetActive(true);
            loadComfirmText.text = string.Format(LocalizationManager.Localize("System.ConfirmLoad"), slotIndex+1);

            // SE
            AudioManager.Instance.PlaySFX("SystemAlert");
        }

        comfirmationPopUp.DOFade(1.0f, animationTime * 0.5f);
        comfirmationPopUp.interactable = true;
        comfirmationPopUp.blocksRaycasts = true;
    }

    public void ClosePopUp()
    {
        comfirmationPopUp.DOFade(0.0f, animationTime * 0.5f).OnComplete(() => {
            comfirmationPopUp.interactable = false;
            comfirmationPopUp.blocksRaycasts = false;

            saveComfirm.SetActive(false);
            loadComfirm.SetActive(false);
        });

        // SE
        AudioManager.Instance.PlaySFX("SystemCancel");
    }

    public void ComfirmPopUp()
    {
        if (isSaving)
        {
            SaveDataManager.SaveJsonData(currentSelectingSlotIndex, saveCommentInput.text);
            PlayerPrefs.SetInt("LastSavedSlot", currentSelectingSlotIndex);
            PlayerPrefs.Save();

            UpdateSlotInfo();
            ClosePopUp();

            // SE
            AudioManager.Instance.PlaySFX("SystemSave");
        }
        else
        {
            SaveDataManager.LoadJsonData(currentSelectingSlotIndex);

            // ロードが使えるところは二か所ある
            if (isMainMenu)
            {
                StartCoroutine(this.SceneTransition());
            }
            else
            {
                StartCoroutine(homeSceneUI.SceneTransition("Home", 1.0f));
            }

            // SE
            AudioManager.Instance.PlaySFX("SystemActionPanel");
        }
    }

    private void UpdateSlotInfo()
    {
        for (int i = 0; i < totalSlotNum; i++)
        {
            isDataAvailable[i] = SaveDataManager.GetDataInfo(i, out string slotName, out string comment, out string dateTime);
            
            if (PlayerPrefs.GetInt("LastSavedSlot", -1) == i) slotName = "<color=red>" + slotName;
            saveSlot[i].SlotText.text = slotName;
            saveSlot[i].DateText.text = dateTime;
            slotComment[i] = comment;
        }
    }

    public IEnumerator SceneTransition(string sceneName = "Home", float animationTime = 1.0f)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to

        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);

        yield return new WaitForSeconds(animationTime);
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 

        asyncLoad.allowSceneActivation = true;
    }
}

public static class AutoSave
{
    public static readonly int AutoSaveSlot = 0; // オートセーブ用のスロット

    /// <summary>
    /// オートセーブ実行
    /// </summary>
    public static void ExecuteAutoSave()
    { 
        // PlayerDataが存在しない時はオートセーブ行わない、そもそもデバッグ時にしか発生しない状況のはず
        if (ReferenceEquals(ProgressManager.Instance.PlayerData, null)) return;
        if (!ProgressManager.Instance.IsInitialized) return;

        SaveDataManager.SaveJsonData(AutoSave.AutoSaveSlot, LocalizationManager.Localize("System.AutoSave"));
        PlayerPrefs.SetInt("LastSavedSlot", AutoSave.AutoSaveSlot);
        PlayerPrefs.Save();
    }
}