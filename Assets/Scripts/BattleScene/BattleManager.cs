using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class Battle : MonoBehaviour
{
#if DEBUG_MODE
    [Header("Debug")]
    [SerializeField] private bool isDebug = false;
#endif

    [Header("Setting")]
    [SerializeField] private float characterSpace = 150.0f;
    [SerializeField, Range(1.1f, 2.0f)] private float turnEndDelay = 1.1f;
    [SerializeField, Range(1.1f, 2.0f)] private float stunWaitDelay = 0.55f;
    [SerializeField, Range(1.1f, 2.0f)] private float enemyAIDelay = 2.0f;
    [SerializeField, Range(0.1f, 1.0f)] private float characterMoveTime = 0.5f;  // < �L�������^�[�Q�b�g�̑O�Ɉړ����鎞��
    [SerializeField, Range(0.1f, 1.0f)] private float attackAnimPlayTime = 0.2f; // < �U���A�j���[�V�����̈ێ�����
    [SerializeField, Range(0.1f, 1.0f)] private float buffIconFadeTime = 0.4f; // < �o�t�A�C�R���̏o���E��������
    [SerializeField] private float formationPositionX = 600.0f;
    [SerializeField] private Sprite buffIconFrame;
    [SerializeField] private GameObject buffCounterText;

    [Header("References")]
    [SerializeField] private RectTransform playerFormation;
    [SerializeField] private RectTransform enemyFormation;
    [SerializeField] private TurnBase turnBaseManager;
    [SerializeField] private ActionPanel actionPanel;
    [SerializeField] private CharacterArrow characterArrow;
    [SerializeField] private RectTransform actionTargetArrow;
    [SerializeField] private BattleSceneTransition sceneTransition;
    [SerializeField] private FloatingText floatingTextOrigin;
    [SerializeField] private BattleSceneTutorial battleSceneTutorial;
    [SerializeField] private CharacterInfoPanel characterInfoPanel;
    [SerializeField] private BattleLog battleLogScript;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject escapeButton;
    [SerializeField] private CanvasGroup escapePopup;
    [SerializeField] private Button autoBattleButton;
    [SerializeField] private TMP_Text autoBattleText;
    [SerializeField] private Image autoBattleIcon;

    [Header("Debug")]
    [SerializeField] private bool isAutoBattling;
    [SerializeField] private List<Battler> characterList = new List<Battler>();
    [SerializeField] private List<Battler> enemyList = new List<Battler>();
    [SerializeField] private Battler arrowPointingTarget = null;
    [SerializeField] private List<Buff> buffedCharacters = new List<Buff>();
    [SerializeField] private Coroutine playerAI;
    [SerializeField] private bool isPlayerAIRunning = false;

    public float CharacterMoveTime { get { return characterMoveTime; } }
    public float AttackAnimPlayTime { get { return attackAnimPlayTime; } }

    private void Awake()
    {
        AlphaFadeManager.Instance.FadeIn(5.0f);

#if DEBUG_MODE
        if (isDebug) ProgressManager.Instance.DebugModeInitialize(true); // �f�o�b�O�p
#endif

        var actors = new List<Character>();
        if (BattleSetup.IsCustomFormation())
        {
            // ����C�x���g
            var formation = BattleSetup.GetCustomFormation();
            actors = formation;
        }
        else
        {
            // �t�H�[���[�V�����ɂ���L���������̂܂ܗ��p
            var playerCharacters = ProgressManager.Instance.GetFormationParty(false);
            for (int i = 0; i < playerCharacters.Count(); i++)
            {
                if (playerCharacters[i].characterID != -1)
                {
                    actors.Add(ProgressManager.Instance.GetCharacterByID(playerCharacters[i].characterID));
                }
            }
        }

        List<EnemyDefine> enemyList = BattleSetup.GetEnemyList(false);
        InitializeBattleScene(actors, enemyList);

        // �P�ރ{�^��
        escapeButton.SetActive(BattleSetup.isAllowEscape && !BattleSetup.isEventBattle);

        // �����o�g���{�^��
        if (ProgressManager.Instance.GetCurrentStageProgress() > 1)
        {
            autoBattleButton.gameObject.SetActive(true);
            autoBattleText.gameObject.SetActive(false);
            autoBattleIcon.color = Color.white;
            isAutoBattling = false;
        }
            
        // Send references
        ItemExecute.Instance.Initialize(this);
        AbilityExecute.Instance.Initialize(this);
        EquipmentMethods.Initialization(this);
        BuffManager.Init();
    }

    private void Start()
    {
        sceneTransition.StartScene(NextTurn);

        // Start BGM
        if (BattleSetup.BattleBGM != string.Empty)
        {
            AudioManager.Instance.PlayMusicWithFade(BattleSetup.BattleBGM, 2.0f);
        }
    }

    // Start is called before the first frame update
    void InitializeBattleScene(List<Character> actors, List<EnemyDefine> enemies)
    {
        const float max_positionY = 130.0f;
        const float totalGap = 330.0f;

        // �v���C���[�L��������
        float positionX = (actors.Count * characterSpace) * 0.5f;
        float positionY_gap = totalGap / actors.Count;
        float positionY = max_positionY - positionY_gap;
        if (actors.Count == 1) positionY = -50; // 1�l�������Ȃ��ꍇ
        foreach (Character actor in actors)
        {
            GameObject obj = Instantiate<GameObject>(actor.battler, playerFormation);
            obj.transform.localPosition = new Vector3(positionX, positionY, 0.0f);

            Battler component = obj.GetComponent<Battler>();
            component.InitializeCharacterData(actor);
            
            characterList.Add(component);

            positionX -= characterSpace;
            positionY -= positionY_gap;
        }

        // �����j
        if (characterList.Any(x => x.equipment != null && x.equipment.pathName == "Equip_Kei"))
        {
            foreach (var battler in characterList)
            {
                if (battler.equipment == null || battler.equipment.pathName != "Equip_Kei")
                {
                    battler.attack += 15;
                }
            }
        }

        // �G�L��������
        positionX = -(enemies.Count * characterSpace) * 0.5f;
        positionY_gap = totalGap / enemies.Count;
        positionY = max_positionY - positionY_gap;
        int siblingIndex = 0;
        if (enemies.Count == 1) positionY = -50; // 1�l�������Ȃ��ꍇ
        foreach (EnemyDefine enemy in enemies)
        {
            GameObject obj = Instantiate<GameObject>(enemy.battler, enemyFormation);
            obj.transform.localPosition = new Vector3(positionX, positionY, 0.0f);
            obj.transform.SetSiblingIndex(siblingIndex);

            Battler component = obj.GetComponent<Battler>();
            component.InitializeEnemyData(enemy);

            enemyList.Add(component);

            positionX += characterSpace;
            positionY -= positionY_gap;
            siblingIndex++;
        }

        // �s���������߂�
        turnBaseManager.Initialization(characterList, enemyList);

        // �����ʒu
        playerFormation.DOLocalMoveX(-formationPositionX * 2.1f, 0.0f, true);
        enemyFormation.DOLocalMoveX(formationPositionX * 2.1f, 0.0f, true);
        playerFormation.GetComponent<CanvasGroup>().alpha = 0.0f;
        enemyFormation.GetComponent<CanvasGroup>().alpha = 0.0f;
    }

    // �o�g���r������V�K�G�L������ǉ�
    public void AddEnemy(Battler newEnemy, EnemyDefine data)
    {
        enemyList.Add(newEnemy);
        turnBaseManager.AddEnemy(newEnemy, data);
    }

    /// <summary>
    /// ���̃^�[��
    /// </summary>
    public void NextTurn(bool isFirstTurn)
    {
        // �퓬���O�@�ċN�s�\�̍��m
        for (int i = 0; i < turnBaseManager.DeathBattler.Count; i++)
        {
            AddBattleLog(System.String.Format(Assets.SimpleLocalization.Scripts.LocalizationManager.Localize("BattleLog.Retire"), turnBaseManager.DeathBattler[i].CharacterNameColored));
        }
        turnBaseManager.DeathBattler.Clear();

        // �^�[�����n�߂�O�ɐ퓬���I����Ă��邩���`�F�b�N
        if (IsVictory())
        {
            BattleEnd(true);
            return;
        }

        if (IsDefeat())
        {
            BattleEnd(false);
            return;
        }

        // �Ăяo��
        GetCurrentBattler().OnTurnEnd();

        if (!isFirstTurn)
        {
            if (!IsCharacterInBuff(GetCurrentBattler(), BuffType.continuous_action)) // �A���s��
            {
                turnBaseManager.NextBattler();
            }
        }
        else
        {
            // �ŏ��̃^�[��
            // �L�����N�^�[���ʒu�ɕt��
            playerFormation.DOLocalMoveX(-formationPositionX, 0.5f).SetEase(Ease.OutQuart);
            enemyFormation.DOLocalMoveX(formationPositionX, 0.5f).SetEase(Ease.OutQuart);
            playerFormation.GetComponent<CanvasGroup>().DOFade(1.0f, 0.25f);
            enemyFormation.GetComponent<CanvasGroup>().DOFade(1.0f, 0.25f);

            // SE
            AudioManager.Instance.PlaySFX("FormationCharge", 0.5f);

            // �`���[�g���A���ɓ���
            if (ProgressManager.Instance.GetCurrentStageProgress() == 1)
            {
                battleSceneTutorial.StartBattleTutorial();
            }
        }

        Battler currentTurnCharacter = turnBaseManager.GetCurrentTurnBattler();
        currentTurnCharacter.UpdateAbilityCooldown(); // ����Z�̃`���[�W��Ԃ��X�V

        // �Ăяo��
        GetCurrentBattler().OnTurnBegin();

        if (currentTurnCharacter.isEnemy)
        {
            actionPanel.SetEnablePanel(false);

            // AI �s��
            StartCoroutine(EnemyAI());
        }
        else if (isAutoBattling)
        {
            actionPanel.SetEnablePanel(false);

            // �I�[�g�o�g��
            playerAI = StartCoroutine(PlayerAI());
        }
        else
        {
            // �s���o����܂łɒx��������
            StartCoroutine(TurnEndDelay());
        }
    }

    /// <summary>
    /// ���^�C�A�����L�����N�^�[���o����^�[������O��
    /// </summary>
    public void UpdateTurnBaseManager(bool rearrange)
    {
        turnBaseManager.UpdateTurn(rearrange);
    }

    // �}�E�X���w���Ă���Ƃ��낪Battler�����݂��Ă���Ȃ�Ԃ�
    public Battler GetBattlerByPosition(Vector2 mousePosition, bool allowTeammate, bool allowEnemy, bool aliveOnly)
    {
        if (allowEnemy)
        {
            for (int i = 0; i < enemyList.Count; i++)
            {
                Vector2 size = enemyList[i].GetCharacterSize() * new Vector2(0.5f, 1.0f);
                Vector2 position = (enemyList[i].GetGraphicRectTransform().position / canvas.scaleFactor) + new Vector3(0.0f, size.y * 0.5f);
                if ((enemyList[i].isAlive || !aliveOnly) && enemyList[i].isTargettable
                    && mousePosition.x > position.x - size.x * 0.5f
                    && mousePosition.x < position.x + size.x * 0.5f
                    && mousePosition.y > position.y - size.y * 0.5f
                    && mousePosition.y < position.y + size.y * 0.5f)
                {
                    return enemyList[i];
                }
            }
        }

        if (allowTeammate)
        {
            for (int i = 0; i < characterList.Count; i++)
            {
                Vector2 size = characterList[i].GetCharacterSize() * new Vector2(0.5f, 1.0f);
                Vector2 position = (characterList[i].GetGraphicRectTransform().position / canvas.scaleFactor) + new Vector3(0.0f, size.y * 0.5f);
                if ((characterList[i].isAlive || !aliveOnly)
                    && mousePosition.x > position.x - size.x * 0.5f
                    && mousePosition.x < position.x + size.x * 0.5f
                    && mousePosition.y > position.y - size.y * 0.5f
                    && mousePosition.y < position.y + size.y * 0.5f)
                {
                    return characterList[i];
                }
            }
        }

        return null;
    }

    public List<Battler> GetAllEnemy()
    {
        return enemyList;
    }
    public List<Battler> GetAllTeammate()
    {
        return characterList;
    }

    IEnumerator EnemyAI()
    {
        Battler currentCharacter = turnBaseManager.GetCurrentTurnBattler();
        characterArrow.SetCharacter(currentCharacter, currentCharacter.GetCharacterSize().y);

        yield return new WaitForSeconds(enemyAIDelay);

        // �o�t���Ƀ`�F�b�N
        bool isCharacterStunned = IsCharacterInBuff(currentCharacter, BuffType.stun);
        UpdateBuffForCharacter(GetCurrentBattler());

        // �U���ڕW��I��
        // is character stunned
        if (isCharacterStunned || IsCharacterInBuff(currentCharacter, BuffType.stun))
        {
            yield return new WaitForSeconds(stunWaitDelay);
            NextTurn(false);
        }
        else
        {
            // �ǂ̍s������邩�����߂�
            var possibleAction = currentCharacter.GetAllPossibleAction();
            if (possibleAction.Count == 0)
            {
                // ����s�����Ȃ��A�ҋ@����
                IdleCommand();
            }
            else
            {
                Battler targetCharacter = null;
                List<Battler> targetCharacters = null;
                var action = currentCharacter.GetNextAction(possibleAction);

                switch (action.actionType)
                {
                    case EnemyActionType.NormalAttack:
                        {
                            targetCharacter = turnBaseManager.GetRandomPlayerCharacter(true);
                            StartCoroutine(AttackAnimation(currentCharacter, targetCharacter, NextTurn));
                        }
                        break;
                    case EnemyActionType.SpecialAbility:
                        {
                            if (action.ability.castType == CastType.Enemy)
                            {
                                if (action.ability.isAOE)
                                {
                                    // �S���I��
                                    targetCharacters = turnBaseManager.GetAllPlayerCharacters();
                                }
                                else
                                {
                                    // �v���C���[�L�����������_���ɑI������
                                    targetCharacter = turnBaseManager.GetRandomPlayerCharacter(true);
                                }
                            }
                            else if (action.ability.castType == CastType.Teammate)
                            {
                                if (action.ability.abilityType == AbilityType.Heal)
                                {
                                    // ���Ã^�C�v�̋Z�Ȃ�ႢHP�̒��Ԃɂ���
                                    targetCharacter = turnBaseManager.GetEnemyCharacterWithLowestHP();
                                }
                                else
                                {
                                    targetCharacter = turnBaseManager.GetRandomEnemyCharacter(false);
                                }
                            }
                            else // CastType.Self
                            {
                                targetCharacter = null;
                            }

                            if (targetCharacters == null)
                            {
                                AbilityExecute.Instance.SetTargetBattler(targetCharacter);
                            }
                            else
                            {
                                AbilityExecute.Instance.SetTargetBattlers(targetCharacters);
                            }
                            AbilityExecute.Instance.Invoke(action.ability.functionName, 0);
                            currentCharacter.DeductSP(action.ability.consumeSP);
                            currentCharacter.SetAbilityOnCooldown(action.ability, action.ability.cooldown);
                        }
                        break;
                    case EnemyActionType.Idle:
                        {
                            IdleCommand();
                        }
                        break;
                }
            }

        }
    }

    IEnumerator PlayerAI()
    {
        isPlayerAIRunning = true;
        yield return new WaitForSeconds(turnEndDelay);
        
        Battler currentCharacter = turnBaseManager.GetCurrentTurnBattler();
        characterArrow.SetCharacter(currentCharacter, currentCharacter.GetCharacterSize().y);

        // �o�t���Ƀ`�F�b�N
        bool isCharacterStunned = IsCharacterInBuff(currentCharacter, BuffType.stun);
        UpdateBuffForCharacter(GetCurrentBattler());

        // �U���ڕW��I��
        // is character stunned
        if (isCharacterStunned || IsCharacterInBuff(currentCharacter, BuffType.stun))
        {
            yield return new WaitForSeconds(stunWaitDelay);
            NextTurn(false);
        }
        else
        {
            // �ǂ̍s������邩�����߂�
            if (!currentCharacter.EnableNormalAttack)
            {
                // ����s�����Ȃ��A�ҋ@����
                IdleCommand();
            }
            else
            {
                StartCoroutine(AttackAnimation(currentCharacter, turnBaseManager.GetRandomEnemyCharacter(true), NextTurn));
            }
        }
        isPlayerAIRunning = false;
    }

    IEnumerator TurnEndDelay()
    {
        actionPanel.SetEnablePanel(false);

        if (!IsCharacterInBuff(GetCurrentBattler(), BuffType.continuous_action)) // �A���s��
        {
            yield return new WaitForSeconds(turnEndDelay);
        }

        Battler currentCharacter = turnBaseManager.GetCurrentTurnBattler();
        characterArrow.SetCharacter(currentCharacter, currentCharacter.GetCharacterSize().y);

        var originPos = currentCharacter.GetGraphicRectTransform().position;
        var offset = currentCharacter.isEnemy ? new Vector3(-currentCharacter.GetCharacterSize().x * 0.25f, currentCharacter.GetCharacterSize().y * 0.5f) : new Vector3(currentCharacter.GetCharacterSize().x * 0.25f, currentCharacter.GetCharacterSize().y * 0.5f);
        originPos = originPos + offset;
        actionTargetArrow.position = originPos;

        // �o�t���Ƀ`�F�b�N
        bool isCharacterStunned = IsCharacterInBuff(currentCharacter, BuffType.stun);
        UpdateBuffForCharacter(GetCurrentBattler());

        // is character stunned
        if (isCharacterStunned || IsCharacterInBuff(currentCharacter, BuffType.stun))
        {
            yield return new WaitForSeconds(stunWaitDelay);
            NextTurn(false);
        }
        else
        {
            actionPanel.SetEnablePanel(true);
        }
    }

    public void PointTargetWithArrow(Battler target, float animTime)
    {
        if (arrowPointingTarget == target) return;

        Battler currentBattler = turnBaseManager.GetCurrentTurnBattler();

        if (target == currentBattler) return; // �����Ɏw�����Ƃ͂ł��Ȃ�

        var originPos = currentBattler.GetGraphicRectTransform().position;
        originPos = currentBattler.isEnemy ? new Vector2(originPos.x - currentBattler.GetCharacterSize().x * 0.25f, originPos.y + currentBattler.GetCharacterSize().y * 0.5f) : new Vector2(originPos.x + currentBattler.GetCharacterSize().x * 0.25f, originPos.y + currentBattler.GetCharacterSize().y * 0.5f);
        var targetPos = target.GetGraphicRectTransform().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.25f, targetPos.y + target.GetCharacterSize().y * 0.5f) : new Vector2(targetPos.x, targetPos.y + target.GetCharacterSize().y * 0.5f);
        var length = Vector2.Distance(originPos, targetPos) / CanvasReferencer.Instance.GetScaleFactor();

        actionTargetArrow.sizeDelta = new Vector2(actionTargetArrow.rect.width, 100.0f);
        actionTargetArrow.DOSizeDelta(new Vector2(actionTargetArrow.rect.width, length), animTime);
        actionTargetArrow.GetComponent<Image>().DOFade(0.2f, animTime);

        // rotate
        // Calculate direction vector
        Vector3 diff = targetPos - originPos;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        actionTargetArrow.rotation = Quaternion.Euler(0f, 0f, rot_z - 90.0f);

        arrowPointingTarget = target;
    }

    public void UnPointArrow(float animTime)
    {
        actionTargetArrow.GetComponent<Image>().DOFade(0.0f, animTime);
        arrowPointingTarget = null;
    }

    public void AttackCommand(Battler target)
    {
        StartCoroutine(AttackAnimation(turnBaseManager.GetCurrentTurnBattler(), target, NextTurn));
    }

    public void IdleCommand()
    {
        var battler = GetCurrentBattler();

        if (battler.max_mp == 0)
        {
            // �񕜂ł���SP���Ȃ�
            NextTurn(false);

            // SE�Đ�
            AudioManager.Instance.PlaySFX("SystemActionPanel");

            // ���O ({0}�@���ҋ@����B�I)
            AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Idle"), battler.CharacterNameColored));
            return;
        }

        // ��SP��15%~20%���񕜂���
        int healAmount = Mathf.RoundToInt((float)battler.max_mp * UnityEngine.Random.Range(0.15f, 0.2f));

        // �G�̐��j��P�͉񕜒l��{�ɂ���
        if (battler.isEnemy && battler.IsFemale)
        {
            healAmount = healAmount * 2;
        }

        // �A�j���V����
        var sequence = DOTween.Sequence();
                sequence
                .AppendCallback(() =>
                {
                    // text
                    var floatingText = Instantiate(floatingTextOrigin, battler.transform);
                    floatingText.Init(2f, battler.GetMiddleGlobalPosition(), new Vector2(0.0f, 150.0f), "+"+healAmount.ToString(), 64, CustomColor.SP());

                    // play SE
                    AudioManager.Instance.PlaySFX("PowerCharge");

                    // effect
                    battler.AddSP(healAmount);

                    // ���O ({0}�@���x�e���Ƃ����BSP�@{1}�@�񕜂����B)
                    AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.IdleSP"), battler.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.SP())));
                })
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    // �������ꏈ��
                    if (battler.equipment != null && battler.equipment.pathName == "Cushion")
                    {
                        EquipmentMethods.CushionExecute(healAmount);
                    }

                    // �^�[���I��
                    NextTurn(false);
                });
    }

    public void Attack(Battler attacker, Battler target)
    {
        StartCoroutine(AttackAnimation(attacker, target, NextTurn));
    }

    /// <summary>
    /// �U���p�V�[�P���X
    /// </summary>
    IEnumerator AttackAnimation(Battler attacker, Battler target, Action<bool> callback)
    {
        // �ߋ����U������Ȃ�
        if (attacker.AttackCallback != string.Empty)
        {
            AbilityExecute.Instance.Invoke(attacker.AttackCallback, 0);
            if (attacker.IsAoENormalAttack)
            {
                if (attacker.isEnemy)
                {
                    AbilityExecute.Instance.SetTargetBattlers(turnBaseManager.GetAllPlayerCharacters());
                }
                else
                {
                    AbilityExecute.Instance.SetTargetBattlers(turnBaseManager.GetAllEnemyCharacters());
                }
            }
            else
            {
                AbilityExecute.Instance.SetTargetBattler(target);
            }
            yield break;
        }

        Transform originalParent = attacker.transform.parent;
        int originalChildIndex = attacker.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.5f, targetPos.y);
        var originalPos = attacker.GetComponent<RectTransform>().position;
        attacker.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);

        // ���O (xxx�@�̍U���I)
        AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Attack"), attacker.CharacterNameColored));

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);
        AudioManager.Instance.PlaySFX(attacker.GetCharacterVoiceName(BattlerSoundEffectType.Attack));

        yield return new WaitForSeconds(characterMoveTime * 0.5f);
        // change character hirachy temporary
        attacker.transform.SetParent(target.transform);
        attacker.PlayAnimation(BattlerAnimationType.attack);
        yield return new WaitForSeconds(characterMoveTime * 0.5f);

        attacker.SpawnAttackVFX(target);

        // attack miss?
        bool isMiss = (UnityEngine.Random.Range(0, 100) > CalculateHitChance(attacker.speed - target.speed));

        if (attacker.CantMissAttack) isMiss = false; // ��΃~�X���Ȃ�

        if (!isMiss)
        {
            // play SE
            AudioManager.Instance.PlaySFX(attacker.GetSoundEffects(BattlerSoundEffectType.Attack), 0.8f);

            // �U���v�Z
            int levelAdjustedDamage = CalculateDamage(attacker, target);
            int realDamage = target.DeductHP(attacker, levelAdjustedDamage);

            // play SE
            if (!string.IsNullOrEmpty(target.GetSoundEffects(BattlerSoundEffectType.Attacked)))
            {
                AudioManager.Instance.PlaySFX(target.GetSoundEffects(BattlerSoundEffectType.Attacked), 0.8f);
            }
            else
            {
                AudioManager.Instance.PlaySFX("Attacked", 0.8f);
            }
            AudioManager.Instance.PlaySFX(target.GetCharacterVoiceName(BattlerSoundEffectType.Attacked));

            // animation
            target.Shake(attackAnimPlayTime + characterMoveTime);
            target.PlayAnimation(BattlerAnimationType.attacked);

            if (realDamage > 0)
            {
                // create floating text
                var floatingText = Instantiate(floatingTextOrigin, target.transform);
                floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - attacker.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                // ���O ({0}�@�Ɂ@{1}�@�̃_���[�W��^�����I)
                AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // �������ꏈ�� (�G���i�̐��j)
                if (attacker.equipment != null && attacker.equipment.pathName == "Equip_Erena") EquipmentMethods.ErenaSeikakuExecute(target);
            }
            else
            {
                // ���O (���ʂ͂Ȃ������B)
                AddBattleLog(LocalizationManager.Localize("BattleLog.NoEffect"));
            }
        }
        else
        {
            // play SE
            AudioManager.Instance.PlaySFX("Miss", 0.5f);

            // animation
            RectTransform targetGraphic = target.GetGraphicRectTransform();
            float enemyPos = targetGraphic.localPosition.x;
            targetGraphic.DOLocalMoveX(enemyPos + ((target.transform.position.x - attacker.transform.position.x) * 0.5f), attackAnimPlayTime).SetEase(Ease.InOutBounce)
                .OnComplete(() => { targetGraphic.DOLocalMoveX(enemyPos, characterMoveTime); });

            // move character shadow with it
            RectTransform shadow = target.GetShadowRectTransform();
            shadow.DOLocalMoveX(enemyPos + ((target.transform.position.x - attacker.transform.position.x) * 0.5f), attackAnimPlayTime).SetEase(Ease.InOutBounce)
                .OnComplete(() => { shadow.DOLocalMoveX(0.0f, characterMoveTime); });

            // create floating text
            var floatingText = Instantiate(floatingTextOrigin, target.transform);
            floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - attacker.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), "MISS", 32, CustomColor.miss());

            // ���O ({0}�@�ɔ�����ꂽ�I)
            AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.MissAttack"), target.CharacterNameColored));
        }

        yield return new WaitForSeconds(attackAnimPlayTime);

        attacker.PlayAnimation(BattlerAnimationType.idle); 
        target.PlayAnimation(BattlerAnimationType.idle);

        attacker.GetComponent<RectTransform>().DOMove(originalPos, characterMoveTime);

        yield return new WaitForSeconds(characterMoveTime * 0.5f);
        // return to original parent
        attacker.transform.SetParent(originalParent);
        attacker.transform.SetSiblingIndex(originalChildIndex);
        yield return new WaitForSeconds(characterMoveTime * 0.5f);

        attacker.afterAttackEvent.Invoke();
        callback?.Invoke(false);
    }

    // �U���͌v�Z����
    public static int CalculateDamage(Battler attacker, Battler target, bool randomizeDamage = true)
    {
        return CalculateDamage(attacker.attack, target.defense, attacker.currentLevel, target.currentLevel, randomizeDamage);
    }
    public static int CalculateDamage(int damage, int defense, int attackerLevel, int defenderLevel, bool randomizeDamage = true)
    {
        // randomize damage
        int multiplier = randomizeDamage ? UnityEngine.Random.Range(0, 7) : 0;
        // calculate damage ([a.ATK \ *0.5F] - [b.DEF * 0.25F]) * Clamp.(a.LVL / b.LVL, 0.5F, 1.5F)
        int value = Mathf.RoundToInt(((float)damage * 0.5f) - ((float)defense * 0.25f) * Mathf.Clamp((float)attackerLevel / (float)defenderLevel, 0.5f, 1.5f)) + multiplier;

        // 1~999
        return Mathf.Clamp(value, 1, 999);
    }

    /// <summary>
    /// �U���̖��������v�Z
    /// </summary>
    int CalculateHitChance(int dexterityDifference)
    {
        if (dexterityDifference < 0)
        {
            const int baseHitChance = 85;
            const float chanceModifier = 1.25f;

            int calculatedHitChance = baseHitChance + Mathf.RoundToInt((float)dexterityDifference * chanceModifier);

            // Ensure the calculatedHitChance is within valid bounds (0 to 100)
            return Mathf.Clamp(calculatedHitChance, 0, 100);
        }

        // If the defender has equal or lower speed, chance of hitting is 100%
        return 100;
    }

    public Battler GetCurrentBattler()
    {
        return turnBaseManager.GetCurrentTurnBattler();
    }

    // �I�[�g�o�g��
    public void OnClickAutoBattleButton()
    {
        isAutoBattling = !isAutoBattling;

        if (isAutoBattling)
        {
            autoBattleIcon.DOColor(CustomColor.autobattle(), 0.15f);
            autoBattleText.gameObject.SetActive(true);

            Time.timeScale = 2.0f;
            
            if (!GetCurrentBattler().isEnemy && actionPanel.IsEnabled)
            {
                actionPanel.SetEnablePanel(false);

                // �I�[�g�o�g��
                if (isPlayerAIRunning)
                {
                    isPlayerAIRunning = false;
                    StopCoroutine(playerAI);
                }
                playerAI = StartCoroutine(PlayerAI());
            }
        }
        else
        {
            autoBattleIcon.DOColor(Color.white, 0.15f);
            autoBattleText.gameObject.SetActive(false);

            Time.timeScale = 1.0f;

            if (isPlayerAIRunning)
            {
                isPlayerAIRunning = false;
                StopCoroutine(playerAI);
            }
        }
    }

    /// <summary>
    /// �L�����N�^�[�Ƀo�t��ǉ�
    /// </summary>
    public void AddBuffToBattler(Battler target, BuffType buff, int turn, int value)
    {
        if (IsCharacterInBuff(target, buff))
        {
            // ���ɂ��̃o�t�����Ă���
            var instance = buffedCharacters.FirstOrDefault(x => x.target == target && x.type == buff);

            // �I��������
            RemoveBuffInstance(instance);

            // �������l�̕����㏑������
            turn = Mathf.Max(turn, instance.remainingTurn);
            value = Mathf.Max(value, instance.value);
        }

        Buff _buff = new Buff();
        _buff.type = buff;
        _buff.data = BuffManager.BuffList[buff];
        _buff.target = target;
        _buff.remainingTurn = turn;
        _buff.value = value;

        _buff.data.start.Invoke(target, value);

        // �퓬���O���o�͂���
        if (_buff.data.battleLogStart != string.Empty)
        {
            string log = _buff.data.battleLogStart;
            // {0}�̓L�������A{1}�͐��l
            if (log.Contains("{0}")) log = log.Replace("{0}", target.CharacterNameColored);
            if (log.Contains("{1}")) log = log.Replace("{1}", _buff.value.ToString());
            // ���O�ǉ�
            AddBattleLog(log);
        }

        // Graphic
        // create icon
        _buff.graphic = new GameObject(_buff.data.name + "[" + turn.ToString() + "]");
        var frame = _buff.graphic.AddComponent<Image>();
        frame.sprite = _buff.data.icon;
        frame.raycastTarget = false;
        frame.rectTransform.SetParent(_buff.target.transform);
        frame.rectTransform.position =  GetPositionOfFirstBuff(_buff.target);
        frame.rectTransform.sizeDelta = new Vector2(25.0f, 25.0f);
        frame.color = new Color(1f, 1f, 1f, 0.0f);
        frame.DOFade(1.0f, buffIconFadeTime);
        
        var countingText = Instantiate(buffCounterText, frame.transform);
        _buff.text = countingText.GetComponent<TMP_Text>();
        _buff.text.text = turn.ToString();
        _buff.text.rectTransform.localPosition = new Vector2(0.0f, 17.0f);

        buffedCharacters.Add(_buff);

        ArrangeBuffIcon(target);
        characterInfoPanel.UpdateIcons(target);
    }

    /// <summary>
    /// �L�����̃o�t�������I��
    /// </summary>
    public bool RemoveBuffForCharacter(Battler target, BuffType buff)
    {
        if (IsCharacterInBuff(target, buff))
        {
            // ���ɂ��̃o�t�����Ă���
            var instance = buffedCharacters.FirstOrDefault(x => x.target == target && x.type == buff);

            // �I��������
            RemoveBuffInstance(instance);

            return true;
        }

        return false;
    }

    /// <summary>
    /// �L�����̃o�t��S�������I��
    /// </summary>
    public bool RemoveAllBuffForCharacter(Battler target)
    {
        for (int i = 0; i < (int)BuffType.max; i++)
        {
            BuffType buff = (BuffType)i;
            if (IsCharacterInBuff(target, buff))
            {
                // ���ɂ��̃o�t�����Ă���
                var instance = buffedCharacters.FirstOrDefault(x => x.target == target && x.type == buff);

                // �I��������
                RemoveBuffInstance(instance);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// �L�����ɂ������Ă���o�t���X�V
    /// </summary>
    public void UpdateBuffForCharacter(Battler target)
    {
        var buffList = GetAllBuffForSpecificBattler(target);
        for (int i = 0; i < buffList.Count; i++)
        {
            var buff = buffList[i];
            
            buff.remainingTurn--;
            buff.data.update.Invoke(buff.target, buff.value);
            buff.text.text = buff.remainingTurn.ToString();
            
            // �퓬���O���o�͂���
            if (buff.data.battleLogUpdate != string.Empty)
            {
                string log = buff.data.battleLogUpdate;
                // {0}�̓L�������A{1}�͐��l
                if (log.Contains("{0}")) log = log.Replace("{0}", target.CharacterNameColored);
                if (log.Contains("{1}")) log = log.Replace("{1}", buff.value.ToString());
                // ���O�ǉ�
                AddBattleLog(log);
            }

            if (buff.remainingTurn <= 0)
            {
                RemoveBuffInstance(buff);
            }
        }
        characterInfoPanel.UpdateIcons(target);
    }

    /// <summary>
    /// ����̃o�t������
    /// </summary>
    /// <param name="instance"></param>
    private void RemoveBuffInstance(Buff instance)
    {
        instance.data.end.Invoke(instance.target, instance.value);
        instance.graphic.GetComponent<Image>().DOFade(0.0f, buffIconFadeTime);
        instance.text.DOFade(0.0f, buffIconFadeTime);
        Destroy(instance.graphic, buffIconFadeTime + 0.1f);
        buffedCharacters.Remove(instance);
        ArrangeBuffIcon(instance.target);
        
        // �퓬���O���o�͂���
        if (instance.data.battleLogEnd != string.Empty)
        {
            string log = instance.data.battleLogEnd;
            // {0}�̓L�������A{1}�͐��l
            if (log.Contains("{0}")) log = log.Replace("{0}", instance.target.CharacterNameColored);
            if (log.Contains("{1}")) log = log.Replace("{1}", instance.value.ToString());
            // ���O�ǉ�
            AddBattleLog(log);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsCharacterInBuff(Battler battler, BuffType buffType)
    {
        List<Buff> list = GetAllBuffForSpecificBattler(battler);

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].type == buffType && list[i].target == battler)
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// �L�����N�^�[�������Ă���o�t��S���擾����
    /// </summary>
    public List<Buff> GetAllBuffForSpecificBattler(Battler battler)
    {
        // Use LINQ to get all elements that match the condition
        IEnumerable<Buff> rtn = buffedCharacters.Where(x => x.target == battler);

        return rtn.ToList();
    }

    /// <summary>
    /// �L�����������̃o�t�������Ă���ꍇ�o�t�̕\���ʒu���A�����W����
    /// </summary>
    public void ArrangeBuffIcon(Battler battler)
    {
        var buffs = GetAllBuffForSpecificBattler(battler);

        // Check if any matches are found
        if (buffs.Any())
        {
            // �X�^�[�g�ʒu
            Vector3 position = GetPositionOfFirstBuff(battler);
            Vector3 addition = BuffPositionAddition();

            foreach (Buff buff in buffs)
            {
                buff.graphic.GetComponent<RectTransform>().position = position;
                position += addition;
            }
        }
    }

    public Vector3 GetPositionOfFirstBuff(Battler battler)
    {
        return battler.GetMiddleGlobalPosition() + new Vector2(-battler.GetCharacterSize().x * 0.25f, battler.GetCharacterSize().y * 0.5f);
    }

    /// <summary>
    /// �o�t�̃A�����W�p�F�A�C�R���ʒu�̉��Z�l���擾
    /// </summary>
    public Vector3 BuffPositionAddition()
    {
        Vector3 addition = new Vector3(50.0f, 0.0f, 0.0f);

        return addition;
    }

    /// <summary>
    /// �s���p�l����\���E��\��
    /// </summary>
    public void SetDisplayActionPanel(bool enable)
    {
        actionPanel.SetEnablePanel(enable);
    }

    /// <summary>
    ///  �G��S���|����
    /// </summary>
    private bool IsVictory()
    {
        // �G�S�ł�
        Battler result = enemyList.Find(s => s.isAlive && s.isEnemy);
        if (result == null)
        {
            // �����҂��Ȃ�
            return true;
        }

        // �퓬������
        return false;
    }

    /// <summary>
    /// �����S�����^�C�A����
    /// </summary>
    private bool IsDefeat()
    {
        // �����S�ł�
        Battler result = characterList.Find(s => s.isAlive && !s.isEnemy);
        if (result == null)
        {
            // �����҂��Ȃ�
            return true;
        }

        // �퓬������
        return false;
    }

    private void BattleEnd(bool isVictory)
    {
        actionTargetArrow.gameObject.SetActive(false);
        characterArrow.gameObject.SetActive(false);
        actionPanel.SetEnablePanel(false);
        autoBattleButton.gameObject.SetActive(false);

        Time.timeScale = 1.0f;

        // �L�����̏�Ԃ��f�[�^�ɍX�V
        if (isVictory && !BattleSetup.isEventBattle) // story mode�ŕ��������̓��g���C����邩������Ȃ��̂ŁA�f�[�^�X�V���Ȃ�
        {
            foreach (Battler battler in characterList)
            {
                ProgressManager.Instance.UpdateCharacterByBattler(battler.characterID, battler);
            }
        }

        if (BattleSetup.isDLCBattle)
        {
            Debug.Log("current progress (dlc): " + ProgressManager.Instance.GetCurrentDLCStageProgress().ToString());
        }
        else
        {
            Debug.Log("current progress: " + ProgressManager.Instance.GetCurrentStageProgress().ToString());
        }

        // �����C�׃��g?
        bool isEvent = BattleSetup.isEventBattle; 
        if (!isEvent)
        {
            sceneTransition.EndScene(isVictory, ChangeScene);
        }
        else if (!BattleSetup.isDLCBattle)
        {
            // �`���[�g���A���I��
            if (ProgressManager.Instance.GetCurrentStageProgress() == 1)
            {
                // �s�k�C�x���g(0.5�b�҂��Ă���)
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { NovelSingletone.Instance.PlayNovel("Tutorial3", true, sceneTransition.EndTutorial); });
            }
            else if (ProgressManager.Instance.GetCurrentStageProgress() == 7)
            {
                AudioManager.Instance.StopMusicWithFade();

                // �s�k�C�x���g(�G���i��)
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { NovelSingletone.Instance.PlayNovel("Chapter2-3 AfterEvent", true, sceneTransition.EndScene); });
            }
            else if (ProgressManager.Instance.GetCurrentStageProgress() == 13)
            {
                AudioManager.Instance.StopMusicWithFade();

                // �s�k�C�x���g(�ߗR���o��)
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { NovelSingletone.Instance.PlayNovel("Chapter4-3 AfterEvent", true, sceneTransition.EndScene); });
            }
        }
        else if (BattleSetup.isDLCBattle)
        {
            // �`���[�g���A���I��
            if (ProgressManager.Instance.GetCurrentDLCStageProgress() == 8)
            {
                // �s�k�C�x���g(�X�g�[���[��������)
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { NovelSingletone.Instance.PlayNovel("DLC/DLC 2-2 AfterBattle", true, sceneTransition.EndScene); });

                // BGM�؂�ւ�
                AudioManager.Instance.StopMusicWithFade(2.0f);
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        // �V�[�����I������O�ɂ��ׂ�����
        EquipmentMethods.Finalize();

        // �}�E�X�J�[�\�����߂邱�Ƃ�ۏ؂���
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // �V�[�����[�h
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// �퓬���O��ǉ�
    /// </summary>
    public void AddBattleLog(string text)
    {
        battleLogScript.RegisterNewLog(text);
    }

    /// <summary>
    /// ���Ƀ^�[�����񂷃L�������w��L�����ɕύX
    /// </summary>
    public void ChangeBattlerTurnOrder(Battler battler)
    {
        turnBaseManager.SetNextCharacter(battler);
    }

#region Escape
    public void ShowEscapePopUp()
    {
        escapePopup.DOFade(1.0f, 0.25f);
        escapePopup.interactable = true;
        escapePopup.blocksRaycasts = true;

        AudioManager.Instance.PlaySFX("SystemAlert2");
    }
    
    public void CancelEscape()
    {
        escapePopup.DOFade(0.0f, 0.25f);
        escapePopup.interactable = false;
        escapePopup.blocksRaycasts = false;

        AudioManager.Instance.PlaySFX("SystemCancel");
    }

    public void ConfirmEscape()
    {
        AudioManager.Instance.PlaySFX("Escape");

        const float AnimTime = 1.0f;
        playerFormation.DOLocalMoveX(-formationPositionX * 2.1f, AnimTime);
        AudioManager.Instance.StopMusicWithFade(AnimTime);
        AlphaFadeManager.Instance.FadeOut(AnimTime);
        DOTween.Sequence().AppendInterval(AnimTime + Time.deltaTime).AppendCallback(() => 
        {
            ChangeScene("Home");
            AudioManager.Instance.PlaySFX("BattleTransition");
        });
    }
#endregion Escape
}
