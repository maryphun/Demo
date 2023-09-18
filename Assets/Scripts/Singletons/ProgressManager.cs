using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SimpleLocalization.Scripts;

/// <summary>
/// �v���C���[�̃Q�[���i���͑S�Ă����ɋL�^����
/// �Z�[�u���[�h�͂��̍\���̂�ۑ�������ǂ��Ƃ����F��
/// </summary>
public struct PlayerData
{
    public int currentStage;             //< ���X�e�[�W��
    public int currentMoney;             //< ����
    public int currentResourcesPoint;    //< �����|�C���g
    public List<Character> characters;     //< �����Ă���L�����N�^�[
    public FormationSlotData[] formationCharacters; //< �p�[�e�B�[�Ґ�
    public int formationSlotUnlocked;    //< ������ꂽ�X���b�g
}


public class ProgressManager : SingletonMonoBehaviour<ProgressManager>
{
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
        playerData.currentMoney = 100;
        playerData.currentResourcesPoint = 50;
        playerData.characters = new List<Character>();
        playerData.formationCharacters = new FormationSlotData[5];
        playerData.formationSlotUnlocked = 2;

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
                playerData.formationCharacters[i] = new FormationSlotData(playerData.characters[i], true);
            }
            else
            {
                playerData.formationCharacters[i] = new FormationSlotData(null, false);
            }
        } 
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
    /// �V�������Ԓǉ�
    /// </summary>
    public void AddPlayerCharacter(PlayerCharacterDefine newCharacter)
    {
        var obj = new Character();

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


#if DEBUG_MODE
    public void DebugModeInitialize()
    {
        if (isDebugModeInitialized) return;
        ProgressManager.Instance.InitializeProgress();
        ProgressManager.Instance.SetMoney(Random.Range(200, 9999));
        ProgressManager.Instance.SetResearchPoint(Random.Range(200, 9999));
        isDebugModeInitialized = true;
    }
#else
public void DebugModeInitialize()
    {
        
    }
#endif
}
