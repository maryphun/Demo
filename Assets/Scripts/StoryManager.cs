using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private bool isAfterBattle = false;

    // Start is called before the first frame update
    void Start()
    {
        // ��ʑJ��
        AlphaFadeManager.Instance.FadeIn(1.0f);

        if (isAfterBattle)
        {
            AfterStory();
        }
        else
        {
            Story();
        }
    }

    /// <summary>
    /// �퓬�O�p
    /// </summary>
    void Story()
    {
        BattleSetup.Reset(true);
        switch (ProgressManager.Instance.GetCurrentStageProgress())
        {
            case 2:
                {
                    BattleSetup.AddEnemy("Drone");
                    BattleSetup.AddEnemy("Android");
                    StartCoroutine(SceneTransition("Battle", 0));
                    AudioManager.Instance.PlayMusicWithCrossFade("BattleTutorial", 2.0f);
                }
                break;
            case 3:
                {
                    BattleSetup.AddEnemy("Akiho");
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// �퓬��p
    /// </summary>
    void AfterStory()
    {
        switch (ProgressManager.Instance.GetCurrentStageProgress())
        {
            case 2:
                {
                    NovelSingletone.Instance.PlayNovel("Chapter1-1", true, GoToHomeScreen);
                }
                break;
            default:
                break;
        }
    }

    public void GoToBattle()
    {
        StartCoroutine(SceneTransition("Battle", 0.5f));
    }

    public void GoToHomeScreen()
    {
        StartCoroutine(SceneTransition("Home", 0.5f));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        if (animationTime > 0)
        {
            yield return new WaitForSeconds(animationTime);
        }
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
