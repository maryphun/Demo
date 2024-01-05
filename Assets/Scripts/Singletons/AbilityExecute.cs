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
    /// �ً}�� (�퓬��)
    /// </summary>
    public void DeepBreath()
    {
        var target = battleManager.GetCurrentBattler();

        // �c��HP�����Ȃ��قǉ񕜗ʂ������Ȃ�
        float percentage = (1.0f - ((float)target.current_hp / (float)target.max_hp));
        int healAmount = (int)(((float)target.max_hp * 0.40f) * percentage);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.DeepBreath");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ���O ({0}�@��{1}����)
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

                    // �퓬���O
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.heal())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }
    /// <summary>
    /// �p���t���p���` (�퓬��)
    /// </summary>
    public void PowerfulPunch()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int dmg = (self.attack * 2);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.PowerfulPunch");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.PowerfulPunch"), CustomColor.abilityName())));

        // �L�����ړ��̏���
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

                    // �c�������R���|�l���g
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

                    // �퓬���O
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
    /// ���|�� (�퓬��)
    /// </summary>
    public void Tackle()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Tackle");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Tackle"), CustomColor.abilityName())));

        // �L�����ړ��̏���
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

                    // �c�������R���|�l���g
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
    /// �z���G�� (�G����l)
    /// </summary>
    public void SuckingTentacle()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int suckAmount = (int)((float)target.current_mp * 0.50f);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.SuckingTentacle");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@���G��� {1} �ɐL�΂��[�[�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Tentacle"), self.CharacterNameColored, target.CharacterNameColored));

        // �L�����ړ��̏���
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

                    // ���O ({1}�@��SP {0} �z�������I )
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

                        // ���O ({1}�@��HP {0} �z�������I )
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
    /// �G��T�[�r�X (�G����l)
    /// </summary>
    public void TentacleService()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];
        int spAmount = self.current_mp;

        Transform originalParent = self.RectTransform.parent;
        int siblingIndex = self.RectTransform.GetSiblingIndex();
        Vector3 originalPosition = self.RectTransform.position;

        // ���O ({0}�@���G��� {1} �ɐL�΂��[�[�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Tentacle"), self.CharacterNameColored, target.CharacterNameColored));
        
        // �L�����ړ�
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
                // �P��
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

                    // ���O (SP {0} �� {1} �ɕ����^�����I)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.GiveSP"), CustomColor.AddColor(spAmount, CustomColor.SP()), target.CharacterNameColored));
                }
                else
                {
                    // ���O (���ʂ͂Ȃ������B)
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

                // �߂�
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
    /// �Z��K�X (�G����l)
    /// </summary>
    public void Capture()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int attackDownAmount = (int)((float)target.attack * 0.25f);
        int speedDownAmount = (int)((float)target.speed * 0.25f);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Capture");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@���G��� {1} �ɐL�΂��[�[�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Tentacle"), self.CharacterNameColored, target.CharacterNameColored));

        // �L�����ړ��̏���
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
    /// ���ȏC��
    /// </summary>
    public void SelfRepair()
    {
        var target = battleManager.GetCurrentBattler();
        int healAmount = (int)((float)target.max_hp * 0.35f);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.SelfRepair");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ���O ({0}�@��{1}����)
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

                    // �퓬���O
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(healAmount, CustomColor.heal())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// ���ȋ���
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

        // �o�t��I��
        Tuple<BuffType, int> buff = possibleBuff[UnityEngine.Random.Range(0, possibleBuff.Count)];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.SelfEnchant");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ���O ({0}�@��{1}����)
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
    /// �u�[�X�g
    /// </summary>
    public void Booster()
    {
        var target = battleManager.GetCurrentBattler();
        int attackAddAmount = (int)((float)target.attack * 0.15f);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.Booster");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // ���O ({0}�@��{1}����)
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

                    // �퓬���O
                    battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.AttackUpStart"), target.CharacterNameColored, CustomColor.AddColor(attackAddAmount, CustomColor.damage())));
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// ��������Z
    /// </summary>
    public void HealAttack()
    {
        const int damage = 30;

        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.HealAttack");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

        // �G�t�F�N�g (Holy)
        var vfx = VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        vfx.transform.SetSiblingIndex(0); // �L�����̌��ɉ�
        self.PlayAnimation(BattlerAnimationType.magic);

        float originalLocalPosY = self.GetGraphicRectTransform().localPosition.y;
        self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY + 30.0f, 0.75f);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.HealAttack"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // �G�t�F�N�g (Light)
                    VFXSpawner.SpawnVFX("DarkBolt", target.transform, target.GetGraphicRectTransform().position);
                    
                    // text
                    floatingText = CreateFloatingText(target.transform);
                    floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), damage.ToString(), 64, CustomColor.damage());
                    
                    // effect
                    target.DeductHP(self, damage);

                    // �퓬���O
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
    /// ��������Z
    /// </summary>
    public void GreatRegen()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        float percentage = UnityEngine.Random.Range(0.05f, 0.15f);
        int hpHealAmount = Mathf.RoundToInt((float)target.max_hp * percentage);
        int spHealAmount = Mathf.RoundToInt((float)target.max_mp * 0.8f);
        
        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GreatRegen");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // �G�t�F�N�g (Holy)
        VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        self.PlayAnimation(BattlerAnimationType.magic);

        float originalLocalPosY = self.GetGraphicRectTransform().localPosition.y;
        self.GetGraphicRectTransform().DOLocalMoveY(originalLocalPosY + 30.0f, 0.75f);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.GreatRegen"), CustomColor.abilityName())));

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // animation
                    self.PlayAnimation(BattlerAnimationType.idle);

                    // �G�t�F�N�g (Light)
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

                    // �퓬���O
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
    /// �~���N�⋋
    /// </summary>
    public void Milk()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];
        int hpHealAmount = UnityEngine.Random.Range(20, 31);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Milk");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Milk"), CustomColor.abilityName())));


        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.25f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);
        var endPoint = target.GetMiddleGlobalPosition();

        self.PlayAnimation(BattlerAnimationType.magic);
        self.ColorTint(Color.grey, 0.15f);

        // SE
        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);

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

               // �퓬���O
               battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.HealHP"), target.CharacterNameColored, CustomColor.AddColor(hpHealAmount, CustomColor.heal())));
           })
           .AppendInterval(0.5f)
           .AppendCallback(() =>
           {
               battleManager.NextTurn(false);
           });
    }

    /// <summary>
    /// ���Z�~���N
    /// </summary>
    public void MilkCookie()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.MilkCookie");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.MilkCookie"), CustomColor.abilityName())));


        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.25f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);
        var endPoint = target.GetMiddleGlobalPosition();

        self.PlayAnimation(BattlerAnimationType.magic);
        self.ColorTint(Color.grey, 0.15f);

        // SE
        AudioManager.Instance.PlaySFX("Sperm9", 0.8f);

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

               // �퓬���O
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
    /// �~���N�⋋�I�[�o�[�h���C�u
    /// </summary>
    public void MilkOverdrive()
    {
        var self = battleManager.GetCurrentBattler();
        targetBattlers = battleManager.GetAllTeammate();
        targetBattlers.Remove(self);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.MilkOverdrive");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.MilkOverdrive"), CustomColor.abilityName())));


        float projectileTime = 1.0f;
        var rectTransform = self.GetComponent<RectTransform>();
        var originalPos = rectTransform.position;
        var selfPos = self.GetMiddleGlobalPosition();
        var startPoint = self.isEnemy ? new Vector2(selfPos.x - self.GetCharacterSize().x * 0.25f, selfPos.y) : new Vector2(selfPos.x + self.GetCharacterSize().x * 0.5f, selfPos.y);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);

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

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint, endPoint, projectileTime, true);
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

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint, endPoint, projectileTime, true);
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

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint, endPoint, projectileTime, true);
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

                        // calculate projectile time base on range
                        CreateProjectile("Milk Projectile", startPoint, endPoint, projectileTime, true);
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

                    // ���O ({0}�@����� {1} �I)
                    battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.TeamHeal"));

                    rectTransform.DOMove(originalPos, 0.5f);

                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// ���Ԑ����Z  / 005�� / 072��
    /// </summary>
    public void QuickAttack()
    {
        const float StrikeTime = 0.2f; // �^�񒆂���G���U������ړ�����
        const float AttackStopTime = 0.25f; // �U�����鎞�̒�~����
        const float ReturnTime = 0.2f; // �U������߂鎞��

        var self = battleManager.GetCurrentBattler();

        const int numOfTarget = 3;
        var targets = new Battler[numOfTarget];
        targets[0] = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];
        targets[1] = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];
        targets[2] = targetBattlers[UnityEngine.Random.Range(0, targetBattlers.Count)];

        // ���f�[�^���L�^
        Transform originalParent = self.transform.parent;
        int originalChildIndex = self.transform.GetSiblingIndex();
        var originalPos = self.GetComponent<RectTransform>().position;

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.QuickAttack");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.1f);

        // �L������^�񒆂Ɉړ�����
        Vector3 middle = new Vector3(CanvasReferencer.Instance.GetResolution().x * 0.5f, CanvasReferencer.Instance.GetResolution().y * 0.5f, 0.0f);
        self.GetComponent<RectTransform>().DOMove(middle + new Vector3(0.0f, -140.0f, 0.0f), 0.25f);

        // �c�������R���|�l���g
        FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(4.0f, 0.05f, self.GetGraphicRectTransform().GetComponent<Image>());

        // ���O ({0}�@����� {1} �I)
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
                    int realDamage = targets[0].DeductHP(self, Battle.CalculateDamage(self, targets[0]));

                    // text
                    floatingText = CreateFloatingText(targets[0].transform);
                    floatingText.Init(2.0f, targets[0].GetMiddleGlobalPosition(), (targets[0].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // �퓬���O
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

                    // �퓬���O
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

                    // �퓬���O
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
    /// �I�[�o�[�h���C�u   / 005�� / 072��
    /// </summary>
    public void Overdrive()
    {
        var self = battleManager.GetCurrentBattler();

        const int ComboTurn = 3;
        const int StunTurn = 3;

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Overdrive");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�I {1} �����ꂩ��{2}��A���s������B)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Overdrive"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Overdrive"), CustomColor.abilityName()), self.CharacterNameColored, ComboTurn));

        battleManager.AddBuffToBattler(self, BuffType.continuous_action, ComboTurn, 0);
        var overdrive = self.gameObject.AddComponent<AbilityOverdrive>();
        overdrive.Init(battleManager, self, StunTurn);

        // play SE
        AudioManager.Instance.PlaySFX("Heartbeat", 1.5f);

        // �c�����쐬
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

                    // ���O (�܂�{0}�̃^�[���I)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.ContinueTurn"), self.CharacterNameColored));
                });
    }

    /// <summary>
    /// �����˂�   / 005�� / 072��
    /// </summary>
    public void Fist()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Fist");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
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

                    // �U���v�Z
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

                    // ���O ({0}�@�Ɂ@{1}�@�̃_���[�W��^�����I)
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

                    // ���O (�܂�{0}�̃^�[���I)
                    battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.ContinueTurn"), self.CharacterNameColored));
                });
    }

    /// <summary>
    /// �V���b�N�E�F�[�u�E�p���T�[ �������邱�ƂŊl���������Z
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

        // ���O ({0}�@���� {1} ����I)
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

    /// �^���N�̃f�t�H���g�U��
    public void TankAttack()
    {
        var self = battleManager.GetCurrentBattler();
        var selfRect = self.GetComponent<RectTransform>();
        var target = targetBattlers[0];

        const float ChargeTime = 0.75f;
        self.Shake(ChargeTime);
        AudioManager.Instance.PlaySFX("TankStandby");

        // ���O (xxx�@�̃r�[���U���I)
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

                // ���O ({0}�@�Ɂ@{1}�@�̃_���[�W��^�����I)
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
    /// ���Ȃ鏂�@�G���i�{�X��
    /// </summary>
    public void DivineShield()
    {
        var self = battleManager.GetCurrentBattler();

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.DivineShield");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@�́@{1}�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.DivineShield"), CustomColor.abilityName())));

        // �A�j���V����
        self.PlayAnimation(BattlerAnimationType.magic);

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);

        const string ObjName = "ErenaShield";


        try
        {
            // �O�̃V�[���h�����݂��Ă�����u������
            var obj = self.transform.Find(ObjName);
            if (obj != null)
            {
                obj.GetComponent<ErenaShield>().Destroy();
            }

            // �Ƃ��Ƀ`���[�W������
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

               // ���O (���̍U�������S�h��I)
               battleManager.AddBattleLog(LocalizationManager.Localize("BattleLog.DivineShield"));

               // �A�j���V����
               self.PlayAnimation(BattlerAnimationType.idle);

               battleManager.NextTurn(false);
           }
        );
    }

    /// <summary>
    /// ���Ȃ錕�@�G���i�{�X��
    /// </summary>
    public void DivineRapier()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.DivineRapier");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@�́@{1}�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.DivineRapier"), CustomColor.abilityName())));

        // �A�j���V����
        self.PlayAnimation(BattlerAnimationType.magic);

        // �c�����쐬
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

               // �c�������R���|�l���g
               FadeEffect fadeEffect = projectile.gameObject.AddComponent<FadeEffect>();
               fadeEffect.Initialize(projectileTime, 0.05f, projectile.GetComponent<Image>());

               // SE
               AudioManager.Instance.PlaySFX("Throw", 0.75f);

               // ��]
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

               // ���O �i{0}�@�̖h��͂� {1} �_�������B�j
               battleManager.AddBattleLog(string.Format(LocalizationManager.Localize("BattleLog.ShieldDownValue"), target.CharacterNameColored, CustomColor.AddColor(defAmt, CustomColor.SP())));

               // deal damage
               int realDamage = target.DeductHP(self, Battle.CalculateDamage(self, target) * 2);

               // �퓬���O
               battleManager.AddBattleLog(System.String.Format(LocalizationManager.Localize("BattleLog.Damage"), target.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

               // text
               floatingText = CreateFloatingText(target.transform);
               floatingText.Init(2.0f, target.GetMiddleGlobalPosition(), (target.GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

               // play SE
               AudioManager.Instance.PlaySFX("Holy", 0.7f);

               // ��]
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
    /// �X�^���V�[���h�@�G���i�{�X��
    /// </summary>
    public void StunShield()
    {
        var self = battleManager.GetCurrentBattler();

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.StunShield");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@�́@{1}�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.StunShield"), CustomColor.abilityName())));

        // �A�j���V����
        self.PlayAnimation(BattlerAnimationType.magic);

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);

        const string ObjName = "ErenaShield";


        try
        {
            // �O�̃V�[���h�����݂��Ă�����u������
            var obj = self.transform.Find(ObjName);
            if (obj != null)
            {
                obj.GetComponent<ErenaShield>().Destroy();
            }

            // �Ƃ��Ƀ`���[�W������
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

               // ���O (���̍U�������S�h��I)
               battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.StunShield"), self.CharacterNameColored));

               // �A�j���V����
               self.PlayAnimation(BattlerAnimationType.idle);

               battleManager.NextTurn(false);
           }
        );
    }

    /// <summary>
    /// �����̈ӎv�@�G���i�Z
    /// </summary>
    public void Repel()
    {
        var self = battleManager.GetCurrentBattler();

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Repel");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@�́@{1}�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Repel"), CustomColor.abilityName())));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);

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
    /// �s���̃I�[���@�G���i�Z
    /// </summary>
    public void GuardUp()
    {
        var self = battleManager.GetCurrentBattler();

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GuardUp");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@�́@{1}�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.GuardUp"), CustomColor.abilityName())));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);

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
    /// �s�ł̉���@�G���i�Z
    /// </summary>
    public void GuardianAngel()
    {
        var self = battleManager.GetCurrentBattler();
        var targets = battleManager.GetAllTeammate();

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GuardianAngel");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@�́@{1}�I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Usage"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.GuardianAngel"), CustomColor.abilityName())));

        // SE
        AudioManager.Instance.PlaySFX("Magic2", 1f);

        self.PlayAnimation(BattlerAnimationType.magic);

        const string ObjName = "ErenaShield";
        for (int i = 0; i < targets.Count; i++)
        {
            try
            {
                // �O�̃V�[���h�����݂��Ă�����u������
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
    /// �ŗ������̃n�b�L���O����Z
    /// </summary>
    public void Hacking()
    {
        // ���̕�����擾
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();

        // ����̎������擾
        var target = targetBattlers[0];

        // ���������v�Z
        float successRate = target.currentLevel >= kei.currentLevel ? 0.0f : 0.40f + (0.064f * (kei.currentLevel - target.currentLevel));

        if (!target.IsMachine) successRate = 0.0f; // �@�B�ވȊO�͐������@0%�@
        if (target.character_name == LocalizationManager.Localize("Name.Tank")) successRate = 0.01f; // ����̓G�͐������Ȃ�

        successRate = UnityEngine.Mathf.Clamp(successRate, 0.0f, 1.0f);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.Hacking");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ���O ({0}�@���n�b�L���O�𒧂ށI�����m���� {1})
        var successRatePercentage = String.Format("{0:0.##\\%}", (successRate * 100.0f)); 
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking"), kei.CharacterNameColored, CustomColor.AddColor(successRatePercentage, Color.cyan)));

        // ������
        bool isSuccess = UnityEngine.Random.Range(0.0f, 1.0f) < successRate;

        // �������s
        if (target.gameObject.name == "Tank_Enemy") isSuccess = false;

        // ����̓�������U�~�߂�
        weapons.leftWeapon.SetEnableMovement(false);
        weapons.rightWeapon.SetEnableMovement(false);

        weapons.leftWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);
        weapons.rightWeapon.Rect.DOMove(target.GetMiddleGlobalPosition(), 1f).SetEase(Ease.Linear);

        // �A�j���V����
        const float animtionTime = 1.0f;
        kei.PlayAnimation(BattlerAnimationType.magic);

        // �c��
        FadeEffect fadeEffect = weapons.leftWeapon.Rect.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(animtionTime, 0.05f, weapons.leftWeapon.GetComponent<Image>());
        fadeEffect = weapons.rightWeapon.Rect.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(animtionTime, 0.05f, weapons.rightWeapon.GetComponent<Image>());

        // SE
        AudioManager.Instance.PlaySFX("Machine");

        if (isSuccess)
        {
            // ���̓���Z���폜
            kei.SetAbilityActive("Hacking", false);
            kei.SetAbilityActive("SuicideAttack", true);
            kei.SetAbilityActive("Reprogram", true);
            kei.SetAbilityActive("EffeciencyBoost", true);

            // �����@�\�ǉ�
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

                        // �A�j���V����
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
                    .AppendInterval(animtionTime * 0.5f) // �G���ړ���
                    .AppendCallback(() =>
                    {
                        // SE
                        AudioManager.Instance.PlaySFX("Machine", 0.5f);

                        // ���̏��ɖ߂�
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

                        // �A�j���V����
                        kei.PlayAnimation(BattlerAnimationType.idle);
                    })
                    .AppendInterval((animtionTime * 0.5f) + 0.25f)
                    .AppendCallback(() =>
                    {
                        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Hacking_Fail"), target.CharacterNameColored));

                        // SE
                        AudioManager.Instance.PlaySFX("Machine", 0.5f);
                        AudioManager.Instance.PlaySFX("Miss");

                        // ���̏��ɖ߂�
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
    /// �����w�� �P�C
    /// </summary>
    public void SuicideAttack()
    {
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();
        var target = targetBattlers[0];
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // ���S���擾

        // ���̓���Z���폜
        kei.SetAbilityActive("SuicideAttack", false);
        kei.SetAbilityActive("Reprogram", false);
        kei.SetAbilityActive("EffeciencyBoost", false);

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.SuicideAttack");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), kei.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.SuicideAttack"), CustomColor.abilityName())));

        const float chargeTime = 0.5f;

        // �c�������R���|�l���g
        FadeEffect fadeEffect = puppet.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(chargeTime, 0.05f, puppet.Graphic);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
        AudioManager.Instance.PlaySFX("Machine", 0.5f);

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

                // ���O ({0}�@����������I{1} �̃_���[�W��^�����I)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Suicide"), puppet.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // ����
                puppet.DeductHP(puppet, 99999, true);
            })
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// �n�b�L���O�����L�����������ł���Z
    /// </summary>
    public void Suicide()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.Suicide");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // ���O ({0}�@����� {1} �I)
        battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.AbilityExecute"), self.CharacterNameColored,
                                                 CustomColor.AddColor(LocalizationManager.Localize("Ability.Suicide"), CustomColor.abilityName())));

        const float chargeTime = 0.5f;

        // �c�������R���|�l���g
        FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(chargeTime, 0.05f, self.Graphic);

        // play SE
        AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);
        AudioManager.Instance.PlaySFX("Machine", 0.5f);

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

                // ���O ({0}�@����������I{1} �̃_���[�W��^�����I)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Suicide"), self.CharacterNameColored, CustomColor.AddColor(realDamage, CustomColor.damage())));

                // ����
                self.DeductHP(self, 99999, true);
            })
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// �� ���t�@�N�^�����O
    /// </summary>
    public void Reprogram()
    {
        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // ���S���擾

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.Reprogram");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ���O ({0}�@����� {1} �I)
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

                // ���O ({0}�@�̍U���͂�{1}�_�����B�h���{2}�_�����B)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.Reprogram"), puppet.CharacterNameColored, 
                    CustomColor.AddColor(attackAmt, CustomColor.damage()), CustomColor.AddColor(defenseAmt, CustomColor.SP())));

                battleManager.NextTurn(false);
            });
    }

    /// <summary>
    /// �� �V�X�e���C��
    /// </summary>
    public void EffeciencyBoost()
    {

        var kei = battleManager.GetCurrentBattler();
        var weapons = kei.GetComponent<KeiWeaponController>();
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // ���S���擾

        // �Z����\��
        var floatingText = CreateFloatingAbilityText(kei.transform);
        string abilityName = LocalizationManager.Localize("Ability.EffeciencyBoost");
        floatingText.Init(2.0f, kei.GetMiddleGlobalPosition() + new Vector2(0.0f, kei.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, kei.character_color);

        // ���O ({0}�@����� {1} �I)
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

                // ���O ({0}�@�� HP�� {1} �񕜂����I)
                battleManager.AddBattleLog(String.Format(LocalizationManager.Localize("BattleLog.HealHP"), puppet.CharacterNameColored, CustomColor.AddColor(healAmt, CustomColor.heal())));



                battleManager.NextTurn(false);
            });
    }

    /// ���̃f�t�H���g�U��
    public void KeiAttack()
    {
        var self = battleManager.GetCurrentBattler();
        var weapons = self.GetComponent<KeiWeaponController>();
        bool left = UnityEngine.Random.Range(0, 10) >= 5;
        var weapon = left ? weapons.leftWeapon : weapons.rightWeapon;
        var originalPosition = weapon.transform.position;
        int originalSiblingIndex = weapon.transform.GetSiblingIndex();
        var target = targetBattlers[0];
        var puppet = weapons.ControlledUnit.GetComponent<Battler>(); // ���S���擾
        
        // ����̓�������U�~�߂�
        if (left)
        {
            weapons.leftWeapon.SetEnableMovement(false);
        }
        else
        {
            weapons.rightWeapon.SetEnableMovement(false);
        }
        self.PlayAnimation(BattlerAnimationType.attack);


        const float ChargeTime = 0.25f;
        const float StayTime = 0.25f;
        const float ReturnTime = 0.5f;

        // �c�������R���|�l���g
        FadeEffect fadeEffect = weapon.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(ChargeTime, 0.02f, weapon.GetComponent<Image>());

        // ���S���\��
        if (puppet != null)
        {
            puppet.SetTransparent(0.1f, ChargeTime * 0.5f);
        }

        // ���O (xxx�@�̍U���I)
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

                // ���O ({0}�@�Ɂ@{1}�@�̃_���[�W��^�����I)
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
                    weapon.transform.DOLocalMove(weapons.LeftWeaponLocalPosition, ReturnTime * 0.5f).SetEase(Ease.Linear);
                }
                else
                {
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
    #endregion abilities
}
