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

    [Header("�����V�[�����e�Ǘ�")]
    // �ŗ����V�[��
    [SerializeField] public Dictionary<int, List<string>> CharacterID_To_HornyNovelNameList = new Dictionary<int, List<string>>();     // ������
    [SerializeField] public Dictionary<int, List<string>> CharacterID_To_BrainwashNovelNameList = new Dictionary<int, List<string>>(); // ���]
    [SerializeField] public Dictionary<int, List<string>> CharacterID_To_ResearchNovelNameList = new Dictionary<int, List<string>>(); // ���j����

    void InitList()
    {
        // �������V�i���I���X�g
        CharacterID_To_HornyNovelNameList.Add(3, new List<string> { "Akiho/Horny_1", "Akiho/Horny_2", "Akiho/Horny_3" });
        CharacterID_To_HornyNovelNameList.Add(4, new List<string> { "Rikka/Horny_1", "Rikka/Horny_2", "Rikka/Horny_3" });
        CharacterID_To_HornyNovelNameList.Add(5, new List<string> { "Erena/Horny_1", "Erena/Horny_2", "Erena/Horny_3" });
        CharacterID_To_HornyNovelNameList.Add(6, new List<string> { "Kei/Horny_1", "Kei/Horny_2", "Kei/Horny_3" });
        CharacterID_To_HornyNovelNameList.Add(7, new List<string> { "Nayuta/Horny_1", "Nayuta/Horny_2", "Nayuta/Horny_3" });
        // ���]�V�i���I���X�g
        CharacterID_To_BrainwashNovelNameList.Add(3, new List<string> { "Akiho/BrainWash_1", "Akiho/BrainWash_2", "Akiho/BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(4, new List<string> { "Rikka/BrainWash_1", "Rikka/BrainWash_2", "Rikka/BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(5, new List<string> { "Erena/BrainWash_1", "Erena/BrainWash_2", "Erena/BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(6, new List<string> { "Kei/BrainWash_1", "Kei/BrainWash_2", "Kei/BrainWash_3" });
        CharacterID_To_BrainwashNovelNameList.Add(7, new List<string> { "Nayuta/BrainWash_1", "Nayuta/BrainWash_2", "Nayuta/BrainWash_3" });
        // ���j�����V�i���I���X�g
        CharacterID_To_ResearchNovelNameList.Add(3, new List<string> { "Akiho/Research_1", "Akiho/Research_2", "Akiho/Research_3" });
        CharacterID_To_ResearchNovelNameList.Add(4, new List<string> { "Rikka/Research_1", "Rikka/Research_2", "Rikka/Research_3" });
        CharacterID_To_ResearchNovelNameList.Add(5, new List<string> { "Erena/Research_1", "Erena/Research_2", "Erena/Research_3" });
        CharacterID_To_ResearchNovelNameList.Add(6, new List<string> { "Kei/Research_1", "Kei/Research_2", "Kei/Research_3" });
        CharacterID_To_ResearchNovelNameList.Add(7, new List<string> { "Nayuta/Research_1", "Nayuta/Research_2", "Nayuta/Research_3" });
    }

    private void Awake()
    {
        InitList();
    }

    public void OpenTrainPanel()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemOpen");

        // UI �t�F�C�h
        canvasGroup.DOFade(1.0f, animationTime);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // ������
        previousCharacterBtn.gameObject.SetActive(true);
        nextCharacterBtn.gameObject.SetActive(true);
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

            if (characters.Count == 1)
            {
                // 1�l�������Ȃ�
                previousCharacterBtn.gameObject.SetActive(false);
                nextCharacterBtn.gameObject.SetActive(false);
            }
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
            if (characters[currentIndex].corruptionEpisode >= characters[currentIndex].characterData.characterStatus[i].requiredCorruptionEpisode
                && characters[currentIndex].hornyEpisode >= characters[currentIndex].characterData.characterStatus[i].requiredHornyEpisode)
            {
                currentMood.text = LocalizationManager.Localize(characters[currentIndex].characterData.characterStatus[i].moodNameID);
                characterImg.sprite = characters[currentIndex].characterData.characterStatus[i].character;
            }
        }

        darkGaugeFill.fillAmount = ((float)characters[currentIndex].corruptionEpisode) / (float)CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID].Count;
        holyCoreGaugeFill.fillAmount = ((float)characters[currentIndex].holyCoreEpisode) / (float)CharacterID_To_ResearchNovelNameList[characters[currentIndex].characterData.characterID].Count;

        // �{�^�����X�V (Has next scenario?)
        hornyActionBtn.interactable = characters[currentIndex].hornyEpisode < CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID].Count;
        corruptActionBtn.interactable = characters[currentIndex].corruptionEpisode < CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID].Count;
        researchBtn.interactable = characters[currentIndex].holyCoreEpisode < CharacterID_To_ResearchNovelNameList[characters[currentIndex].characterData.characterID].Count;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void HornyTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        List<string> episodeList = CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID];

        // �V�i���I�Đ�
        canvasGroup.interactable = false;

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() => 
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);
            NovelSingletone.Instance.PlayNovel("TrainScene/" + episodeList[characters[currentIndex].hornyEpisode], true, ReturnFromEpisode);
            characters[currentIndex].hornyEpisode++;
        });
    }

    /// <summary>
    /// ���]����
    /// </summary>
    public void BrainwashTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        List<string> episodeList = CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID];

        // �V�i���I�Đ�
        AlphaFadeManager.Instance.FadeOut(1.0f);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() =>
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);
            NovelSingletone.Instance.PlayNovel("TrainScene/" + episodeList[characters[currentIndex].corruptionEpisode], true, ReturnFromEpisode);
            characters[currentIndex].corruptionEpisode++;
        });
    }

    /// <summary>
    /// ���j����
    /// </summary>
    public void HolyCoreResearch()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        List<string> episodeList = CharacterID_To_ResearchNovelNameList[characters[currentIndex].characterData.characterID];

        // �V�i���I�Đ�
        AlphaFadeManager.Instance.FadeOut(1.0f);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() =>
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);
            NovelSingletone.Instance.PlayNovel("TrainScene/" + episodeList[characters[currentIndex].holyCoreEpisode], true, ReturnFromEpisode);
            characters[currentIndex].holyCoreEpisode++;
        });
    }

    private void ReturnFromEpisode()
    {
        AlphaFadeManager.Instance.FadeOutThenFadeIn(1.0f);
        UpdateCharacterData();
        canvasGroup.interactable = true;
    }
}
