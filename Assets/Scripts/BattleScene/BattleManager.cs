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
    [Header("Debug")]
    [SerializeField] private bool isDebug = false;

    [Header("Setting")]
    [SerializeField] private float characterSpace = 150.0f;
    [SerializeField, Range(1.1f, 2.0f)] private float turnEndDelay = 1.1f;
    [SerializeField, Range(1.1f, 2.0f)] private float stunWaitDelay = 0.55f;
    [SerializeField, Range(1.1f, 2.0f)] private float enemyAIDelay = 2.0f;
    [SerializeField, Range(0.1f, 1.0f)] private float characterMoveTime = 0.5f;  // < キャラがターゲットの前に移動する時間
    [SerializeField, Range(0.1f, 1.0f)] private float attackAnimPlayTime = 0.2f; // < 攻撃アニメーションの維持時間
    [SerializeField, Range(0.1f, 1.0f)] private float buffIconFadeTime = 0.4f; // < バフアイコンの出現・消し時間
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

    [Header("Debug")]
    [SerializeField] private List<Battler> characterList = new List<Battler>();
    [SerializeField] private List<Battler> enemyList = new List<Battler>();
    [SerializeField] private Battler arrowPointingTarget = null;
    [SerializeField] private List<Buff> buffedCharacters = new List<Buff>();

    private void Awake()
    {
        AlphaFadeManager.Instance.FadeIn(5.0f);

#if DEBUG_MODE
        if (isDebug) ProgressManager.Instance.DebugModeInitialize(true); // デバッグ用
#endif

        var actors = new List<Character>();
        if (BattleSetup.IsCustomFormation())
        {
            // 特殊イベント
            var formation = BattleSetup.GetCustomFormation();
            actors = formation;
        }
        else
        {
            // フォーメーションにいるキャラをそのまま流用
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

        // 撤退ボタン
        escapeButton.SetActive(BattleSetup.isAllowEscape);

        // Send references
        ItemExecute.Instance.Initialize(this);
        AbilityExecute.Instance.Initialize(this);
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

        // プレイヤーキャラ生成
        float positionX = (actors.Count * characterSpace) * 0.5f;
        float positionY_gap = totalGap / actors.Count;
        float positionY = max_positionY - positionY_gap;
        if (actors.Count == 1) positionY = -50; // 1人しかいない場合
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

        // 敵キャラ生成
        positionX = -(enemies.Count * characterSpace) * 0.5f;
        positionY_gap = totalGap / enemies.Count;
        positionY = max_positionY - positionY_gap;
        int siblingIndex = 0;
        if (enemies.Count == 1) positionY = -50; // 1人しかいない場合
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

        // 行動順を決める
        turnBaseManager.Initialization(characterList, enemyList);

        // 初期位置
        playerFormation.DOLocalMoveX(-formationPositionX * 2.1f, 0.0f, true);
        enemyFormation.DOLocalMoveX(formationPositionX * 2.1f, 0.0f, true);
        playerFormation.GetComponent<CanvasGroup>().alpha = 0.0f;
        enemyFormation.GetComponent<CanvasGroup>().alpha = 0.0f;
    }

    /// <summary>
    /// 次のターン
    /// </summary>
    public void NextTurn(bool isFirstTurn)
    {
        // 戦闘ログ　再起不能の告知
        for (int i = 0; i < turnBaseManager.DeathBattler.Count; i++)
        {
            AddBattleLog(System.String.Format(Assets.SimpleLocalization.Scripts.LocalizationManager.Localize("BattleLog.Retire"), turnBaseManager.DeathBattler[i].CharacterNameColored));
        }
        turnBaseManager.DeathBattler.Clear();

        // ターンを始める前に戦闘が終わっているかをチェック
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

        if (!isFirstTurn)
        {
            turnBaseManager.NextBattler();
        }
        else
        {
            // 最初のターン
            // キャラクターが位置に付く
            playerFormation.DOLocalMoveX(-formationPositionX, 0.5f).SetEase(Ease.OutQuart);
            enemyFormation.DOLocalMoveX(formationPositionX, 0.5f).SetEase(Ease.OutQuart);
            playerFormation.GetComponent<CanvasGroup>().DOFade(1.0f, 0.25f);
            enemyFormation.GetComponent<CanvasGroup>().DOFade(1.0f, 0.25f);

            // SE
            AudioManager.Instance.PlaySFX("FormationCharge", 0.5f);

            // チュートリアルに入る
            if (ProgressManager.Instance.GetCurrentStageProgress() == 1)
            {
                battleSceneTutorial.StartBattleTutorial();
            }
        }

        Battler currentTurnCharacter = turnBaseManager.GetCurrentTurnBattler();

        if (currentTurnCharacter.isEnemy)
        {
            actionPanel.SetEnablePanel(false);

            // AI 行動
            StartCoroutine(EnemyAI());
        }
        else
        {
            // 行動出来るまでに遅延させる
            StartCoroutine(TurnEndDelay());
        }
    }

    /// <summary>
    /// リタイアしたキャラクターが出たらターンから外す
    /// </summary>
    public void UpdateTurnBaseManager(bool rearrange)
    {
        turnBaseManager.UpdateTurn(rearrange);
    }

    // マウスが指しているところがBattlerが存在しているなら返す
    public Battler GetBattlerByPosition(Vector2 mousePosition, bool allowTeammate, bool allowEnemy, bool aliveOnly)
    {
        if (allowEnemy)
        {
            for (int i = 0; i < enemyList.Count; i++)
            {
                Vector2 size = enemyList[i].GetCharacterSize() * new Vector2(0.5f, 1.0f);
                Vector2 position = (enemyList[i].GetGraphicRectTransform().position / canvas.scaleFactor) + new Vector3(0.0f, size.y * 0.5f);
                if ((enemyList[i].isAlive || !aliveOnly)
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

    IEnumerator EnemyAI()
    {
        Battler currentCharacter = turnBaseManager.GetCurrentTurnBattler();
        characterArrow.SetCharacter(currentCharacter, currentCharacter.GetCharacterSize().y);

        yield return new WaitForSeconds(enemyAIDelay);

        // バフを先にチェック
        bool isCharacterStunned = IsCharacterInBuff(currentCharacter, BuffType.stun);
        UpdateBuffForCharacter(GetCurrentBattler());

        // 攻撃目標を選択
        // is character stunned
        if (isCharacterStunned)
        {
            yield return new WaitForSeconds(stunWaitDelay);
            NextTurn(false);
        }
        else
        {
            // どこ行動を取るかを決める
            var possibleAction = currentCharacter.GetAllPossibleAction();
            if (possibleAction.Count == 0)
            {
                // 取れる行動がない、待機する
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
                            targetCharacter = turnBaseManager.GetRandomPlayerCharacter();
                            StartCoroutine(AttackAnimation(currentCharacter, targetCharacter, NextTurn));
                        }
                        break;
                    case EnemyActionType.SpecialAbility:
                        {
                            if (action.ability.castType == CastType.Enemy)
                            {
                                if (action.ability.isAOE)
                                {
                                    // 全部選ぶ
                                    targetCharacters = turnBaseManager.GetAllPlayerCharacters();
                                }
                                else
                                {
                                    // プレイヤーキャラをランダムに選択する
                                    targetCharacter = turnBaseManager.GetRandomPlayerCharacter();
                                }
                            }
                            else if (action.ability.castType == CastType.Teammate)
                            {
                                if (action.ability.abilityType == AbilityType.Heal)
                                {
                                    // 治療タイプの技なら低いHPの仲間にする
                                    targetCharacter = turnBaseManager.GetEnemyCharacterWithLowestHP();
                                }
                                else
                                {
                                    targetCharacter = turnBaseManager.GetRandomEnemyCharacter();
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

    IEnumerator TurnEndDelay()
    {
        actionPanel.SetEnablePanel(false);
        yield return new WaitForSeconds(turnEndDelay);

        Battler currentCharacter = turnBaseManager.GetCurrentTurnBattler();
        characterArrow.SetCharacter(currentCharacter, currentCharacter.GetCharacterSize().y);

        var originPos = currentCharacter.GetGraphicRectTransform().position;
        var offset = currentCharacter.isEnemy ? new Vector3(-currentCharacter.GetCharacterSize().x * 0.25f, currentCharacter.GetCharacterSize().y * 0.5f) : new Vector3(currentCharacter.GetCharacterSize().x * 0.25f, currentCharacter.GetCharacterSize().y * 0.5f);
        originPos = originPos + offset;
        actionTargetArrow.position = originPos;

        // バフを先にチェック
        bool isCharacterStunned = IsCharacterInBuff(currentCharacter, BuffType.stun);
        UpdateBuffForCharacter(GetCurrentBattler());

        // is character stunned
        if (isCharacterStunned)
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

        if (target == currentBattler) return; // 自分に指すことはできない

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
            // 回復できるSPがない
            NextTurn(false);

            // SE再生
            AudioManager.Instance.PlaySFX("SystemActionPanel");

            // ログ ({0}　が待機する。！)
            AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Idle"), battler.CharacterNameColored));
            return;
        }

        // 総SPの15%~20%を回復する
        int healAmount = Mathf.RoundToInt((float)battler.max_mp * UnityEngine.Random.Range(0.15f, 0.2f));

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

                    // ログ ({0}　が休憩をとった。SP　{1}　回復した。)
                    AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.IdleSP"), battler.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.SP())));
                })
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    NextTurn(false);
                });
    }

    /// <summary>
    /// 攻撃用シーケンス
    /// </summary>
    IEnumerator AttackAnimation(Battler attacker, Battler target, Action<bool> callback)
    {
        Transform originalParent = attacker.transform.parent;
        int originalChildIndex = attacker.transform.GetSiblingIndex();

        var targetPos = target.GetComponent<RectTransform>().position;
        targetPos = target.isEnemy ? new Vector2(targetPos.x - target.GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + target.GetCharacterSize().x * 0.5f, targetPos.y);
        var originalPos = attacker.GetComponent<RectTransform>().position;
        attacker.GetComponent<RectTransform>().DOMove(targetPos, characterMoveTime);

        // ログ (xxx　の攻撃！)
        AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Attack"), attacker.CharacterNameColored));

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

        yield return new WaitForSeconds(characterMoveTime * 0.5f);
        // change character hirachy temporary
        attacker.transform.SetParent(target.transform);
        yield return new WaitForSeconds(characterMoveTime * 0.5f);

        attacker.SpawnAttackVFX(target);

        // attack miss?
        bool isMiss = (UnityEngine.Random.Range(0, 100) > CalculateHitChance(attacker.speed - target.speed));

        if (!isMiss)
        {
            // 攻撃計算
            int levelAdjustedDamage = CalculateDamage(attacker, target);
            int realDamage = target.DeductHP(levelAdjustedDamage);

            // play SE
            AudioManager.Instance.PlaySFX("Attacked", 0.8f);

            // animation
            target.Shake(attackAnimPlayTime + characterMoveTime);
            attacker.PlayAnimation(BattlerAnimationType.attack);
            target.PlayAnimation(BattlerAnimationType.attacked);

            // create floating text
            var floatingText = Instantiate(floatingTextOrigin, target.transform);
            floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - attacker.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

            // ログ ({0}　に　{1}　のダメージを与えた！)
            AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));
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

            // ログ ({0}　に避けられた！)
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

        callback?.Invoke(false);
    }

    // 攻撃力計算公式
    public static int CalculateDamage(Battler attacker, Battler target, bool randomizeDamage = true)
    {
        // randomize damage
        int multiplier = UnityEngine.Random.Range(0, 7);
        // calculate damage ([a.ATK \ *0.5F] - [b.DEF * 0.25F]) * Clamp.(a.LVL / b.LVL, 0.5F, 1.5F)
        return Mathf.RoundToInt(((float)attacker.attack * 0.5f) - ((float)target.defense * 0.25f) * Mathf.Clamp((float)attacker.currentLevel / (float)target.currentLevel, 0.5f, 1.5f)) + multiplier;
    }

    /// <summary>
    /// 攻撃の命中率を計算
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

    /// <summary>
    /// キャラクターにバフを追加
    /// </summary>
    public void AddBuffToBattler(Battler target, BuffType buff, int turn, int value)
    {
        if (IsCharacterInBuff(target, buff))
        {
            // 既にこのバフ持っている
            var instance = buffedCharacters.FirstOrDefault(x => x.target == target && x.type == buff);

            // 終了させる
            RemoveBuffInstance(instance);

            // 高い数値の方を上書きする
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

        // 戦闘ログ出力する
        if (_buff.data.battleLogStart != string.Empty)
        {
            string log = _buff.data.battleLogStart;
            // {0}はキャラ名、{1}は数値
            if (log.Contains("{0}")) log.Replace("{0}", target.CharacterNameColored);
            if (log.Contains("{1}")) log.Replace("{1}", _buff.value.ToString());
            // ログ追加
            AddBattleLog(log);
        }

        // Graphic
        // create icon
        _buff.graphic = new GameObject(_buff.data.name + "[" + turn.ToString() + "]");
        var frame = _buff.graphic.AddComponent<Image>();
        frame.sprite = buffIconFrame;
        frame.raycastTarget = false;
        frame.rectTransform.SetParent(_buff.target.transform);
        frame.rectTransform.position =  GetPositionOfFirstBuff(_buff.target);
        frame.rectTransform.sizeDelta = new Vector2(37.0f, 37.0f);
        frame.color = new Color(1f, 1f, 1f, 0.0f);
        frame.DOFade(1.0f, buffIconFadeTime);
        
        Image icon = new GameObject(_buff.data.icon.name).AddComponent<Image>();
        icon.sprite = _buff.data.icon;
        icon.raycastTarget = false;
        icon.rectTransform.SetParent(frame.transform);
        icon.rectTransform.localPosition = Vector2.zero;
        icon.rectTransform.sizeDelta = new Vector2(30.0f, 30.0f);
        icon.color = new Color(1f, 1f, 1f, 0.0f);
        icon.DOFade(1.0f, buffIconFadeTime);

        var countingText = Instantiate(buffCounterText, frame.transform);
        _buff.text = countingText.GetComponent<TMP_Text>();
        _buff.text.text = turn.ToString();
        _buff.text.rectTransform.localPosition = new Vector2(0.0f, 17.0f);

        buffedCharacters.Add(_buff);

        ArrangeBuffIcon(target);
        characterInfoPanel.UpdateIcons(target);
    }

    /// <summary>
    /// キャラにかけられているバフを更新
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
            
            // 戦闘ログ出力する
            if (buff.data.battleLogUpdate != string.Empty)
            {
                string log = buff.data.battleLogUpdate;
                // {0}はキャラ名、{1}は数値
                if (log.Contains("{0}")) log.Replace("{0}", target.CharacterNameColored);
                if (log.Contains("{1}")) log.Replace("{1}", buff.value.ToString());
                // ログ追加
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
    /// 特定のバフを消す
    /// </summary>
    /// <param name="instance"></param>
    private void RemoveBuffInstance(Buff instance)
    {
        instance.data.end.Invoke(instance.target, instance.value);
        instance.graphic.GetComponent<Image>().DOFade(0.0f, buffIconFadeTime);
        Destroy(instance.graphic, 0.5f);
        buffedCharacters.Remove(instance);
        ArrangeBuffIcon(instance.target);
        
        // 戦闘ログ出力する
        if (instance.data.battleLogEnd != string.Empty)
        {
            string log = instance.data.battleLogEnd;
            // {0}はキャラ名、{1}は数値
            if (log.Contains("{0}")) log.Replace("{0}", instance.target.CharacterNameColored);
            if (log.Contains("{1}")) log.Replace("{1}", instance.value.ToString());
            // ログ追加
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
    /// キャラクターが持っているバフを全部取得する
    /// </summary>
    public List<Buff> GetAllBuffForSpecificBattler(Battler battler)
    {
        // Use LINQ to get all elements that match the condition
        IEnumerable<Buff> rtn = buffedCharacters.Where(x => x.target == battler);

        return rtn.ToList();
    }

    /// <summary>
    /// キャラが複数のバフを持っている場合バフの表示位置をアレンジする
    /// </summary>
    public void ArrangeBuffIcon(Battler battler)
    {
        var buffs = GetAllBuffForSpecificBattler(battler);

        // Check if any matches are found
        if (buffs.Any())
        {
            // スタート位置
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
    /// バフのアレンジ用：アイコン位置の加算値を取得
    /// </summary>
    public Vector3 BuffPositionAddition()
    {
        Vector3 addition = new Vector3(50.0f, 0.0f, 0.0f);

        return addition;
    }

    /// <summary>
    ///  敵を全員倒せた
    /// </summary>
    private bool IsVictory()
    {
        // 敵全滅か
        Battler result = enemyList.Find(s => s.isAlive);
        if (result == null)
        {
            // 生存者いない
            return true;
        }

        // 戦闘が続く
        return false;
    }

    /// <summary>
    /// 味方全員リタイアした
    /// </summary>
    private bool IsDefeat()
    {
        // 味方全滅か
        Battler result = characterList.Find(s => s.isAlive);
        if (result == null)
        {
            // 生存者いない
            return true;
        }

        // 戦闘が続く
        return false;
    }

    private void BattleEnd(bool isVictory)
    {
        actionTargetArrow.gameObject.SetActive(false);
        characterArrow.gameObject.SetActive(false);
        actionPanel.SetEnablePanel(false);

        // キャラの状態をデータに更新
        if ((isVictory || !BattleSetup.isStoryMode) && !BattleSetup.isEventBattle) // story modeで負けた時はリトライされるかもしれないので、データ更新しない
        {
            foreach (Battler battler in characterList)
            {
                ProgressManager.Instance.UpdateCharacterByBattler(battler.characterID, battler);
            }
        }

        // 負けイべント?
        bool isEvent = BattleSetup.isEventBattle; 
        if (!isEvent)
        {
            sceneTransition.EndScene(isVictory, ChangeScene);
        }
        else
        {
            // チュートリアル終了
            if (ProgressManager.Instance.GetCurrentStageProgress() == 1)
            {
                // 敗北イベント(0.5秒待ってから)
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { NovelSingletone.Instance.PlayNovel("Tutorial3", true, sceneTransition.EndTutorial); });
            }
            else if (ProgressManager.Instance.GetCurrentStageProgress() == 7)
            {
                AudioManager.Instance.StopMusicWithFade();

                // 敗北イベント(那由多戦)
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { NovelSingletone.Instance.PlayNovel("Chapter2-3 AfterEvent", true, sceneTransition.EndScene); });
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 戦闘ログを追加
    /// </summary>
    public void AddBattleLog(string text)
    {
        battleLogScript.RegisterNewLog(text);
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
        // データ更新
        foreach (Battler battler in characterList)
        {
            ProgressManager.Instance.UpdateCharacterByBattler(battler.characterID, battler);
        }

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
