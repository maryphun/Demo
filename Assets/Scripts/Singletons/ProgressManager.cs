using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SimpleLocalization.Scripts;
using System.Linq;

/// <summary>
/// �v���C���[�̃Q�[���i���͑S�Ă����ɋL�^����
/// �Z�[�u���[�h�͂��̍\���̂�ۑ�������ǂ��Ƃ����F��
/// </summary>
[System.Serializable]
public struct PlayerData
{
    public int currentStage;             //< ���X�e�[�W��
    public int currentMoney;             //< ����
    public int currentResourcesPoint;    //< �����|�C���g
    public List<Character> characters;     //< �����Ă���L�����N�^�[
    public FormationSlotData[] formationCharacters; //< �p�[�e�B�[�Ґ�
    public List<ItemDefine> inventory;   //< �����A�C�e��
    public List<EquipmentData> equipment;   //< ��������
    public List<HomeDialogue> homeDialogue;   //< �z�[���V�[���̃Z���t���Ǘ�����
    public int formationSlotUnlocked;    //< ������ꂽ�X���b�g
}

public class ProgressManager : SingletonMonoBehaviour<ProgressManager>
{
    public PlayerData PlayerData { get { return playerData; } }
    PlayerData playerData;

#if DEBUG_MODE
    bool isDebugModeInitialized = false;
#endif

    /// <summary>
    /// �Q�[���i����������Ԃɂ���
    /// </summary>
    public void InitializeProgress()
    {
        playerData = new PlayerData();

        playerData.currentStage = 1; // �����X�e�[�W (�`���[�g���A��)
        playerData.currentMoney = 500;
        playerData.currentResourcesPoint = 0;
        playerData.characters = new List<Character>();
        playerData.formationCharacters = new FormationSlotData[5];
        playerData.inventory = new List<ItemDefine>();
        playerData.equipment = new List<EquipmentData>();
        playerData.homeDialogue = new List<HomeDialogue>();
        playerData.formationSlotUnlocked = 2;

        // �����z�[���V�[���L����
        HomeDialogue no5 = Resources.Load<HomeDialogue>("HomeDialogue/No5");
        playerData.homeDialogue.Add(no5);
        Resources.UnloadAsset(no5);

        // �����L���� 
        PlayerCharacterDefine battler = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/1.Battler");
        AddPlayerCharacter(battler);
        Resources.UnloadAsset(battler);

        PlayerCharacterDefine tentacle = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/2.TentacleMan");
        AddPlayerCharacter(tentacle);
        Resources.UnloadAsset(tentacle);

        PlayerCharacterDefine clone = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/3.Clone");
        AddPlayerCharacter(clone);
        Resources.UnloadAsset(clone);

        // �����L�����������I�Ƀp�[�e�B�[�ɕғ�����
        for (int i = 0; i < playerData.formationCharacters.Length; i++)
        {
            if (i < playerData.formationSlotUnlocked)
            {
                playerData.formationCharacters[i] = new FormationSlotData(playerData.characters[i].characterData.characterID, true);
            }
            else
            {
                playerData.formationCharacters[i] = new FormationSlotData(-1, false);
            }
        } 
    }

    public void ApplyLoadedData(PlayerData data)
    {
        playerData = data;
    }

    /// <summary>
    /// ���݂̃Q�[���i�s�󋵂��擾
    /// </summary>
    public int GetCurrentStageProgress()
    {
        return playerData.currentStage;
    }

    /// <summary>
    /// 
    /// </summary>
    public void StageProgress()
    {
        playerData.currentStage++;
    }

    /// <summary>
    /// �L�����N�^�[�������X�V
    /// </summary>
    public void UpdateCharacterData(List<Character> characters)
    {
        playerData.characters = characters;
    }

