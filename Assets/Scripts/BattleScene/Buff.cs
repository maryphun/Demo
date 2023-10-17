using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.SimpleLocalization.Scripts;
using TMPro;

[System.Serializable]
public struct BuffData
{
    public Sprite icon;
    public string name;
    public Action<Battler, int> start;
    public Action<Battler, int> update;
    public Action<Battler, int> end;
    public bool isBad;
}

[System.Serializable]
public enum BuffType
{
    stun,
    hurt,
    heal,
    shield_up,
    shield_down,
    attack_up,
    attack_down,
    speed_up,
    speed_down,
}

[System.Serializable]
public struct Buff
{
    public BuffType type;
    public BuffData data;

    public Battler target;
    public int remainingTurn;
    public int value;

    public GameObject icon;
    public TMP_Text text;
}

public static class BuffManager
{
    public static Dictionary<BuffType, BuffData> BuffList = new Dictionary<BuffType, BuffData>();
    public static Battler currentBattler;

    public static void Init()
    {
        // stun
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/stunned");
            data.name = LocalizationManager.Localize("Buff.Stun");
            data.start = StunStart;
            data.end = StunEnd;
            data.update = StunUpdate;
            data.isBad = true;
            BuffList.Add(BuffType.stun, data);
        }
        // hurt
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/hurt");
            data.name = LocalizationManager.Localize("Buff.Hurt");
            data.start = HurtStart;
            data.end = HurtEnd;
            data.isBad = true;
            BuffList.Add(BuffType.hurt, data);
        }
        // heal
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/heal");
            data.name = LocalizationManager.Localize("Buff.Heal");
            data.start = HealStart;
            data.end = HealEnd;
            data.isBad = false;
            BuffList.Add(BuffType.heal, data);
        }
        // shield up
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/shield_up");
            data.name = LocalizationManager.Localize("Buff.Shield_up");
            data.start = ShieldUpStart;
            data.end = ShieldUpEnd;
            data.isBad = false;
            BuffList.Add(BuffType.shield_up, data);
        }
        // shield down
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/shield_down");
            data.name = LocalizationManager.Localize("Buff.Shield_down");
            data.start = ShieldDownStart;
            data.end = ShieldDownEnd;
            data.isBad = true;
            BuffList.Add(BuffType.shield_down, data);
        }
        // attack up
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/attack_up");
            data.name = LocalizationManager.Localize("Buff.Attack_up");
            data.start = AttackUpStart;
            data.end = AttackUpEnd;
            data.isBad = false;
            BuffList.Add(BuffType.attack_up, data);
        }
        // attack down
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/attack_down");
            data.name = LocalizationManager.Localize("Buff.Attack_down");
            data.start = AttackDownStart;
            data.end = AttackDownEnd;
            data.isBad = true;
            BuffList.Add(BuffType.attack_down, data);
        }
        // speed up
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/speed_up");
            data.name = LocalizationManager.Localize("Buff.Speed_up");
            data.start = SpeedUpStart;
            data.end = SpeedUpEnd;
            data.isBad = false;
            BuffList.Add(BuffType.speed_up, data);
        }
        // speed down
        {
            BuffData data = new BuffData();
            data.icon = Resources.Load<Sprite>("Icon/speed_down");
            data.name = LocalizationManager.Localize("Buff.Speed_down");
            data.start = SpeedDownStart;
            data.end = SpeedDownEnd;
            data.isBad = true;
            BuffList.Add(BuffType.speed_down, data);
        }
    }

    public static void CurrentTurn(Battler battler)
    {
        currentBattler = battler;
    }

    public static void StunStart(Battler target, int value)
    {

    }
    public static void StunUpdate(Battler target, int value)
    {

    }
    public static void StunEnd(Battler target, int value)
    {

    }
    public static void HurtStart(Battler target, int value)
    {

    }
    public static void HurtUpdate(Battler target, int value)
    {
        target.DeductHP(value);
    }
    public static void HurtEnd(Battler target, int value)
    {

    }
    public static void HealStart(Battler target, int value)
    {

    }
    public static void HealUpdate(Battler target, int value)
    {
        target.Heal(value);
    }
    public static void HealEnd(Battler target, int value)
    {

    }
    public static void ShieldUpStart(Battler target, int value)
    {
        target.defense += value;
    }
    public static void ShieldUpUpdate(Battler target, int value)
    {

    }
    public static void ShieldUpEnd(Battler target, int value)
    {
        target.defense -= value;
    }
    public static void ShieldDownStart(Battler target, int value)
    {
        target.defense -= value;
    }
    public static void ShieldDownUpdate(Battler target, int value)
    {

    }
    public static void ShieldDownEnd(Battler target, int value)
    {
        target.defense += value;
    }
    public static void AttackUpStart(Battler target, int value)
    {
        target.attack += value;
    }
    public static void AttackUpUpdate(Battler target, int value)
    {

    }
    public static void AttackUpEnd(Battler target, int value)
    {
        target.attack -= value;
    }
    public static void AttackDownStart(Battler target, int value)
    {
        target.attack -= value;
    }
    public static void AttackDownUpdate(Battler target, int value)
    {

    }
    public static void AttackDownEnd(Battler target, int value)
    {
        target.attack += value;
    }
    public static void SpeedUpStart(Battler target, int value)
    {
        target.speed += value;
    }
    public static void SpeedUpUpdate(Battler target, int value)
    {

    }
    public static void SpeedUpEnd(Battler target, int value)
    {
        target.speed -= value;
    }
    public static void SpeedDownStart(Battler target, int value)
    {
        target.speed -= value;
    }
    public static void SpeedDownUpdate(Battler target, int value)
    {

    }
    public static void SpeedDownEnd(Battler target, int value)
    {
        target.speed += value;
    }
}
