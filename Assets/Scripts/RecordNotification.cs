using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;
using TMPro;

// 侵食記録
[System.Serializable]
public class Record
{
    public string recordNameID; // 記録データ名
    public string novelData; // シナリオデータ名
    public bool isNotified; // 初回Popup表示したか
    public bool isChecked;  // 見たか

    // 新規追加
    public Record(string RecordNameID, string NovelData)
    {
        recordNameID = RecordNameID;
        novelData = NovelData;
        isNotified = false;
        isChecked = false;
    }
}

public class RecordNotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private RectTransform notificationPopup;
    [SerializeField] private RectTransform recordTutorialPopup;
    [SerializeField] public TMP_Text recordTutorialText;
    [SerializeField] public GameObject recordButton;
    [SerializeField] public HomeCharacter homeCharacter;

    private GameObject tempRecordBtn;

    // Start is called before the first frame update
    void Start()
    {
        if (ProgressManager.Instance.HasUnnotifiedRecord())
        {
            canvasGrp.alpha = 1.0f;
            canvasGrp.blocksRaycasts = true;
            canvasGrp.interactable = true;

            StartCoroutine(NotificationPopup());
        }
    }

    IEnumerator NotificationPopup()
    {
        yield return new WaitForSeconds(0.5f);

        notificationPopup.gameObject.SetActive(true);
        AudioManager.Instance.PlaySFX("SystemAlert");
        notificationPopup.DOScaleY(1.0f, 0.75f).SetEase(Ease.OutElastic);
    }

    public void OnClickStartRecord()
    {
        AudioManager.Instance.PlaySFX("SystemDecide");
        Record record = ProgressManager.Instance.GetUnnotifiedRecord();
        NovelSingletone.Instance.PlayNovel("Record/" + record.novelData, true, EndRecord);
        ProgressManager.Instance.RecordNotified(record.recordNameID);
        ProgressManager.Instance.RecordChecked(record.recordNameID);
        
        notificationPopup.gameObject.SetActive(false);

        // BGM停止
        AudioManager.Instance.StopMusicWithFade();
    }

    public void OnClickCheckLater()
    {
        AudioManager.Instance.PlaySFX("SystemCancel");
        Record record = ProgressManager.Instance.GetUnnotifiedRecord();
        ProgressManager.Instance.RecordNotified(record.recordNameID);
        
        notificationPopup.gameObject.SetActive(false);

        if (ProgressManager.Instance.GetRecordsList().Count == 1)
        {
            DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
            {
                ShowRecordTutorialPopup();
            });
        }
        else
        {
            EndUI();
        }
    }

    public void ShowRecordTutorialPopup()
    {
        AudioManager.Instance.PlaySFX("SystemAlert");

        // show cancel popup
        recordTutorialPopup.gameObject.SetActive(true);
        recordTutorialPopup.DOScaleY(1.0f, 0.75f).SetEase(Ease.OutElastic);
        recordButton.gameObject.SetActive(true);

        // 画面左側の「侵食記録」ボタンで記録を確認できます。
        recordTutorialText.text = LocalizationManager.Localize("System.RecordTutorial").Replace("{s}", LocalizationManager.Localize("System.Record"));

        tempRecordBtn = Instantiate(recordButton);
        Destroy(tempRecordBtn.GetComponent<Button>());
    }

    public void OnClickTutorialConfirm()
    {
        AudioManager.Instance.PlaySFX("SystemDecide");

        recordTutorialPopup.gameObject.SetActive(false);
        Destroy(tempRecordBtn);

        EndUI();
    }

    public void EndRecord()
    {
        if (ProgressManager.Instance.GetRecordsList().Count == 1)
        {
            DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
            {
                ShowRecordTutorialPopup();
            });
        }
        else
        {
            EndUI();
        }

        // BGM再開
        AudioManager.Instance.PlayMusicWithFade("Loop 32 (HomeScene)", 2.0f);
    }

    // 終了
    public void EndUI()
    {
        const float animTime = 0.5f;

        canvasGrp.DOFade(0.0f, animTime);
        DOTween.Sequence().AppendInterval(animTime).AppendCallback(() => { canvasGrp.blocksRaycasts = false; });
        canvasGrp.interactable = false;

        // ホームキャラを台詞を喋らせる
        DOTween.Sequence().AppendInterval(animTime).AppendCallback(() => { homeCharacter.TriggerDialogue(); });
    }
}
