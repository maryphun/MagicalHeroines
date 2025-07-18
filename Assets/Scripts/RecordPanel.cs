using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RecordPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RecordSlot recordBtnOrigin;
    [SerializeField] private RectTransform contentSize;
    [SerializeField] private GameObject recordButton;

    [Header("Debug")]
    [SerializeField] private List<RecordSlot> recordBtn;

    private const float AnimationTime = 0.75f;

    private void Start()
    {
        if (ProgressManager.Instance.GetRecordsList().Count > 0)
        {
            recordButton.SetActive(true);
        }
    }

    public void OpenRecordPanel()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, AnimationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        InitializeData();
    }

    public void CloseRecordPanel()
    {
        // SE çƒê∂
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, AnimationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        foreach (RecordSlot btn in recordBtn)
        {
            Destroy(btn.gameObject, AnimationTime);
        }

        recordBtn.Clear();
        recordBtn = null;
    }

    public void InitializeData()
    {
        var records = ProgressManager.Instance.GetRecordsList();
        recordBtn = new List<RecordSlot>();

        for (int i = 0; i < records.Count; i++)
        {
            var newBtn = Instantiate(recordBtnOrigin.gameObject, recordBtnOrigin.transform.parent).GetComponent<RecordSlot>();
            var rect = newBtn.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, i * (-rect.sizeDelta.y + 5.0f));
            newBtn.Init(records[i]);
            recordBtn.Add(newBtn);
        }

        contentSize.sizeDelta = new Vector2(contentSize.sizeDelta.x, (records.Count+1) * (recordBtnOrigin.GetComponent<RectTransform>().sizeDelta.y + 5.0f));
    }
}
