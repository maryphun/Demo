using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;
using System.Linq;

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
    #endregion common methods

    #region abilities
    public void DeepBreath()
    {
        var target = battleManager.GetCurrentBattler();
        int healAmount = (int)((float)target.max_hp * 0.20f);

        // 技名を表示
        var floatingText = CreateFloatingText(target.transform);
        string abilityName = LocalizationManager.Localize("Ability.DeepBreath");
        floatingText.Init(2.0f, target.GetMiddleGlobalPosition() + new Vector2(0.0f, target.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, target.character_color);

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
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    battleManager.NextTurn(false);
                });
    }
    public void SuckingTentacle()
    {
        var self = battleManager.GetCurrentBattler();
        var target = targetBattlers[0];

        int suckAmount = (int)((float)target.current_mp * 0.50f);

        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.SuckingTentacle");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

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
        VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        self.PlayAnimation(BattlerAnimationType.magic);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);

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

        float percentage = Random.Range(0.05f, 0.15f);
        int hpHealAmount = Mathf.RoundToInt((float)target.max_hp * percentage);
        int spHealAmount = Mathf.RoundToInt((float)target.max_mp * 0.8f);
        
        // 技名を表示
        var floatingText = CreateFloatingText(self.transform);
        string abilityName = LocalizationManager.Localize("Ability.GreatRegen");
        floatingText.Init(2.0f, self.GetMiddleGlobalPosition() + new Vector2(0.0f, self.GetCharacterSize().y * 0.25f), new Vector2(0.0f, 100.0f), abilityName, 40, self.character_color);

        // エフェクト (Holy)
        VFXSpawner.SpawnVFX("Holy", self.transform, self.GetGraphicRectTransform().position);
        self.PlayAnimation(BattlerAnimationType.magic);

        // Audio
        AudioManager.Instance.PlaySFX("MagicCharge", 0.5f);

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
                        move += Vector2.MoveTowards(move, move + new Vector2(Random.Range(-100, 100), Random.Range(-100, 100)), Random.Range(0.0f, 300.0f));
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
                    battleManager.NextTurn(false);
                });
    }

    /// <summary>
    /// 立花戦特殊技
    /// </summary>
    public void QuickAttack()
    {
        var self = battleManager.GetCurrentBattler();

        const int numOfTarget = 3;
        var targets = new Battler[numOfTarget];
        targets[0] = targetBattlers[Random.Range(0, targetBattlers.Count)];
        targets[1] = targetBattlers[Random.Range(0, targetBattlers.Count)];
        targets[2] = targetBattlers[Random.Range(0, targetBattlers.Count)];

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
        self.GetComponent<RectTransform>().DOAnchorPos(new Vector3(0.0f, -140.0f, 0.0f), 0.25f);

        // 残像生成コンポネント
        FadeEffect fadeEffect = self.gameObject.AddComponent<FadeEffect>();
        fadeEffect.Initialize(4.0f, 0.1f, self.GetGraphicRectTransform().GetComponent<Image>());

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
                    self.GetComponent<RectTransform>().DOMove(targetPos, 0.4f);
                })
                .AppendInterval(0.4f)
                .AppendCallback(() =>
                {
                    // deal damage
                    int realDamage = targets[0].DeductHP(Battle.CalculateDamage(self, targets[0]));

                    // text
                    floatingText = CreateFloatingText(targets[0].transform);
                    floatingText.Init(2.0f, targets[0].GetMiddleGlobalPosition(), (targets[0].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.5f);

                    // animation
                    targets[0].Shake(0.75f);
                    self.PlayAnimation(BattlerAnimationType.attack);
                    targets[0].PlayAnimation(BattlerAnimationType.attacked);
                })
                .AppendInterval(0.1f)
                .AppendCallback(() =>
                {
                    // return to middle
                    self.GetComponent<RectTransform>().DOAnchorPos(new Vector3(0.0f, -140.0f, 0.0f), 0.5f);

                    // cancel animation
                    self.PlayAnimation(BattlerAnimationType.idle);
                    targets[0].PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);

                    // move to second target
                    self.transform.SetParent(targets[1].transform);
                    var targetPos = targets[1].GetComponent<RectTransform>().position;
                    targetPos = targets[1].isEnemy ? new Vector2(targetPos.x - targets[1].GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + targets[1].GetCharacterSize().x * 0.5f, targetPos.y);
                    self.GetComponent<RectTransform>().DOMove(targetPos, 0.4f);
                })
                .AppendInterval(0.4f)
                .AppendCallback(() =>
                {
                    // deal damage
                    int realDamage = targets[1].DeductHP(Battle.CalculateDamage(self, targets[1]));

                    // text
                    floatingText = CreateFloatingText(targets[1].transform);
                    floatingText.Init(2.0f, targets[1].GetMiddleGlobalPosition(), (targets[1].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.5f);

                    // animation
                    targets[1].Shake(0.75f);
                    self.PlayAnimation(BattlerAnimationType.attack);
                    targets[1].PlayAnimation(BattlerAnimationType.attacked);
                })
                .AppendInterval(0.1f)
                .AppendCallback(() =>
                {
                    // return to middle
                    self.GetComponent<RectTransform>().DOAnchorPos(new Vector3(0.0f, -140.0f, 0.0f), 0.5f);

                    // cancel animation
                    self.PlayAnimation(BattlerAnimationType.idle);
                    targets[1].PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    // play SE
                    AudioManager.Instance.PlaySFX("CharacterMove", 0.5f);

                    // move to third target
                    self.transform.SetParent(targets[3].transform);
                    var targetPos = targets[2].GetComponent<RectTransform>().position;
                    targetPos = targets[2].isEnemy ? new Vector2(targetPos.x - targets[2].GetCharacterSize().x * 0.5f, targetPos.y) : new Vector2(targetPos.x + targets[2].GetCharacterSize().x * 0.5f, targetPos.y);
                    self.GetComponent<RectTransform>().DOMove(targetPos, 0.4f);
                })
                .AppendInterval(0.4f)
                .AppendCallback(() =>
                {
                    // deal damage
                    int realDamage = targets[2].DeductHP(Battle.CalculateDamage(self, targets[2]));

                    // text
                    floatingText = CreateFloatingText(targets[2].transform);
                    floatingText.Init(2.0f, targets[2].GetMiddleGlobalPosition(), (targets[2].GetMiddleGlobalPosition() - self.GetMiddleGlobalPosition()) + new Vector2(0.0f, 100.0f), realDamage.ToString(), 64, CustomColor.damage());

                    // play SE
                    AudioManager.Instance.PlaySFX("Attacked", 0.5f);

                    // animation
                    targets[2].Shake(0.75f);
                    self.PlayAnimation(BattlerAnimationType.attack);
                    targets[2].PlayAnimation(BattlerAnimationType.attacked);
                })
                .AppendInterval(0.1f)
                .AppendCallback(() =>
                {
                    // return to middle
                    self.GetComponent<RectTransform>().DOMove(new Vector3(0.0f, -140.0f, 0.0f), 0.5f);

                    // cancel animation
                    self.PlayAnimation(BattlerAnimationType.idle);
                    targets[2].PlayAnimation(BattlerAnimationType.idle);
                })
                .AppendInterval(0.5f)
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
    #endregion abilities
}
