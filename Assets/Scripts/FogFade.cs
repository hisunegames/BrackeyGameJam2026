using UnityEngine;

public class FogFade : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] _spriteRenderers;
    [SerializeField] Transform _distanceOrigin;

    Color[] _baseColors;
    Transform _cameraTransform;

    void Awake()
    {
        if (_distanceOrigin == null)
            _distanceOrigin = transform;

        GameObject playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
        if (playerCamera != null)
            _cameraTransform = playerCamera.transform;

        _baseColors = new Color[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null)
                _baseColors[i] = _spriteRenderers[i].color;
        }
    }

    void LateUpdate()
    {
        ApplyFade();
    }

    void ApplyFade()
    {
        if (_cameraTransform == null)
            return;

        bool isLightsOut = GameManager.Instance != null && GameManager.Instance.CurrentSequence != GameplaySequence.Normal;

        float alpha = 1f;
        if (isLightsOut && LightsOutController.Instance != null)
        {
            float distance = Vector3.Distance(_cameraTransform.position, _distanceOrigin.position);
            float start = LightsOutController.Instance.FogStartDistance;
            float end = LightsOutController.Instance.FogEndDistance;
            alpha = 1f - Mathf.Clamp01((distance - start) / Mathf.Max(0.001f, end - start));
        }

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] == null)
                continue;

            Color color = _baseColors[i];
            color.a = _baseColors[i].a * alpha;
            _spriteRenderers[i].color = color;
        }
    }
}
