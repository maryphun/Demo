using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Enemy", menuName = "�쐬/�G�L��������")]
public class EnemyDefine : ScriptableObject
{
    [Header("��{����")]
    [SerializeField] public string enemyName = "�X���C��";
    [SerializeField] public int maxHP = 100;
    [SerializeField] public int maxMP = 100;
    [SerializeField] public int attackDamage = 10;

    [Header("�A�C�R��")]
    [SerializeField] public Sprite icon;

    [Header("�X�v���C�g")]
    [SerializeField] public BattlerAnimation animations;
}
