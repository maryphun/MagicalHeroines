using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class Battler : MonoBehaviour
{
    [Header("Preset")]
    [SerializeField] private bool autoInit = false;

    [Header("Setting")]
    [SerializeField] private BattlerAnimation animations;
    [SerializeField] private BattlerSoundEffect soundEffects;
    [SerializeField] private BattlerSoundEffect characterVoices;
    [SerializeField] private VFX attackVFX;
    [SerializeField] private float breathScale = 0.005f; // キャラのアニメション
    [SerializeField] private bool enableNormalAttack = true; // 普通攻撃出来るか
    [SerializeField] private bool isFemale = false; // 女なのか
    [SerializeField] private bool isMachine = false; // 機械なのか
    [SerializeField] private string attackCallback; // 特殊攻撃アニメーション
    [SerializeField] private bool isAoENormalAttack = false; // 普通攻撃が範囲攻撃か
    [SerializeField] private bool cantMissAttack = false; // 普通攻撃がMissできるか

    [Header("Debug：デバッグ用なのでここで設定する物は全部無効です。\nEnemyDefineとPlayerCharacterDefineで設定してください")]
    [SerializeField] public string character_name;
    [SerializeField] public int characterID;
    [SerializeField] public Sprite icon;
    [SerializeField] public bool isEnemy;
    [SerializeField] public Color character_color = Color.white;
    [SerializeField] public int max_hp;
    [SerializeField] public int max_mp;
    [SerializeField] public int current_hp;
    [SerializeField] public int current_mp;
    [SerializeField] public int attack;
    [SerializeField] public int defense;
    [SerializeField] public int speed;
    [SerializeField] public int currentLevel;
    [SerializeField] public bool isAlive;
    [SerializeField] public bool isBreathing;
    [SerializeField] public bool isTargettable; // 目標に出来るか
    [SerializeField] public BattlerAnimationType currentAnimation;
    [SerializeField] public UnityEvent onDeathEvent;
    [SerializeField] public CountableUnityEvent onAttackedEvent;
    [SerializeField] public UnityEvent onTurnBeginEvent;
    [SerializeField] public UnityEvent onTurnEndEvent;
    [SerializeField] public UnityEvent afterAttackEvent;
    [SerializeField] public List<Ability> abilities;
    [SerializeField] public EquipmentDefine equipment;
    [SerializeField] public List<EnemyActionPattern> actionPattern; // 敵AI作成用
    [SerializeField] public List<Pair<Ability, int>> abilityOnCooldown; // チャージ中の特殊技
    [SerializeField] public List<Ability> disabledAbilities; // 使用不可の特殊技

    [Header("References")]
    [SerializeField] private Image graphic;
    [SerializeField] private TMP_Text name_UI;
    [SerializeField] private Image hpBarFill;
    [SerializeField] private Image shadow;
    [SerializeField] private Image deadIcon;
    [SerializeField] private GameObject deadVFX;

    [HideInInspector] public float Ease { get { return ease; } }
    [HideInInspector] public RectTransform RectTransform { get { return GetComponent<RectTransform>(); } }
    [HideInInspector] public Image Graphic { get { return graphic; } }
    [HideInInspector] public bool EnableNormalAttack { get { return enableNormalAttack; } set { enableNormalAttack = value; } }
    [HideInInspector] public bool IsMachine { get { return isMachine; } }
    [HideInInspector] public bool IsFemale { get { return isFemale; } }
    [HideInInspector] public bool CantMissAttack { get { return cantMissAttack; } }
    [HideInInspector] public string CharacterNameColored { get { return CustomColor.AddColor(character_name, character_color); } }
    [HideInInspector] public string AttackCallback { get { return attackCallback; } }
    [HideInInspector] public bool IsAoENormalAttack { get { return isAoENormalAttack; } }

    private Vector3 originalScale;
    private float ease = 0.0f;
    private RectTransform graphicRect;
    private Image mpBarFill;

    [Serializable]
    public class CountableUnityEvent
    {
        [SerializeField] private UnityEvent<Battler, Battler, int> evt;
        [SerializeField] public int EventCount { get; private set; }

        public CountableUnityEvent()
        {
            evt = new UnityEvent<Battler, Battler, int>();
            EventCount = 0;
        }

        public void AddListener(UnityAction<Battler, Battler, int> call)
        {
            evt.AddListener(call);
            EventCount++;
        }

        public void RemoveListener(UnityAction<Battler, Battler, int> call)
        {
            evt.RemoveListener(call);
            EventCount--;
        }

        public void RemoveAllListeners()
        {
            evt.RemoveAllListeners();
            EventCount = 0;
        }

        public void Invoke(Battler attacked, Battler attacker, int value)
        {
            evt.Invoke(attacked, attacker, value);
        }
    }

    private void Awake()
    {
        graphicRect = graphic.GetComponent<RectTransform>();

        onDeathEvent = new UnityEvent();
        onDeathEvent.RemoveAllListeners();
        onTurnBeginEvent = new UnityEvent();
        onTurnBeginEvent.RemoveAllListeners();
        onTurnEndEvent = new UnityEvent();
        onTurnEndEvent.RemoveAllListeners();
        afterAttackEvent = new UnityEvent();
        afterAttackEvent.RemoveAllListeners();
        onAttackedEvent = new CountableUnityEvent();
        onAttackedEvent.RemoveAllListeners();

        if (autoInit)
        {
            Initialize();
            HideBars();
            name_UI.alpha = 0.0f;
        }
    }

    /// <summary>
    /// 敵キャラクターの設定データをロードしてBattlerを生成する
    /// </summary>
    public void InitializeEnemyData(EnemyDefine enemy)
    {
        character_name = LocalizationManager.Localize(enemy.enemyName);
        characterID = -1; // 敵キャラはIDを持っていない
        isEnemy = true;
        character_color = enemy.character_color;
        icon = enemy.icon;
        max_hp = enemy.maxHP;
        max_mp = enemy.maxMP;
        current_hp = enemy.maxHP;
        current_mp = enemy.maxMP;
        attack = enemy.attack;
        defense = enemy.defense;
        speed = enemy.speed;
        currentLevel = enemy.level;
        PlayAnimation(BattlerAnimationType.idle);
        abilities = new List<Ability>();
        actionPattern = new List<EnemyActionPattern>();
        abilityOnCooldown = new List<Pair<Ability, int>>();

        foreach (var action in enemy.actionPattern)
        {
            if (action.chance <= 0.0f)
            {
                // 倍率が0
                continue;
            }

            if (action.actionType == EnemyActionType.SpecialAbility && action.ability.requiredLevel > currentLevel)
            {
                // レベル不足
                continue;
            }

            actionPattern.Add(action);
            if (action.actionType == EnemyActionType.SpecialAbility)
            {
                abilities.Add(action.ability);
                if (action.ability.disableOnDefault) disabledAbilities.Add(action.ability);
            }
        }

        Initialize();
    }

    /// <summary>
    /// プレイヤーキャラクターの設定データをロードしてBattlerを生成する
    /// </summary>
    public void InitializeCharacterData(Character character)
    {
        character_name = LocalizationManager.Localize(character.characterData.nameID);
        if (character.is_corrupted) character_name = LocalizationManager.Localize(character.characterData.corruptedName);
        characterID = character.characterData.characterID;
        isEnemy = false;
        character_color = character.characterData.color;
        icon = character.characterData.icon;
        max_hp = character.current_maxHp;
        max_mp = character.current_maxMp;
        current_hp = Mathf.Min(character.current_hp, max_hp);
        current_mp = Mathf.Min(character.current_mp, max_mp);
        attack = character.current_attack;
        defense = character.current_defense;
        speed = character.current_speed;
        currentLevel = character.current_level;
        PlayAnimation(BattlerAnimationType.idle);
        abilityOnCooldown = new List<Pair<Ability, int>>();

        abilities = new List<Ability>();
        if (character.characterData.abilities.Count > 0)
        {
            for (int i = 0; i < character.characterData.abilities.Count; i++)
            {
                if (currentLevel >= character.characterData.abilities[i].requiredLevel
                    && character.hornyEpisode >= character.characterData.abilities[i].requiredHornyness)
                {
                    abilities.Add(character.characterData.abilities[i]);
                    if (character.characterData.abilities[i].disableOnDefault) disabledAbilities.Add(character.characterData.abilities[i]);
                }
            }
            abilities.Sort((x, y) => x.requiredLevel.CompareTo(y.requiredLevel));
        }

        ApplyEquipmentFromCharacter(character.characterData.characterID);
        Initialize();
    }

    /// <summary>
    /// キャラが持っている装備を有効化
    /// </summary>
    public void ApplyEquipmentFromCharacter(int characterID)
    {
        if (ProgressManager.Instance.GetCharacterEquipment(characterID, ref equipment))
        {
            try
            {
                if (equipment.battleStartFunctionName != string.Empty)
                {
                    EquipmentExecute.Instance.StartCoroutine(equipment.battleStartFunctionName, this);
                }
                max_hp = Mathf.Max(max_hp + equipment.hp, 1);
                max_mp = Mathf.Max(max_mp + equipment.sp, max_mp == 0 ? 0 : 1);
                attack = Mathf.Max(attack + equipment.atk, 0);
                defense = Mathf.Max(defense + equipment.def, 0);
                speed = Mathf.Max(speed + equipment.spd, 0);

                if (equipment.hp > 0) current_hp += equipment.hp;
                current_hp = Mathf.Clamp(current_hp, 1, max_hp);
                if (equipment.sp > 0) current_mp += equipment.sp;
                current_mp = Mathf.Clamp(current_mp, 1, max_mp);
            }
            catch (Exception ex)
            {
                Debug.LogError($"An exception occurred: {ex.Message}");
            }
        }
    }

    public void AddAbilityToCharacter(Ability ability)
    {
        abilities.Add(ability);
        abilities.Sort((x, y) => x.requiredLevel.CompareTo(y.requiredLevel));
    }

    public void RemoveAbilityFromCharacter(Ability ability)
    {
        abilities.Remove(ability);
    }

    public void RemoveAbilityFromCharacter(string abilityfunctionName)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].functionName == abilityfunctionName)
            {
                abilities.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// キャラクター編成画面の場合表示データが違う
    /// </summary>
    public void SetupFormationPanelMode()
    {
        name_UI.alpha = 0.0f;
    }

    public void Initialize()
    {
        graphic.sprite = animations.idle;
        name_UI.text = "Lv" + currentLevel + " <size=20>" + character_name;
        name_UI.color = character_color;
        UpdateHPBar();
        isAlive = current_hp > 0;   // 最初からリタイア状態のもありかもしれない
        isTargettable = true;
        isBreathing = true;

        if (max_mp > 0)
        {
            // MP表示
            var originObj = hpBarFill.transform.parent.gameObject; // HPBARを複製
            var obj = Instantiate(originObj, originObj.transform.parent);
            obj.name = "SP Bar";
            var rect = obj.GetComponent<RectTransform>();
            rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y - originObj.GetComponent<RectTransform>().sizeDelta.y);

            mpBarFill = obj.transform.GetChild(0).GetComponent<Image>();
            mpBarFill.color = Color.blue;

            UpdateMPBar();
        }
        else
        {
            mpBarFill = null;
        }

        originalScale = graphic.rectTransform.localScale;
    }

    public Vector2 GetCharacterSize()
    {
        if (graphicRect == null) return Vector2.zero;
        return new Vector2(graphicRect.rect.width * Mathf.Abs(graphicRect.localScale.x) * CanvasReferencer.Instance.GetScaleFactor(), graphicRect.rect.height * Mathf.Abs(graphicRect.localScale.y) * CanvasReferencer.Instance.GetScaleFactor());
    }

    public RectTransform GetGraphicRectTransform()
    {
        return graphicRect;
    }
    public RectTransform GetShadowRectTransform()
    {
        return shadow.GetComponent<RectTransform>();
    }

    /// <summary>
    /// キャラクターの画像中央を取得
    /// </summary>
    public Vector2 GetMiddleGlobalPosition()
    {
        return new Vector2(graphicRect.position.x, graphicRect.position.y + GetCharacterSize().y * 0.5f);
    }

    private void Update()
    {
        if (!isAlive) return;
        if (!isBreathing) return;

        ease = (ease + Time.deltaTime);

        Mathf.PingPong(ease, 1.0f);

        float value = (EaseInOutSine(ease) * breathScale);

        graphic.rectTransform.localScale = new Vector3(originalScale.x, originalScale.y - value, originalScale.z);
    }

    private float EaseInOutSine(float x) 
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1.0f) / 2.0f;
    }

    public void PlayAnimation(BattlerAnimationType type)
    {
        if (CheckDead())
        {
            // 死亡状態はリタイアアニメションしか流れない
            graphic.sprite = animations.retire;
            currentAnimation = BattlerAnimationType.retire;
            return;
        }

        // アニメション再生時に画像をぽにゅぽにゅにする
        if (currentAnimation != type && isBreathing && breathScale > 0.001f)
        {
            const float animTime = 0.6f;
            Vector3 localScale = GetGraphicRectTransform().localScale;
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    isBreathing = false;
                    GetGraphicRectTransform().DOScale(localScale * 0.98f, animTime / 6f).SetEase(DG.Tweening.Ease.OutCubic);
                })
                .AppendInterval(animTime / 6f)
                .AppendCallback(() =>
                {
                    GetGraphicRectTransform().DOScale(localScale, animTime - (animTime / 6f)).SetEase(DG.Tweening.Ease.OutElastic);
                })
                .AppendInterval(animTime / 6f)
                .AppendCallback(() =>
                {
                    isBreathing = true;
                });
        }

        currentAnimation = type;
        switch (type)
        {
            case BattlerAnimationType.attack:
                graphic.sprite = animations.attack;
                return;
            case BattlerAnimationType.attacked:
                graphic.sprite = animations.attacked;
                return;
            case BattlerAnimationType.idle:
                graphic.sprite = animations.idle;
                return;
            case BattlerAnimationType.item:
                graphic.sprite = animations.item;
                return;
            case BattlerAnimationType.magic:
                graphic.sprite = animations.magic;
                return;
            case BattlerAnimationType.retire:
                graphic.sprite = animations.retire;
                return;
            default:
                return;
        }
    }

    public void SpawnAttackVFX(Battler target)
    {
        var obj = Instantiate(attackVFX.gameObject, target.transform);

        obj.GetComponent<RectTransform>().position = target.GetMiddleGlobalPosition();
    }

    /// <summary>
    /// ダメージを食らった
    /// </summary>
    public int DeductHP(Battler source, int damage, bool ignoreBuff = false)
    {
        if (!ignoreBuff && onAttackedEvent.EventCount > 0)
        {
            int oldHP = current_hp;
            onAttackedEvent.Invoke(this, source, damage);
            return oldHP - current_hp; // HP を計算
        }

        int realDamage = damage;

        // 少なくても1ダメージは保証される
        realDamage = Mathf.Max(1, realDamage);

        if (realDamage > 0)
        {
            current_hp = Mathf.Max(0, current_hp - realDamage);
            UpdateHPBar();
            if (CheckDead())
            {
                KillBattler();

                // 装備特殊処理 (ダイヤモンドの聖核装備)
                if (equipment != null && equipment.pathName == "Equip_Daiya")
                {
                    // 装備を消す
                    equipment = null;

                    // 復活
                    ReviveBattler(0.35f, 1.5f);

                    DOTween.Sequence()
                    .AppendInterval(1.5f)
                    .AppendCallback(() =>
                    {
                        FindObjectOfType<Battle>().AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.DaiyaRevive"), CharacterNameColored));
                        AudioManager.Instance.PlaySFX("Holy");
                    });

                    return realDamage;
                }
            }

            FindObjectOfType<Battle>().UpdateTurnBaseManager(false);
            return realDamage;
        }

        // dealt no damage
        return 0;
    }

    /// <summary>
    /// リタイア
    /// </summary>
    public void KillBattler()
    {
        if (!isAlive) return; // すでにリタイア

        isAlive = false;
        graphic.rectTransform.localScale = originalScale;
        PlayAnimation(BattlerAnimationType.retire);

        onDeathEvent.Invoke();
        onDeathEvent.RemoveAllListeners();

        FindObjectOfType<Battle>().RemoveAllBuffForCharacter(this);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.2f)
                .AppendCallback(() =>
                {
                    HideBars();
                    graphic.DOFade(0.0f, 1.0f);
                    shadow.DOFade(0.0f, 1.0f);
                    name_UI.DOFade(0.0f, 1.0f);

                    var obj = Instantiate(deadVFX, graphic.transform);
                    obj.GetComponent<RectTransform>().position = GetMiddleGlobalPosition();

                    // create icon
                    Image img = new GameObject("DeadIcon").AddComponent<Image>();
                    img.sprite = Resources.Load<Sprite>("Icon/Dead");
                    img.raycastTarget = false;
                    img.rectTransform.SetParent(graphicRect);
                    img.rectTransform.position = GetMiddleGlobalPosition();
                    img.color = new Color(0.58f, 0.58f, 0.58f, 0.0f);
                    img.DOFade(1.0f, 0.75f);

                    // play SE
                    AudioManager.Instance.PlaySFX("Retired");
                    AudioManager.Instance.PlaySFX(GetSoundEffects(BattlerSoundEffectType.Retire));
                    AudioManager.Instance.PlaySFXDelay(GetCharacterVoiceName(BattlerSoundEffectType.Retire), 1.5f);
                });
    }

    /// <summary>
    /// 復活
    /// </summary>
    public void ReviveBattler(float hpPercentage, float delay)
    {
        if (isAlive) return;

        isAlive = true;
        graphic.rectTransform.localScale = originalScale;
        PlayAnimation(BattlerAnimationType.idle);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(delay)
                .AppendCallback(() =>
                {
                    // hp 回復
                    Heal((int)((float)max_hp * hpPercentage));

                    UnhideBars();
                    graphic.DOFade(1.0f, 1.0f);
                    shadow.DOFade(1.0f, 1.0f);
                    name_UI.DOFade(1.0f, 1.0f);

                    // VFX
                    var vfx = VFXSpawner.SpawnVFX("Holy", graphic.transform, GetMiddleGlobalPosition());
                    vfx.transform.localScale = new Vector3(3, 3, 3);

                    // delete icon
                    Destroy(graphicRect.Find("DeadIcon").gameObject);
                });
    }

    /// <summary>
    /// 治療
    /// </summary>
    public void Heal(int amount)
    {
        current_hp = Mathf.Min(current_hp + amount, max_hp);
        UpdateHPBar();
    }

    /// <summary>
    /// SP回復
    /// </summary>
    public void AddSP(int amount)
    {
        current_mp = Mathf.Min(current_mp + amount, max_mp);
        UpdateMPBar();
    }

    /// <summary>
    /// SP消耗
    /// </summary>
    public void DeductSP(int amount)
    {
        current_mp = Mathf.Max(current_mp - amount, 0);
        UpdateMPBar();
    }

    /// <summary>
    /// 死んでいるかどうかを確認
    /// </summary>
    public bool CheckDead()
    {
        return current_hp <= 0;
    }

    /// <summary>
    /// 特殊技をチャージ状態にする
    /// </summary>
    /// <param name="ability"></param>
    public void SetAbilityOnCooldown(Ability ability, int turn)
    {
        if (IsAbilityOnCooldown(ability) > 0)
        {
            // 重ね追加を防止
            for (int i = 0; i < abilityOnCooldown.Count; i++)
            {
                if (abilityOnCooldown[i].First == ability) abilityOnCooldown[i].Second = turn+1;
            }
        }
        else
        {
            abilityOnCooldown.Add(new Pair<Ability, int>(ability, turn+1));
        }
    }

    /// <summary>
    /// 特殊技チャージを更新
    /// </summary>
    public void UpdateAbilityCooldown()
    {
        if (abilityOnCooldown.Count > 0)
        {
            for (int i = 0; i < abilityOnCooldown.Count; i++)
            {
                abilityOnCooldown[i].Second--;
                if (abilityOnCooldown[i].Second <= 0)
                {
                    // remove element
                    abilityOnCooldown.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// 残りチャージ時間を返す
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public int IsAbilityOnCooldown(Ability ability)
    {
        if (abilityOnCooldown.Count > 0)
        {
            for (int i = 0; i < abilityOnCooldown.Count; i++)
            {
                if (abilityOnCooldown[i].First == ability) return abilityOnCooldown[i].Second;
            }
        }
        return -1;
    }

    /// <summary>
    ///  使用可・不可の切り替え
    /// </summary>
    public void SetAbilityActive(string abilityfunctionName, bool active)
    {
        if (active)
        {
            var ability = GetAbility(abilityfunctionName);
            if (ability != null)
            {
                disabledAbilities.Remove(ability);
            }
            else
            {
                Debug.Log("Ability absent. (" + abilityfunctionName + ")");
            }
        }
        else
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i].functionName == abilityfunctionName && !disabledAbilities.Contains(abilities[i]))
                {
                    disabledAbilities.Add(abilities[i]);
                }
            }
        }
    }

    /// <summary>
    /// 使用可能か
    /// </summary>
    public bool IsAbilityActive(Ability ability)
    {
        for (int i = 0; i < disabledAbilities.Count; i++)
        {
            if (disabledAbilities[i] == ability)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 技名で技を取得
    /// </summary>
    public Ability GetAbility(string abilityfunctionName)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].functionName == abilityfunctionName)
            {
                return abilities[i];
            }
        }

        return null;
    }

    /// <summary>
    /// HP Barを更新
    /// </summary>
    /// <returns></returns>
    public void UpdateHPBar()
    {
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

        hpBarFill.DOFillAmount(((float)current_hp / (float)max_hp), 0.2f);
        hpBarFill.DOColor(gradient.Evaluate(((float)current_hp / (float)max_hp)), 0.2f);
    }

    /// <summary>
    /// MP Barを更新
    /// </summary>
    public void UpdateMPBar()
    {
        if (mpBarFill)
        {
            mpBarFill.DOFillAmount(((float)current_mp / (float)max_mp), 0.2f);
        }
    }

    public void HideBars()
    {
        hpBarFill.transform.parent.gameObject.SetActive(false);
        if (mpBarFill)
        {
            mpBarFill.transform.parent.gameObject.SetActive(false);
        }
    }

    public void UnhideBars()
    {
        hpBarFill.transform.parent.gameObject.SetActive(true);
        if (mpBarFill)
        {
            mpBarFill.transform.parent.gameObject.SetActive(true);
        }
    }

    public void Shake(float time)
    {
        StartCoroutine(ShakeBody(time));
    }

    IEnumerator ShakeBody(float time)
    {
        Vector3 originalLocalPosition = graphicRect.localPosition;
        const int shakeCount = 10;
        float magnitude = 10.0f;
        for (float elapsedTime = 0.0f; elapsedTime < time; elapsedTime += time / ((float)shakeCount))
        {
            magnitude = -(magnitude * 0.75f);
            graphicRect.localPosition = new Vector3(originalLocalPosition.x + magnitude, originalLocalPosition.y, originalLocalPosition.z);
            yield return new WaitForSeconds(time / ((float)shakeCount));
        }

        // return to original
        graphicRect.localPosition = originalLocalPosition;
    }
    public void CreateGlowEffect(float finalScale, float time)
    {
        Image effect = Instantiate(graphicRect.gameObject, graphicRect.transform.parent).GetComponent<Image>();
        effect.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        effect.rectTransform.position = GetMiddleGlobalPosition();
        effect.rectTransform.DOScale(finalScale * graphicRect.localScale, time).SetEase(DG.Tweening.Ease.Linear);
        effect.DOFade(0.0f, time + Time.deltaTime).SetEase(DG.Tweening.Ease.Linear).OnComplete(() => { Destroy(effect.gameObject); });
    }
    public void ColorTint(Color color, float time)
    {
        graphic.DOComplete();
        Color originalColor = graphic.color;
        graphic.color = color;
        graphic.DOColor(originalColor, time);
    }


    public string GetSoundEffects(BattlerSoundEffectType type)
    {
        switch (type)
        {
            case BattlerSoundEffectType.Attack:
                if (soundEffects.attack == null || soundEffects.attack.Count <= 0) return string.Empty;
                return soundEffects.attack[UnityEngine.Random.Range(0, soundEffects.attack.Count)].name;
            case BattlerSoundEffectType.Attacked:
                if (soundEffects.attacked == null || soundEffects.attacked.Count <= 0) return string.Empty;
                return soundEffects.attacked[UnityEngine.Random.Range(0, soundEffects.attacked.Count)].name;
            case BattlerSoundEffectType.Retire:
                if (soundEffects.retire == null || soundEffects.retire.Count <= 0) return string.Empty;
                return soundEffects.retire[UnityEngine.Random.Range(0, soundEffects.retire.Count)].name;
            default:
                return string.Empty;
        }
    }
    public string GetCharacterVoiceName(BattlerSoundEffectType type)
    {
        switch (type)
        {
            case BattlerSoundEffectType.Attack:
                if (characterVoices.attack == null || characterVoices.attack.Count <= 0) return string.Empty;
                return characterVoices.attack[UnityEngine.Random.Range(0, characterVoices.attack.Count)].name;
            case BattlerSoundEffectType.Attacked:
                if (characterVoices.attacked == null || characterVoices.attacked.Count <= 0) return string.Empty;
                return characterVoices.attacked[UnityEngine.Random.Range(0, characterVoices.attacked.Count)].name;
            case BattlerSoundEffectType.Retire:
                if (characterVoices.retire == null || characterVoices.retire.Count <= 0) return string.Empty;
                return characterVoices.retire[UnityEngine.Random.Range(0, characterVoices.retire.Count)].name;
            default:
                return string.Empty;
        }
    }

    public void SetTransparent(float alpha, float time)
    {
        Graphic.DOFade(alpha, time);
        name_UI.DOFade(alpha, time);
        hpBarFill.transform.parent.gameObject.SetActive(alpha == 1.0f);
        if (mpBarFill != null) mpBarFill.transform.parent.gameObject.SetActive(alpha == 1.0f);
    }

    public void OnTurnBegin()
    {
        onTurnBeginEvent.Invoke();
    }

    public void OnTurnEnd()
    {
        onTurnEndEvent.Invoke();
    }

    #region EnemyAI

    public List<EnemyActionPattern> GetAllPossibleAction()
    {
        var rtn = new List<EnemyActionPattern>();

        foreach (var action in actionPattern)
        {
            // 行動条件チェック
            if (action.hasThresholdCondition)
            {
                float hpPercentage = (float)current_hp / (float)max_hp;
                float spPercentage = (float)current_mp / (float)max_mp;

                if (hpPercentage > action.HpThreshold && action.HpThreshold != 0) continue;
                if (spPercentage > action.SpThreshold && action.SpThreshold != 0) continue;
            }

            if (action.actionType == EnemyActionType.SpecialAbility)
            {
                // SP 不足
                if (current_mp < action.ability.consumeSP)
                {
                    continue;
                }

                // チャージ中
                if (IsAbilityOnCooldown(action.ability) > 0)
                {
                    continue;
                }

                // 使用不可
                if (!IsAbilityActive(action.ability))
                {
                    continue;
                }
            }

            rtn.Add(action);
        }

        // 取れるアクシオンがない場合は待機する
        return rtn;
    }

    public float GetAllChance(List<EnemyActionPattern> posibleActionPattern)
    {
        float rtn = 0.0f;
        foreach (var action in posibleActionPattern)
        {
            rtn += action.chance;
        }

        return rtn;
    }

    public EnemyActionPattern GetNextAction(List<EnemyActionPattern> posibleActionPattern)
    {
        float fullChance = GetAllChance(posibleActionPattern);

        float randomValue = UnityEngine.Random.Range(0.0f, fullChance);

        // Loop through the elements and subtract their weight from the random value until you find the selected element
        for (int i = 0; i < posibleActionPattern.Count; i++)
        {
            if (randomValue < posibleActionPattern[i].chance)
            {
                return posibleActionPattern[i];
            }

            randomValue -= posibleActionPattern[i].chance;
        }

        // This should never happen, but just in case
        Debug.LogError("Error in GetRandomElement(). Returning the last element.");
        return posibleActionPattern[posibleActionPattern.Count - 1];
    }

    /// <summary>
    /// 向きを変える
    /// </summary>
    public void ReverseFacing()
    {
        originalScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
    }

    #endregion EnemyAI
}
