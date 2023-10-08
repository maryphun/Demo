using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
}
