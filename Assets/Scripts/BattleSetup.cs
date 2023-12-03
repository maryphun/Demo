using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleSetup
{
    static List<int> teammates; // insert characterID
    static List<EnemyDefine> enemies;
    public static bool isStoryMode = false;
    public static bool isEventBattle = false;

    public static string BattleBGM { get { return battleBGM; } }
    private static string battleBGM = string.Empty;

    public static List<EquipmentDefine> equipmentReward;
    public static List<ItemDefine> itemReward;
    public static int moneyReward;
    public static int researchPointReward;
    public static SideQuestData sideQuestIncrement;
    public static bool isAllowEscape;

    public static List<EnemyDefine> GetEnemyList(bool clear)
    {
        var rtn = new List<EnemyDefine>(enemies);
        if (clear)
        {
            enemies.Clear();
        }

        return rtn;
    }

    /// <summary>
    /// フォーメーションが決まれているか
    /// </summary>
    public static bool IsCustomFormation()
    {
        return teammates.Count > 0;
    }

    /// <summary>
    /// 特殊イベント用
    /// </summary>
    public static List<int> GetCustomFormation()
    {
        return teammates;
    }

    /// <summary>
    /// 敵をまとめて設定
    /// </summary>
    public static void SetEnemy(List<EnemyDefine> enemyList)
    {
        enemies = enemyList;
    }

    /// <summary>
    /// 使う前に必ず呼んでおくこと
    /// </summary>
    public static void Reset(bool isStory)
    {
        teammates = new List<int>();
        enemies = new List<EnemyDefine>();
        isStoryMode = isStory;
        battleBGM = string.Empty;
        equipmentReward = new List<EquipmentDefine>();
        itemReward = new List<ItemDefine>();
        sideQuestIncrement = new SideQuestData(0,0,0);
        moneyReward = 0;
        researchPointReward = 0;
        isAllowEscape = false;
        isEventBattle = false;
    }

    /// <summary>
    /// Enemy prefab name in Resources/EnemyList/
    /// </summary>
    public static void AddEnemy(string enemyPrefabName)
    {
        if (ReferenceEquals(enemies, null))
        {
            enemies = new List<EnemyDefine>();
        }
        var enemy = Resources.Load<EnemyDefine>("EnemyList/" + enemyPrefabName);

        if (enemy != null)
        {
            enemies.Add(enemy);
        }
        else
        {
            Debug.LogWarning("EnemyList/" + enemyPrefabName + " doesn't exist!");
        }
    }

    /// <summary>
    /// チームメイトを追加
    /// </summary>
    public static void AddTeammate(int characterID)
    {
        teammates.Add(characterID);
    }
    public static void AddTeammate(PlayerCharacerID characterID)
    {
        teammates.Add((int)characterID);
    }

    /// <summary>
    /// バトルBGMを設定
    /// </summary>
    public static void SetBattleBGM(string clipName)
    {
        battleBGM = clipName;
    }

    /// <summary>
    /// 戦闘奨励を設定
    /// </summary>
    public static void SetReward(int money, int researchPoint)
    {
        moneyReward = money;
        researchPointReward = researchPoint;
    }


    public static void AddItemReward(string itemName)
    {
        ItemDefine itemData = Resources.Load<ItemDefine>("ItemList/" + itemName);
        AddItemReward(itemData);
        //Resources.UnloadAsset(itemData);
    }
    public static void AddEquipmentReward(string equipmentName)
    {
        EquipmentDefine equipmentData = Resources.Load<EquipmentDefine>("EquipmentList/" + equipmentName);

        if (!ProgressManager.Instance.PlayerHasEquipment(equipmentData)) // すでにある装備は繰り返して貰わない
        {
            AddEquipmentReward(equipmentData);
        }

        //Resources.UnloadAsset(equipmentData);
    }
    public static void AddItemReward(ItemDefine item)
    {
        itemReward.Add(item);
    }
    public static void AddEquipmentReward(EquipmentDefine equipment)
    {
        equipmentReward.Add(equipment);
    }

    public static void SetSideQuestIncrement(int food, int bank, int research)
    {
        sideQuestIncrement = new SideQuestData(food, bank, research);
    }

    public static void SetAllowEscape(bool value)
    {
        isAllowEscape = value;
    }
    public static void SetEventBattle(bool value)
    {
        isEventBattle = value;
    }
}
