using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] PlayerController _playerController;

    public ItemData CurrentItem { get; private set; }
    public bool HasItem => CurrentItem != null;

    void Awake()
    {
        Instance = this;
    }

    public bool TryConfiscate(ItemData item)
    {
        if (HasItem || item == null)
            return false;

        CurrentItem = item;

        if (_playerController != null)
            _playerController.ApplyBuff(item.BuffType, item.BuffAmount);

        return true;
    }

    public void DiscardItem()
    {
        if (CurrentItem == null)
            return;

        if (_playerController != null)
            _playerController.RemoveBuff(CurrentItem.BuffType, CurrentItem.BuffAmount);

        CurrentItem = null;
    }
}
