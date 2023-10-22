using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class WorldMapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StagesUI stagesUIScript;

    private void Start()
    {
        // BGM�Đ�
        AudioManager.Instance.PlayMusicWithFade("WorldMap", 6.0f);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeIn(1.0f);
    }

    public void ToHomeScene()
    {
        const float animationTime = 1.0f;
        StartCoroutine(SceneTransition("Home", animationTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // BGM��~
        AudioManager.Instance.StopMusicWithFade(1.0f);

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        yield return new WaitForSeconds(animationTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void DebugNextStage()
    {
        ProgressManager.Instance.StageProgress();
        stagesUIScript.PlayStageAnimation();
    }

    public void ResourceGatheringQuest()
    {
        // �G�L������ݒu
        BattleSetup.Reset(false);
        BattleSetup.AddEnemy("Android");
        BattleSetup.AddEnemy("Drone");
        BattleSetup.SetBattleBGM("BattleTutorial");
        BattleSetup.SetReward(Random.Range(300, 600), Random.Range(100, 300));
        BattleSetup.AddEquipmentReward("Stick");
        BattleSetup.AddItemReward("�~�}��");
        BattleSetup.AddItemReward("�~�}��");
        BattleSetup.AddItemReward("�H�p��");
        BattleSetup.AddItemReward("�H�p��");

        const float animationTime = 1.0f;

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);

        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("Battle", LoadSceneMode.Single); });
    }

    public void NextStory()
    {
        const float animationTime = 1.0f;

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("Story", LoadSceneMode.Single); });

        // SE
        AudioManager.Instance.PlaySFX("QuestStart");

        // Switch BGM
        AudioManager.Instance.StopMusicWithFade(animationTime);
    }
}
