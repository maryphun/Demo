using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MemoryPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;
    [SerializeField] private Sprite lockedMemorySprite;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private Transform memoryHandle;

    public void OpenMemoryPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        // UI �t�F�C�h
        canvasGrp.DOFade(1.0f, animationTime);
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        // ������
        SetupMemories();
    }

    public void CloseMemoryPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGrp.DOFade(0.0f, animationTime);
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;
    }

    /// <summary>
    /// ������
    /// </summary>
    private void SetupMemories()
    {
        for (int i = 0; i < memoryHandle.childCount; i++)
        {
            if (memoryHandle.GetChild(i).gameObject.activeSelf)
            {
                memoryHandle.GetChild(i).GetComponent<MemorySlot>().Initialization(lockedMemorySprite);
            }
        }
    }
}