    /// <summary>
    /// �����Ă��钇�Ԃ̃��X�g���擾
    /// </summary>
    public List<Character> GetAllCharacter(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.characters;
        }
        else
        {
            List<Character> characterListCopy = new List<Character>(playerData.characters);
            return characterListCopy;
        }
    }

    /// <summary>
    /// CharacterID ����L�����N�^�[���擾����
    /// </summary>
    /// <returns></returns>
    public Character GetCharacterByID(int characterID)
    {
        List<Character> characterListCopy = new List<Character>(playerData.characters);

        return characterListCopy.Find(item => item.characterData.characterID == characterID);
    }

    /// <summary>
    /// �g���钇�ԃL�����̃��X�g���擾
    /// </summary>
    public List<Character> GetAllUsableCharacter()
    {
        List<Character> usableCharacter = playerData.characters.Where(data => data.is_corrupted || !data.characterData.is_heroin).ToList();

        return usableCharacter;
    }

    /// <summary>
    /// �V�������Ԓǉ�
    /// </summary>
    public void AddPlayerCharacter(PlayerCharacterDefine newCharacter)
    {
        var obj = new Character();

        obj.pathName = newCharacter.name;

        obj.localizedName = LocalizationManager.Localize(newCharacter.detail.nameID);
        obj.characterData = newCharacter.detail;
        obj.battler = newCharacter.battler;
        obj.current_level = newCharacter.detail.starting_level;
        obj.current_maxHp = newCharacter.detail.base_hp;
        obj.current_maxMp = newCharacter.detail.base_mp;
        obj.current_hp = newCharacter.detail.base_hp;
        obj.current_mp = newCharacter.detail.base_mp;
        obj.current_attack = newCharacter.detail.base_attack;
        obj.current_defense = newCharacter.detail.base_defense;
        obj.current_speed = newCharacter.detail.base_speed;
        obj.is_corrupted = !(newCharacter.detail.is_heroin); // �q���C���L�����͂Ƃ肠�����g�p�ł��Ȃ�
        
        playerData.characters.Add(obj);
    }

    /// <summary>
    /// ���ݎ����ʂ��擾
    /// </summary>
    public int GetCurrentMoney()
    {
        return playerData.currentMoney;
    }

    /// <summary>
    /// �����ʂ�ύX
    /// </summary>
    public void SetMoney(int newValue)
    {
        playerData.currentMoney = Mathf.Max(newValue, 0);
    }

    /// <summary>
    /// ���ݎ����ʂ��擾
    /// </summary>
    public int GetCurrentResearchPoint()
    {
        return playerData.currentResourcesPoint;
    }

    /// <summary>
    /// �����ʂ�ύX
    /// </summary>
    public void SetResearchPoint(int newValue)
    {
        playerData.currentResourcesPoint = Mathf.Max(newValue, 0);
    }

    /// <summary>
    /// �p�[�e�B�[�Ґ��ő吔���擾
    /// </summary>
    public int GetUnlockedFormationCount()
    {
        return playerData.formationSlotUnlocked;
    }

    /// <summary>
    /// �p�[�e�B�[�Ґ��ő吔�𑝉�
    /// </summary>
    public void UnlockedFormationCount()
    {
        playerData.formationSlotUnlocked ++;
    }

    /// <summary>
    /// �o���p�[�e�B�[�擾
    /// </summary>
    public FormationSlotData[] GetFormationParty(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.formationCharacters;
        }
        else
        {
            FormationSlotData[] partyListCopy = (FormationSlotData[])playerData.formationCharacters.Clone();
            return partyListCopy;
        }
    }

    /// <summary>
    /// �o���p�[�e�B�[�擾
    /// </summary>
    public void SetFormationParty(FormationSlotData[] characters)
    {
        playerData.formationCharacters = characters;
    }
    
    /// <summary>
    /// �����Ă���A�C�e���̃��X�g���擾
    /// </summary>
    public List<ItemDefine> GetItemList(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.inventory;
        }
        else
        {
            List<ItemDefine> itemListCopy = new List<ItemDefine>(playerData.inventory);
            return itemListCopy;
        }
    }

    /// <summary>
    /// �C���x���g�����X�V
    /// </summary>
    public void SetItemList(List<ItemDefine> newList)
    {
        playerData.inventory = newList;
    }

    /// <summary>
    /// �A�C�e���l��
    /// </summary>
    public void AddItemToInventory(ItemDefine item)
    {
        playerData.inventory.Add(item);
    }

    /// <summary>
    /// �A�C�e���l��
    /// </summary>
    public void RemoveItemFromInventory(ItemDefine item)
    {
        playerData.inventory.Remove(item);
    }

    /// <summary>
    /// �z�[���䎌�L�����N�^�[���擾
    /// </summary>
    public List<HomeDialogue> GetHomeCharacter() 
    {
        return playerData.homeDialogue;
    }

    /// <summary>
    /// ��������肷��
    /// </summary>
    public void AddNewEquipment(EquipmentDefine data)
    {
        EquipmentData newEquipment = new EquipmentData(data);
        playerData.equipment.Add(newEquipment);
    }

    /// <summary>
    /// �w��̑����A�C�e���𑕔�����, �Z�[�u�f�[�^�̓s����CharacterID�Ƃ��ăf�[�^���c���A�����ƃL������R����
    /// </summary>
    public void ApplyEquipmentToCharacter(EquipmentDefine data, Character character)
    {
        for (int i = 0; i < playerData.equipment.Count; i++)
        {
            if (playerData.equipment[i].data.pathName == data.pathName)
            {
                playerData.equipment[i].SetEquipCharacter(character.characterData.characterID);
            }
        }
    }

    /// <summary>
    /// �������O��
    /// </summary>
    public void UnapplyEquipment(EquipmentDefine data)
    {
        for (int i = 0; i < playerData.equipment.Count; i++)
        {
            if (playerData.equipment[i].data.pathName == data.pathName)
            {
                playerData.equipment[i].Unequip();
            }
        }
    }

    public List<EquipmentData> GetEquipmentData(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.equipment;
        }
        else
        {
            List<EquipmentData> copy = new List<EquipmentData>(playerData.equipment);
            return copy;
        }
    }

