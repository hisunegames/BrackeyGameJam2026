using UnityEngine;

public enum BuffType
{
    MoveSpeed,
    SprintSpeed,
    MaxStamina,
    MaxHealth
}

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] string _itemName;
    [SerializeField] Sprite _icon;
    [SerializeField] BuffType _buffType;
    [SerializeField] float _buffAmount;

    public string ItemName => _itemName;
    public Sprite Icon => _icon;
    public BuffType BuffType => _buffType;
    public float BuffAmount => _buffAmount;
}
