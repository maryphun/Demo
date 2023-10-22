using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.SimpleLocalization.Scripts;

[RequireComponent(typeof(CanvasGroup))]
public class TrainPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animationTime = 0.5f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image characterImg;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text currentMood;
    [SerializeField] private Image darkGaugeFill, holyCoreGaugeFill;
    [SerializeField] private RectTransform previousCharacterBtn, nextCharacterBtn;
    [SerializeField] private Button hornyActionBtn, corruptActionBtn, researchBtn;
    [SerializeField] private GameObject unavailablePanel;

    [Header("Debug")]
    [SerializeField] private Vector2 previousCharacterBtnPos, nextCharacterBtnPos;
    [SerializeField] private List<Character> characters;
    [SerializeField] private int currentIndex;

    public void OpenTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        // UI �t�F�C�h
        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // ������
        previousCharacterBtnPos = previousCharacterBtn.localPosition;
        nextCharacterBtnPos = nextCharacterBtn.localPosition;

        // �f�[�^�����[�h
        characters = ProgressManager.Instance.GetAllCharacter(false);

        // �q���C������Ȃ��L������r��
        currentIndex = 0;
        characters.RemoveAll(s => !s.characterData.is_heroin);

        if (characters.Count <= 0)
        {
            // �ߊl�����q���C�������Ȃ�
            unavailablePanel.SetActive(true);
            return;
        }
        else
        {
            characterImg.gameObject.SetActive(true);
        }
        UpdateCharacterData();
    }

    public void QuitTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemCancel");

        canvasGroup.DOFade(0.0f, animationTime);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // �f�[�^���N���A
        characters.Clear();
        characters = null;
    }

    public void NextCharacter()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemSwitch");

        // UI Animation
        const float animTime = 0.2f;
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                nextCharacterBtn.DOLocalMoveX(nextCharacterBtnPos.x + 10.0f, animTime * 0.5f);
            })
            .AppendInterval(animTime * 0.5f)
            .AppendCallback(() =>
            {
                nextCharacterBtn.DOLocalMoveX(nextCharacterBtnPos.x, animTime * 0.5f);
            });

        // Calculate Index
        currentIndex++;
        if (currentIndex >= characters.Count)
        {
            currentIndex = 0;
        }

        // Change character
        UpdateCharacterData();
    }

    public void PreviousCharacter()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemSwitch");

        // UI Animation
        const float animTime = 0.2f;
        DOTween.Sequence()
            .AppendCallback(() => 
            { 
                previousCharacterBtn.DOLocalMoveX(previousCharacterBtnPos.x - 10.0f, animTime * 0.5f); 
            })
            .AppendInterval(animTime * 0.5f)
            .AppendCallback(() => 
            {
                previousCharacterBtn.DOLocalMoveX(previousCharacterBtnPos.x, animTime * 0.5f);
            });

        // Calculate Index
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = characters.Count-1;
        }

        // Change character
        UpdateCharacterData();
    }

    public void UpdateCharacterData()
    {
        // ���O
        characterName.text = characters[currentIndex].localizedName;

        // �����𖞂����Ă���u�S��v
        for (int i = 0; i < characters[currentIndex].characterData.characterStatus.Count; i++)
        {
            if (characters[currentIndex].dark_gauge >= characters[currentIndex].characterData.characterStatus[i].requiredDarkGauge
                && characters[currentIndex].horny_gauge >= characters[currentIndex].characterData.characterStatus[i].requiredHornyGauge)
            {
                currentMood.text = LocalizationManager.Localize(characters[currentIndex].characterData.characterStatus[i].moodNameID);
                characterImg.sprite = characters[currentIndex].characterData.characterStatus[i].character;
            }
        }

        darkGaugeFill.fillAmount = ((float)characters[currentIndex].dark_gauge) / 100.0f;
        holyCoreGaugeFill.fillAmount = ((float)characters[currentIndex].holyCore_ResearchRate) / 100.0f;

        // �{�^�����X�V
        hornyActionBtn.interactable = characters[currentIndex].horny_gauge < 100.0f;
        corruptActionBtn.interactable = characters[currentIndex].dark_gauge < 100.0f;
        researchBtn.interactable = characters[currentIndex].holyCore_ResearchRate < 100.0f;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void HornyTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        characters[currentIndex].horny_gauge += 25;
        UpdateCharacterData();
    }

    /// <summary>
    /// ���]����
    /// </summary>
    public void BrainwashTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        characters[currentIndex].dark_gauge += 25;
        UpdateCharacterData();
    }

    /// <summary>
    /// ���j����
    /// </summary>
    public void HolyCoreResearch()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        characters[currentIndex].holyCore_ResearchRate += 25;
        UpdateCharacterData();
    }
}
