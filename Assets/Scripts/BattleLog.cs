using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using TMPro;


// Custom Type that's like a mutable tuple type variable.
public class Pair<T, U>
{
    public Pair()
    {
    }

    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }

    public T First { get; set; }
    public U Second { get; set; }
};

[RequireComponent(typeof(RectTransform))]
public class BattleLog : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] int maxLogDisplay = 4; // 同時に表示できるログ行数
    [SerializeField] float displayTime = 5.0f; // 表示時間
    [SerializeField, Range(0.0f, 0.5f)] float animSpeed = 0.25f;
    [SerializeField, Range(0.0f, 1.5f)] float fadeTime = 1f;

    [Header("Reference")]
    [SerializeField] RectTransform parentObj;
    [SerializeField] GameObject logObjectOrigin;

    [Header("Debug")]
    [SerializeField] List<Pair<GameObject, float>> logObjects; // Pair<オブジェクト、残り表示時間>

    // Start is called before the first frame update
    void Start()
    {
        parentObj = GetComponent<RectTransform>();
        logObjects = new List<Pair<GameObject, float>>();
    }

    /// <summary>
    /// ローカライズ済みのテキストを入れてください
    /// </summary>
    public void RegisterNewLog(string text)
    {
        GameObject obj = Instantiate(logObjectOrigin, logObjectOrigin.transform.parent);
        obj.name = "Log";
        obj.SetActive(true);
        var textComponent = obj.GetComponentInChildren<TMP_Text>();
        if (!ReferenceEquals(textComponent, null))
        {
            textComponent.text = text;
            textComponent.ForceMeshUpdate(true, true);
        }
        var canvasGrp = obj.GetComponent<CanvasGroup>();
        canvasGrp.alpha = 0.0f;
        canvasGrp.DOFade(1.0f, animSpeed * 2.0f);

        // resize
        float y_deltaSize = obj.GetComponent<RectTransform>().sizeDelta.y;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(textComponent.GetRenderedValues().x + 40.0f, y_deltaSize);
        obj.GetComponent<RectTransform>().localPosition = Vector2.zero;
        obj.GetComponent<RectTransform>().localScale = Vector3.zero;
        obj.GetComponent<RectTransform>().DOScale(Vector3.one, animSpeed);

        logObjects.Insert(0, new Pair<GameObject, float>(obj, displayTime));

        // move older logs upward
        float alphaPerCnt = 1.0f / maxLogDisplay;
        for (int i = 0; i < logObjects.Count; i++)
        {
            logObjects[i].First.GetComponent<RectTransform>().DOLocalMoveY(i * y_deltaSize, animSpeed);
            logObjects[i].First.GetComponent<CanvasGroup>().DOFade(1.0f - (i * alphaPerCnt), animSpeed);
        }

        if (logObjects.Count > maxLogDisplay)
        {
            RemoveLog(logObjects[logObjects.Count-1].First);
            logObjects.RemoveAt(logObjects.Count - 1);
        }
    }

    /// <summary>
    /// ログを消す処理
    /// </summary>
    /// <param name="obj"></param>
    private void RemoveLog(GameObject obj)
    {
        var canvasGrp = obj.GetComponent<CanvasGroup>();
        canvasGrp.DOFade(0.0f, fadeTime).OnComplete(() => { Destroy(obj); });
    }

    private void Update()
    {
        if (logObjects.Count == 0) return;

        for (int i = 0; i < logObjects.Count; i++)
        {
            logObjects[i].Second = Mathf.Max(logObjects[i].Second - Time.deltaTime, 0.0f);

            // 時間切れ
            if (logObjects[i].Second == 0.0f)
            {
                RemoveLog(logObjects[i].First);
                logObjects.RemoveAt(i);
                i--;
            }
        }
    }
}
