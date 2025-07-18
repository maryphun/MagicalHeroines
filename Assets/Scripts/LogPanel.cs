using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LogPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float fadeAnimationTime = 0.5f;
    [SerializeField] private Sprite dialogueBoxWithName; 
    [SerializeField] private Sprite dialogueBoxWithoutName;

    [Header("References")]
    [SerializeField] private LogDialogField logDialogOrigin;
    [SerializeField] private CanvasGroup logPanelCanvas;
    [SerializeField] private RectTransform scrollSize;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private ToggleAutoButton autoplayer;
    [SerializeField] private SkipDialogueUI skipDialogue;
    [SerializeField] private OptionPanel optionPanel;
    [SerializeField] private NovelEditor.NovelPlayer novelplayer;

    [Header("Debug")]
    [SerializeField] private List<LogDialogField> logObjs;

    public void OpenPanel()
    {
        if (optionPanel.IsOpen()) return;
        if (!novelplayer.IsPlaying) return;

        logPanelCanvas.DOFade(1.0f, fadeAnimationTime);
        logPanelCanvas.interactable = true;
        logPanelCanvas.blocksRaycasts = true;

        SetupLogPanel();
        autoplayer.ForceStopAutoPlay();
        skipDialogue.ForceStopSkipping();
    }

    public void ClosePanel()
    {
        if (!novelplayer.IsPlaying) return;

        logPanelCanvas.interactable = false;
        logPanelCanvas.blocksRaycasts = false;

        logPanelCanvas.DOFade(0.0f, fadeAnimationTime);

        // ログを消す
        for (int i = 0; i < logObjs.Count; i++)
        {
            Destroy(logObjs[i].gameObject, fadeAnimationTime);
        }
        logObjs.Clear();
        logObjs = null;
    }

    private void SetupLogPanel()
    {
        logDialogOrigin.gameObject.SetActive(false);
        var log = LoggerManager.Instance.GetLog();

        logObjs = new List<LogDialogField>();

        const float LogHeight = 170;
        const float LogGap = 20;

        for (int i = 0; i < log.Count; i++)
        {
            logObjs.Add(Instantiate(logDialogOrigin.gameObject, logDialogOrigin.transform.parent).GetComponent<LogDialogField>());

            // Ｓｐｒｉｔｅを変更
            if (log[i].Item1 == string.Empty)
            {
                // 名前なし
                logObjs[i].Sprite.sprite = dialogueBoxWithoutName;
            }
            else
            {
                logObjs[i].Sprite.sprite = dialogueBoxWithName;
            }

            // 内容書き込み
            logObjs[i].Name.text = log[i].Item1;
            logObjs[i].Dialog.text = log[i].Item2;

            // 位置調整
            logObjs[i].Sprite.rectTransform.anchoredPosition = new Vector2(0.0f, LogGap + ((LogHeight+LogGap) * i));

            // 表示
            logObjs[i].gameObject.SetActive(true);
        }

        const float MinScrollSize = 800.0f;
        scrollSize.sizeDelta = new Vector2(0.0f, Mathf.Max(MinScrollSize, (LogHeight + LogGap) * (log.Count)));

        // Reset into the bottom
        scrollView.verticalNormalizedPosition = 0;
    }

    // Hotkey
    private void Update()
    {
        if (!logPanelCanvas.interactable)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                OpenPanel();
            }
        }
        else
        {
            if (Input.mouseScrollDelta.y < 0 && scrollView.verticalNormalizedPosition <= 0)
            {
                ClosePanel();
            }
        }
    }
}
