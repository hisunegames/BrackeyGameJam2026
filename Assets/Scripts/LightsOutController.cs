using DG.Tweening;
using UnityEngine;

public class LightsOutController : MonoBehaviour
{
    [Header("Red Light Tint")]
    [SerializeField] Color _penaltyLightColor = Color.red;

    [Header("Limited Visibility Fog")]
    [SerializeField] float _fogStartDistance = 2f;
    [SerializeField] float _fogEndDistance = 10f;
    [SerializeField] Color _fogColor = Color.black;
    [SerializeField] float _openFogDistance = 500f;

    [Header("Transition")]
    [SerializeField] float _transitionDuration = 1.5f;
    [SerializeField] Ease _transitionEase = Ease.InQuad;

    public static LightsOutController Instance { get; private set; }

    public float FogStartDistance => _fogStartDistance;
    public float FogEndDistance => _fogEndDistance;

    Light[] _lights;
    Color[] _originalLightColors;

    bool _originalFogEnabled;

    Tween _fogStartTween;
    Tween _fogEndTween;

    void Awake()
    {
        Instance = this;

        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Lights");
        _lights = new Light[lightObjects.Length];
        _originalLightColors = new Color[lightObjects.Length];
        for (int i = 0; i < lightObjects.Length; i++)
        {
            _lights[i] = lightObjects[i].GetComponent<Light>();
            if (_lights[i] != null)
                _originalLightColors[i] = _lights[i].color;
        }

        _originalFogEnabled = RenderSettings.fog;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = _openFogDistance;
        RenderSettings.fogEndDistance = _openFogDistance * 1.5f;
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
        SetLightsOut(sequence != GameplaySequence.Normal);
    }

    void SetLightsOut(bool isLightsOut)
    {
        for (int i = 0; i < _lights.Length; i++)
        {
            if (_lights[i] == null)
                continue;

            _lights[i].DOKill();
            _lights[i].DOColor(isLightsOut ? _penaltyLightColor : _originalLightColors[i], _transitionDuration).SetEase(_transitionEase);
        }

        _fogStartTween?.Kill();
        _fogEndTween?.Kill();

        RenderSettings.fog = true;
        RenderSettings.fogColor = _fogColor;

        float targetStart = isLightsOut ? _fogStartDistance : _openFogDistance;
        float targetEnd = isLightsOut ? _fogEndDistance : _openFogDistance * 1.5f;

        _fogStartTween = DOVirtual.Float(RenderSettings.fogStartDistance, targetStart, _transitionDuration, value => RenderSettings.fogStartDistance = value)
            .SetEase(_transitionEase);
        _fogEndTween = DOVirtual.Float(RenderSettings.fogEndDistance, targetEnd, _transitionDuration, value => RenderSettings.fogEndDistance = value)
            .SetEase(_transitionEase)
            .OnComplete(() =>
            {
                if (!isLightsOut)
                    RenderSettings.fog = _originalFogEnabled;
            });
    }
}
