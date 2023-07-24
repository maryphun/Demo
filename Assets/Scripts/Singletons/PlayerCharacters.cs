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
public struct PlayerCharacter
{
    // system
    [Header("�V�X�e��")]
    public int characterID;

    // graphic
    [Header("�摜")]
    public Sprite icon;
    public BattlerAnimation animations;

    [Header("����")]
    // battle related
    public string name;
    public int max_hp;
    public int max_mp;
    public int attackDamage;
}

public class PlayerCharacters : SingletonMonoBehaviour<PlayerCharacters>
{

}
