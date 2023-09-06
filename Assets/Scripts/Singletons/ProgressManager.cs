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
    }

    public int GetCurrentStageProgress()
    {
        return playerData.currentStage;
    }

    public void StageProgress()
    {
        playerData.currentStage++;
    }
}
