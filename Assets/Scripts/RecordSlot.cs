using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class RecordSlot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text recordText;

    [Header("Debug")]
    [SerializeField] private string novelID;
    [SerializeField] private string nameID;
    [SerializeField] private Record slotRecord;

    public void Init(Record record)
    {
        gameObject.SetActive(true);
        this.novelID = "Record/" + record.novelData;
        this.nameID = record.recordNameID;
        recordText.text = LocalizationManager.Localize(nameID);
        recordText.color = !record.isChecked ? Color.yellow : Color.grey;

        slotRecord = record;
    }

    public void OnClickRecord()
    {
        NovelSingletone.Instance.PlayNovel(novelID, true, OnEndNovel);
        ProgressManager.Instance.RecordChecked(nameID);

        // SE
        AudioManager.Instance.PlaySFX("SystemDecide");
        // BGMí‚é~
        AudioManager.Instance.StopMusicWithFade();
    }

    public void OnEndNovel()
    {
        Init(slotRecord);

        // BGMçƒäJ
        AudioManager.Instance.PlayMusicWithFade("Loop 32 (HomeScene)", 2.0f);
    }
}
