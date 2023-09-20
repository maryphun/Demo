using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class Battler : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private BattlerAnimation animations;
    [SerializeField] private VFX attackVFX;

    [Header("Debug：デバッグ用なのでここで設定する物は全部無効です。\nEnemyDefineとPlayerCharacterDefineで設定してください")]
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

    [Header("References")]
    [SerializeField] private Image graphic;
    [SerializeField] private TMP_Text name_UI;

    private Vector3 originalScale;
    private float ease = 0.0f;
    private RectTransform graphicRect;

    private void Awake()
    {
        graphicRect = graphic.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 敵キャラクターの設定データをロードしてBattlerを生成する
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

        Initialize();
    }
    /// <summary>
    /// プレイヤーキャラクターの設定データをロードしてBattlerを生成する
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

        Initialize();
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
        name_UI.text = character_name;
        name_UI.color = character_color;

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

    /// <summary>
    /// キャラクターの画像中央を取得
    /// </summary>
    public Vector2 GetMiddleGlobalPosition()
    {
        return new Vector2(graphicRect.position.x -GetCharacterSize().x * 0.25f, graphicRect.position.y + GetCharacterSize().y * 0.5f);
    }

    private void Update()
    {
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
}
