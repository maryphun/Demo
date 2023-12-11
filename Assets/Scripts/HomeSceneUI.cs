using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeSceneUI : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool isDebug = false;

    private void Awake()
    {
        if (isDebug) ProgressManager.Instance.DebugModeInitialize();
    }

    private void Start()
    {
        AlphaFadeManager.Instance.FadeIn(1.0f);

        AudioManager.Instance.PlayMusicWithFade("Loop 32 (HomeScene)", 2.0f);

        // HomeScene�ɓ��邽�тɃI�[�g�Z�[�u�����s����
        AutoSave.ExecuteAutoSave();
    }

    public void ToWorldMapScene()
    {
        // SE�Đ�
        AudioManager.Instance.PlaySFX("SystemPrebattle");

        AudioManager.Instance.PauseMusic();

        const float animationTime = 1.0f;
        StartCoroutine(SceneTransition("WorldMap", animationTime));
    }
    public void ToBlackMarketScene()
    {
        // SE�Đ�
        AudioManager.Instance.PlaySFX("BattleTransition");

        const float animationTime = 1.0f;
        AudioManager.Instance.StopMusicWithFade(animationTime);
        StartCoroutine(SceneTransition("BlackMarket", animationTime));
    }

    public IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        yield return new WaitForSeconds(animationTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
