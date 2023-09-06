using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeSceneUI : MonoBehaviour
{
    private void Start()
    {
        AlphaFadeManager.Instance.FadeIn(1.0f);
    }

    public void ToWorldMapScene()
    {
        const float animationTime = 1.0f;
        StartCoroutine(SceneTransition("WorldMap", animationTime));
    }

    IEnumerator SceneTransition(string sceneName, float animationTime)
    {
        // �V�[���J��
        AlphaFadeManager.Instance.FadeOut(animationTime);
        yield return new WaitForSeconds(animationTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
