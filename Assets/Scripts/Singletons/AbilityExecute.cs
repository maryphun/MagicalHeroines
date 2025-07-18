using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;
using System.Linq;
using System;

public class AbilityExecute : SingletonMonoBehaviour<AbilityExecute>
{
    [Header("References")]
    [SerializeField] private Battle battleManager;
    [SerializeField] private List<Battler> targetBattlers;

    /// <summary>
    /// Send references
    /// </summary>
    /// <param name="battleManagerScript"></param>
    public void Initialize(Battle battleManagerScript)
    {
        battleManager = battleManagerScript;
    }

    #region common methods
    public void SetTargetBattlers(List<Battler> targets)
    {
        targetBattlers = targets;
    }

    public void SetTargetBattler(Battler target)
    {
        targetBattlers = new List<Battler>();
        targetBattlers.Add(target);
    }

    static public FloatingText CreateFloatingText(Transform parent)
    {
        GameObject origin = Resources.Load<GameObject>("Prefabs/FloatingNumber");
        var obj = Instantiate(origin);
        obj.transform.SetParent(parent);
        var floatingTextComponent = obj.GetComponent<FloatingText>();

        return floatingTextComponent;
    }
    private FloatingText CreateFloatingAbilityText(Transform parent)
    {
        GameObject origin = Resources.Load<GameObject>("Prefabs/AbilityName");
        var obj = Instantiate(origin);
        obj.transform.SetParent(parent);
        var floatingTextComponent = obj.GetComponent<FloatingText>();

        return floatingTextComponent;
    }

    private void CreateFadingImage(Sprite sprite, float fadeTime)
    {
        Vector2 position = battleManager.GetCurrentBattler().GetMiddleGlobalPosition();

        // Create Canvas
        Image img = new GameObject("Using Item Graphic").AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.rectTransform.position = position;

        img.transform.SetParent(battleManager.GetCurrentBattler().transform.parent.parent);

        img.rectTransform.DOMoveY(position.y + 150.0f, fadeTime);
        img.DOFade(0.0f, fadeTime);

        Destroy(img.gameObject, fadeTime);
    }

    private void CreateMovingImage(Sprite sprite, Vector3 targetPosition, float animTime)
    {
        Vector2 position = battleManager.GetCurrentBattler().GetMiddleGlobalPosition();

        // Create Canvas
        Image img = new GameObject("Using Item Graphic").AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.rectTransform.position = position;

        img.transform.SetParent(battleManager.GetCurrentBattler().transform.parent.parent);

        img.rectTransform.DOMove(targetPosition, animTime, true).SetEase(Ease.Linear);

        Destroy(img.gameObject, animTime);
    }

    private RectTransform CreateProjectile(string projectileName, Vector3 start, Vector3 end, float time, bool rotateTowardDirection = true)
    {
        GameObject data = Resources.Load<GameObject>("Prefabs/VFX/" + projectileName);
        var obj = Instantiate(data);

        obj.transform.SetParent(battleManager.GetCurrentBattler().transform.parent.parent);

        var rect = obj.GetComponent<RectTransform>();
        rect.position = start;
        rect.DOMove(end, time, true).SetEase(Ease.Linear);
        
        // rotate
        // Calculate direction vector
        if (rotateTowardDirection)
        {
            Vector3 diff = end - start;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            rect.rotation = Quaternion.Euler(0f, 0f, rot_z - 90.0f);
        }

        Destroy(obj, time);
        return rect;
    }
    #endregion common methods

    #region abilities
    /// <summary>
    /// 緊急回復 (戦闘員)
    /// </summary>
    public void DeepBreath()
    {
        var target = battleManager.GetCurrentBattler();

        // 残りHPが少ないほど回復量が多くなる
        float percentage = (1.0f - ((float)target.current_hp / (float)target.max_hp));
        int healAmount = (int)(((float)target.max_hp * 0.40f) * percentage);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.DeepBreath");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ログ ({0}　が{1}する)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecuteSelf"), target.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.DeepBreath"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());

                    // effect
                    target.Heal(healAmount);

                    // play SE
                    AudioManager.Instance.PlaySFX("Hard-Breathing");

