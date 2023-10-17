using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public static class FileManager
{
    public static bool WriteToFile(string a_FileName, string a_FileContents)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

        try
        {
            File.WriteAllText(fullPath, a_FileContents);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
            return false;
        }
    }

    public static bool LoadFromFile(string a_FileName, out string result)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

        try
        {
            result = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            result = "";
            return false;
        }
    }
}

public static class SaveDataManager
{
    public static void SaveJsonData(int slotIndex, string comment)
    {
        SaveData sd = new SaveData();
        sd.dataComment = comment;
        sd.serializablePlayerData = ConvertPlayerDataToSerializableData(ProgressManager.Instance.PlayerData);

        Debug.Log(sd.serializablePlayerData.inventory.Count.ToString() + " item in inventory serialized");
        for (int i = 0; i < sd.serializablePlayerData.inventory.Count; i ++)
        {
            Debug.Log(sd.serializablePlayerData.inventory[i] + " serialized.");
        }

        if (FileManager.WriteToFile("SaveData" + slotIndex.ToString("00") + ".dat", sd.ToJson()))
        {
            Debug.Log("Save successful as SaveData" + slotIndex.ToString("00") + ".dat");
        }
    }

    public static void LoadJsonData(int slotIndex)
    {
        if (FileManager.LoadFromFile("SaveData" + slotIndex.ToString("00") + ".dat", out var json))
        {
            SaveData sd = new SaveData();
            sd = SaveData.LoadFromJson(json);

            ProgressManager.Instance.ApplyLoadedData(ConvertSerializableDataToPlayerData(sd.serializablePlayerData));

            Debug.Log("Load SaveData" + slotIndex.ToString("00") + ".dat");
        }
    }

    public static string GetDataInfo(int slotIndex)
    {
        if (FileManager.LoadFromFile("SaveData" + slotIndex.ToString("00") + ".dat", out var json))
        {
            SaveData sd = new SaveData();
            sd = SaveData.LoadFromJson(json);

            PlayerData pd = ConvertSerializableDataToPlayerData(sd.serializablePlayerData);

            string slotInfo = (slotIndex + 1).ToString() + "  Chapter " + ((pd.currentStage / 3)+1).ToString() + "-" + ((pd.currentStage % 3)+1).ToString();
            if (sd.dataComment != string.Empty) slotInfo += " [" + sd.dataComment + "]";
            return slotInfo;
        }

        return (slotIndex + 1).ToString() + "  <color=grey>No Data</color>";
    }


    public static bool IsDataExist(int slotIndex)
    {
        return (FileManager.LoadFromFile("SaveData" + slotIndex.ToString("00") + ".dat", out var json));
    }

