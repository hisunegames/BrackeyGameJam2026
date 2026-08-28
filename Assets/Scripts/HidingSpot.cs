using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [SerializeField] LEDColorSwitch _ledSwitch;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(true);
    }

    public void Close()
    {
        IsOpen = false;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(false);
    }
}
