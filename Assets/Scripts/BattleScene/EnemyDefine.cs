using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Enemy", menuName = "�쐬/�G�L��������")]
public class EnemyDefine : ScriptableObject
{
    [Header("��{����")]
    [SerializeField] public string enemyName = "�X���C��";
    [SerializeField] public int maxHP = 100;
    [SerializeField] public int maxMP = 100;
    [SerializeField] public int attack = 10;
    [SerializeField] public int defense = 10;
    [SerializeField] public int speed = 10;
    [SerializeField] public int level = 1;
    [SerializeField] public Color character_color = new Color(1,1,1,1);

    [Header("�A�C�R��")]
    [SerializeField] public Sprite icon;

    [Header("���t�@�����X")]
    [SerializeField] public GameObject battler;
}
