using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public struct BattlerAnimation
{
    public Sprite idle, attack, magic, item, attacked, retire;
}

[System.Serializable]
public struct BattlerSoundEffect
{
    public AudioClip attack, attacked, retire;
}

[System.Serializable]
public enum BattlerAnimationType
{
    idle, attack, magic, item, attacked, retire,
}

[System.Serializable]
public struct PlayerCharacter
{
    // system
    [Header("�V�X�e��")]
    public int characterID;

    // graphic
    [Header("�摜")]
    public Sprite icon;
    public Sprite sprite;
    public BattlerAnimation animations;
    public Color color;

    [Header("�퓬�֘A")]
    // battle related
    public string nameID;
    public int starting_level;
    public int base_hp;
    public int base_mp;
    public int base_attack;
    public int base_defense;
    public int base_speed;
    public int hp_growth;
    public int mp_growth;
    public int attack_growth;
    public int defense_growth;
    public int speed_growth;

    [Header("�琬")]
    public int levelUp_base_cost;
    public int levelUp_cost_increment;

    [Header("�X�e�[�^�X")]
    public bool is_heroin;      // �q���C��
    public List<CharacterStatus> characterStatus;

    [Header("����Z���X�g")]
    public List<Ability> abilities;
}

public class Character
{
    public PlayerCharacter characterData; // ��{����
    public GameObject battler; // �퓬��
    public int dark_gauge;
    public int horny_gauge;
    public int holyCore_ResearchRate;

    public string localizedName;
    public int current_level;
    public int current_maxHp;
    public int current_maxMp;
    public int current_hp;
    public int current_mp;
    public int current_attack;
    public int current_defense;
    public int current_speed;

    public bool is_corrupted;    // �ŗ���������

    public List<Ability> abilities; 
}

[System.Serializable]
public struct CharacterStatus
{
    [SerializeField] public string moodNameID;
    [SerializeField] public Color textColor;
    [SerializeField] public Sprite character;
    [SerializeField, Range(0, 100)] public int requiredDarkGauge;
    [SerializeField, Range(0, 100)] public int requiredHornyGauge;
}

/// <summary>
/// �p�[�e�B�[�Ǘ��p
/// </summary>
public class FormationSlotData
{
    public Character characterData;
    public bool isFilled;

    public FormationSlotData(Character data, bool flag)
    {
        characterData = data;
        isFilled = flag;
    }
}
