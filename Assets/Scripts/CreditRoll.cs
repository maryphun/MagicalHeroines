using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class CreditRoll : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private bool isEndGameRoll = false;
    [SerializeField] private float rollTime = 5.0f;

    [Header("References")]
    [SerializeField] private Button backButton;
    [SerializeField] private RectTransform roll;
    [SerializeField] private TMP_Text buttonText;

    [Header("Debug")]
    [SerializeField] private bool isShowButton = false;

    // Start is called before the first frame update
    void Start()
    {
        AlphaFadeManager.Instance.FadeIn(0);
        backButton.gameObject.SetActive(false);
        isShowButton = false;
        roll.DOAnchorPosY(1020.0f, rollTime).SetEase(Ease.Linear);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(rollTime + 2.0f)
                .AppendCallback(() =>
                {
                    EndScene();
                });

        buttonText.text = (!isEndGameRoll) ? LocalizationManager.Localize("System.Back") : LocalizationManager.Localize("System.EndCredit");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isShowButton)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
            {
                isShowButton = true;
                backButton.gameObject.SetActive(true);
            }
        }
    }

    public void EndScene()
    {
        if (isEndGameRoll)
        {
            StartCoroutine(SceneTransition("Reward", 0.5f));
            return;
        }
        StartCoroutine(SceneTransition("Title", 0.5f));
    }
    
    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // ƒV[ƒ“‘JˆÚ
        AlphaFadeManager.Instance.FadeOut(animationTime);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to
        if (animationTime > 0)
        {
            yield return new WaitForSeconds(animationTime);
        }
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 
        asyncLoad.allowSceneActivation = true;
    }
}
