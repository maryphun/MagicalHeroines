using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class DemoManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text label;
    [SerializeField] Button button;
    [SerializeField] CanvasGroup buttonGroup;

#if DEMO
    // Start is called before the first frame update
    void Start()
    {
        // ゲーム設定をロード
        PlayerPrefsManager.LoadPlayerPrefs();

        if (DemoParameter.isDemoEnded)
        {
            label.text = LocalizationManager.Localize(DemoParameter.DemoEndTextID);
            label.DOFade(1.0f, 0.1f);
            button.gameObject.SetActive(true);
            buttonGroup.interactable = true;
            buttonGroup.DOFade(1.0f, 1.0f);
        }
        else
        {
            label.text = LocalizationManager.Localize(DemoParameter.DemoStartTextID);
            label.DOFade(1.0f, 0.1f);
            button.gameObject.SetActive(true);
            button.interactable = false;

            DOTween.Sequence().AppendInterval(1.5f).AppendCallback(() => 
            {
                button.interactable = true;
                buttonGroup.interactable = true;
                buttonGroup.DOFade(1.0f, 1.0f);
            });
        }
    }
#else
    void Awake()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }
#endif

    public void OnClickUnderstoodButton()
    {
        AudioManager.Instance.PlaySFX("SystemDecide");

#if DEMO
        if (DemoParameter.isDemoEnded)
        {
            DemoParameter.isDemoEnded = false;
        }
#endif

        StartCoroutine(SceneTransition("Title", 1.0f));
    }

    public IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        yield return new WaitForSeconds(animationTime);
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 
        asyncLoad.allowSceneActivation = true;
    }
}
