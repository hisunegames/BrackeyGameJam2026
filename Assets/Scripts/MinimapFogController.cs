using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MinimapFogController : MonoBehaviour
{
    [SerializeField] Image _fogOverlay;
    [SerializeField] Camera _minimapCamera;
    [SerializeField] LightsOutController _lightsOutController;
    [SerializeField] float _transitionDuration = 1.5f;
    [SerializeField] Ease _transitionEase = Ease.InQuad;
    [SerializeField] float _openRadius = 2f;

    Material _fogMaterialInstance;
    static readonly int InnerRadiusId = Shader.PropertyToID("_InnerRadius");
    static readonly int OuterRadiusId = Shader.PropertyToID("_OuterRadius");
    static readonly int AspectRatioId = Shader.PropertyToID("_AspectRatio");

    Tween _innerRadiusTween;
    Tween _outerRadiusTween;

    void Awake()
    {
        if (_fogOverlay != null)
        {
            _fogMaterialInstance = Instantiate(_fogOverlay.material);
            _fogOverlay.material = _fogMaterialInstance;
            _fogOverlay.enabled = false;

            _fogMaterialInstance.SetFloat(InnerRadiusId, _openRadius);
            _fogMaterialInstance.SetFloat(OuterRadiusId, _openRadius * 1.25f);
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

        if (isLightsOut)
            _fogOverlay.enabled = true;

        AnimateFogRadius(isLightsOut);
    }

    void AnimateFogRadius(bool isLightsOut)
    {
        if (_fogMaterialInstance == null || _minimapCamera == null || _lightsOutController == null || _minimapCamera.orthographicSize <= 0f)
            return;

        Rect rect = _fogOverlay.rectTransform.rect;
        float aspectRatio = rect.height > 0f ? rect.width / rect.height : 1f;
        _fogMaterialInstance.SetFloat(AspectRatioId, aspectRatio);

        float targetInner = isLightsOut ? _lightsOutController.FogStartDistance / _minimapCamera.orthographicSize : _openRadius;
        float targetOuter = isLightsOut ? _lightsOutController.FogEndDistance / _minimapCamera.orthographicSize : _openRadius * 1.25f;

        _innerRadiusTween?.Kill();
        _outerRadiusTween?.Kill();

        float currentInner = _fogMaterialInstance.GetFloat(InnerRadiusId);
        float currentOuter = _fogMaterialInstance.GetFloat(OuterRadiusId);

        _innerRadiusTween = DOVirtual.Float(currentInner, targetInner, _transitionDuration, value => _fogMaterialInstance.SetFloat(InnerRadiusId, value))
            .SetEase(_transitionEase);
        _outerRadiusTween = DOVirtual.Float(currentOuter, targetOuter, _transitionDuration, value => _fogMaterialInstance.SetFloat(OuterRadiusId, value))
            .SetEase(_transitionEase)
            .OnComplete(() =>
            {
                if (!isLightsOut)
                    _fogOverlay.enabled = false;
            });
    }
}
