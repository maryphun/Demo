using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;

// �ǂ̃L�����N�^�[�̃^�[���ɂȂ�̂��Ǘ�����N���X
public class TurnBase : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Range(0.0f, 2.0f)] private float rearrangeAnimationTime = 1.0f;
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
        // ������
        characterInOrder = new List<Tuple<Battler, Image>>();

        // List����������
        battlerList = new List<Battler>(playerCharacters); // ���̃f�[�^���e�����Ȃ����߂ɃR�s�[������Ƃ�
        battlerList = battlerList.Concat(enemies).ToList();

        // ���S�������̂�r��
        battlerList.RemoveAll(s => s.isAlive == false);

        // �l�����̃A�C�R���𐶐�����
        for (int i = 0; i < battlerList.Count; i++)
        {
            iconList.Add(Instantiate(originIcon, transform));
            iconList[i].color = Color.white;
            iconList[i].sprite = battlerList[i].icon;
            iconList[i].GetComponent<TurnBaseInformation>().Initialize(battlerList[i].character_color, battlerList[i].character_name, battlerList[i].speed.ToString());
            characterInOrder.Add(new Tuple<Battler, Image>(battlerList[i], iconList[i]));
        }

        // �s�����Ԃ��v�Z
        characterInOrder.Sort((a, b) =>
        {
            // �u�f�����v���r����
           int speedComparison = b.Item1.speed.CompareTo(a.Item1.speed);
            if (speedComparison != 0)
            {
                return speedComparison;
            }
            else
            {
                // �u�f�����v�������ꍇ�A�v���C���[�L�����N�^�[��D�悷��
                return a.Item1.isEnemy.CompareTo(b.Item1.isEnemy);
            }
        });

        // �A�C�R�������
        IconArrangeInstant();
    }

    /// <summary>
    /// �ŏ�����X�V
    /// </summary>
    public void UpdateTurn(bool rearrange)
    {
        // ���^�C�A�̃L���������邩���m�F
        for (int i = 0; i < characterInOrder.Count; i++)
        {
            if (!characterInOrder[i].Item1.isAlive)
            {
                // �A�C�R�����\����
                characterInOrder[i].Item2.DOColor(new Color(0,0,0,0), rearrangeAnimationTime);

                characterInOrder.RemoveAt(i);
                i--;
            }
        }

        if (rearrange)
        {
            IconArrangeInstant();
        }
    }

    public Battler GetCurrentTurnBattler()
    {
        return characterInOrder.First().Item1;
    }

    // ���ɏ��Ԃ�����Ă���v���C���[�L�����N�^�[���l��
    public Battler GetNextPlayerChaacter()
    {
        for (int i = 0; i < characterInOrder.Count; i++) 
        {
            if (!characterInOrder[i].Item1.isEnemy)
            {
                return characterInOrder[i].Item1;
            }
        }

        return null;
    }

    // �v���C���[�L�����N�^�[�������_���Ɋl��
    public Battler GetRandomPlayerCharacter()
    {
        Battler randomPlayer = characterInOrder[UnityEngine.Random.Range(0, characterInOrder.Count)].Item1;
        if (!randomPlayer.isEnemy)
        {
            return randomPlayer;
        }

        return GetRandomPlayerCharacter();
    }

    // �G�L�����������_���Ɏ擾
    public Battler GetRandomEnemyCharacter()
    {
        Battler randomPlayer = characterInOrder[UnityEngine.Random.Range(0, characterInOrder.Count)].Item1;
        if (randomPlayer.isEnemy)
        {
            return randomPlayer;
        }

        return GetRandomPlayerCharacter();
    }


    // �G�L�����������_���Ɏ擾 (HP�Ⴂ����D��)
    public Battler GetEnemyCharacterWithLowestHP()
    {
        // Get the enemy with the lowest current_hp
        var lowestHpEnemy = characterInOrder
            .Where(tuple => tuple.Item1.isEnemy) // Filter out only enemy battlers
            .OrderBy(tuple => tuple.Item1.current_hp) // Order by current_hp in ascending order
            .FirstOrDefault(); // Take the first (lowest) enemy battler or null if the list is empty

        if (lowestHpEnemy == null)
        {
            // enemy list is empty
            Debug.Log("Enemy list is empty");
            return null;
        }

        return lowestHpEnemy.Item1;
    }

    /// �A�C�R�������
    private void IconArrangeInstant()
    {
        float iconPosition = startPosition;
        for (int i = 0; i < characterInOrder.Count; i++)
        {
            var iconRect = characterInOrder[i].Item2.GetComponent<RectTransform>();
            iconRect.localPosition = new Vector3(iconPosition, iconRect.localPosition.y, iconRect.localPosition.z);
            iconPosition += iconRect.rect.width + gapSpace;
        }
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
        yield return new WaitForSeconds(rearrangeAnimationTime * 0.5f);

        // �A�C�R�����ĕ���
        float iconPosition = startPosition;
        for (int i = 0; i < characterInOrder.Count -1; i++)
        {
            var iconRect = characterInOrder[i].Item2.GetComponent<RectTransform>();
            iconRect.DOLocalMoveX(iconPosition, 0.5f);
            iconPosition += iconRect.rect.width + gapSpace;
        }

        yield return new WaitForSeconds(rearrangeAnimationTime * 0.5f);

        int lastIndex = characterInOrder.Count - 1;
        var lastIconRect = characterInOrder[lastIndex].Item2.GetComponent<RectTransform>();
        lastIconRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        lastIconRect.localPosition = new Vector3(iconPosition + 200.0f, lastIconRect.localPosition.y, lastIconRect.localPosition.z);

        lastIconRect.DOLocalMoveX(iconPosition, 0.5f);
        characterInOrder[lastIndex].Item2.DOFade(1.0f, 0.5f);
    }
}
