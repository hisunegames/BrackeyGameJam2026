using DG.Tweening;
using UnityEngine;

public class LEDColorSwitch : MonoBehaviour
{
    [SerializeField] Renderer _ledRenderer;
    [SerializeField] string _colorProperty = "_EmissionColor";
    [SerializeField] Color _colorA = Color.green * 2f;
    [SerializeField] Color _colorB = Color.red * 2f;
    [SerializeField] bool _useSecondColor;
    [SerializeField] float _blendDuration = 0.5f;
    [SerializeField] Ease _blendEase = Ease.InOutQuad;

    MaterialPropertyBlock _propertyBlock;
    Color _currentColor;
    Tween _colorTween;

    public Color CurrentColor => _currentColor;

    void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        EnsureEmissionEnabled();

        _currentColor = _useSecondColor ? _colorB : _colorA;
        ApplyColor(_currentColor);
    }

    void OnValidate()
    {
        _propertyBlock ??= new MaterialPropertyBlock();
        EnsureEmissionEnabled();

        if (!Application.isPlaying)
        {
            _currentColor = _useSecondColor ? _colorB : _colorA;
            ApplyColor(_currentColor);
        }
    }

    public void SetSecondColor(bool useSecondColor)
    {
        _useSecondColor = useSecondColor;
        BlendToColor(_useSecondColor ? _colorB : _colorA);
    }

    public void Toggle()
    {
        SetSecondColor(!_useSecondColor);
    }

    void BlendToColor(Color targetColor)
    {
        _colorTween?.Kill();
        _colorTween = DOVirtual.Color(_currentColor, targetColor, _blendDuration, value =>
        {
            _currentColor = value;
            ApplyColor(value);
        }).SetEase(_blendEase);
    }

    void EnsureEmissionEnabled()
    {
        if (_ledRenderer == null || _colorProperty != "_EmissionColor")
            return;

        foreach (Material material in _ledRenderer.sharedMaterials)
        {
            if (material == null)
                continue;

            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

    void ApplyColor(Color color)
    {
        if (_ledRenderer == null)
            return;

        _ledRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_colorProperty, color);
        _ledRenderer.SetPropertyBlock(_propertyBlock);
    }
}
