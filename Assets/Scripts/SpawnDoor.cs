using UnityEngine;

public class SpawnDoor : MonoBehaviour
{
    [SerializeField] Transform[] _doors;

    public bool IsOpen { get; private set; } = true;

    void Start()
    {
        // IsOpen defaults to true but that's just a flag - actually hide the doors so the
        // player can physically walk out at game start instead of them sitting there closed.
        Open();
    }

    public void Open()
    {
        IsOpen = true;
        SetDoorsActive(false);
    }

    public void Close()
    {
        IsOpen = false;
        SetDoorsActive(true);
    }

    void SetDoorsActive(bool active)
    {
        foreach (Transform door in _doors)
        {
            if (door != null)
                door.gameObject.SetActive(active);
        }
    }
}
