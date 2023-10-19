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
        // BGM再生
        AudioManager.Instance.PlayMusicWithFade("WorldMap", 6.0f);

        // 画面遷移
        AlphaFadeManager.Instance.FadeIn(1.0f);
    }

    public void ToHomeScene()
    {
        const float animationTime = 1.0f;
        StartCoroutine(SceneTransition("Home", animationTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // BGM停止
        AudioManager.Instance.StopMusicWithFade(1.0f);

        // シーン遷移
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
        // 敵キャラを設置
        SetupEnemy.ClearEnemy();
        SetupEnemy.AddEnemy("Android");
        SetupEnemy.AddEnemy("Drone");

        const float animationTime = 1.0f;

        // シーン遷移
        AlphaFadeManager.Instance.FadeOut(animationTime);

        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("Battle", LoadSceneMode.Single); });

        // Switch BGM
        AudioManager.Instance.PlayMusicWithCrossFade("BattleTutorial", 2.0f);
    }
}
