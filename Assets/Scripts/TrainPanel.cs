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
    [SerializeField] private TMP_Text hornyActionCost, corruptActionCost, researchCost;
    [SerializeField] private GameObject unavailablePanel;
    [SerializeField] private CanvasGroup underDevelopmentPopUp;
    [SerializeField] private CanvasGroup newBattlerPopup;
    [SerializeField] private TMPro.TMP_Text newBattlerPopupText;
    [SerializeField] private CanvasGroup researchPointPanel;
    [SerializeField] private HomeCharacter homeCharacterScript;

    [Header("Debug")]
    [SerializeField] private Vector2 previousCharacterBtnPos, nextCharacterBtnPos;
    [SerializeField] private List<Character> characters;
    [SerializeField] private int currentIndex;
    [SerializeField] private int[] cost = new int[3];

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
        var characterStatus = characters[currentIndex].GetCurrentStatus();
        currentMood.text = LocalizationManager.Localize(characterStatus.moodNameID);
        characterImg.sprite = characterStatus.character;

        darkGaugeFill.fillAmount = ((float)characters[currentIndex].corruptionEpisode) / (float)CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID].Count;
        holyCoreGaugeFill.fillAmount = ((float)characters[currentIndex].holyCoreEpisode) / (float)CharacterID_To_ResearchNovelNameList[characters[currentIndex].characterData.characterID].Count;

        // �{�^�����X�V (Has next scenario?)
        hornyActionBtn.interactable = characters[currentIndex].hornyEpisode < CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID].Count;
        corruptActionBtn.interactable = characters[currentIndex].corruptionEpisode < CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID].Count;
        researchBtn.interactable = characters[currentIndex].holyCoreEpisode < CharacterID_To_ResearchNovelNameList[characters[currentIndex].characterData.characterID].Count;

        // ������
        if (!hornyActionBtn.interactable)
        {
            // (��������)
            hornyActionCost.text = "<color=#d400ff><size=25>" + LocalizationManager.Localize("System.ResearchComplete");
        }
        else
        {
            cost[0] = (characters[currentIndex].hornyEpisode * 50) + 50;
            hornyActionCost.text = LocalizationManager.Localize("System.ResearchPointCost") + "\n<size=25><color=#ed94ff>" + cost[0];

            // �|�C���g�s��
            if (ProgressManager.Instance.GetCurrentResearchPoint() < cost[0]) hornyActionBtn.interactable = false;
        }

        // �ŗ���
        if (!corruptActionBtn.interactable)
        {
            // (��������)
            corruptActionCost.text = "<color=#d400ff><size=25>" + LocalizationManager.Localize("System.ResearchComplete");
        }
        else
        {
            cost[1] = (characters[currentIndex].corruptionEpisode * 50) + 50;
            corruptActionCost.text = LocalizationManager.Localize("System.ResearchPointCost") + "\n<size=25><color=#ed94ff>" + cost[1];

            // �|�C���g�s��
            if (ProgressManager.Instance.GetCurrentResearchPoint() < cost[1]) corruptActionBtn.interactable = false;
        }

        // ���j����
        if (!researchBtn.interactable)
        {
            // (��������)
            researchCost.text = "<color=#d400ff><size=25>" + LocalizationManager.Localize("System.ResearchComplete");
        }
        else
        {
            cost[2] = (characters[currentIndex].holyCoreEpisode * 50) + 50;
            researchCost.text = LocalizationManager.Localize("System.ResearchPointCost") + "\n<size=25><color=#ed94ff>" + cost[2];

            // �|�C���g�s��
            if (ProgressManager.Instance.GetCurrentResearchPoint() < cost[2]) researchBtn.interactable = false;
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void HornyTraining()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        // �b��
        {
            underDevelopmentPopUp.DOFade(1.0f, 0.5f);
            underDevelopmentPopUp.interactable = true;
            underDevelopmentPopUp.blocksRaycasts = true;
            return; // �J����
        }

        // BGM ��~
        AudioManager.Instance.PauseMusic();

        List<string> episodeList = CharacterID_To_HornyNovelNameList[characters[currentIndex].characterData.characterID];

        // �V�i���I�Đ�
        canvasGroup.interactable = false;

        // �|�C���g����
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() - cost[0]);

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
        // BGM ��~
        AudioManager.Instance.PauseMusic();

        List<string> episodeList = CharacterID_To_BrainwashNovelNameList[characters[currentIndex].characterData.characterID];

        // �V�i���I�Đ�
        AlphaFadeManager.Instance.FadeOut(1.0f);

        // �|�C���g����
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() - cost[1]);

        // ��ʑJ��
        AlphaFadeManager.Instance.FadeOut(0.5f);
        DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() =>
        {
            AlphaFadeManager.Instance.FadeIn(0.5f);
            NovelSingletone.Instance.PlayNovel("TrainScene/" + episodeList[characters[currentIndex].corruptionEpisode], true, ReturnFromEpisode);
            characters[currentIndex].corruptionEpisode++;
        });

        // �L�����l��?
        if (characters[currentIndex].corruptionEpisode >= episodeList.Count - 1)
        {
            // �ŗ���
            characters[currentIndex].is_corrupted = true;
        }
    }

    /// <summary>
    /// ���j����
    /// </summary>
    public void HolyCoreResearch()
    {
        // SE �Đ�
        AudioManager.Instance.PlaySFX("SystemTrainPanel");

        // �b��
        {
            underDevelopmentPopUp.DOFade(1.0f, 0.5f);
            underDevelopmentPopUp.interactable = true;
            underDevelopmentPopUp.blocksRaycasts = true;
            return; // �J����
        }

        // BGM ��~
        AudioManager.Instance.PauseMusic();

        List<string> episodeList = CharacterID_To_ResearchNovelNameList[characters[currentIndex].characterData.characterID];

        // �V�i���I�Đ�
        AlphaFadeManager.Instance.FadeOut(1.0f);

        // �|�C���g����
        ProgressManager.Instance.SetResearchPoint(ProgressManager.Instance.GetCurrentResearchPoint() - cost[2]);

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
        // update heroin data
        if (characters[currentIndex].is_corrupted)
        {
            CallNewBattlerPopUp();
            AddNewHomeCharacter(characters[currentIndex].characterData.characterID);

            // update character name
            characters[currentIndex].localizedName = LocalizationManager.Localize(characters[currentIndex].characterData.corruptedName);
        }

        canvasGroup.interactable = true;
        UpdateCharacterData();

        // Play home music
        AudioManager.Instance.PlayMusicWithFade("HomeScene", 4.0f);
    }

    public void CloseUnderDevelopmentPopup()
    {
        underDevelopmentPopUp.DOKill(false);
        underDevelopmentPopUp.DOFade(0.0f, 0.1f).OnComplete(() =>
        { 
            underDevelopmentPopUp.interactable = false;
            underDevelopmentPopUp.blocksRaycasts = false;
        });
    }

    public void CallNewBattlerPopUp()
    {
        // SE
        AudioManager.Instance.PlaySFX("SystemNewHeroin");

        // Update Text
        newBattlerPopupText.text = CorruptedMessage(characters[currentIndex].characterData.characterID);

        // UI
        newBattlerPopup.DOKill(false);
        newBattlerPopup.DOFade(1.0f, 0.5f);
        newBattlerPopup.interactable = true;
        newBattlerPopup.blocksRaycasts = true;
    }
    public void CloseNewBattlerPopup()
    {
        newBattlerPopup.DOKill(false);
        newBattlerPopup.DOFade(0.0f, 0.1f).OnComplete(() =>
        {
            newBattlerPopup.interactable = false;
            newBattlerPopup.blocksRaycasts = false;
        });
    }

    /// <summary>
    /// �ŗ��������̃V�X�e�����b�Z�[�W
    /// </summary>
    public string CorruptedMessage(int characterID)
    {
        string s = string.Empty;
        switch ((PlayerCharacerID)characterID)
        {
            case PlayerCharacerID.Akiho: // ����
                s = "<color=#FFC0CB>" + LocalizationManager.Localize("Name.Akiho") + "</color>";
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Rikka: // ����
                s = "<color=#ADD8E6>" + LocalizationManager.Localize("Name.Rikka") + "</color>";
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Erena: // �G���i
                s = "<color=#F1E5AC>" + LocalizationManager.Localize("Name.Erena") + "</color>";
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Kei: // ��
                s = "<color=#ADD8E6>" + LocalizationManager.Localize("Name.Kei") + "</color>";
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            case PlayerCharacerID.Nayuta: // �ߗR��
                s = "<color=#8b0000>" + LocalizationManager.Localize("Name.Nayuta") + "</color>";
                return LocalizationManager.Localize("System.NewBattler").Replace("{s}", s);
            default:
                return string.Empty;
        }
    }

    // �z�[���䎌�L�����ǉ�
    public void AddNewHomeCharacter(int characterID)
    {
        switch (characterID)
        {
            case 3: // ����
                HomeDialogue akiho = Resources.Load<HomeDialogue>("HomeDialogue/Akiho");
                ProgressManager.Instance.AddHomeCharacter(akiho);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 4: // ����
                HomeDialogue rikka = Resources.Load<HomeDialogue>("HomeDialogue/Rikka");
                ProgressManager.Instance.AddHomeCharacter(rikka);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 5: // �G���i
                HomeDialogue erena = Resources.Load<HomeDialogue>("HomeDialogue/Erena");
                ProgressManager.Instance.AddHomeCharacter(erena);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 6: // ��
                HomeDialogue kei = Resources.Load<HomeDialogue>("HomeDialogue/Kei");
                ProgressManager.Instance.AddHomeCharacter(kei);
                homeCharacterScript.SetToLastCharacter();
                return;
            case 7: // �ߗR��
                HomeDialogue nayuta = Resources.Load<HomeDialogue>("HomeDialogue/Nayuta");
                ProgressManager.Instance.AddHomeCharacter(nayuta);
                homeCharacterScript.SetToLastCharacter();
                return;
            default:
                return;
        }
    }

    public void DisplayResearchPointPanel(bool display)
    {
        researchPointPanel.DOFade(display ? 1.0f: 0.0f, 0.5f);
    }
}
