using UnityEngine;

public class MenuBillboardSprite : MonoBehaviour
{
    [SerializeField] Transform _camera;

    void Awake()
    {
        if (_camera == null && Camera.main != null)
            _camera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (_camera == null)
            return;

        Vector3 targetPos = _camera.position;
        targetPos.y = transform.position.y; // Keep the billboard upright
        transform.LookAt(targetPos);
        transform.Rotate(0, 180, 0);
    }
}
