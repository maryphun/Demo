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

    private FloatingText CreateFloatingText(Transform parent)
    {
        GameObject origin = Resources.Load<GameObject>("Prefabs/FloatingNumber");
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
    /// 深呼吸 (戦闘員)
    /// </summary>
    public void DeepBreath()
    {
        var target = battleManager.GetCurrentBattler();
        int healAmount = (int)((float)target.max_hp * 0.20f);

        // 技名を表示
        var floatingText = CreateFloatingText(target.transform);
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

        int dmg = (self.attack * 2);

        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
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
                    // attack
                    self.PlayAnimation(BattlerAnimationType.attacked);
                    self.SpawnAttackVFX(target);
                    AudioManager.Instance.PlaySFX("PowerfulPunch", 1f);
                    target.Shake(0.75f);
                    target.DeductHP(dmg);

                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), dmg.ToString(), 64, CustomColor.damage());

                    // 戦闘ログ
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(dmg, CustomColor.damage())));
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
    /// 全力タックル (戦闘員)
    /// </summary>
    public void Tackle()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
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
        var floatingText = CreateFloatingText(self.transform);
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

                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, self.GetMiddleGlobalPosition(), new Vector2(0.0f, 100.0f), "+" + suckAmount.ToString(), 64, CustomColor.SP());

                    // ログ ({1}　のSP {0} 吸収した！ )
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.SPDrain"), target.CharacterNameColored, CustomColor.AddColor(suckAmount, CustomColor.SP())));
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
    /// 触手サービス (触手怪人)
    /// </summary>
    public void TentacleService()
    {

    }

    /// <summary>
    /// 丸呑み (触手怪人)
    /// </summary>
    public void Capture()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int attackDownAmount = (int)((float)target.attack * 0.25f);
        int speedDownAmount = (int)((float)target.speed * 0.25f);

        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
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

    public void SelfRepair()
    {
        var target = battleManager.GetCurrentBattler();
        int healAmount = (int)((float)target.max_hp * 0.25f);

        // 技名を表示
        var floatingText = CreateFloatingText(target.transform);
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
    /// 明穂戦特殊技
    /// </summary>
    public void HealAttack()
    {
        const int damage = 30;

        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
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
                    target.DeductHP(damage);
                    
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
        var floatingText = CreateFloatingText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GreatRegen");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // エフェクト (Holy)
        VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        self.PlayAnimation(BattlerAnimationType.magic);

        float originalLocalPosY = self.GetGraphicRectTransform().localPosition.y;
        self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY + 30.0f, 0.75f);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);

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
    /// 立花戦特殊技
    /// </summary>
    public void QuickAttack()
    {
        const float StrikeTime = 0.2f; // 真ん中から敵を攻撃する移動時間
        const float AttackStopTime = 0.25f; // 攻撃する時の停止時間
        const float ReturnTime = 0.2f; // 攻撃から戻る時間

        var self = battleManager.GetCurrentBattler();

        const int numOfTarget = 3;
        var targets = new Battler[numOfTarget];
        targets[0] = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];
        targets[1] = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];
        targets[2] = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];

        // 元データを記録
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();
        var originalPos = self.GetComponent<RectTransform>().position;

        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
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
                    int realDamage = targets[0].DeductHP(Battle.CalculateDamage(self, targets[0]));

                    // text
                    floatingText = CreateFloatingText(targets[0].transform);
                    floatingText.Init(2.0f, targets[0].GetMiddleGlobalPosition(), (targets[0].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

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
                    int realDamage = targets[1].DeductHP(Battle.CalculateDamage(self, targets[1]));

                    // text
                    floatingText = CreateFloatingText(targets[1].transform);
                    floatingText.Init(2.0f, targets[1].GetMiddleGlobalPosition(), (targets[1].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

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
                    int realDamage = targets[2].DeductHP(Battle.CalculateDamage(self, targets[2]));

                    // text
                    floatingText = CreateFloatingText(targets[2].transform);
                    floatingText.Init(2.0f, targets[2].GetMiddleGlobalPosition(), (targets[2].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

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

    // タンクのデフォルト攻撃
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
                int realDamage = target.DeductHP(Battle.CalculateDamage(self, target));

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
        float successRate = target.currentLevel >= kei.currentLevel ? 0.0f : 0.07f * (kei.currentLevel - target.currentLevel);

        if (!target.IsMachine) successRate = 0.0f; // 機械類以外は成功率　0%　
        if (target.gameObject.name == "Tank_Enemy") successRate = 0.01f; // 特定の敵は成功しない

        successRate = UnityEngine.Mathf.Clamp(successRate, 0.0f, 1.0f);

        // 技名を表示
        var floatingText = CreateFloatingText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.Hacking");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ログ ({0}　がハッキングを挑む！成功確率は {1})
        var successRatePercentage = String.Format("{0:0.##\\%}", successRate); 
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking"), kei.CharacterNameColored, CustomColor.AddColor(successRatePercentage, Color.cyan)));

        // 成功か
        bool isSuccess = UnityEngine.Random.Range(0.0f, 1.0f) < successRate;

        // 強制失敗
        if (target.gameObject.name == "Tank_Enemy") isSuccess = false;

        // 武器の動きを一旦止める
        weapons.leftWeapon.SetEnableMovement(false);
        weapons.rightWeapon.SetEnableMovement(false);

        weapons.leftWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
        weapons.rightWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);

        // アニメション
        const float animtionTime = 1.0f;
        kei.PlayAnimation(BattlerAnimationType.magic);

        if (isSuccess)
        {
            // 京を攻撃できなくなる
            kei.isTargettable = false;

            // この特殊技を削除
            kei.RemoveAbilityFromCharacter("Hacking");

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

                        // アニメション
                        kei.PlayAnimation(BattlerAnimationType.idle);
                    })
                    .AppendInterval((animtionTime * 0.5f) + 0.25f)
                    .AppendCallback(() =>
                    {
                        battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.Hacking_Success"));
                        
                        var puppet = target.gameObject.AddComponent<KeiControlledUnit>();
                        puppet.StartControl(kei, 0);
                    })
                    .AppendInterval(animtionTime * 0.5f) // 敵を移動中
                    .AppendCallback(() =>
                    {
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
                        
                        // アニメション
                        kei.PlayAnimation(BattlerAnimationType.idle);
                    })
                    .AppendInterval((animtionTime * 0.5f) + 0.25f)
                    .AppendCallback(() =>
                    {
                        battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.Hacking_Fail"));

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
    #endregion abilities
}
