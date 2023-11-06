using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NovelEditor;

public class ScenarioDebuggerUI : MonoBehaviour
{
    [SerializeField] Canvas UICanvas;
    [SerializeField] NovelPlayer player;
    [SerializeField] NovelData data;

    private bool isPlaying = false;

    public void PlayScript()
    {
        UICanvas.enabled = false;
        NovelSingletone.Instance.PlayNovel(data, true);
        isPlaying = true;
    }

    private void Update()
    {
        if (isPlaying)
        {
            // �I���`�F�b�N
            if (NovelSingletone.Instance.IsEnded())
            {
                // �I��
                Debug.Log("End");
                isPlaying = false;
                UICanvas.enabled = true;
            }
        }
    }
}
