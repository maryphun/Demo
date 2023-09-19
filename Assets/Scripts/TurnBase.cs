using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;

// どのキャラクターのターンになるのか管理するクラス
public class TurnBase : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float startPosition = -350.0f;
    [SerializeField] private float gapSpace = 5.0f;

    [Header("References")]
    [SerializeField] private Image originIcon;

    [Header("Debug")]
    [SerializeField] private List<Battler> battlerList;
    [SerializeField] private List<Image> iconList;
    [SerializeField] private List<Tuple<Battler, Image>> characterInOrder;

    public void Initialization(List<Battler> playerCharacters, List<Battler> enemies)
    {
        // 初期化
        characterInOrder = new List<Tuple<Battler, Image>>();

        // Listを合成する
        battlerList = new List<Battler>(playerCharacters); // 元のデータを影響しないためにコピーを作っとく
        battlerList = battlerList.Concat(enemies).ToList();

        // 人数分のアイコンを生成する
        for (int i = 0; i < battlerList.Count; i++)
        {
            iconList.Add(Instantiate(originIcon, transform));
            iconList[i].color = Color.white;
            iconList[i].sprite = battlerList[i].icon;
            iconList[i].GetComponent<TurnBaseInformation>().Initialize(battlerList[i].character_color, battlerList[i].character_name, battlerList[i].speed.ToString());
            characterInOrder.Add(new Tuple<Battler, Image>(battlerList[i], iconList[i]));
        }

        // 行動順番を計算
        characterInOrder.Sort((a, b) =>
        {
            // 「素早さ」を比較する
           int speedComparison = b.Item1.speed.CompareTo(a.Item1.speed);
            if (speedComparison != 0)
            {
                return speedComparison;
            }
            else
            {
                // 「素早さ」が同じ場合、プレイヤーキャラクターを優先する
                return a.Item1.isEnemy.CompareTo(b.Item1.isEnemy);
            }
        });

        // アイコンを並ぶ
        float iconPosition = startPosition;
        for (int i = 0; i < characterInOrder.Count; i++)
        {
            var iconRect = characterInOrder[i].Item2.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(iconPosition, iconRect.localPosition.y, iconRect.localPosition.z);
            iconPosition += iconRect.rect.width + gapSpace;
        }
    }

    public Battler GetCurrentTurnBattler()
    {
        return characterInOrder.First().Item1;
    }

    public void NextBattler()
    {
        // Move the first character to the last position
        var firstElement = characterInOrder.First();
        characterInOrder.RemoveAt(0);
        characterInOrder.Add(firstElement);

        // UI
        firstElement.Item2.DOFade(0.0f, 0.5f);
        firstElement.Item2.GetComponent<RectTransform>().DOScale(2.0f, 0.5f);

        StartCoroutine(IconArrangeAnimation());
    }

    IEnumerator IconArrangeAnimation()
    {
        yield return new WaitForSeconds(0.5f);

        // アイコンを再並ぶ
        float iconPosition = startPosition;
        for (int i = 0; i < characterInOrder.Count -1; i++)
        {
            var iconRect = characterInOrder[i].Item2.GetComponent<RectTransform>();
            iconRect.DOLocalMoveX(iconPosition, 0.5f);
            iconPosition += iconRect.rect.width + gapSpace;
        }

        yield return new WaitForSeconds(0.5f);

        int lastIndex = characterInOrder.Count - 1;
        var lastIconRect = characterInOrder[lastIndex].Item2.GetComponent<RectTransform>();
        lastIconRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        lastIconRect.localPosition = new Vector3(iconPosition + 200.0f, lastIconRect.localPosition.y, lastIconRect.localPosition.z);

        lastIconRect.DOLocalMoveX(iconPosition, 0.5f);
        characterInOrder[lastIndex].Item2.DOFade(1.0f, 0.5f);
    }
}
