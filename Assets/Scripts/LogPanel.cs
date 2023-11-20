using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LogPanel : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 1.0f)] private float fadeAnimationTime = 0.5f;
    [SerializeField] private Sprite dialogueBoxWithName; 
    [SerializeField] private Sprite dialogueBoxWithoutName;

    [Header("References")]
    [SerializeField] private LogDialogField logDialogOrigin;
    [SerializeField] private CanvasGroup logPanelCanvas;
    [SerializeField] private RectTransform scrollSize;

    [Header("Debug")]
    [SerializeField] private List<LogDialogField> logObjs;

    public void OpenPanel()
    {
        logPanelCanvas.DOFade(1.0f, fadeAnimationTime);
        logPanelCanvas.interactable = true;
        logPanelCanvas.blocksRaycasts = true;

        SetupLogPanel();
    }

    public void ClosePanel()
    {
        logPanelCanvas.DOFade(0.0f, fadeAnimationTime);
        logPanelCanvas.interactable = false;
        logPanelCanvas.blocksRaycasts = false;
    }

    private void SetupLogPanel()
    {
        logDialogOrigin.gameObject.SetActive(false);
        var log = LoggerManager.Instance.GetLog();

        logObjs = new List<LogDialogField>();

        const float LogHeight = 170;
        const float LogGap = 20;

        for (int i = 0; i < log.Count; i++)
        {
            logObjs.Add(Instantiate(logDialogOrigin.gameObject, logDialogOrigin.transform.parent).GetComponent<LogDialogField>());

            // �r������������ύX
            if (log[i].Item1 == string.Empty)
            {
                // ���O�Ȃ�
                logObjs[i].Sprite.sprite = dialogueBoxWithoutName;
            }
            else
            {
                logObjs[i].Sprite.sprite = dialogueBoxWithName;
            }

            // ���e��������
            logObjs[i].Name.text = log[i].Item1;
            logObjs[i].Dialog.text = log[i].Item2;

            // �ʒu����
            logObjs[i].Sprite.rectTransform.anchoredPosition = new Vector2(0.0f, LogGap + ((LogHeight+LogGap) * i));

            // �\��
            logObjs[i].gameObject.SetActive(true);
        }

        const float MinScrollSize = 800.0f;
        scrollSize.sizeDelta = new Vector2(0.0f, Mathf.Max(MinScrollSize, (LogHeight + LogGap) * (log.Count)));
    }
}
