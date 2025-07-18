using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StageHandler : MonoBehaviour
{
    const int totalStageNum = 16;

    [Header("Setting")]
    [SerializeField] private float stageIconSpacing = 350.0f;
    [SerializeField] private float arrowOffsetY = 50.0f;
    [SerializeField] private int startingStage = 0;
    [SerializeField] private bool isDLCWorld = false;

    [Header("References")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private StageArrow stageArrow;
    [SerializeField] private GameObject checkedIcon;
    [SerializeField] private RectTransform[] stageIcon = new RectTransform[totalStageNum];

    private void Start()
    {
        if (ProgressManager.Instance.IsGameEnded() && !isDLCWorld) return;
        if (ProgressManager.Instance.IsDLCEnded() && isDLCWorld) return;
        Init();
    }

    public void Init()
    {
        int currentStage = ProgressManager.Instance.GetCurrentStageProgress() - startingStage;

        if (DLCManager.isDLCEnabled && isDLCWorld)
        {
            currentStage = ProgressManager.Instance.GetCurrentDLCStageProgress() - startingStage;
        }

        rect.anchoredPosition = new Vector3((currentStage-1) * (-stageIconSpacing), rect.anchoredPosition.y, 0.0f);

        stageArrow.SetStage(stageIcon[currentStage-1], arrowOffsetY);


        for (int i = 0; i < currentStage-1; i++)
        {
            var obj = Instantiate(checkedIcon, stageIcon[i]);
            obj.GetComponent<RectTransform>().localPosition = Vector3.zero;
            obj.GetComponent<RectTransform>().sizeDelta = stageIcon[i].sizeDelta;
            obj.SetActive(true);
        }
    }
}
