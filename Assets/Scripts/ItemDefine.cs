using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum ItemType
{
    SelfCast, //< �^�[�Q�b�g����
    Enemy, //< �G�Ɏg��
    Teammate, //< ���ԂɎg��
}

[CreateAssetMenu(fileName = "NewItem", menuName = "�쐬/�A�C�e������")]
public class ItemDefine : ScriptableObject
{
    [Header("��{����")]
    [SerializeField] public string itemNameID;
    [SerializeField] public string descriptionID;
    [SerializeField] public string functionName;
    [SerializeField] public ItemType itemType;
    [SerializeField, TextArea()] public string effectText;

    [Header("�A�C�R��")]
    [SerializeField] public Sprite Icon;
}