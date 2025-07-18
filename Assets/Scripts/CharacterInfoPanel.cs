using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(CanvasGroup))]
public class CharacterInfoPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float fadeTime = 0.35f;
    [SerializeField] private float hideDelay = 1.0f;

    [Header("References")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_Text characterLevel;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text characterHPValue;
    [SerializeField] private TMP_Text characterSPValue;
    [SerializeField] private Image characterHPFill;
    [SerializeField] private Image characterSPFill;
    [SerializeField] private CanvasGroup SPBar;
    [SerializeField] private RectTransform buffHandler;

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Battle battleManager;
    [SerializeField] private BattleSceneTutorial tutorialManager;

    [Header("Debug")]
    [SerializeField] private bool isDisplaying;
    [SerializeField] private Battler currentBattler;
    [SerializeField] private float hideDelayCnt;
    [SerializeField] private List<Buff> characterBuffs;
    [SerializeField] private List<GameObject> buffIcons;

    private void Start()
    {
        // init
        isDisplaying = false;
        GetComponent<CanvasGroup>().alpha = 0.0f;
    }

    public void SetCharacter(Battler battler)
    {
        currentBattler = battler;

        characterIcon.sprite = currentBattler.icon;
        characterLevel.text = LocalizationManager.Localize("Battle.Level") + "<space=2em>" + currentBattler.currentLevel;
        characterName.text =  currentBattler.character_name;
        characterHPValue.text = "<size=125%>" + currentBattler.current_hp.ToString() + "</size>/" + currentBattler.max_hp.ToString();
        characterHPFill.fillAmount = (float)currentBattler.current_hp / (float)currentBattler.max_hp;

        var gradient = new Gradient();
        // Blend color from green at 0% to red at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(Color.red, 0.35f);
        colors[1] = new GradientColorKey(Color.green, 1.0f);
        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);
        gradient.SetKeys(colors, alphas);
        characterHPFill.color = gradient.Evaluate(characterHPFill.fillAmount);

        if (currentBattler.max_mp > 0)
        {
            SPBar.alpha = 1.0f;
            characterSPValue.text = "<size=125%>" + currentBattler.current_mp.ToString() + "</size>/" + currentBattler.max_mp.ToString();
            characterSPFill.fillAmount = (float)currentBattler.current_mp / (float)currentBattler.max_mp;
        }
        else
        {
            SPBar.alpha = 0.0f;
        }

        // Display character buff
        UpdateIcons(currentBattler);

        isDisplaying = true;
        GetComponent<CanvasGroup>().DOFade(1.0f, fadeTime);
    }

    public void Hide()
    {
        isDisplaying = false;
        GetComponent<CanvasGroup>().DOFade(0.0f, fadeTime);
    }

    private void Update()
    {
        if (tutorialManager.IsPlayingTutorial) return; // チュートリアル中は再生しない

        if (isDisplaying)
        {
            // HP と SPを情報を常に更新
            characterHPValue.text = "<size=125%>" + currentBattler.current_hp.ToString() + "</size>/" + currentBattler.max_hp.ToString();
            characterSPValue.text = "<size=125%>" + currentBattler.current_mp.ToString() + "</size>/" + currentBattler.max_mp.ToString();

            characterHPFill.fillAmount = (float)currentBattler.current_hp / (float)currentBattler.max_hp;
            if (currentBattler.max_mp > 0) characterSPFill.fillAmount = (float)currentBattler.current_mp / (float)currentBattler.max_mp;
        }
        

        {
            Vector3 mousePosition = Input.mousePosition / mainCanvas.scaleFactor;
            var obj = (battleManager.GetBattlerByPosition(mousePosition, true, true, true));
            if (obj != null)
            {
                SetCharacter(obj);
                return;
            }
            else
            {
                hideDelayCnt += Time.deltaTime;
                if (hideDelayCnt > hideDelay)
                {
                    hideDelayCnt = 0.0f;
                    Hide();
                }
            }
        }
    }

    public void UpdateIcons(Battler target)
    {
        if (!isDisplaying) return;
        if (currentBattler != target) return;

        // Clear old buff icons if there are any.
        if (buffIcons != null) foreach (var obj in buffIcons) Destroy(obj);
        buffIcons = new List<GameObject>();

        // Create new buff icons
        characterBuffs = battleManager.GetAllBuffForSpecificBattler(currentBattler);
        Vector3 iconPosition = Vector3.zero;
        Vector3 addition = battleManager.BuffPositionAddition();
        foreach (var buff in characterBuffs)
        {
            var newObj = Instantiate(buff.graphic);
            newObj.transform.SetParent(buffHandler);
            newObj.GetComponent<RectTransform>().localPosition = iconPosition;
            newObj.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            Destroy(newObj.GetComponentInChildren<TMP_Text>());
            iconPosition += addition;
            buffIcons.Add(newObj);
        }
    }
}