    private static SerializablePlayerData ConvertPlayerDataToSerializableData(PlayerData pd)
    {
        SerializablePlayerData data = new SerializablePlayerData();

        // ���XSerialize�ł���f�[�^
        data.currentStage = pd.currentStage;
        data.currentMoney = pd.currentMoney;
        data.currentResourcesPoint = pd.currentResourcesPoint;
        data.formationSlotUnlocked = pd.formationSlotUnlocked;
        data.formationCharacters = pd.formationCharacters;

        // Serialize������Ȃ��f�[�^
        {
            data.inventory = new List<string>();
            foreach (ItemDefine item in pd.inventory)
            {
                data.inventory.Add(item.pathName);
            }
        }

        {
            data.homeDialogue = new List<SaveLoad.SerializedHomeDialogue>();
            foreach (HomeDialogue homeDialogue in pd.homeDialogue)
            {
                SaveLoad.SerializedHomeDialogue newData = new SaveLoad.SerializedHomeDialogue();
                newData.pathName = homeDialogue.pathName;

                data.homeDialogue.Add(newData);
            }
        }

        {
            data.characters = new List<SaveLoad.SerializedCharacter>();
            foreach (Character character in pd.characters)
            {
                SaveLoad.SerializedCharacter newData = new SaveLoad.SerializedCharacter();
                newData.pathName = character.pathName;

                newData.dark_gauge = character.dark_gauge;
                newData.horny_gauge = character.horny_gauge;
                newData.holyCore_ResearchRate = character.holyCore_ResearchRate;

                newData.localizedName = character.localizedName;
                newData.current_level = character.current_level;
                newData.current_maxHp = character.current_maxHp;
                newData.current_maxMp = character.current_maxMp;
                newData.current_hp = character.current_hp;
                newData.current_mp = character.current_mp;
                newData.current_attack = character.current_attack;
                newData.current_defense = character.current_defense;
                newData.current_speed = character.current_speed;

                newData.is_corrupted = character.is_corrupted;

                data.characters.Add(newData);
            }
        }

        return data;
    }
    private static PlayerData ConvertSerializableDataToPlayerData(SerializablePlayerData serializableData)
    {
        PlayerData playerData = new PlayerData();

        // ���XSerialize�ł���f�[�^
        playerData.currentStage = serializableData.currentStage;
        playerData.currentMoney = serializableData.currentMoney;
        playerData.currentResourcesPoint = serializableData.currentResourcesPoint;
        playerData.formationSlotUnlocked = serializableData.formationSlotUnlocked;
        playerData.formationCharacters = serializableData.formationCharacters;

        // Serialize������Ȃ��f�[�^
        {
            playerData.inventory = new List<ItemDefine>();
            foreach (string path in serializableData.inventory)
            {
                ItemDefine newData = Resources.Load<ItemDefine>("ItemList/" + path);
                playerData.inventory.Add(newData);
            }
        }

        {
            playerData.homeDialogue = new List<HomeDialogue>();
            foreach (SaveLoad.SerializedHomeDialogue homeDialogue in serializableData.homeDialogue)
            {
                HomeDialogue newData = Resources.Load<HomeDialogue>("HomeDialogue/" + homeDialogue.pathName);
                playerData.homeDialogue.Add(newData);
            }
        }

        {
            playerData.characters = new List<Character>();
            foreach (SaveLoad.SerializedCharacter character in serializableData.characters)
            {
                Character newData = new Character();

                var scriptableObject = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/" + character.pathName);

                newData.characterData = scriptableObject.detail;
                newData.battler = scriptableObject.battler;
                newData.pathName = character.pathName;

                newData.dark_gauge = character.dark_gauge;
                newData.horny_gauge = character.horny_gauge;
                newData.holyCore_ResearchRate = character.holyCore_ResearchRate;

                newData.localizedName = character.localizedName;
                newData.current_level = character.current_level;
                newData.current_maxHp = character.current_maxHp;
                newData.current_maxMp = character.current_maxMp;
                newData.current_hp = character.current_hp;
                newData.current_mp = character.current_mp;
                newData.current_attack = character.current_attack;
                newData.current_defense = character.current_defense;
                newData.current_speed = character.current_speed;

                newData.is_corrupted = character.is_corrupted;

                playerData.characters.Add(newData);
            }
        }

        return playerData;
    }
}

[System.Serializable]
public struct SerializablePlayerData
{
    public int currentStage;
    public int currentMoney;
    public int currentResourcesPoint; 
    public List<string> inventory; 
    public int formationSlotUnlocked;
    public FormationSlotData[] formationCharacters;

    public List<SaveLoad.SerializedCharacter> characters;
    public List<SaveLoad.SerializedHomeDialogue> homeDialogue;
}

namespace SaveLoad
{
    public struct SerializedHomeDialogue
    {
        public string pathName;
    }

    public struct SerializedCharacter
    {
        public string pathName;

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

        public bool is_corrupted;
    }
}

[System.Serializable]
public class SaveData
{
    public string dataComment; // ����
    public SerializablePlayerData serializablePlayerData;

    public string ToJson()
    {
        //JsonSerializerSettings settings = new JsonSerializerSettings
        //{
        //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //};

        return JsonConvert.SerializeObject(this);
    }

    public static SaveData LoadFromJson(string a_Json)
    {
        return JsonConvert.DeserializeObject<SaveData>(a_Json);
    }
}