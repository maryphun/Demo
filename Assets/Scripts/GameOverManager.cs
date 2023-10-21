using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SaveLoadPanel loadPanel;

    private void Start()
    {
        AlphaFadeManager.Instance.FadeIn(0.5f);
    }

    public void Retry()
    {
        AlphaFadeManager.Instance.FadeOut(0.5f);

        DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => 
        {
            SceneManager.LoadScene("Battle");
        });
    }

    public void Load()
    {
        loadPanel.OpenSaveLoadPanel(true);
    }

    public void TitleMenu()
    {
        AlphaFadeManager.Instance.FadeOut(0.5f);

        DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
        {
            SceneManager.LoadScene("Title");
        });
    }
}
