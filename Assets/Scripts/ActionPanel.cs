using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
enum CommandType
{
    Attack,
    Item,
    Skill,
}

public class ActionPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float animTime = 0.5f;
    [SerializeField] private Texture2D cursorTexture;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private Battle battleManager;
    [SerializeField] private TMPro.TMP_Text tipsText;
    [SerializeField] private Canvas mainCanvas;

    [Header("Debug")]
    [SerializeField] private bool isSelectingTarget;
    [SerializeField] private bool isSelectedTarget;
    [SerializeField] private CommandType commandType;

    private void Awake()
    {
        canvasGrp.alpha = 0.0f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        isSelectingTarget = false;
    }

    public void SetEnablePanel(bool boolean)
    {
        canvasGrp.DOFade(boolean ? 1.0f:0.0f, animTime);
        canvasGrp.interactable = boolean;
        canvasGrp.blocksRaycasts = boolean;

        isSelectingTarget = false;
    }

    public void OnClickAttack()
    {
        // �U�������I��
        isSelectingTarget = true;

        canvasGrp.alpha = 0.25f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        // �J�[�\����ύX
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f), CursorMode.Auto);

        // tips�\��
        tipsText.DOFade(1.0f, 0.25f);

        // �U��
        commandType = CommandType.Attack;
    }

    public void OnClickItem()
    {

        // �A�C�e��
        commandType = CommandType.Item;
    }

    public void OnClickSkill()
    {

        // ����Z
        commandType = CommandType.Item;
    }

    public void OnClickIdle()
    {
        // �X�L�b�v�^�[��
        battleManager.NextTurn(false);
    }

    private void Update()
    {
        if (isSelectingTarget)
        {
            // �E�N���b�N
            if (Input.GetMouseButtonDown(1))
            {
                CancelAttack();
            }
            else
            {
                // arrow that follow the mouse
                Vector3 mousePosition = Input.mousePosition / mainCanvas.scaleFactor;
                var targetBattler = battleManager.GetBattlerByPosition(mousePosition, true);

                if (!ReferenceEquals(targetBattler, null))
                {
                    isSelectedTarget = true;
                    battleManager.PointTargetWithArrow(targetBattler, 0.25f);

                    if (Input.GetMouseButtonDown(0))
                    {
                        switch (commandType)
                        {
                            case CommandType.Attack: // �U��
                                battleManager.AttackCommand(targetBattler);
                                CancelAttack();
                                break;
                            case CommandType.Item: // �A�C�e��

                                break;
                            case CommandType.Skill: // ����Z

                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (isSelectedTarget)
                {
                    battleManager.UnPointArrow(0.25f);
                }
            }
        }
    }

    private void CancelAttack()
    {
        if (commandType != CommandType.Attack) return;

        // ������
        canvasGrp.alpha = 1f;
        canvasGrp.interactable = true;
        canvasGrp.blocksRaycasts = true;

        isSelectingTarget = false;

        // �J�[�\����߂�
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // tips������
        tipsText.DOFade(0.0f, 0.25f);

        if (isSelectedTarget)
        {
            battleManager.UnPointArrow(0.25f);
        }
    }
}
