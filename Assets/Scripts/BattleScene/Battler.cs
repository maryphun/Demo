using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private VFX attackVFX;

    [Header("Debug�F�f�o�b�O�p�Ȃ̂ł����Őݒ肷�镨�͑S�������ł��B\nEnemyDefine��PlayerCharacterDefine�Őݒ肵�Ă�������")]
    [SerializeField] public string character_name;
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
    [SerializeField] public List<Ability> abilities;
    [SerializeField] public EquipmentDefine equipment;

    [Header("References")]
    [SerializeField] private Image graphic;
    [SerializeField] private TMP_Text name_UI;
    [SerializeField] private Image hpBarFill;
    [SerializeField] private Image shadow;
    [SerializeField] private Image deadIcon;
    [SerializeField] private GameObject deadVFX;

    [HideInInspector] public float Ease { get { return ease; } }

    private Vector3 originalScale;
    private float ease = 0.0f;
    private RectTransform graphicRect;
    private Image mpBarFill;

    private void Awake()
    {
        graphicRect = graphic.GetComponent<RectTransform>();

        if (autoInit)
        {
            Initialize();
            HideBars();
            name_UI.alpha = 0.0f;
        }
    }

    /// <summary>
    /// �G�L�����N�^�[�̐ݒ�f�[�^�����[�h����Battler�𐶐�����
    /// </summary>
    public void InitializeEnemyData(EnemyDefine enemy)
    {
        character_name = LocalizationManager.Localize(enemy.enemyName);
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
        abilities = new List<Ability>(); // TODO: �G�̓���Z������

        Initialize();
    }
    /// <summary>
    /// �v���C���[�L�����N�^�[�̐ݒ�f�[�^�����[�h����Battler�𐶐�����
    /// </summary>
    public void InitializeCharacterData(Character character)
    {
        character_name = LocalizationManager.Localize(character.characterData.nameID);
        isEnemy = false;
        character_color = character.characterData.color;
        icon = character.characterData.icon;
        max_hp = character.current_maxHp;
        max_mp = character.current_maxMp;
        current_hp = character.current_hp;
        current_mp = character.current_mp;
        attack = character.current_attack;
        defense = character.current_defense;
        speed = character.current_speed;
        currentLevel = character.current_level;

        abilities = new List<Ability>();
        if (character.characterData.abilities.Count > 0)
        {
            for (int i = 0; i < character.characterData.abilities.Count; i++)
            {
                if (currentLevel >= character.characterData.abilities[i].requiredLevel
                    && character.horny_gauge >= character.characterData.abilities[i].requiredHornyness)
                {
                    abilities.Add(character.characterData.abilities[i]);
                }
            }
            abilities.Sort((x, y) => x.requiredLevel.CompareTo(y.requiredLevel));
        }

        ApplyEquipmentFromCharacter(character.characterData.characterID);
        Initialize();
    }

    /// <summary>
    /// �L�����������Ă��鑕����L����
    /// </summary>
    public void ApplyEquipmentFromCharacter(int characterID)
    {
        equipment = ProgressManager.Instance.GetCharacterEquipment(characterID);

        if (equipment != null)
        {
            try
            {
                var method = EquipmentExecute.Instance.GetType().GetMethod(equipment.battleStartFunctionName);

                if (method != null)
                {
                    var arguments = new object[] { this };
                    method.Invoke(EquipmentExecute.Instance, arguments);
                }
                else
                {
                    Debug.LogError($"Method {equipment.battleStartFunctionName} not found in EquipmentExecute.Instance");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An exception occurred: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// �L�����N�^�[�Ґ���ʂ̏ꍇ�\���f�[�^���Ⴄ
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
        isAlive = current_hp > 0;   // �ŏ����烊�^�C�A��Ԃ̂����肩������Ȃ�

        if (max_mp > 0)
        {
            // MP�\��
            var originObj = hpBarFill.transform.parent.gameObject; // HPBAR�𕡐�
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
        return new Vector2(graphicRect.rect.width * Mathf.Abs(graphicRect.localScale.x), graphicRect.rect.height * Mathf.Abs(graphicRect.localScale.y));
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
    /// �L�����N�^�[�̉摜�������擾
    /// </summary>
    public Vector2 GetMiddleGlobalPosition()
    {
        return new Vector2(graphicRect.position.x, graphicRect.position.y + GetCharacterSize().y * 0.5f);
    }

    private void Update()
    {
        if (!isAlive) return;

        ease = (ease + Time.deltaTime);

        Mathf.PingPong(ease, 1.0f);

        float value = (EaseInOutSine(ease) * 0.005f);

        graphic.rectTransform.localScale = new Vector3(originalScale.x, originalScale.y - value, originalScale.z);
    }

    private float EaseInOutSine(float x) 
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1.0f) / 2.0f;
    }

    public void PlayAnimation(BattlerAnimationType type)
    {
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
    /// �_���[�W��H�����
    /// </summary>
    public int DeductHP(int damage, bool ignoreDefense = false)
    {
        int deduction = (ignoreDefense ? 0 : defense);
        int realDamage = damage - deduction;

        if (realDamage > 0)
        {
            current_hp = Mathf.Max(0, current_hp - realDamage);
            UpdateHPBar();
            if (CheckDead())
            {
                isAlive = false;
                graphic.rectTransform.localScale = originalScale;
                PlayAnimation(BattlerAnimationType.retire);

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
                        });
            }

            FindObjectOfType<Battle>().UpdateTurnBaseManager(false);
            return realDamage;
        }

        // dealt no damage
        return 0;
    }

    /// <summary>
    /// ����
    /// </summary>
    public void Heal(int amount)
    {
        current_hp = Mathf.Min(current_hp + amount, max_hp);
        UpdateHPBar();
    }

    /// <summary>
    /// SP��
    /// </summary>
    public void AddSP(int amount)
    {
        current_mp = Mathf.Min(current_mp + amount, max_mp);
        UpdateMPBar();
    }

    /// <summary>
    /// SP����
    /// </summary>
    public void DeductSP(int amount)
    {
        current_mp = Mathf.Max(current_mp - amount, 0);
        UpdateMPBar();
    }

    /// <summary>
    /// ����ł��邩�ǂ������m�F
    /// </summary>
    public bool CheckDead()
    {
        return current_hp <= 0;
    }

    /// <summary>
    /// HP Bar���X�V
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
    /// MP Bar���X�V
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
}
