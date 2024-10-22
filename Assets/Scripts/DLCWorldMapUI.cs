using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;

public class DLCWorldMapUI : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private string BGM = "Specification";

    [Header("References")]
    [SerializeField] private StageHandler stagehandler;
    [SerializeField] private TMPro.TMP_Text chapterName;
    [SerializeField] private Button startButton;

    private void Start()
    {
        // BGM�Đ�
        AudioManager.Instance.PlayMusicWithCrossFade(BGM, 6.0f);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeIn(1.0f);

        if (chapterName != null)
        {
            chapterName.text = GetChapterName(ProgressManager.Instance.GetCurrentDLCStageProgress());
        }

        if (ProgressManager.Instance.IsDLCEnded())
        {
            if (stagehandler != null) stagehandler.gameObject.SetActive(false);
            if (chapterName != null) chapterName.gameObject.SetActive(false);
            if (startButton != null) startButton.gameObject.SetActive(false);
        }
    }

    public void BackButton()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        const float animationTime = 1.0f;

        string targetMap = "EndGameContent";
        if (ProgressManager.Instance.GetCurrentStageProgress() == 21)
        {
            targetMap = "Home";
        }

        StartCoroutine(SceneTransition(targetMap, animationTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // BGM��~
        AudioManager.Instance.StopMusicWithFade(1.0f);

        // �V�[���J��
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false; //Don't let the Scene activate until you allow it to

        AlphaFadeManager.Instance.FadeOut(animationTime);

        yield return new WaitForSeconds(animationTime);
        while (asyncLoad.progress < 0.9f) yield return null; // wait until the scene is completely loaded 

        asyncLoad.allowSceneActivation = true;
    }

    public void NextStory()
    {
        const float animationTime = 1.0f;

        DLCManager.isEnterDLCStage = true;

        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        DOTween.Sequence()
            .AppendInterval(animationTime)
            .AppendCallback(() => { SceneManager.LoadScene("Story", LoadSceneMode.Single); });

        // SE
        AudioManager.Instance.PlaySFX("QuestStart");

        // Switch BGM
        AudioManager.Instance.StopMusicWithFade(animationTime);

        // �I�[�g�Z�[�u�����s����
        AutoSave.ExecuteAutoSave();
    }

    // DLC_TODO
    private string GetChapterName(int progress)
    {
        return LocalizationManager.Localize("DLCStage-" + progress);
    }
}
