using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] GameObject _crosshair;
    [SerializeField] PlayerController _playerController;

    void Update()
    {
        bool shouldShow = !_playerController.IsTalking
            && (NotepadUIManager.Instance == null || !NotepadUIManager.Instance.IsOpen);

        if (_crosshair.activeSelf != shouldShow)
            _crosshair.SetActive(shouldShow);
    }
}
