using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NovelEditor;

public class ScenarioDebuggerUI : MonoBehaviour
{
    [SerializeField] Canvas UICanvas;
    [SerializeField] NovelPlayer player;
    [SerializeField] NovelData data;

    private bool isPlaying = false;
    private int screenshotNumber = 0;

    private void Start()
    {
        PlayerPrefsManager.LoadPlayerPrefs();
    }

    public void PlayScript()
    {
        UICanvas.enabled = false;
        NovelSingletone.Instance.PlayNovel(data, true, End);
        isPlaying = true;
    }

    private void Update()
    {
        //if (isPlaying)
        //{
        //    // 終了チェック
        //    if (NovelSingletone.Instance.IsEnded())
        //    {
        //        // 終了
        //        Debug.Log("End");
        //        isPlaying = false;
        //        UICanvas.enabled = true;
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.F2))
        {
            screenshotNumber++;
            string fileName = screenshotNumber.ToString() + ".png";
            Debug.Log("output " + fileName);
            ScreenCapture.CaptureScreenshot(fileName, 1);
        }
    }

    private void End()
    {
        UICanvas.enabled = true;
        isPlaying = false;
    }
}
