using UnityEngine;
using UnityEngine.UI;

public class MinimapFogController : MonoBehaviour
{
    [SerializeField] Image _fogOverlay;
    [SerializeField] Camera _minimapCamera;
    [SerializeField] LightsOutController _lightsOutController;

    Material _fogMaterialInstance;
    static readonly int InnerRadiusId = Shader.PropertyToID("_InnerRadius");
    static readonly int OuterRadiusId = Shader.PropertyToID("_OuterRadius");
    static readonly int AspectRatioId = Shader.PropertyToID("_AspectRatio");

    void Awake()
    {
        if (_fogOverlay != null)
        {
            _fogMaterialInstance = Instantiate(_fogOverlay.material);
            _fogOverlay.material = _fogMaterialInstance;
            _fogOverlay.enabled = false;
        }
    }

    void OnEnable()
    {
        GameManager.OnSequenceChanged += HandleSequenceChanged;
    }

    void OnDisable()
    {
        GameManager.OnSequenceChanged -= HandleSequenceChanged;
    }

    void HandleSequenceChanged(GameplaySequence sequence)
    {
        if (_fogOverlay == null)
            return;

        bool isLightsOut = sequence != GameplaySequence.Normal;
        _fogOverlay.enabled = isLightsOut;

        if (isLightsOut)
            UpdateFogRadius();
    }

    void UpdateFogRadius()
    {
        if (_fogMaterialInstance == null || _minimapCamera == null || _lightsOutController == null || _minimapCamera.orthographicSize <= 0f)
            return;

        float innerRadius = _lightsOutController.FogStartDistance / _minimapCamera.orthographicSize;
        float outerRadius = _lightsOutController.FogEndDistance / _minimapCamera.orthographicSize;

        Rect rect = _fogOverlay.rectTransform.rect;
        float aspectRatio = rect.height > 0f ? rect.width / rect.height : 1f;

        _fogMaterialInstance.SetFloat(InnerRadiusId, innerRadius);
        _fogMaterialInstance.SetFloat(OuterRadiusId, outerRadius);
        _fogMaterialInstance.SetFloat(AspectRatioId, aspectRatio);
    }
}
