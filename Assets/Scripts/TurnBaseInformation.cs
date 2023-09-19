using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

public class TurnBaseInformation : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationSpeed = 0.25f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image informationHover;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text characterSpeed;

    [Header("Debug")]
    [SerializeField] private bool isDisplay = false;

    // Start is called before the first frame update
    void Awake()
    {
        canvasGroup.alpha = 0.0f;
        isDisplay = false;
    }

    public void Initialize(Color characterColor, string name, string speed)
    {
        characterName.text = name;
        characterName.color = characterColor;
        characterSpeed.text = LocalizationManager.Localize("System.Speed") + speed;
    }

    public void OnHover()
    {
        if (isDisplay) return;

        isDisplay = true;
        canvasGroup.DOComplete();
        canvasGroup.DOFade(1.0f, animationSpeed);
    }

    public void UnHover()
    {
        if (!isDisplay) return;

        isDisplay = false;
        //�@instant
        canvasGroup.DOComplete();
        canvasGroup.alpha = 0.0f;
    }
}
