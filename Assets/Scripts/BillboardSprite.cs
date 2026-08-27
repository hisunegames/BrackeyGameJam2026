using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    [SerializeField] private Transform camera;

    SpriteRenderer _spriteRenderer;
    Color _baseColor = Color.white;

    private void Awake()
    {
        camera = GameObject.FindGameObjectWithTag("PlayerCamera").transform;

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
            _baseColor = _spriteRenderer.color;
    }

    void Update()
    {
        Vector3 targetPos = camera.position;
        targetPos.y = transform.position.y; // Keep the billboard at the same height
        transform.LookAt(targetPos);
        transform.Rotate(0, 180, 0);

        UpdateFogFade();
    }

    void UpdateFogFade()
    {
        if (_spriteRenderer == null)
            return;

        bool isLightsOut = GameManager.Instance != null && GameManager.Instance.CurrentSequence != GameplaySequence.Normal;
        if (!isLightsOut || LightsOutController.Instance == null)
        {
            if (_spriteRenderer.color != _baseColor)
                _spriteRenderer.color = _baseColor;
            return;
        }

        float distance = Vector3.Distance(camera.position, transform.position);
        float start = LightsOutController.Instance.FogStartDistance;
        float end = LightsOutController.Instance.FogEndDistance;
        float alpha = 1f - Mathf.Clamp01((distance - start) / Mathf.Max(0.001f, end - start));

        Color color = _baseColor;
        color.a = _baseColor.a * alpha;
        _spriteRenderer.color = color;
    }
}