#if DEBUG_MODE
    public void DebugModeInitialize(bool addEnemy = false)
    {
        if (isDebugModeInitialized) return;
        ProgressManager.Instance.InitializeProgress();
        ProgressManager.Instance.SetMoney(Random.Range(200, 9999));
        ProgressManager.Instance.SetResearchPoint(Random.Range(200, 9999));
        isDebugModeInitialized = true;

        // �����ł���q���C����ǉ�
        PlayerCharacterDefine Akiho = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/4.Akiho");
        AddPlayerCharacter(Akiho);
        Resources.UnloadAsset(Akiho);

        PlayerCharacterDefine Rikka = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/5.Rikka");
        AddPlayerCharacter(Rikka);
        Resources.UnloadAsset(Rikka);

        // �A�C�e������������������
        ItemDefine bread = Resources.Load<ItemDefine>("ItemList/�H�p��");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(bread);
        Resources.UnloadAsset(bread);

        ItemDefine croissant = Resources.Load<ItemDefine>("ItemList/�N�����b�T��");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(croissant);
        Resources.UnloadAsset(croissant);

        ItemDefine m24 = Resources.Load<ItemDefine>("ItemList/M24");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(m24);
        Resources.UnloadAsset(m24);

        ItemDefine aid = Resources.Load<ItemDefine>("ItemList/�~�}��");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(aid);
        Resources.UnloadAsset(aid);

        // �G�L������ݒu
        if (addEnemy)
        {
            SetupEnemy.ClearEnemy();
            SetupEnemy.AddEnemy("Akiho_Enemy");
            SetupEnemy.AddEnemy("Rikka_Enemy");
        }
    }
#else
public void DebugModeInitialize() { }
#endif
}
