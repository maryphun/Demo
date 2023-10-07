using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class TrainPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    public void OpenTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void QuitTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void NextCharacter()
    {
        
    }

    public void PreviousCharacter()
    {

    }

    /// <summary>
    /// ��������
    /// </summary>
    public void HornyTraining()
    {

    }

    /// <summary>
    /// ���]����
    /// </summary>
    public void BrainwashTraining()
    {

    }

    /// <summary>
    /// ���j����
    /// </summary>
    public void HolyCoreResearch()
    {

    }
}
