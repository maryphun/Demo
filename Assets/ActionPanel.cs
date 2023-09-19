using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
        // 攻撃相手を選ぶ
        isSelectingTarget = true;

        canvasGrp.alpha = 0.25f;
        canvasGrp.interactable = false;
        canvasGrp.blocksRaycasts = false;

        // カーソルを変更
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f), CursorMode.Auto);

        // tips表示
        tipsText.DOFade(1.0f, 0.25f);
    }

    public void OnClickItem()
    {

    }

    public void OnClickSkill()
    {

    }

    public void OnClickIdle()
    {
        // スキップターン
        battleManager.NextTurn(false);
    }

    private void Update()
    {
        if (isSelectingTarget)
        {
            // 右クリック
            if (Input.GetMouseButtonDown(1))
            {
                // 取り消し
                canvasGrp.alpha = 1f;
                canvasGrp.interactable = true;
                canvasGrp.blocksRaycasts = true;

                isSelectingTarget = false;

                // カーソルを戻す
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                // tipsを消す
                tipsText.DOFade(0.0f, 0.25f);

                if (isSelectedTarget)
                {
                    battleManager.UnPointArrow(0.25f);
                }
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
                }
                else if (isSelectedTarget)
                {
                    battleManager.UnPointArrow(0.25f);
                }
            }
        }
    }
}
