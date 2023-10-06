using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization.Scripts;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class BattleSceneTutorial : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup tutorialUI;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private CanvasGroup attackBtn, skillBtn, itemBtn, idleBtn;

    [Header("Debug")]
    [SerializeField] private Tween currentTween;
    [SerializeField] private TutorialStep step;

    enum TutorialStep
    {
        Basic,
        TurnBase,
        Speed, //���Ԃ͍s�����x�ɂ���Č��܂�A�s�����x�������L��������s����I���ł���B
        Action, // �L�����N�^�[������s����4��ނ���܂��B
        Attack, // �u�U���v�͈�Ԋ�{�̍U����i�ɂȂ�܂��B�U���҂Ƒ���̃��x���Ń_���[�W���e������܂��B
        AttackDamage, // ����͑���̕������|�I�Ƀ��x������Ȃ̂ŁA���܂�_���[�W��^�����Ȃ��ł��傤�B
        Skill, // �u����Z�v���g�p���āA�L�����N�^�[�̌ŗL�X�L���𔭓��ł��܂��B
        SkillLearn, // ����Z�̓��x���A�b�v�ȂǂŏK���ł��܂��B
        Item, // �u�A�C�e���v�̓`�[���S�̂ŋ��L����܂����A�g�p����L�����N�^�[�̃^�[�������Ղ��܂��B
        ItemUse, // �N���g���̂����l���Ȃ���΂Ȃ�܂���B
        Idle, // �u�ҋ@�v���������^�[�����I�����邱�ƂɂȂ�܂����ASP���񕜂��邱�Ƃ��ł��܂��B
        BattleStart, // �ł́A���ۂɐ���Ă݂܂��傤�B
    }

    private void Start()
    {
        tutorialUI.alpha = 0.0f;
        tutorialUI.blocksRaycasts = false;
        tutorialUI.interactable = false;
    }

    // Battle Scene�p�`���[�g���A���V�[�P���X
    public void StartBattleTutorial()
    {
        tutorialUI.DOFade(1.0f, 0.5f);
        tutorialUI.blocksRaycasts = true;
        tutorialUI.interactable = true;
        step = TutorialStep.Basic;

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-1"), 1.0f);
                    StartCoroutine(WaitForInput());
                    step = TutorialStep.Basic;
                });
    }

    IEnumerator WaitForInput()
    {
        yield return new WaitUntil(IsMouseButtonDown);
        Debug.Log("Input Detected");

        if (currentTween.IsPlaying())
        {
            Debug.Log("Tween not completed");
            currentTween.Complete();
        }
        else
        {
            // next
            switch (step)
            {
                case TutorialStep.Basic:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-2"), 1.0f);
                    step = TutorialStep.TurnBase;
                    break;
                case TutorialStep.TurnBase:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-3"), 1.0f);
                    step = TutorialStep.Speed;
                    break;
                case TutorialStep.Speed:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-4"), 1.0f);
                    step = TutorialStep.Action;
                    attackBtn.DOFade(1.0f, 0.5f);
                    skillBtn.DOFade(1.0f, 0.5f);
                    itemBtn.DOFade(1.0f, 0.5f);
                    idleBtn.DOFade(1.0f, 0.5f);
                    break;
                case TutorialStep.Action:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-5"), 1.0f);
                    step = TutorialStep.Attack;
                    skillBtn.DOFade(0.0f, 0.5f);
                    itemBtn.DOFade(0.0f, 0.5f);
                    idleBtn.DOFade(0.0f, 0.5f);
                    break;
                case TutorialStep.Attack:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-6"), 1.0f);
                    step = TutorialStep.AttackDamage;
                    break;
                case TutorialStep.AttackDamage:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-7"), 1.0f);
                    step = TutorialStep.Skill;
                    attackBtn.DOFade(0.0f, 0.5f);
                    skillBtn.DOFade(1.0f, 0.5f);
                    break;
                case TutorialStep.Skill:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-8"), 1.0f);
                    step = TutorialStep.SkillLearn;
                    break;
                case TutorialStep.SkillLearn:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-9"), 1.0f);
                    step = TutorialStep.Item;
                    skillBtn.DOFade(0.0f, 0.5f);
                    itemBtn.DOFade(1.0f, 0.5f);
                    break;
                case TutorialStep.Item:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-10"), 1.0f);
                    step = TutorialStep.ItemUse;
                    break;
                case TutorialStep.ItemUse:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-11"), 1.0f);
                    step = TutorialStep.Idle;
                    itemBtn.DOFade(0.0f, 0.5f);
                    idleBtn.DOFade(1.0f, 0.5f);
                    break;
                case TutorialStep.Idle:
                    currentTween = tutorialText.DOText(LocalizationManager.Localize("Dialog.Tutorial-3-12"), 1.0f);
                    step = TutorialStep.BattleStart;
                    idleBtn.DOFade(0.0f, 0.5f);
                    break;
                case TutorialStep.BattleStart:
                    {
                        // End Battle Tutorial
                        tutorialUI.alpha = 0.0f;
                        tutorialUI.blocksRaycasts = false;
                        tutorialUI.interactable = false;
                        break;
                    }
            }
        }

        yield return null;

        // �ċA
        StartCoroutine(WaitForInput());
    }

    bool IsMouseButtonDown()
    {
        return Input.GetMouseButtonDown(0);
    }
}