                    // VFX
                    var vfx = VFXSpawner.SpawnVFX("Worm", target.transform, target.GetGraphicRectTransform().position);
                    vfx.GetComponent<Image>().color = CustomColor.invisible();
                    vfx.GetComponent<Image>().DOFade(1.0f, 0.2f);
                    vfx.GetComponent<Image>().DOFade(0.0f, 0.2f).SetDelay(0.5f);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.heal())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }
    /// <summary>
    /// パワフルパンチ (戦闘員)
    /// </summary>
    public void PowerfulPunch()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int dmg = Mathf.FloorToInt((float)self.attack * 2.5f);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.PowerfulPunch");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.PowerfulPunch"), CustomColor.abilityName())));

        // キャラ移動の準備
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 1.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 1.5f, targetPos.y);
        var originalPos = self.GetComponent<RectTransform>().position;
        const float characterMoveTime = 0.35f;
        const float animationTime = 0.8f;
        const float strikeTime = 0.1f;
        const float attackStayTime = 0.35f;

        // Shake
        Coroutine shake = null;
        Vector2 positionPreShake = Vector2.zero;

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

                    // move
                    self.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // charge
                    self.PlayAnimation(BattlerAnimationType.attack);
                    AudioManager.Instance.PlaySFX("Charge");
                    positionPreShake = self.GetComponent<RectTransform>().position;
                    shake = ShakeManager.Instance.ShakeObject(self.GetComponent<RectTransform>(), animationTime, 5.0f);
                    self.ColorTint(Color.blue, animationTime);
                })
                .AppendInterval(animationTime)
                .AppendCallback(() =>
                {
                    ShakeManager.Instance.StopObjectShake(shake);
                    self.GetComponent<RectTransform>().position = positionPreShake;
                    var position = target.GetComponent<RectTransform>().position;
                    position = target.isEnemy ? new Vector2(position.x - target.GetCharacterSize().x * 0.5f, position.y) : new Vector2(position.x + target.GetCharacterSize().x * 0.5f, position.y);
                    self.DOComplete(true);
                    self.GetComponent<RectTransform>().DOMove(position, strikeTime).SetEase(Ease.Linear);

                    // 残像生成コンポネント
                    FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
                    fadeEffect.Initialize(strikeTime, 0.05f, self.GetGraphicRectTransform().GetComponent<Image>());
                })
                .AppendInterval(strikeTime)
                .AppendCallback(() =>
                {
                    var realDmg = Battle.CalculateDamage(dmg, target.defense, self.currentLevel, target.currentLevel, false);

                    // attack
                    self.PlayAnimation(BattlerAnimationType.attacked);
                    self.SpawnAttackVFX(target);
                    AudioManager.Instance.PlaySFX("PowerfulPunch", 1f);
                    target.Shake(0.75f);
                    target.DeductHP(self, realDmg);

                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDmg.ToString(), 64, CustomColor.damage());

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDmg, CustomColor.damage())));
                })
                .AppendInterval(attackStayTime)
                .AppendCallback(() =>
                {
                    self.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);

                    target.PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);

                    // stun for 1 turn
                    battleManager.AddBuffToBattler(self, BuffType.stun, 1, 0);
                });
    }

    /// <summary>
    /// 共倒れ (戦闘員)
    /// </summary>
    public void Tackle()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Tackle");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Tackle"), CustomColor.abilityName())));

        // キャラ移動の準備
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 1.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 1.5f, targetPos.y);
        var originalPos = self.GetComponent<RectTransform>().position;
        const float characterMoveTime = 0.35f;
        const float animationTime = 0.8f;
        const float strikeTime = 0.1f;
        const float attackStayTime = 0.35f;

        // Shake
        Coroutine shake = null;
        Vector2 positionPreShake = Vector2.zero;

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

                    // move
                    self.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // charge
                    self.PlayAnimation(BattlerAnimationType.attack);
                    AudioManager.Instance.PlaySFX("Charge");
                    positionPreShake = self.GetComponent<RectTransform>().position;
                    shake = ShakeManager.Instance.ShakeObject(self.GetComponent<RectTransform>(), animationTime, 5.0f);
                    self.ColorTint(Color.blue, animationTime);
                })
                .AppendInterval(animationTime)
                .AppendCallback(() =>
                {
                    ShakeManager.Instance.StopObjectShake(shake);
                    self.GetComponent<RectTransform>().position = positionPreShake;
                    var position = target.GetComponent<RectTransform>().position;
                    position = target.isEnemy ? new Vector2(position.x - target.GetCharacterSize().x * 0.5f, position.y) : new Vector2(position.x + target.GetCharacterSize().x * 0.5f, position.y);
                    self.DOComplete(true);
                    self.GetComponent<RectTransform>().DOMove(position, strikeTime).SetEase(Ease.Linear);
                    self.GetGraphicRectTransform().DORotate(new Vector3(0, 0, -40.0f), animationTime, RotateMode.Fast);

                    // 残像生成コンポネント
                    FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
                    fadeEffect.Initialize(strikeTime, 0.05f, self.GetGraphicRectTransform().GetComponent<Image>());
                })
                .AppendInterval(strikeTime)
                .AppendCallback(() =>
                {
                    // attack
                    self.PlayAnimation(BattlerAnimationType.attacked);
                    self.SpawnAttackVFX(target);
                    AudioManager.Instance.PlaySFX("PowerfulPunch", 1f);
                    target.Shake(0.75f);

                    // stun for 1 turn
                    battleManager.AddBuffToBattler(target, BuffType.stun, 1, 0);

                    // text
                    floatingText = CreateFloatingText(target.transform);
                    string stunString = LocalizationManager.Localize("Buff.Stun");
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), stunString, 64, CustomColor.stun());
                })
                .AppendInterval(attackStayTime)
                .AppendCallback(() =>
                {
                    self.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);
                    // rotate
                    self.GetGraphicRectTransform().DORotate(Vector3.zero, animationTime, RotateMode.Fast);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);

                    target.PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);

                    // stun for 1 turn
                    battleManager.AddBuffToBattler(self, BuffType.stun, 1, 0);
                });
    }

    /// <summary>
    /// 吸収触手 (触手怪人)
    /// </summary>
    public void SuckingTentacle()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int suckAmount = (int)((float)target.current_mp * 0.50f);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.SuckingTentacle");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　が触手を {1} に伸ばすーー！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Tentacle"), self.CharacterNameColored, target.CharacterNameColored));

        // キャラ移動の準備
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.15f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.15f, targetPos.y);
        var originalPos = self.GetComponent<RectTransform>().position;
        const float characterMoveTime = 0.35f;
        const float animationTime = 1f;

        AudioSource audio = AudioManager.Instance.GetSFXSource();

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

                    // move
                    self.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    self.PlayAnimation(BattlerAnimationType.magic);
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    self.Shake(animationTime);
                    target.Shake(animationTime);

                    // suck sp
                    self.AddSP(suckAmount);
                    target.DeductSP(suckAmount);
                    
                    // SE
                    audio = AudioManager.Instance.PlaySFX("Tentacle");
                    AudioManager.Instance.PlaySFX("TentacleDrain");

                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, self.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + suckAmount.ToString(), 64, CustomColor.SP());

                    // ログ ({1}　のSP {0} 吸収した！ )
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.SPDrain"), target.CharacterNameColored, CustomColor.AddColor(suckAmount, CustomColor.SP())));
                })
                .AppendInterval(animationTime * 0.5f)
                .AppendCallback(() =>
                {
                    if (target.IsFemale)
                    {
                        int hpSuckAmount = (int)((float)suckAmount * UnityEngine.Random.Range(0.5f, 0.9f));

                        // text
                        floatingText = CreateFloatingText(target.transform);
                        floatingText.Init(2.0f, self.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + suckAmount.ToString(), 64, CustomColor.heal());

                        // suck hp
                        self.Heal(hpSuckAmount);
                        target.DeductHP(self, hpSuckAmount);

                        // ログ ({1}　のHP {0} 吸収した！ )
                        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.HPDrain"), target.CharacterNameColored, CustomColor.AddColor(suckAmount, CustomColor.heal())));

                        // SE
                        AudioManager.Instance.PlaySFX("TentacleDrain");
                    }
                })
                .AppendInterval(animationTime * 0.5f)
                .AppendCallback(() =>
                {
                    self.PlayAnimation(BattlerAnimationType.idle);
                    target.PlayAnimation(BattlerAnimationType.idle);

                    self.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);

                    if (audio) audio.Stop();
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 触手サービス (触手怪人)
    /// </summary>
    public void TentacleService()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];
        int spAmount = self.current_mp;

        Transform originalParent = self.RectTransform.parent;
        int siblingIndex = self.RectTransform.GetSiblingIndex();
        Vector3 originalPosition = self.RectTransform.position;

        // ログ ({0}　が触手を {1} に伸ばすーー！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Tentacle"), self.CharacterNameColored, target.CharacterNameColored));
        
        // キャラ移動
        self.RectTransform.DOLocalMoveX(self.RectTransform.localPosition.x + 200.0f, 0.15f);

        // play SE
        AudioSource audio = AudioManager.Instance.GetSFXSource();
        AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                self.RectTransform.SetParent(target.RectTransform);
                self.RectTransform.DOLocalMove(new Vector3(200.0f, 0.0f, 0.0f), 0.2f);

                // play SE
                AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);
            })
            .AppendInterval(0.2f)
            .AppendCallback(()=> 
            {
                // 襲撃
                self.ReverseFacing();
                self.RectTransform.DOLocalMoveX(target.GetCharacterSize().x * 0.25f, 0.15f);

                // play SE
                AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);
            })
            .AppendInterval(0.15f)
            .AppendCallback(() =>
            {
                if (target.IsFemale)
                {
                    self.PlayAnimation(BattlerAnimationType.magic);
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    self.Shake(1.0f);
                    target.Shake(1.0f);

                    // give sp
                    target.AddSP(spAmount);
                    self.DeductSP(spAmount);

                    // SE
                    audio = AudioManager.Instance.PlaySFX("Tentacle");
                    AudioManager.Instance.PlaySFX("TentacleDrain");

                    // ログ (SP {0} を {1} に分け与えた！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.GiveSP"), CustomColor.AddColor(spAmount, CustomColor.SP()), target.CharacterNameColored));
                }
                else
                {
                    // ログ (効果はなかった。)
                    battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.NoEffect"));
                }
            })
            .AppendInterval(1.0f)
            .AppendCallback(() =>
            {
                if (target.IsFemale)
                {
                    self.PlayAnimation(BattlerAnimationType.idle);
                    target.PlayAnimation(BattlerAnimationType.idle);

                    if (audio) audio.Stop();
                }

                // 戻る
                self.RectTransform.DOLocalMoveX(self.RectTransform.localPosition.x + 200.0f, 0.15f);

                // play SE
                AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);
            })
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                self.RectTransform.SetParent(originalParent);
                self.RectTransform.SetSiblingIndex(siblingIndex);

                self.RectTransform.DOMove(new Vector3(self.RectTransform.position.x, originalPosition.y, 0.0f), 0.2f);

                // play SE
                AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);
            })
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                self.RectTransform.DOMove(originalPosition, 0.15f);

                // play SE
                AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);
            })
            .AppendInterval(0.15f)
            .AppendCallback(() =>
            {
                self.ReverseFacing();
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 媚薬ガス (触手怪人)
    /// </summary>
    public void Capture()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int attackDownAmount = (int)((float)target.attack * 0.25f);
        int speedDownAmount = (int)((float)target.speed * 0.25f);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Capture");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　が触手を {1} に伸ばすーー！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Tentacle"), self.CharacterNameColored, target.CharacterNameColored));

        // キャラ移動の準備
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        var originalPos = self.GetComponent<RectTransform>().position;
        const float characterMoveTime = 0.35f;
        const float animationTime = 1f;

        AudioSource audio = AudioManager.Instance.GetSFXSource();

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

                    // move
                    self.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    self.PlayAnimation(BattlerAnimationType.magic);
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    self.Shake(animationTime);
                    target.Shake(animationTime);

                    // debuff
                    battleManager.AddBuffToBattler(target, BuffType.attack_down, UnityEngine.Random.Range(2, 4), attackDownAmount);
                    battleManager.AddBuffToBattler(target, BuffType.speed_down, UnityEngine.Random.Range(2, 4), speedDownAmount);

                    // SE
                    audio = AudioManager.Instance.PlaySFX("Tentacle");
                })
                .AppendInterval(animationTime)
                .AppendCallback(() =>
                {
                    self.PlayAnimation(BattlerAnimationType.idle);
                    target.PlayAnimation(BattlerAnimationType.idle);

                    self.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);

                    if (audio) audio.Stop();
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 自己修復
    /// </summary>
    public void SelfRepair()
    {
        var target = battleManager.GetCurrentBattler();
        int healAmount = (int)((float)target.max_hp * 0.35f);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.SelfRepair");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ログ ({0}　が{1}する)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecuteSelf"), target.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.SelfRepair"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());

                    // effect
                    target.Heal(healAmount);

                    // play SE
                    AudioManager.Instance.PlaySFX("SelfRepair");

                    // VFX
                    VFXSpawner.SpawnVFX("SelfRepair", target.transform, target.GetGraphicRectTransform().position);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.heal())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 自己強化
    /// </summary>
    public void SelfEnchant()
    {
        var target = battleManager.GetCurrentBattler();

        // buff, value
        List<Tuple<BuffType, int>> possibleBuff = new List<Tuple<BuffType, int>>();
        possibleBuff.Add(new Tuple<BuffType, int>(BuffType.attack_up, (int)UnityEngine.Random.Range(target.attack * 0.1f, target.attack * 0.5f)));
        possibleBuff.Add(new Tuple<BuffType, int>(BuffType.heal, (int)UnityEngine.Random.Range(target.max_hp * 0.1f, target.max_hp * 0.2f)));
        possibleBuff.Add(new Tuple<BuffType, int>(BuffType.shield_up, (int)UnityEngine.Random.Range(3, 10)));
        possibleBuff.Add(new Tuple<BuffType, int>(BuffType.speed_up, (int)UnityEngine.Random.Range(4, 20)));

        // バフを選択
        Tuple<BuffType, int> buff = possibleBuff[UnityEngine.Random.Range(0, possibleBuff.Count)];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.SelfEnchant");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ログ ({0}　が{1}する)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecuteSelf"), target.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.SelfEnchant"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // effect
                    battleManager.AddBuffToBattler(target, buff.Item1, UnityEngine.Random.Range(2, 5), buff.Item2);

                    // TODO: change effect
                    // play SE
                    AudioManager.Instance.PlaySFX("SelfRepair");

                    // VFX
                    VFXSpawner.SpawnVFX("SelfRepair", target.transform, target.GetGraphicRectTransform().position);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// ブースト
    /// </summary>
    public void Booster()
    {
        var target = battleManager.GetCurrentBattler();
        int attackAddAmount = (int)((float)target.attack * 0.15f);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.Booster");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ログ ({0}　が{1}する)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), target.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Booster"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // effect
                    target.attack = target.attack + attackAddAmount;

                    // play SE
                    AudioManager.Instance.PlaySFX("SelfRepair");

                    // VFX
                    VFXSpawner.SpawnVFX("SelfRepair", target.transform, target.GetGraphicRectTransform().position);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.AttackUpStart"), target.CharacterNameColored, CustomColor.AddColor(attackAddAmount, CustomColor.damage())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 明穂戦特殊技
    /// </summary>
    public void HealAttack()
    {
        const int damage = 30;

        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.HealAttack");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // エフェクト (Holy)
        var vfx = VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        vfx.transform.SetSiblingIndex(0); // キャラの後ろに回す
        self.PlayAnimation(BattlerAnimationType.magic);

        float originalLocalPosY = self.GetGraphicRectTransform().localPosition.y;
        self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY + 30.0f, 0.75f);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.HealAttack"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // エフェクト (Light)
                    VFXSpawner.SpawnVFX("DarkBolt", target.transform, target.GetGraphicRectTransform().position);
                    
                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), damage.ToString(), 64, CustomColor.damage());
                    
                    // effect
                    target.DeductHP(self, damage);

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(damage, CustomColor.damage())));

                    // animation
                    target.Shake(0.75f);
                    self.PlayAnimation(BattlerAnimationType.idle);
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    // Audio
                    AudioManager.Instance.PlaySFX("Bolt");
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY, 0.2f);
                    target.PlayAnimation(BattlerAnimationType.idle);
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 明穂戦特殊技
    /// </summary>
    public void GreatRegen()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        float percentage = UnityEngine.Random.Range(0.05f, 0.15f);
        int hpHealAmount = Mathf.RoundToInt((float)target.max_hp * percentage);
        int spHealAmount = Mathf.RoundToInt((float)target.max_mp * 0.8f);
        
        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GreatRegen");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // エフェクト (Holy)
        VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        self.PlayAnimation(BattlerAnimationType.magic);

        float originalLocalPosY = self.GetGraphicRectTransform().localPosition.y;
        self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY + 30.0f, 0.75f);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.GreatRegen"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // animation
                    self.PlayAnimation(BattlerAnimationType.idle);

                    // エフェクト (Light)
                    for (int i = 0; i < 5; i ++)
                    {
                        Vector2 move = Vector2.zero;
                        move += Vector2.MoveTowards(move, move + new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100)), UnityEngine.Random.Range(0.0f, 300.0f));
                        VFXSpawner.SpawnVFX("Recovery", target.transform, target.GetMiddleGlobalPosition() + move);
                    }

                    // text
                    floatingText = CreateFloatingText(self.transform);
                    floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, 50.0f), new Vector2(0.0f, 100.0f), "+" + hpHealAmount.ToString(), 64, CustomColor.heal());

                    // effect
                    target.Heal(hpHealAmount);

                    // Audio
                    AudioManager.Instance.PlaySFX("Heal");

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(hpHealAmount, CustomColor.heal())));
                })
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    // SP
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + spHealAmount.ToString(), 64, CustomColor.SP());
                    target.AddSP(spHealAmount);
                })
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY, 0.2f);

                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// ミルク補給
    /// </summary>
    public void Milk()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];
        int hpHealAmount = UnityEngine.Random.Range(20, 31);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Milk");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Milk"), CustomColor.abilityName())));


        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.25f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);
        var endPoint = target.GetMiddleGlobalPosition();

        self.PlayAnimation(BattlerAnimationType.magic);
        self.ColorTint(Color.grey, 0.15f);

        // SE
        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        // calculate projectile time base on range
        float projectileTime = 1.0f;
        CreateProjectile("Milk Projectile", startPoint, endPoint, projectileTime, true);

        DOTween.Sequence().AppendInterval(projectileTime)
           .AppendCallback(() =>
           {
               self.PlayAnimation(BattlerAnimationType.idle);
               target.PlayAnimation(BattlerAnimationType.idle);
               target.ColorTint(Color.grey, 0.15f);

               // vfx
               VFXSpawner.SpawnVFX("Recovery", target.transform, target.GetMiddleGlobalPosition());

               // heal target
               target.Heal(hpHealAmount);

               // text
               floatingText = CreateFloatingText(target.transform);
               floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, 50.0f), new Vector2(0.0f, 100.0f), "+" + hpHealAmount.ToString(), 64, CustomColor.heal());
               
               // SE
               AudioManager.Instance.PlaySFX("Heal", 0.75f);

               // 戦闘ログ
               battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(hpHealAmount, CustomColor.heal())));
           })
           .AppendInterval(0.5f)
           .AppendCallback(() =>
           {
               battleManager.NextTurn(false);
           });
    }

    /// <summary>
    /// 特濃ミルク
    /// </summary>
    public void MilkCookie()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.MilkCookie");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.MilkCookie"), CustomColor.abilityName())));


        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.25f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);
        var endPoint = target.GetMiddleGlobalPosition();

        self.PlayAnimation(BattlerAnimationType.magic);
        self.ColorTint(Color.grey, 0.15f);

        // SE
        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        // calculate projectile time base on range
        float projectileTime = 1.0f;
        CreateProjectile("Milk Projectile", startPoint, endPoint, projectileTime, true);

        DOTween.Sequence().AppendInterval(projectileTime)
           .AppendCallback(() =>
           {
               self.PlayAnimation(BattlerAnimationType.idle);
               target.PlayAnimation(BattlerAnimationType.idle);
               target.ColorTint(Color.grey, 0.15f);

               // vfx
               VFXSpawner.SpawnVFX("Recovery", target.transform, target.GetMiddleGlobalPosition());

               // SE
               AudioManager.Instance.PlaySFX("Heal", 0.75f);

               // remove
               bool isSuccess = 
               battleManager.RemoveBuffForCharacter(target, BuffType.attack_down) || 
               battleManager.RemoveBuffForCharacter(target, BuffType.hurt) ||
               battleManager.RemoveBuffForCharacter(target, BuffType.shield_down) ||
               battleManager.RemoveBuffForCharacter(target, BuffType.speed_down);

               // 戦闘ログ
               if (isSuccess)
               {

               }
               else
               {
                   battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.NoEffect"));
               }
           })
           .AppendInterval(0.5f)
           .AppendCallback(() =>
           {
               battleManager.NextTurn(false);
           });
    }

    /// <summary>
    /// ミルク補給オーバードライブ
    /// </summary>
    public void MilkOverdrive()
    {
        var self = battleManager.GetCurrentBattler();
        targetBattlers = battleManager.GetAllTeammate();
        targetBattlers.Remove(self);
        targetBattlers.RemoveAll(x => !x.isAlive);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.MilkOverdrive");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.MilkOverdrive"), CustomColor.abilityName())));


        float projectileTime = 1.0f;
        var rectTransform = self.GetComponent<RectTransform>();
        var originalPos = rectTransform.position;
        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2((selfPos.x - self.GetCharacterSize().x * 0.25f), selfPos.y) : new Vector2((selfPos.x + self.GetCharacterSize().x * 0.5f), selfPos.y);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        // 使用不能となる
        self.SetAbilityActive("Milk", false);
        self.SetAbilityActive("MilkOverdrive", false);

        rectTransform.DOMoveX(originalPos.x - 150.0f, 0.75f);
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.75f)
                .AppendCallback(() =>
                {
                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        var endPoint = targetBattlers[i].GetMiddleGlobalPosition();

                        // SE
                        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);
                        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint + new Vector2(-275.0f, -25.0f), endPoint, projectileTime, true);

                        // shake akiho
                        self.Shake(0.4f);
                    }
                })
                .AppendInterval(projectileTime)
                .AppendCallback(() =>
                {
                    // SE
                    AudioManager.Instance.PlaySFX("Heal", 0.75f);
                    self.PlayAnimation(BattlerAnimationType.idle);

                    self.Graphic.rectTransform.localPosition = new Vector3(0.0f, self.Graphic.rectTransform.localPosition.y, 0.0f);

                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        targetBattlers[i].PlayAnimation(BattlerAnimationType.idle);

                        // vfx
                        VFXSpawner.SpawnVFX("Recovery", targetBattlers[i].transform, targetBattlers[i].GetMiddleGlobalPosition());

                        if (targetBattlers[i].current_hp < targetBattlers[i].max_hp)
                        {
                            // heal target
                            int healAmount = Mathf.Min((int)((float)targetBattlers[i].max_hp * 0.25f), targetBattlers[i].max_hp - targetBattlers[i].current_hp);
                            targetBattlers[i].Heal(healAmount);

                            // text
                            floatingText = CreateFloatingText(targetBattlers[i].transform);
                            floatingText.Init(2.0f, targetBattlers[i].GetMiddleGlobalPosition() + new Vector2(0.0f, 50.0f), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());
                        }
                    }
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        var endPoint = targetBattlers[i].GetMiddleGlobalPosition();

                        // SE
                        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);
                        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint + new Vector2(-275.0f, -25.0f), endPoint, projectileTime, true);

                        // shake akiho
                        self.Shake(0.4f);
                    }
                })
                .AppendInterval(projectileTime)
                .AppendCallback(() =>
                {
                    // SE
                    AudioManager.Instance.PlaySFX("Heal", 0.75f);
                    self.PlayAnimation(BattlerAnimationType.idle);

                    self.Graphic.rectTransform.localPosition = new Vector3(0.0f, self.Graphic.rectTransform.localPosition.y, 0.0f);

                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        targetBattlers[i].PlayAnimation(BattlerAnimationType.idle);

                        // vfx
                        VFXSpawner.SpawnVFX("Recovery", targetBattlers[i].transform, targetBattlers[i].GetMiddleGlobalPosition());

                        if (targetBattlers[i].current_hp < targetBattlers[i].max_hp)
                        {
                            // heal target
                            int healAmount = Mathf.Min((int)((float)targetBattlers[i].max_hp * 0.25f), targetBattlers[i].max_hp - targetBattlers[i].current_hp);
                            targetBattlers[i].Heal(healAmount);

                            // text
                            floatingText = CreateFloatingText(targetBattlers[i].transform);
                            floatingText.Init(2.0f, targetBattlers[i].GetMiddleGlobalPosition() + new Vector2(0.0f, 50.0f), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());
                        }
                    }
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        var endPoint = targetBattlers[i].GetMiddleGlobalPosition();

                        // SE
                        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);
                        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint + new Vector2(-275.0f, -25.0f), endPoint, projectileTime, true);

                        // shake akiho
                        self.Shake(0.4f);
                    }
                })
                .AppendInterval(projectileTime)
                .AppendCallback(() =>
                {
                    // SE
                    AudioManager.Instance.PlaySFX("Heal", 0.75f);
                    self.PlayAnimation(BattlerAnimationType.idle);
                    self.Graphic.rectTransform.localPosition = new Vector3(0.0f, self.Graphic.rectTransform.localPosition.y, 0.0f);

                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        targetBattlers[i].PlayAnimation(BattlerAnimationType.idle);

                        // vfx
                        VFXSpawner.SpawnVFX("Recovery", targetBattlers[i].transform, targetBattlers[i].GetMiddleGlobalPosition());

                        if (targetBattlers[i].current_hp < targetBattlers[i].max_hp)
                        {
                            // heal target
                            int healAmount = Mathf.Min((int)((float)targetBattlers[i].max_hp * 0.25f), targetBattlers[i].max_hp - targetBattlers[i].current_hp);
                            targetBattlers[i].Heal(healAmount);

                            // text
                            floatingText = CreateFloatingText(targetBattlers[i].transform);
                            floatingText.Init(2.0f, targetBattlers[i].GetMiddleGlobalPosition() + new Vector2(0.0f, 50.0f), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());
                        }
                    }
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        var endPoint = targetBattlers[i].GetMiddleGlobalPosition();

                        // SE
                        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);
                        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint + new Vector2(-275.0f, -25.0f), endPoint, projectileTime, true);

                        // shake akiho
                        self.Shake(0.4f);
                    }
                })
                .AppendInterval(projectileTime)
                .AppendCallback(() =>
                {
                    // SE
                    AudioManager.Instance.PlaySFX("Heal", 0.75f);
                    self.PlayAnimation(BattlerAnimationType.idle);

                    for (int i = 0; i < targetBattlers.Count; i++)
                    {
                        targetBattlers[i].PlayAnimation(BattlerAnimationType.idle);

                        // vfx
                        VFXSpawner.SpawnVFX("Recovery", targetBattlers[i].transform, targetBattlers[i].GetMiddleGlobalPosition());

                        if (targetBattlers[i].current_hp < targetBattlers[i].max_hp)
                        {
                            // heal target
                            int healAmount = targetBattlers[i].max_hp - targetBattlers[i].current_hp;
                            targetBattlers[i].Heal(healAmount);

                            // text
                            floatingText = CreateFloatingText(targetBattlers[i].transform);
                            floatingText.Init(2.0f, targetBattlers[i].GetMiddleGlobalPosition() + new Vector2(0.0f, 50.0f), new Vector2(0.0f, 100.0f), "+" + healAmount.ToString(), 64, CustomColor.heal());
                        }
                    }

                    // ログ ({0}　からの {1} ！)
                    battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.TeamHeal"));

                    self.Graphic.rectTransform.localPosition = new Vector3(0.0f, self.Graphic.rectTransform.localPosition.y, 0.0f);
                    rectTransform.DOMove(originalPos, 0.5f);

                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    rectTransform.position = originalPos;
                    
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 立花戦特殊技  / 005号 / 072号
    /// </summary>
    public void QuickAttack()
    {
        const float StrikeTime = 0.2f; // 真ん中から敵を攻撃する移動時間
        const float AttackStopTime = 0.25f; // 攻撃する時の停止時間
        const float ReturnTime = 0.2f; // 攻撃から戻る時間

        var self = battleManager.GetCurrentBattler();

        const int numOfTarget = 3;
        var targets = new Battler[numOfTarget];
        targets[0] = GetRandomAliveTarget();

        Battler GetRandomAliveTarget()
        {
            Battler battler = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];

            if (!battler.isAlive)
            {
                // 全員リタイアか
                bool Continue = false;
                for (int i = 0; i < targetBattlers.Count; i ++)
                {
                    if (targetBattlers[i].isAlive)
                    {
                        Continue = true;
                    }
                }

                if (Continue)
                {
                    return GetRandomAliveTarget();
                }
            }

            return battler;
        }

        // 元データを記録
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();
        var originalPos = self.GetComponent<RectTransform>().position;

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.QuickAttack");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

        // キャラを真ん中に移動する
        Vector3 middle = new Vector3(CanvasReferencer.Instance.GetResolution().x * 0.5f, CanvasReferencer.Instance.GetResolution().y * 0.5f, 0.0f);
        self.GetComponent<RectTransform>().DOMove(middle + new Vector3(0.0f, -140.0f, 0.0f), 0.25f);

        // 残像生成コンポネント
        FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(4.0f, 0.05f, self.GetGraphicRectTransform().GetComponent<Image>());

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.QuickAttack"), CustomColor.abilityName())));

        // play SE
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);

                    // move to first target
                    self.transform.SetParent(targets[0].transform);
                    var targetPos = targets[0].GetComponent<RectTransform>().position;
                    targetPos = targets[0].isEnemy ? new Vector2(targetPos.x - targets[0].GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + targets[0].GetCharacterSize().x * 0.5f, targetPos.y);
                    self.GetComponent<RectTransform>().DOMove(targetPos, StrikeTime);

                    self.PlayAnimation(BattlerAnimationType.attack);
                })
                .AppendInterval(StrikeTime)
                .AppendCallback(() =>
                {
                    // deal damage
                    int realDamage = targets[0].DeductHP(self, Battle.CalculateDamage(self, targets[0]));

                    // text
                    floatingText = CreateFloatingText(targets[0].transform);
                    floatingText.Init(2.0f, targets[0].GetMiddleGlobalPosition(), (targets[0].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), targets[0].CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));
                    
                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.8f);

                    // VFX
                    self.SpawnAttackVFX(targets[0]);

                    // animation
                    targets[0].Shake(0.75f);
                    targets[0].PlayAnimation(BattlerAnimationType.attacked);
                })
                .AppendInterval(AttackStopTime)
                .AppendCallback(() =>
                {
                    // return to middle
                    self.GetComponent<RectTransform>().DOMove(middle + new Vector3(0.0f, -140.0f, 0.0f), ReturnTime);

                    // cancel animation
                    self.PlayAnimation(BattlerAnimationType.idle);
                    targets[0].PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(ReturnTime)
                .AppendCallback(() =>
                {
                    // 次のターゲットを決める
                    targets[1] = GetRandomAliveTarget();

                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);

                    // move to second target
                    self.transform.SetParent(targets[1].transform);
                    var targetPos = targets[1].GetComponent<RectTransform>().position;
                    targetPos = targets[1].isEnemy ? new Vector2(targetPos.x - targets[1].GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + targets[1].GetCharacterSize().x * 0.5f, targetPos.y);
                    self.GetComponent<RectTransform>().DOMove(targetPos, StrikeTime);

                    self.PlayAnimation(BattlerAnimationType.attack);
                })
                .AppendInterval(StrikeTime)
                .AppendCallback(() =>
                {
                    // deal damage
                    int realDamage = targets[1].DeductHP(self, Battle.CalculateDamage(self, targets[1]));

                    // text
                    floatingText = CreateFloatingText(targets[1].transform);
                    floatingText.Init(2.0f, targets[1].GetMiddleGlobalPosition(), (targets[1].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), targets[1].CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.8f);

                    // VFX
                    self.SpawnAttackVFX(targets[1]);

                    // animation
                    targets[1].Shake(0.75f);
                    targets[1].PlayAnimation(BattlerAnimationType.attacked);
                })
                .AppendInterval(AttackStopTime)
                .AppendCallback(() =>
                {
                    // return to middle
                    self.GetComponent<RectTransform>().DOMove(middle + new Vector3(0.0f, -140.0f, 0.0f), ReturnTime);

                    // cancel animation
                    self.PlayAnimation(BattlerAnimationType.idle);
                    targets[1].PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(ReturnTime)
                .AppendCallback(() =>
                {
                    // 次のターゲットを決める
                    targets[2] = GetRandomAliveTarget();

                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);

                    // move to third target
                    self.transform.SetParent(targets[2].transform);
                    var targetPos = targets[2].GetComponent<RectTransform>().position;
                    targetPos = targets[2].isEnemy ? new Vector2(targetPos.x - targets[2].GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + targets[2].GetCharacterSize().x * 0.5f, targetPos.y);
                    self.GetComponent<RectTransform>().DOMove(targetPos, StrikeTime);

                    self.PlayAnimation(BattlerAnimationType.attack);
                })
                .AppendInterval(StrikeTime)
                .AppendCallback(() =>
                {
                    // deal damage
                    int realDamage = targets[2].DeductHP(self, Battle.CalculateDamage(self, targets[2]));

                    // text
                    floatingText = CreateFloatingText(targets[2].transform);
                    floatingText.Init(2.0f, targets[2].GetMiddleGlobalPosition(), (targets[2].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), targets[2].CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.8f);

                    // VFX
                    self.SpawnAttackVFX(targets[2]);

                    // animation
                    targets[2].Shake(0.75f);
                    targets[2].PlayAnimation(BattlerAnimationType.attacked);
                })
                .AppendInterval(AttackStopTime)
                .AppendCallback(() =>
                {
                    // return to middle
                    self.GetComponent<RectTransform>().DOMove(middle + new Vector3(0.0f, -140.0f, 0.0f), ReturnTime);

                    // cancel animation
                    self.PlayAnimation(BattlerAnimationType.idle);
                    targets[2].PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(ReturnTime)
                .AppendCallback(() =>
                {
                    // move to original position
                    self.GetComponent<RectTransform>().DOMove(originalPos, 0.5f);
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);
                })
                .AppendInterval(1.0f)
                .AppendCallback(() => 
                {
                    // next turn
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// オーバードライブ   / 005号 / 072号
    /// </summary>
    public void Overdrive()
    {
        var self = battleManager.GetCurrentBattler();

        const int ComboTurn = 3;
        const int StunTurn = 3;

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Overdrive");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}！ {1} がこれから{2}回連続行動する。)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Overdrive"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Overdrive"), CustomColor.abilityName()), self.CharacterNameColored, ComboTurn));

        battleManager.AddBuffToBattler(self, BuffType.continuous_action, ComboTurn, 0);
        var overdrive = self.gameObject.AddComponent<AbilityOverdrive>();
        overdrive.Init(battleManager, self, StunTurn);

        // play SE
        AudioManager.Instance.PlaySFX("Heartbeat", 1.5f);

        // 残像を作成
        Image img = new GameObject("FadingImage[" + gameObject.name + "]").AddComponent<Image>();
        img.transform.SetParent(self.transform);
        img.transform.SetSiblingIndex(self.Graphic.transform.GetSiblingIndex()+1);
        img.sprite = self.Graphic.sprite;
        img.raycastTarget = false;
        img.rectTransform.pivot = new Vector2(0.5f, 0.5f); ;
        img.rectTransform.position = self.Graphic.rectTransform.position + new Vector3(0.0f, self.GetCharacterSize().y * 0.5f, 0.0f);
        img.rectTransform.localScale = self.Graphic.rectTransform.localScale;
        img.rectTransform.sizeDelta = self.Graphic.rectTransform.sizeDelta;

        const float fadeTime = 1.0f;
        img.color = new Color(0.5f, 0.5f, 0.5f, fadeTime);
        img.DOFade(0.0f, fadeTime);
        img.rectTransform.DOScale(0.3f, fadeTime);
        Destroy(img.gameObject, fadeTime + Time.deltaTime);


        var sequence = DOTween.Sequence();
        sequence.AppendInterval(fadeTime)
                .AppendCallback(() =>
                {
                    battleManager.SetDisplayActionPanel(true);

                    // ログ (また{0}のターン！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.ContinueTurn"), self.CharacterNameColored));
                });
    }

    /// <summary>
    /// 正拳突き   / 005号 / 072号
    /// </summary>
    public void Fist()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Fist");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Fist"), CustomColor.abilityName())));

        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.5f, targetPos.y);
        var originalPos = self.GetComponent<RectTransform>().position;
        self.GetComponent<RectTransform>().DOMove(targetPos, battleManager.CharacterMoveTime);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(battleManager.CharacterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(battleManager.CharacterMoveTime * 0.5f)
                .AppendCallback(() =>
                {

                    self.SpawnAttackVFX(target);

                    // 攻撃計算
                    int levelAdjustedDamage = Battle.CalculateDamage(self, target);
                    int realDamage = target.DeductHP(self, levelAdjustedDamage);

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.8f);

                    // animation
                    target.Shake(battleManager.AttackAnimPlayTime + battleManager.CharacterMoveTime);
                    self.PlayAnimation(BattlerAnimationType.attack);
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    // create floating text
                    floatingText = CreateFloatingText(self.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // ログ ({0}　に　{1}　のダメージを与えた！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));
                })
                .AppendInterval(battleManager.AttackAnimPlayTime)
                .AppendCallback(() =>
                {
                    self.PlayAnimation(BattlerAnimationType.idle);
                    target.PlayAnimation(BattlerAnimationType.idle);

                    self.GetComponent<RectTransform>().DOMove(originalPos, battleManager.CharacterMoveTime);
                })
                .AppendInterval(battleManager.CharacterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);
                })
                .AppendInterval(battleManager.CharacterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.SetDisplayActionPanel(true);

                    // ログ (また{0}のターン！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.ContinueTurn"), self.CharacterNameColored));
                });
    }

    /// <summary>
    /// ショックウェーブ・パルサー 装備することで獲得する特殊技
    /// </summary>
    public void Stungun()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.5f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);
        var endPoint = target.GetMiddleGlobalPosition();

        // calculate projectile time base on range
        float projectileTime = Vector3.Distance(startPoint, endPoint) / 850.0f;

        // SE
        AudioManager.Instance.PlaySFX("GunShoot");

        // ログ ({0}　から {1} を放つ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Stungun"), self.CharacterNameColored, 
                                                 CustomColor.AddColor(LocalizationManager.Localize("Equipment.Stungun"), CustomColor.itemName())));
        
        CreateProjectile("Stungun Projectile", startPoint, endPoint, projectileTime, true);
        DOTween.Sequence().AppendInterval(projectileTime)
            .AppendCallback(() => 
            {
                // VFX
                VFXSpawner.SpawnVFX("Stungun Hit", target.transform, endPoint);

                // stun target
                battleManager.AddBuffToBattler(target, BuffType.stun, 1, 0);

                // SE
                AudioManager.Instance.PlaySFX("ElectricShock", 0.75f);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// タンクのデフォルト攻撃
    public void TankAttack()
    {
        var self = battleManager.GetCurrentBattler();
        var selfRect = self.GetComponent<RectTransform>();
        var target = targetBattlers[0];

        const float ChargeTime = 0.75f;
        self.Shake(ChargeTime);
        AudioManager.Instance.PlaySFX("TankStandby");

        // ログ (xxx　のビーム攻撃！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.TankAttack"), self.CharacterNameColored));

        var originalPosition = selfRect.localPosition.x;
        DOTween.Sequence().AppendInterval(ChargeTime)
            .AppendCallback(() =>
            {
                // shoot
                selfRect.DOLocalMoveX(originalPosition + 50.0f, 0.05f).SetEase(Ease.Linear);
                AudioManager.Instance.PlaySFX("TankBeam");
                self.ColorTint(Color.red, 0.5f);

                // VFX
                VFXSpawner.SpawnVFX("SparkExplode", self.transform.parent, self.GetMiddleGlobalPosition() + new Vector2(-105f, 150.0f));
            })
            .AppendInterval(0.075f)
            .AppendCallback(() =>
            {
                // deal damage
                int realDamage = target.DeductHP(self, Battle.CalculateDamage(self, target));

                // text
                var floatingText = CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // play SE
                AudioManager.Instance.PlaySFX("Attacked", 0.8f);
                AudioManager.Instance.PlaySFX("Explode", 1f);

                // animation
                target.Shake(0.75f);
                target.PlayAnimation(BattlerAnimationType.attacked);

                // ログ ({0}　に　{1}　のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // VFX
                self.SpawnAttackVFX(target);
            })
            .AppendInterval(0.075f)
            .AppendCallback(() =>
            {
                selfRect.DOLocalMoveX(originalPosition, 0.5f);
                target.PlayAnimation(BattlerAnimationType.idle);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// 軍用歩行型タンクのデフォルト攻撃
    public void DarkTankAttack()
    {
        var self = battleManager.GetCurrentBattler();
        var selfRect = self.GetComponent<RectTransform>();
        var targets = battleManager.GetAllTeammate();

        const float ChargeTime = 0.75f;
        self.Shake(ChargeTime);
        AudioManager.Instance.PlaySFX("TankStandby");

        // ログ (xxx　のビーム攻撃！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.TankAttack"), self.CharacterNameColored));

        var originalPosition = selfRect.localPosition.x;
        DOTween.Sequence().AppendInterval(ChargeTime)
            .AppendCallback(() =>
            {
                // shoot
                selfRect.DOLocalMoveX(originalPosition + 50.0f, 0.05f).SetEase(Ease.Linear);
                AudioManager.Instance.PlaySFX("TankBeam");
                self.ColorTint(Color.red, 0.5f);

                // VFX
                VFXSpawner.SpawnVFX("SparkExplode", self.transform.parent, self.GetMiddleGlobalPosition() + new Vector2(-105f, 150.0f));
            })
            .AppendInterval(0.075f)
            .AppendCallback(() =>
            {
                // 全体攻撃
                for (int i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];

                    // deal damage
                    int realDamage = target.DeductHP(self, Battle.CalculateDamage(self, target));

                    // text
                    var floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.8f);
                    AudioManager.Instance.PlaySFX("Explode", 1f);

                    // animation
                    target.Shake(0.75f);
                    target.PlayAnimation(BattlerAnimationType.attacked);

                    // ログ ({0}　に　{1}　のダメージを与えた！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                    // VFX
                    self.SpawnAttackVFX(target);
                }
            })
            .AppendInterval(0.075f)
            .AppendCallback(() =>
            {
                selfRect.DOLocalMoveX(originalPosition, 0.5f);

                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].PlayAnimation(BattlerAnimationType.idle);
                }
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 聖なる盾　エレナボス戦
    /// </summary>
    public void DivineShield()
    {
        var self = battleManager.GetCurrentBattler();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.DivineShield");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.DivineShield"), CustomColor.abilityName())));

        // アニメション
        self.PlayAnimation(BattlerAnimationType.magic);

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        const string ObjName = "ErenaShield";


        try
        {
            // 前のシールドが存在していたら置き換え
            var obj = self.transform.Find(ObjName);
            if (obj != null)
            {
                obj.GetComponent<ErenaShield>().Destroy();
            }

            // ともにチャージさせる
            Ability stunshield = self.GetAbility("StunShield");
            if (stunshield != null)
            {
                self.SetAbilityOnCooldown(stunshield, stunshield.cooldown);
            }
        }
        catch
        {

        }

        // VFX
        var shield = VFXSpawner.SpawnVFX("Erena Shield", self.transform, self.GetGraphicRectTransform().position + new Vector3(0.0f, 150.0f, 0.0f));
        shield.name = ObjName;
        shield.transform.localScale = new Vector3(3f, 3f, 3f);
        shield.transform.DOScale(1.0f, 1.5f);
        var shieldScript = shield.GetComponent<ErenaShield>();
        shieldScript.Init(battleManager, self, shield.GetComponent<RectTransform>(), ErenaShield.EventType.DivineShield);

        float delayTime = 0.75f;
        DOTween.Sequence().AppendInterval(delayTime)
           .AppendCallback(() =>
           {
               // SE
               AudioManager.Instance.PlaySFX("NewAbility", 0.5f); // shield

               // ログ (次の攻撃を完全防御！)
               battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.DivineShield"));

               // アニメション
               self.PlayAnimation(BattlerAnimationType.idle);

               battleManager.NextTurn(false);
           }
        );
    }

    /// <summary>
    /// 聖なる剣　エレナボス戦
    /// </summary>
    public void DivineRapier()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.DivineRapier");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.DivineRapier"), CustomColor.abilityName())));

        // アニメション
        self.PlayAnimation(BattlerAnimationType.magic);

        // 残像を作成
        Image img = new GameObject("FadingImage[" + gameObject.name + "]").AddComponent<Image>();
        img.transform.SetParent(self.transform);
        img.transform.SetSiblingIndex(self.Graphic.transform.GetSiblingIndex() + 1);
        img.sprite = self.Graphic.sprite;
        img.raycastTarget = false;
        img.rectTransform.pivot = new Vector2(0.5f, 0.5f); ;
        img.rectTransform.position = self.Graphic.rectTransform.position + new Vector3(0.0f, self.GetCharacterSize().y * 0.5f, 0.0f);
        img.rectTransform.localScale = self.Graphic.rectTransform.localScale;
        img.rectTransform.sizeDelta = self.Graphic.rectTransform.sizeDelta;

        const float fadeTime = 1.0f;
        img.color = new Color(0.5f, 0.5f, 0.5f, fadeTime);
        img.DOFade(0.0f, fadeTime);
        img.rectTransform.DOScale(new Vector3(-0.3f, 0.3f, 1.0f), fadeTime);
        Destroy(img.gameObject, fadeTime + Time.deltaTime);

        // VFX
        var obj = VFXSpawner.SpawnVFX("Erena_HolySword", self.transform, self.GetGraphicRectTransform().position + new Vector3(0.0f, 100.0f, 0.0f));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);

        var selfPos = self.GetMiddleGlobalPosition() + new Vector2(0.0f, 100.0f);
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.25f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);
        var endPoint = target.GetMiddleGlobalPosition();

        // calculate projectile time base on range
        float delayTime = 0.75f;

        float projectileTime = 0.4f;
        DOTween.Sequence().AppendInterval(delayTime)
           .AppendCallback(() =>
           {
               var projectile = CreateProjectile("Erena Projectile", startPoint, endPoint, projectileTime, true);

               // 残像生成コンポネント
               FadeEffect fadeEffect = projectile.gameObject.AddComponent<FadeEffect>();
               fadeEffect.Initialize(projectileTime, 0.05f, projectile.GetComponent<Image>());

               // SE
               AudioManager.Instance.PlaySFX("Throw", 0.75f);
               AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

               // 回転
               self.GetGraphicRectTransform().DORotate(new Vector3(0, 0, 15.0f), 0.25f, RotateMode.Fast);
           })
           .AppendInterval(projectileTime - 0.1f)
           .AppendCallback(() =>
           {
               self.PlayAnimation(BattlerAnimationType.idle);
               target.PlayAnimation(BattlerAnimationType.attacked);

               target.ColorTint(Color.yellow, 0.5f);

               // vfx
               VFXSpawner.SpawnVFX("Recovery", target.transform, target.GetMiddleGlobalPosition());

               // SE
               AudioManager.Instance.PlaySFX("Damage4", 0.75f);

               int defAmt = target.defense / 2;
               battleManager.AddBuffToBattler(target, BuffType.shield_down, 4, defAmt);
               battleManager.AddBuffToBattler(target, BuffType.disarm, 2, 0);

               // ログ （{0}　の防御力が {1} 点下げた。）
               battleManager.AddBattleLog(string.Format(LocalizationManager.Localize("BattleLog.ShieldDownValue"), target.CharacterNameColored, CustomColor.AddColor(defAmt, CustomColor.SP())));

               // deal damage
               int realDamage = target.DeductHP(self, Battle.CalculateDamage(self, target) * 2);

               // 戦闘ログ
               battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

               // text
               floatingText = CreateFloatingText(target.transform);
               floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

               // play SE
               AudioManager.Instance.PlaySFX("Holy", 0.7f);

               // 回転
               self.GetGraphicRectTransform().DORotate(Vector3.zero, 0.25f, RotateMode.Fast);
           })
           .AppendInterval(0.5f)
           .AppendCallback(() =>
           {
               target.PlayAnimation(BattlerAnimationType.idle);
               battleManager.NextTurn(false);
           });
    }

    /// <summary>
    /// スタンシールド　エレナボス戦
    /// </summary>
    public void StunShield()
    {
        var self = battleManager.GetCurrentBattler();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.StunShield");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.StunShield"), CustomColor.abilityName())));

        // アニメション
        self.PlayAnimation(BattlerAnimationType.magic);

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        const string ObjName = "ErenaShield";


        try
        {
            // 前のシールドが存在していたら置き換え
            var obj = self.transform.Find(ObjName);
            if (obj != null)
            {
                obj.GetComponent<ErenaShield>().Destroy();
            }

            // ともにチャージさせる
            Ability divineshield = self.GetAbility("DivineShield");
            if (divineshield != null)
            {
                self.SetAbilityOnCooldown(divineshield, divineshield.cooldown);
            }
        }
        catch
        {

        }

        // VFX
        var shield = VFXSpawner.SpawnVFX("Erena Shield", self.transform, self.GetGraphicRectTransform().position + new Vector3(0.0f, 150.0f, 0.0f));
        shield.name = ObjName;
        shield.transform.localScale = new Vector3(3f, 3f, 3f);
        shield.transform.DOScale(1.0f, 1.5f);
        var shieldScript = shield.GetComponent<ErenaShield>();
        shieldScript.Init(battleManager, self, shield.GetComponent<RectTransform>(), ErenaShield.EventType.StunShield);

        float delayTime = 0.75f;
        DOTween.Sequence().AppendInterval(delayTime)
           .AppendCallback(() =>
           {
               // SE
               AudioManager.Instance.PlaySFX("NewAbility", 0.5f); // shield

               // ログ (次の攻撃を完全防御！)
               battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.StunShield"), self.CharacterNameColored));

               // アニメション
               self.PlayAnimation(BattlerAnimationType.idle);

               battleManager.NextTurn(false);
           }
        );
    }

    /// <summary>
    /// 反撃の意思　エレナ技
    /// </summary>
    public void Repel()
    {
        var self = battleManager.GetCurrentBattler();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Repel");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Repel"), CustomColor.abilityName())));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        self.PlayAnimation(BattlerAnimationType.magic);

        battleManager.AddBuffToBattler(self, BuffType.repel, 3, 0);

        // vfx
        VFXSpawner.SpawnVFX("Recovery", self.transform, self.GetMiddleGlobalPosition());

        // Next turn
        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                self.PlayAnimation(BattlerAnimationType.idle);
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 不動のオーラ　エレナ技
    /// </summary>
    public void GuardUp()
    {
        var self = battleManager.GetCurrentBattler();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GuardUp");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.GuardUp"), CustomColor.abilityName())));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        self.PlayAnimation(BattlerAnimationType.magic);

        battleManager.AddBuffToBattler(self, BuffType.shield_up, 2, 99);

        // vfx
        VFXSpawner.SpawnVFX("Recovery", self.transform, self.GetMiddleGlobalPosition());

        // Next turn
        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                self.PlayAnimation(BattlerAnimationType.idle);
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 不滅の加護　エレナ技
    /// </summary>
    public void GuardianAngel()
    {
        var self = battleManager.GetCurrentBattler();
        var targets = battleManager.GetAllTeammate();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GuardianAngel");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.GuardianAngel"), CustomColor.abilityName())));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        self.PlayAnimation(BattlerAnimationType.magic);

        const string ObjName = "ErenaShield";
        for (int i = 0; i < targets.Count; i++)
        {
            try
            {
                // 前のシールドが存在していたら置き換え
                var obj = targets[i].transform.Find(ObjName);
                if (obj != null)
                {
                    obj.GetComponent<ErenaShield>().Destroy();
                }

                // VFX
                var shield = VFXSpawner.SpawnVFX("Erena Shield", targets[i].transform, targets[i].GetGraphicRectTransform().position + new Vector3(0.0f, 150.0f, 0.0f));
                shield.name = ObjName;
                shield.transform.localScale = new Vector3(3f, 3f, 3f);
                shield.transform.DOScale(1.0f, 1.5f);
                var shieldScript = shield.GetComponent<ErenaShield>();
                shieldScript.Init(battleManager, targets[i], shield.GetComponent<RectTransform>(), ErenaShield.EventType.DivineShield);

                // SE
                AudioManager.Instance.PlaySFX("NewAbility", 0.5f); // shield
            }
            catch
            {

            }
        };

        // vfx
        VFXSpawner.SpawnVFX("Recovery", self.transform, self.GetMiddleGlobalPosition());

        // Next turn
        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                self.PlayAnimation(BattlerAnimationType.idle);
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 闇落ち京のハッキング特殊技
    /// </summary>
    public void Hacking()
    {
        // 京の武器を取得
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();

        // 相手の資料を取得
        var target = targetBattlers[0];

        // 成功率を計算
        float successRate = target.currentLevel >= kei.currentLevel ? 0.0f : 0.40f + (0.064f * (kei.currentLevel - target.currentLevel));

        if (!target.IsMachine) successRate = 0.0f; // 機械類以外は成功率　0%　
        if (target.character_name == LocalizationManager.Localize("Name.Tank")) successRate = 0.01f; // 特定の敵は成功しない

        successRate = UnityEngine.Mathf.Clamp(successRate, 0.0f, 1.0f);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.Hacking");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ログ ({0}　がハッキングを挑む！成功確率は {1})
        var successRatePercentage = String.Format("{0:0.##\\%}", (successRate * 100.0f)); 
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking"), kei.CharacterNameColored, CustomColor.AddColor(successRatePercentage, Color.cyan)));

        // 成功か
        bool isSuccess = UnityEngine.Random.Range(0.0f, 1.0f) < successRate;

        // 武器の動きを一旦止める
        weapons.leftWeapon.SetEnableMovement(false);
        weapons.rightWeapon.SetEnableMovement(false);

        weapons.leftWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
        weapons.rightWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);

        // アニメション
        const float animtionTime = 1.0f;
        kei.PlayAnimation(BattlerAnimationType.magic);

        // 残像
        FadeEffect fadeEffect = weapons.leftWeapon.Rect.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(animtionTime, 0.05f, weapons.leftWeapon.GetComponent<Image>());
        fadeEffect = weapons.rightWeapon.Rect.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(animtionTime, 0.05f, weapons.rightWeapon.GetComponent<Image>());

        // SE
        AudioManager.Instance.PlaySFX("Machine");

        if (isSuccess)
        {
            // この特殊技を削除
            kei.SetAbilityActive("Hacking", false);
            kei.SetAbilityActive("SuicideAttack", true);
            kei.SetAbilityActive("Reprogram", true);
            kei.SetAbilityActive("EffeciencyBoost", true);

            // 自爆機能追加
            var ability = Resources.Load<Ability>("AbilityList/Suicide");
            target.AddAbilityToCharacter(ability);

            var sequence = DOTween.Sequence();
            sequence.AppendInterval(animtionTime * 0.5f)
                    .AppendCallback(() =>
                    {
                        weapons.leftWeapon.Rect.DOKill(false);
                        weapons.rightWeapon.Rect.DOKill(false);

                        var targetSprite = target.GetGraphicRectTransform();
                        weapons.leftWeapon.transform.SetParent(targetSprite.parent);
                        weapons.leftWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex() + 1);
                        weapons.rightWeapon.transform.SetParent(targetSprite.parent);
                        weapons.rightWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex());

                        weapons.leftWeapon.Rect.DOLocalMove(weapons.LeftWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                        weapons.rightWeapon.Rect.DOLocalMove(weapons.RightWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);

                        // SE
                        AudioManager.Instance.PlaySFX("Machine", 0.5f);

                        // アニメション
                        kei.PlayAnimation(BattlerAnimationType.idle);
                    })
                    .AppendInterval((animtionTime * 0.5f) + 0.25f)
                    .AppendCallback(() =>
                    {
                        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking_Success"), target.CharacterNameColored));

                        // SE
                        AudioManager.Instance.PlaySFX("Notification");
                        var puppet = target.gameObject.AddComponent<KeiControlledUnit>();
                        weapons.SetControlledUnit(puppet);
                        puppet.StartControl(kei, 0);
                    })
                    .AppendInterval(animtionTime * 0.5f) // 敵を移動中
                    .AppendCallback(() =>
                    {
                        // SE
                        AudioManager.Instance.PlaySFX("Machine", 0.5f);

                        // 元の所に戻す
                        weapons.leftWeapon.Rect.DOMove(kei.GetMiddleGlobalPosition(), animtionTime * 0.25f).SetEase(Ease.Linear);
                        weapons.rightWeapon.Rect.DOMove(kei.GetMiddleGlobalPosition(), animtionTime * 0.25f).SetEase(Ease.Linear);
                    })
                    .AppendInterval(animtionTime * 0.25f)
                    .AppendCallback(() =>
                    {
                        weapons.leftWeapon.Rect.DOKill(false);
                        weapons.rightWeapon.Rect.DOKill(false);

                        var targetSprite = kei.GetGraphicRectTransform();
                        weapons.leftWeapon.transform.SetParent(targetSprite.parent);
                        weapons.leftWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex() + 1);
                        weapons.rightWeapon.transform.SetParent(targetSprite.parent);
                        weapons.rightWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex());

                        weapons.leftWeapon.Rect.DOLocalMove(weapons.LeftWeaponLocalPosition, animtionTime * 0.25f).SetEase(Ease.Linear);
                        weapons.rightWeapon.Rect.DOLocalMove(weapons.RightWeaponLocalPosition, animtionTime * 0.25f).SetEase(Ease.Linear);
                    })
                    .AppendInterval(0.25f)
                    .AppendCallback(() =>
                    {
                        weapons.leftWeapon.SetEnableMovement(true);
                        weapons.rightWeapon.SetEnableMovement(true);
                        battleManager.NextTurn(false);
                    });
        }
        else
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(animtionTime * 0.5f)
                    .AppendCallback(() =>
                    {
                        weapons.leftWeapon.Rect.DOKill(false);
                        weapons.rightWeapon.Rect.DOKill(false);

                        var targetSprite = target.GetGraphicRectTransform();
                        weapons.leftWeapon.transform.SetParent(targetSprite.parent);
                        weapons.leftWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex() + 1);
                        weapons.rightWeapon.transform.SetParent(targetSprite.parent);
                        weapons.rightWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex());

                        weapons.leftWeapon.Rect.DOLocalMove(weapons.LeftWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                        weapons.rightWeapon.Rect.DOLocalMove(weapons.RightWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);

                        // SE
                        AudioManager.Instance.PlaySFX("Machine", 0.5f);

                        // アニメション
                        kei.PlayAnimation(BattlerAnimationType.idle);
                    })
                    .AppendInterval((animtionTime * 0.5f) + 0.25f)
                    .AppendCallback(() =>
                    {
                        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking_Fail"), target.CharacterNameColored));

                        // SE
                        AudioManager.Instance.PlaySFX("Machine", 0.5f);
                        AudioManager.Instance.PlaySFX("Miss");

                        // 元の所に戻す
                        weapons.leftWeapon.Rect.DOMove(kei.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
                        weapons.rightWeapon.Rect.DOMove(kei.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
                    })
                    .AppendInterval(animtionTime * 0.5f)
                    .AppendCallback(() =>
                    {
                        weapons.leftWeapon.Rect.DOKill(false);
                        weapons.rightWeapon.Rect.DOKill(false);

                        var targetSprite = kei.GetGraphicRectTransform();
                        weapons.leftWeapon.transform.SetParent(targetSprite.parent);
                        weapons.leftWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex() + 1);
                        weapons.rightWeapon.transform.SetParent(targetSprite.parent);
                        weapons.rightWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex());

                        weapons.leftWeapon.Rect.DOLocalMove(weapons.LeftWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                        weapons.rightWeapon.Rect.DOLocalMove(weapons.RightWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                    })
                    .AppendInterval(0.5f)
                    .AppendCallback(() =>
                    {
                        weapons.leftWeapon.SetEnableMovement(true);
                        weapons.rightWeapon.SetEnableMovement(true);
                        battleManager.NextTurn(false);
                    });
        }
    }

    /// <summary>
    /// 自爆指示 ケイ
    /// </summary>
    public void SuicideAttack()
    {
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();
        var target = targetBattlers[0];
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // 傀儡を取得

        // この特殊技を削除
        kei.SetAbilityActive("SuicideAttack", false);
        kei.SetAbilityActive("Reprogram", false);
        kei.SetAbilityActive("EffeciencyBoost", false);

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.SuicideAttack");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), kei.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.SuicideAttack"), CustomColor.abilityName())));

        const float chargeTime = 0.5f;

        // 残像生成コンポネント
        FadeEffect fadeEffect = puppet.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(chargeTime, 0.05f, puppet.Graphic);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
        AudioManager.Instance.PlaySFX("Machine", 0.5f);
        AudioManager.Instance.PlaySFX("Kei_Attack2");

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.15f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.15f, targetPos.y);

        puppet.RectTransform.DOMove(targetPos, chargeTime).SetEase(Ease.InOutQuint);
        kei.PlayAnimation(BattlerAnimationType.magic);
        puppet.ColorTint(Color.red, chargeTime);

        DOTween.Sequence()
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // change character hirachy temporary
                puppet.transform.SetParent(target.transform);
            })
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                kei.PlayAnimation(BattlerAnimationType.idle);

                // play SE
                AudioManager.Instance.PlaySFX("Explode");

                // VFX
                var VFXPosition = puppet.GetMiddleGlobalPosition() + ((puppet.GetMiddleGlobalPosition() - target.GetMiddleGlobalPosition()) / 2);
                VFXSpawner.SpawnVFX("Suicide", target.transform, VFXPosition);

                var realDamage = Battle.CalculateDamage(puppet.current_hp, target.defense, kei.currentLevel, target.currentLevel, false);
                target.DeductHP(puppet, realDamage);

                // text
                floatingText = CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - puppet.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // ログ ({0}　が自爆する！{1} のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Suicide"), puppet.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // 即死
                puppet.DeductHP(puppet, 99999, true);
            })
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// ハッキングしたキャラが発動できる技
    /// </summary>
    public void Suicide()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Suicide");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Suicide"), CustomColor.abilityName())));

        const float chargeTime = 0.5f;

        // 残像生成コンポネント
        FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(chargeTime, 0.05f, self.Graphic);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
        AudioManager.Instance.PlaySFX("Machine", 0.5f);
        AudioManager.Instance.PlaySFX("Kei_Attack2");

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.15f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.15f, targetPos.y);

        self.RectTransform.DOMove(targetPos, chargeTime).SetEase(Ease.InOutQuint);
        self.ColorTint(Color.red, chargeTime);

        DOTween.Sequence()
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // change character hirachy temporary
                self.transform.SetParent(target.transform);
            })
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // play SE
                AudioManager.Instance.PlaySFX("Explode");

                // VFX
                var VFXPosition = self.GetMiddleGlobalPosition() + ((self.GetMiddleGlobalPosition() - target.GetMiddleGlobalPosition()) / 2);
                VFXSpawner.SpawnVFX("Suicide", target.transform, VFXPosition);

                var realDamage = Battle.CalculateDamage(self.current_hp, target.defense, self.currentLevel, target.currentLevel, false);
                target.DeductHP(self, realDamage);

                // text
                floatingText = CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // ログ ({0}　が自爆する！{1} のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Suicide"), self.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // 即死
                self.DeductHP(self, 99999, true);
            })
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 京 リファクタリング
    /// </summary>
    public void Reprogram()
    {
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // 傀儡を取得

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.Reprogram");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), kei.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Reprogram"), CustomColor.abilityName())));

        // play SE
        AudioManager.Instance.PlaySFX("Machine");

        kei.PlayAnimation(BattlerAnimationType.magic);

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                kei.PlayAnimation(BattlerAnimationType.idle);

                // play SE
                AudioManager.Instance.PlaySFX("SelfRepair");

                // VFX
                VFXSpawner.SpawnVFX("SelfRepair", puppet.transform, puppet.GetGraphicRectTransform().position);

                // effect
                int attackAmt = Mathf.FloorToInt((float)puppet.attack * 0.33f);
                puppet.attack += attackAmt;
                int defenseAmt = Mathf.FloorToInt((float)puppet.defense * 0.33f);
                puppet.defense += defenseAmt;

                // ログ ({0}　の攻撃力が{1}点増加。防御力{2}点増加。)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Reprogram"), puppet.CharacterNameColored, 
                    CustomColor.AddColor(attackAmt, CustomColor.damage()), CustomColor.AddColor(defenseAmt, CustomColor.SP())));

                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 京 システム修復
    /// </summary>
    public void EffeciencyBoost()
    {

        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // 傀儡を取得

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.EffeciencyBoost");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), kei.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.EffeciencyBoost"), CustomColor.abilityName())));

        // play SE
        AudioManager.Instance.PlaySFX("Machine");

        kei.PlayAnimation(BattlerAnimationType.magic);

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                kei.PlayAnimation(BattlerAnimationType.idle);

                // play SE
                AudioManager.Instance.PlaySFX("SelfRepair");

                // VFX
                VFXSpawner.SpawnVFX("SelfRepair", puppet.transform, puppet.GetGraphicRectTransform().position);

                // effect
                int healAmt = Mathf.FloorToInt((float)puppet.max_hp * 0.33f);
                puppet.Heal(healAmt);

                battleManager.ChangeBattlerTurnOrder(puppet);

                // ログ ({0}　の HPが {1} 回復した！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.HealHP"), puppet.CharacterNameColored, CustomColor.AddColor(healAmt, CustomColor.heal())));



                battleManager.NextTurn(false);
            });
    }

    /// 京のデフォルト攻撃
    public void KeiAttack()
    {
        var self = battleManager.GetCurrentBattler();
        var weapons = self.GetComponent<KeiWeaponController>();
        bool left = UnityEngine.Random.Range(0, 10) >= 5;
        var weapon = left ? weapons.leftWeapon : weapons.rightWeapon;
        var originalPosition = weapon.transform.position;
        int originalSiblingIndex = weapon.transform.GetSiblingIndex();

        Battler target = null;
        if (targetBattlers.Count > 1) // is AOE
        {
            target = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];
        }
        else
        {
            target = targetBattlers[0];
        }

        Battler puppet = null; 
        if (weapons.ControlledUnit != null)
        {
            puppet = weapons.ControlledUnit.GetComponent<Battler>(); // 傀儡を取得
        }

        // 武器の動きを一旦止める
        if (left)
        {
            weapons.leftWeapon.SetEnableMovement(false);
            weapons.leftWeapon.SetAnimationAttack();
        }
        else
        {
            weapons.rightWeapon.SetEnableMovement(false);
            weapons.rightWeapon.SetAnimationAttack();
        }
        self.PlayAnimation(BattlerAnimationType.attack);


        const float ChargeTime = 0.3f;
        const float StayTime = 0.25f;
        const float ReturnTime = 0.5f;

        // 残像生成コンポネント
        FadeEffect fadeEffect = weapon.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(ChargeTime, 0.02f, weapon.GetComponent<Image>());

        // 傀儡を非表示
        if (puppet != null)
        {
            puppet.SetTransparent(0.1f, ChargeTime * 0.5f);
        }

        // ログ (xxx　の攻撃！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Attack"), self.CharacterNameColored));
        
        DOTween.Sequence().AppendInterval(ChargeTime)
            .AppendCallback(() =>
            {
                // shoot
                weapon.transform.DOMove(target.GetMiddleGlobalPosition(), ChargeTime).SetEase(Ease.InOutQuint);
                AudioManager.Instance.PlaySFX("CharacterMove");
            })
            .AppendInterval(ChargeTime * 0.5f)
            .AppendCallback(() =>
            {
                weapon.transform.SetParent(target.transform);
            })
            .AppendInterval(ChargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // deal damage
                int realDamage = target.DeductHP(self, Battle.CalculateDamage(self, target));

                // text
                var floatingText = CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // play SE
                AudioManager.Instance.PlaySFX("Attacked", 0.8f);
                AudioManager.Instance.PlaySFX("Damage5", 1f);

                // animation
                target.Shake(0.75f);
                target.PlayAnimation(BattlerAnimationType.attacked);

                // ログ ({0}　に　{1}　のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // VFX
                self.SpawnAttackVFX(target);
            })
            .AppendInterval(StayTime)
            .AppendCallback(() =>
            {
                weapon.transform.DOMove(originalPosition, ReturnTime).SetEase(Ease.Linear);
                target.PlayAnimation(BattlerAnimationType.idle);
            })
            .AppendInterval(ReturnTime * 0.5f)
            .AppendCallback(() =>
            {
                weapon.transform.SetParent(self.transform);
                weapon.transform.SetSiblingIndex(originalSiblingIndex);
                weapon.transform.DOKill(false);
                if (left)
                {
                    weapons.leftWeapon.SetAnimationNormal();
                    weapon.transform.DOLocalMove(weapons.LeftWeaponLocalPosition, ReturnTime * 0.5f).SetEase(Ease.Linear);
                }
                else
                {
                    weapons.rightWeapon.SetAnimationNormal();
                    weapon.transform.DOLocalMove(weapons.RightWeaponLocalPosition, ReturnTime * 0.5f).SetEase(Ease.Linear);
                }
            })
            .AppendInterval(ReturnTime * 0.5f)
            .AppendCallback(() =>
            {
                if (left)
                {
                    weapons.leftWeapon.SetEnableMovement(true);
                }
                else
                {
                    weapons.rightWeapon.SetEnableMovement(true);
                }

                if (puppet != null)
                {
                    puppet.SetTransparent(1.0f, ReturnTime * 0.5f);
                }
                self.PlayAnimation(BattlerAnimationType.idle);

                if (targetBattlers.Count > 1)
                {
                    KeiDoubleAttack(!left, target);
                }
                else
                {
                    battleManager.NextTurn(false);
                }
            });
    }

    /// <summary>
    /// 京　普通攻撃ｘ２
    /// </summary>
    public void KeiDoubleAttack(bool isLeftWeapon, Battler lastTarget)
    {
        var self = battleManager.GetCurrentBattler();

        // 敵の京のみ有効
        if (!self.isEnemy) return;

        var weapons = self.GetComponent<KeiWeaponController>();
        bool left = isLeftWeapon;
        var weapon = left ? weapons.leftWeapon : weapons.rightWeapon;
        var originalPosition = weapon.transform.position;
        int originalSiblingIndex = weapon.transform.GetSiblingIndex();
        targetBattlers.Remove(lastTarget);
        var target = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];
        Battler puppet = null;
        if (weapons.ControlledUnit != null)
        {
            puppet = weapons.ControlledUnit.GetComponent<Battler>(); // 傀儡を取得
        }

        // 武器の動きを一旦止める
        if (left)
        {
            weapons.leftWeapon.SetEnableMovement(false);
            weapons.leftWeapon.SetAnimationAttack();
        }
        else
        {
            weapons.rightWeapon.SetEnableMovement(false);
            weapons.rightWeapon.SetAnimationAttack();
        }
        self.PlayAnimation(BattlerAnimationType.attack);


        const float ChargeTime = 0.3f;
        const float StayTime = 0.25f;
        const float ReturnTime = 0.5f;

        // 残像生成コンポネント
        FadeEffect fadeEffect = weapon.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(ChargeTime, 0.02f, weapon.GetComponent<Image>());

        // 傀儡を非表示
        if (puppet != null)
        {
            puppet.SetTransparent(0.1f, ChargeTime * 0.5f);
        }

        // ログ (xxx　の攻撃！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Attack"), self.CharacterNameColored));

        DOTween.Sequence().AppendInterval(ChargeTime)
            .AppendCallback(() =>
            {
                // shoot
                weapon.transform.DOMove(target.GetMiddleGlobalPosition(), ChargeTime).SetEase(Ease.InOutQuint);
                AudioManager.Instance.PlaySFX("CharacterMove");
            })
            .AppendInterval(ChargeTime * 0.5f)
            .AppendCallback(() =>
            {
                weapon.transform.SetParent(target.transform);
            })
            .AppendInterval(ChargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // deal damage
                int realDamage = target.DeductHP(self, Battle.CalculateDamage(self, target));

                // text
                var floatingText = CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // play SE
                AudioManager.Instance.PlaySFX("Attacked", 0.8f);
                AudioManager.Instance.PlaySFX("Damage5", 1f);

                // animation
                target.Shake(0.75f);
                target.PlayAnimation(BattlerAnimationType.attacked);

                // ログ ({0}　に　{1}　のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // VFX
                self.SpawnAttackVFX(target);
            })
            .AppendInterval(StayTime)
            .AppendCallback(() =>
            {
                weapon.transform.DOMove(originalPosition, ReturnTime).SetEase(Ease.Linear);
                target.PlayAnimation(BattlerAnimationType.idle);
            })
            .AppendInterval(ReturnTime * 0.5f)
            .AppendCallback(() =>
            {
                weapon.transform.SetParent(self.transform);
                weapon.transform.SetSiblingIndex(originalSiblingIndex);
                weapon.transform.DOKill(false);
                if (left)
                {
                    weapons.leftWeapon.SetAnimationNormal();
                    weapon.transform.DOLocalMove(weapons.LeftWeaponLocalPosition, ReturnTime * 0.5f).SetEase(Ease.Linear);
                }
                else
                {
                    weapons.rightWeapon.SetAnimationNormal();
                    weapon.transform.DOLocalMove(weapons.RightWeaponLocalPosition, ReturnTime * 0.5f).SetEase(Ease.Linear);
                }
            })
            .AppendInterval(ReturnTime * 0.5f)
            .AppendCallback(() =>
            {
                if (left)
                {
                    weapons.leftWeapon.SetEnableMovement(true);
                }
                else
                {
                    weapons.rightWeapon.SetEnableMovement(true);
                }

                if (puppet != null)
                {
                    puppet.SetTransparent(1.0f, ReturnTime * 0.5f);
                }
                self.PlayAnimation(BattlerAnimationType.idle);
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 京、アンドロイドを召喚するか、アンドロイドを強化
    /// </summary>
    public void KeiDroneSupport()
    {
        var self = battleManager.GetCurrentBattler();
        KeiAndroidManagement androidManagement = self.GetComponent<KeiAndroidManagement>();

        // 必須のコンポネントが無ければ作る
        if (androidManagement == null)
        {
            androidManagement = self.gameObject.AddComponent<KeiAndroidManagement>();
        }

        FloatingText floatingText;
        string abilityName;

        // 上限数にたした
        if (androidManagement.GetCurrentSummonCount() >= 2)
        {
            // 自爆攻撃を行う
            var target = targetBattlers[0];
            var puppet = androidManagement.GetRandomSummon();

            // 技名を表示
            floatingText = CreateFloatingAbilityText(self.transform);
            abilityName = LocalizationManager.Localize("Ability.SuicideAttack");
            floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

            const float chargeTime = 0.5f;

            // 残像生成コンポネント
            FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
            fadeEffect.Initialize(chargeTime, 0.05f, self.Graphic);

            // play SE
            AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
            AudioManager.Instance.PlaySFX("Machine", 0.5f);

            var targetPos = target.GetComponent<RectTransform>().position;
            targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.15f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.15f, targetPos.y);

            puppet.RectTransform.DOMove(targetPos, chargeTime).SetEase(Ease.InOutQuint);
            self.PlayAnimation(BattlerAnimationType.magic);
            puppet.ColorTint(Color.red, chargeTime);

            DOTween.Sequence()
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // change character hirachy temporary
                puppet.transform.SetParent(target.transform);
            })
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                self.PlayAnimation(BattlerAnimationType.idle);

                // play SE
                AudioManager.Instance.PlaySFX("Explode");

                // VFX
                var VFXPosition = puppet.GetMiddleGlobalPosition() + ((puppet.GetMiddleGlobalPosition() - target.GetMiddleGlobalPosition()) / 2);
                VFXSpawner.SpawnVFX("Suicide", target.transform, VFXPosition);

                var realDamage = Battle.CalculateDamage(puppet.current_hp, target.defense, self.currentLevel, target.currentLevel, false);
                target.DeductHP(puppet, realDamage);

                // text
                floatingText = CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - puppet.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // ログ ({0}　が自爆する！{1} のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Suicide"), puppet.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // 即死
                puppet.DeductHP(puppet, 99999, true);
            })
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
            return;
        }

        // 技名を表示
        floatingText = CreateFloatingAbilityText(self.transform);
        abilityName = LocalizationManager.Localize("Ability.DroneSupport");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // play SE
        AudioManager.Instance.PlaySFX("SystemTrainPanel");
        self.PlayAnimation(BattlerAnimationType.magic);

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                AudioManager.Instance.PlaySFX("Unequip", 0.5f);
                var droid = androidManagement.SpawnNewAndroid();
                droid.ColorTint(CustomColor.invisible(), 0.5f);
                var originalLocalPos = droid.RectTransform.localPosition;
                droid.RectTransform.localPosition = new Vector3(droid.RectTransform.localPosition.x + 500.0f, droid.RectTransform.localPosition.y + 300.0f, droid.RectTransform.localPosition.z);
                droid.RectTransform.DOLocalMove(originalLocalPos, 0.25f).SetEase(Ease.Linear);
            })
            .AppendInterval(0.25f)
            .AppendCallback(() =>
            {
                self.PlayAnimation(BattlerAnimationType.idle);
                AudioManager.Instance.PlaySFX("Machine", 1f);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 那由多　雷神の相
    /// </summary>
    public void PowerMode()
    {
        var nayuta = battleManager.GetCurrentBattler();
        var powermodeScript = nayuta.GetComponent<Nayuta_Powermode>();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(nayuta.transform);
        string abilityName = LocalizationManager.Localize("Ability.PowerMode");
        floatingText.Init(2.0f, nayuta.GetMiddleGlobalPosition() + new Vector2(0.0f, nayuta.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, nayuta.character_color);

        // play SE
        AudioManager.Instance.PlaySFX("NayutaPowermode");

        nayuta.PlayAnimation(BattlerAnimationType.magic);

        // VFX
        var obj = VFXSpawner.SpawnVFX("Holy", nayuta.transform, nayuta.GetGraphicRectTransform().position);
        obj.transform.SetSiblingIndex(0);
        VFXSpawner.SpawnVFX("Recovery", nayuta.transform, nayuta.GetGraphicRectTransform().position + new Vector3(0.0f, nayuta.GetCharacterSize().y * 0.5f));
        nayuta.ColorTint(Color.magenta, 1.0f);

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                nayuta.PlayAnimation(BattlerAnimationType.idle);

                // effect
                int extraDamage = powermodeScript.ExtraDamage;
                powermodeScript.SetActive(!powermodeScript.IsActive);

                // 技を切り替え
                var ability = nayuta.GetAbility("PowerMode");

                if (powermodeScript.IsActive)
                {
                    // ログ ({0}　の攻撃力が {1} 上がった！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AttackUpStart"), nayuta.CharacterNameColored, CustomColor.AddColor(powermodeScript.ExtraDamage, CustomColor.damage())));

                    ability.abilityNameID = LocalizationManager.Localize("Ability.PowerMode_Off");
                }
                else
                {
                    // ログ ({0}　の攻撃力が {1} 下がった！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AttackDownStart"), nayuta.CharacterNameColored, CustomColor.AddColor(extraDamage, CustomColor.damage())));

                    ability.abilityNameID = LocalizationManager.Localize("Ability.PowerMode_On");
                }

                battleManager.SetDisplayActionPanel(true);
            });
    }

    /// <summary>
    /// 那由多　明鏡止水
    /// </summary>
    public void ChargeAttack()
    {
        var nayuta = battleManager.GetCurrentBattler();
        var powermodeScript = nayuta.GetComponent<Nayuta_Powermode>();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(nayuta.transform);
        string abilityName = LocalizationManager.Localize("Ability.ChargeAttack");
        floatingText.Init(2.0f, nayuta.GetMiddleGlobalPosition() + new Vector2(0.0f, nayuta.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, nayuta.character_color);

        // play SE
        AudioManager.Instance.PlaySFX("NayutaPowermode");

        nayuta.PlayAnimation(BattlerAnimationType.magic);

        // VFX
        var obj = VFXSpawner.SpawnVFX("Holy", nayuta.transform, nayuta.GetGraphicRectTransform().position);
        obj.transform.SetSiblingIndex(0);
        VFXSpawner.SpawnVFX("Recovery", nayuta.transform, nayuta.GetGraphicRectTransform().position + new Vector3(0.0f, nayuta.GetCharacterSize().y * 0.5f));
        nayuta.ColorTint(Color.magenta, 1.0f);

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                nayuta.PlayAnimation(BattlerAnimationType.idle);

                nayuta.Shake(0.5f);

                // effect
                int extraDamage =  Mathf.FloorToInt((float)nayuta.attack * 0.50f);
                powermodeScript.ChargeAttack(extraDamage);

                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 那由多　絶命破斬
    /// </summary>
    public void Ultimate()
    {
        var nayuta = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];
        var powermodeScript = nayuta.GetComponent<Nayuta_Powermode>();

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(nayuta.transform);
        string abilityName = LocalizationManager.Localize("Ability.Ultimate");
        floatingText.Init(2.0f, nayuta.GetMiddleGlobalPosition() + new Vector2(0.0f, nayuta.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, nayuta.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), nayuta.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Ultimate"), CustomColor.abilityName())));

        // play SE
        AudioManager.Instance.PlaySFX("NayutaPowermode");

        nayuta.PlayAnimation(BattlerAnimationType.magic);

        // VFX
        var obj = VFXSpawner.SpawnVFX("Holy", nayuta.transform, nayuta.GetGraphicRectTransform().position);
        obj.transform.SetSiblingIndex(0);
        nayuta.ColorTint(Color.magenta, 1.0f);
        nayuta.Shake(0.5f);

        // キャラ移動の準備
        Transform originalParent = nayuta.transform.parent;
        int originalChildIndex = nayuta.transform.GetSiblingIndex();
        var originalPos = nayuta.RectTransform.position;

        const float chargeTime = 0.5f;

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                obj = VFXSpawner.SpawnVFX("Recovery", nayuta.transform, nayuta.GetGraphicRectTransform().position + new Vector3(0.0f, nayuta.GetCharacterSize().y * 0.5f));
                obj.transform.SetParent(nayuta.transform.parent, true);
                nayuta.SetTransparent(0.0f, 1.0f);

                // play SE
                AudioManager.Instance.PlaySFX("Flee");
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                // relocate
                nayuta.SetTransparent(1.0f, 0.01f);
                nayuta.RectTransform.position = new Vector3(target.RectTransform.position.x + 1000.0f, target.RectTransform.position.y, target.RectTransform.position.z);

                // 残像生成コンポネント
                FadeEffect fadeEffect = nayuta.gameObject.AddComponent<FadeEffect>();
                fadeEffect.Initialize(1.0f, 0.05f, nayuta.Graphic);

                nayuta.ReverseFacing();
                nayuta.RectTransform.DOMoveX(target.RectTransform.position.x, chargeTime * 0.5f).SetEase(Ease.Linear);

                // play SE
                AudioManager.Instance.PlaySFX("Flee");
            })
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                // calculate damage
                int damage = Mathf.FloorToInt(nayuta.attack * (1.0f + (1.0f - (float)nayuta.current_hp / nayuta.max_hp))) * 2;
                damage = Battle.CalculateDamage(damage, target.defense, nayuta.currentLevel, target.currentLevel, true);

                nayuta.Heal(damage);
                target.DeductHP(nayuta, damage, false);

                // count as an attack
                nayuta.afterAttackEvent.Invoke();

                nayuta.SpawnAttackVFX(target);
                target.Shake(0.5f);
                target.ColorTint(Color.red, 0.5f);

                // play SE
                AudioManager.Instance.PlaySFX("Slash");

                nayuta.RectTransform.DOMove(originalPos, chargeTime * 0.5f).SetEase(Ease.Linear);

                // text
                floatingText = AbilityExecute.CreateFloatingText(target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), target.GetMiddleGlobalPosition() + new Vector2(0.0f, 100.0f), damage.ToString(), 64, CustomColor.damage());

                // ログ ({0}　に　{1}　のダメージを与えた！)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(damage, CustomColor.damage())));
            })
            .AppendInterval(chargeTime * 0.5f)
            .AppendCallback(() =>
            {
                nayuta.ReverseFacing();
                nayuta.PlayAnimation(BattlerAnimationType.idle);
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// 那由多　(敵)攻撃
    /// </summary>
    public void NayutaAttack()
    {
        var nayuta = battleManager.GetCurrentBattler();
        NayutaEnemyScript nayutaScript = nayuta.GetComponent<NayutaEnemyScript>();

        // 必須のコンポネントが無ければ作る
        if (nayutaScript == null)
        {
            nayutaScript = nayuta.gameObject.AddComponent<NayutaEnemyScript>();
        }

        if (nayutaScript.target == null || !nayutaScript.target.isAlive)
        {
            // find new target
            nayutaScript.target = targetBattlers[0];

            // VFX
            VFXSpawner.SpawnVFX("LockOn", nayutaScript.target.transform, nayutaScript.target.GetMiddleGlobalPosition());

            // SE
            AudioManager.Instance.PlaySFX("Heartbeat", 1.5f);

            DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                // ログ ({0} が次の標的とされている。)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.NayutaLockOn"), nayutaScript.target.CharacterNameColored));
                battleManager.NextTurn(false);
            });
        }
        else
        {
            // attack
            // 残像生成コンポネント
            FadeEffect fadeEffect = nayuta.gameObject.AddComponent<FadeEffect>();
            fadeEffect.Initialize(0.4f, 0.01f, nayuta.GetGraphicRectTransform().GetComponent<Image>());
            battleManager.Attack(nayuta, nayutaScript.target);

            // リセット
            nayutaScript.target = null;
        }
    }

    /// <summary>
    /// お座り
    /// </summary>
    public void SitDown()
    {
        var self = battleManager.GetCurrentBattler();

        // ログ ダイヤ　お座り！　えらい！！
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.SitDown"), self.CharacterNameColored));

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.SitDown");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // SE
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(1.0f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 嚙みつく
    /// </summary>
    public void Bite()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Bite");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　の 攻撃 ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Attack"), self.CharacterNameColored));

        // キャラ移動の準備
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 1.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 1.5f, targetPos.y);
        var originalPos = self.GetComponent<RectTransform>().position;
        const float characterMoveTime = 0.35f;
        const float animationTime = 0.8f;
        const float strikeTime = 0.1f;
        const float attackStayTime = 0.35f;

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

                    // move
                    self.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(animationTime)
                .AppendCallback(() =>
                {
                    var position = target.GetComponent<RectTransform>().position;
                    position = target.isEnemy ? new Vector2(position.x - target.GetCharacterSize().x * 0.5f, position.y) : new Vector2(position.x + target.GetCharacterSize().x * 0.5f, position.y);
                    self.DOComplete(true);
                    self.GetComponent<RectTransform>().DOMove(position, strikeTime).SetEase(Ease.Linear);

                    // SE
                    AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));
                })
                .AppendInterval(strikeTime)
                .AppendCallback(() =>
                {
                    // attack
                    self.PlayAnimation(BattlerAnimationType.attacked);
                    self.SpawnAttackVFX(target);
                    AudioManager.Instance.PlaySFX("Attacked", 1f);
                    target.Shake(0.75f);
                })
                .AppendInterval(attackStayTime)
                .AppendCallback(() =>
                {
                    self.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);

                    target.PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);

                    // stun for 1 turn
                    battleManager.AddBuffToBattler(target, BuffType.stun, 1, 0);
                });
    }

    /// <summary>
    /// 思考停止
    /// </summary>
    public void StopThinking()
    {
        var self = battleManager.GetCurrentBattler();

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.StopThinking"), CustomColor.abilityName())));

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.StopThinking");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // SE
        AudioManager.Instance.PlaySFX("Sleep");

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(1.0f)
                .AppendCallback(() =>
                {
                    battleManager.AddBuffToBattler(self, BuffType.attack_up, UnityEngine.Random.Range(2, 6), UnityEngine.Random.Range(20, 40));
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 発情
    /// </summary>
    public void Horny()
    {
        var self = battleManager.GetCurrentBattler();

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Horny"), CustomColor.abilityName())));

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Horny");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // SE
        AudioManager.Instance.PlaySFX("Sleep");

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(1.0f)
                .AppendCallback(() =>
                {
                    battleManager.AddBuffToBattler(self, BuffType.horny, 3, 0);
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 共倒れ (ダイヤモンド)
    /// </summary>
    public void DiamondSlash()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.DiamondSlash");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ログ ({0}　からの {1} ！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.DiamondSlash"), CustomColor.abilityName())));

        // キャラ移動の準備
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 1.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 1.5f, targetPos.y);
        var originalPos = self.GetComponent<RectTransform>().position;
        const float characterMoveTime = 0.35f;
        const float animationTime = 0.8f;
        const float strikeTime = 0.1f;
        const float attackStayTime = 0.35f;

        // SE
        AudioManager.Instance.PlaySFX(self.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        // Shake
        Coroutine shake = null;
        Vector2 positionPreShake = Vector2.zero;

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

                    // move
                    self.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // change character hirachy temporary
                    self.transform.SetParent(target.transform);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // charge
                    self.PlayAnimation(BattlerAnimationType.attack);
                    AudioManager.Instance.PlaySFX("Charge");
                    positionPreShake = self.GetComponent<RectTransform>().position;
                    shake = ShakeManager.Instance.ShakeObject(self.GetComponent<RectTransform>(), animationTime, 5.0f);
                    self.ColorTint(Color.blue, animationTime);
                })
                .AppendInterval(animationTime)
                .AppendCallback(() =>
                {
                    ShakeManager.Instance.StopObjectShake(shake);
                    self.GetComponent<RectTransform>().position = positionPreShake;
                    var position = target.GetComponent<RectTransform>().position;
                    position = target.isEnemy ? new Vector2(position.x - target.GetCharacterSize().x * 0.5f, position.y) : new Vector2(position.x + target.GetCharacterSize().x * 0.5f, position.y);
                    self.DOComplete(true);
                    self.GetComponent<RectTransform>().DOMove(position, strikeTime).SetEase(Ease.Linear);
                    self.GetGraphicRectTransform().DORotate(new Vector3(0, 0, -40.0f), animationTime, RotateMode.Fast);

                    // 残像生成コンポネント
                    FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
                    fadeEffect.Initialize(strikeTime, 0.05f, self.GetGraphicRectTransform().GetComponent<Image>());
                })
                .AppendInterval(strikeTime)
                .AppendCallback(() =>
                {
                    // attack
                    self.PlayAnimation(BattlerAnimationType.attacked);
                    self.SpawnAttackVFX(target);
                    AudioManager.Instance.PlaySFX("PowerfulPunch", 1f);
                    target.Shake(0.75f);

                    // stun for 1 turn
                    battleManager.AddBuffToBattler(target, BuffType.stun, 2, 0);

                    // text
                    floatingText = CreateFloatingText(target.transform);
                    string stunString = LocalizationManager.Localize("Buff.Stun");
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), stunString, 64, CustomColor.stun());
                })
                .AppendInterval(attackStayTime)
                .AppendCallback(() =>
                {
                    self.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);
                    // rotate
                    self.GetGraphicRectTransform().DORotate(Vector3.zero, animationTime, RotateMode.Fast);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    // return to original parent
                    self.transform.SetParent(originalParent);
                    self.transform.SetSiblingIndex(originalChildIndex);

                    target.PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(characterMoveTime * 0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 京(LAST BOSS)
    /// </summary>
    public void RepairFriend()
    {
        // 京の武器を取得
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();

        // 相手の資料を取得
        // get all enemy
        var enemies = FindAnyObjectByType<Battle>().GetAllEnemy().Where(c => c.isAlive).ToList();
        if (enemies.Count == 1)
        {
            // 自分が最後の敵
            var playerscharacter = FindAnyObjectByType<Battle>().GetAllTeammate();
            targetBattlers[0] = playerscharacter.FirstOrDefault(c => c.isAlive);
            KeiAttack();
            return;
        }
        else
        {
            // 他人を優先する
            targetBattlers[0] = enemies.FirstOrDefault(c => c.character_name != kei.character_name);
        }
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.RepairFriend");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ログ ({0}　の　{1}！)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), kei.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.RepairFriend"), CustomColor.abilityName())));

        // 武器の動きを一旦止める
        weapons.leftWeapon.SetEnableMovement(false);
        weapons.rightWeapon.SetEnableMovement(false);

        weapons.leftWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
        weapons.rightWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);

        // アニメション
        const float animtionTime = 1.0f;
        kei.PlayAnimation(BattlerAnimationType.magic);

        // 残像
        FadeEffect fadeEffect = weapons.leftWeapon.Rect.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(animtionTime, 0.05f, weapons.leftWeapon.GetComponent<Image>());
        fadeEffect = weapons.rightWeapon.Rect.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(animtionTime, 0.05f, weapons.rightWeapon.GetComponent<Image>());

        // SE
        AudioManager.Instance.PlaySFX("Machine");

        
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(animtionTime * 0.5f)
                .AppendCallback(() =>
                {
                    weapons.leftWeapon.Rect.DOKill(false);
                    weapons.rightWeapon.Rect.DOKill(false);

                    var targetSprite = target.GetGraphicRectTransform();
                    weapons.leftWeapon.transform.SetParent(targetSprite.parent);
                    weapons.leftWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex());
                    weapons.rightWeapon.transform.SetParent(targetSprite.parent);
                    weapons.rightWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex() + 1);

                    weapons.leftWeapon.Rect.DOLocalMove(weapons.LeftWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                    weapons.rightWeapon.Rect.DOLocalMove(weapons.RightWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);

                    // SE
                    AudioManager.Instance.PlaySFX("Machine", 0.5f);

                    // アニメション
                    kei.PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval((animtionTime * 0.5f) + 0.25f)
                .AppendCallback(() =>
                {
                    // heal
                    int amount = Mathf.FloorToInt((float)target.max_hp * 0.15f);
                    target.Heal(amount);

                    // ログ ({0}　の HPが {1} 回復した！)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(amount, CustomColor.heal())));

                    // SP
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + amount.ToString(), 64, CustomColor.heal());

                    // Audio
                    AudioManager.Instance.PlaySFX("Heal");
                    AudioManager.Instance.PlaySFX("Machine", 0.5f);

                    // 元の所に戻す
                    weapons.leftWeapon.Rect.DOMove(kei.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
                    weapons.rightWeapon.Rect.DOMove(kei.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
                })
                .AppendInterval(animtionTime * 0.5f)
                .AppendCallback(() =>
                {
                    weapons.leftWeapon.Rect.DOKill(false);
                    weapons.rightWeapon.Rect.DOKill(false);

                    var targetSprite = kei.GetGraphicRectTransform();
                    weapons.leftWeapon.transform.SetParent(targetSprite.parent);
                    weapons.leftWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex());
                    weapons.rightWeapon.transform.SetParent(targetSprite.parent);
                    weapons.rightWeapon.transform.SetSiblingIndex(targetSprite.GetSiblingIndex() + 1);

                    weapons.leftWeapon.Rect.DOLocalMove(weapons.LeftWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                    weapons.rightWeapon.Rect.DOLocalMove(weapons.RightWeaponLocalPosition, animtionTime * 0.5f).SetEase(Ease.Linear);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    weapons.leftWeapon.SetEnableMovement(true);
                    weapons.rightWeapon.SetEnableMovement(true);
                    battleManager.NextTurn(false);
                });
    }

    #endregion abilities
}
