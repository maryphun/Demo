using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AbilityType
{
    Attack, //< �U��
    Buff,   //< �o�t/�f�o�t
    Heal,   //< ��
    Special,//< ����
}

[CreateAssetMenu(fileName = "NewAbility", menuName = "�쐬/����Z����")]
public class Ability : ScriptableObject
{
    [Header("��{����")]
    public string abilityNameID;
    public string descriptionID;
    public string functionName;
    public int cosumeSP;
    public int requiredLevel;
    public int requiredHornyness; // �����x�v��
    public CastType castType;
    public AbilityType abilityType;
}
