using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public class ProgressManager : SingletonMonoBehaviour<ProgressManager>
{
    PlayerData playerData;

    /// <summary>
    /// �Q�[���i����������Ԃɂ���
    /// </summary>
    public void InitializeProgress()
    {
        playerData = new PlayerData();

        playerData.currentStage = 1; // �����X�e�[�W (�`���[�g���A��)
        playerData.currentMoney = 0;
        playerData.currentResourcesPoint = 0;
        playerData.characters = new List<Character>();

        // �����L���� 
        PlayerCharacterDefine tentacle = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/TentacleMan");
        AddPlayerCharacter(tentacle);
        Resources.UnloadAsset(tentacle);
    }

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
    public List<Character> GetAllCharacter()
    {
        return playerData.characters;
    }

    /// <summary>
    /// �V�������Ԓǉ�
    /// </summary>
    public void AddPlayerCharacter(PlayerCharacterDefine newCharacter)
    {
        var obj = new Character();

        obj.characterData = newCharacter.detail;
        obj.battler = newCharacter.battler;
        obj.level = 1;
    }
}
