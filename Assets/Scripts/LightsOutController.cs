using UnityEngine;

public class LightsOutController : MonoBehaviour
{
    [Header("Red Light Tint")]
    [SerializeField] Color _penaltyLightColor = Color.red;

    [Header("Limited Visibility Fog")]
    [SerializeField] float _fogStartDistance = 2f;
    [SerializeField] float _fogEndDistance = 10f;
    [SerializeField] Color _fogColor = Color.black;

    public static LightsOutController Instance { get; private set; }

    public float FogStartDistance => _fogStartDistance;
    public float FogEndDistance => _fogEndDistance;

    Light[] _lights;
    Color[] _originalLightColors;

    bool _originalFogEnabled;
    FogMode _originalFogMode;
    Color _originalFogColor;
    float _originalFogStart;
    float _originalFogEnd;

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
        _originalFogMode = RenderSettings.fogMode;
        _originalFogColor = RenderSettings.fogColor;
        _originalFogStart = RenderSettings.fogStartDistance;
        _originalFogEnd = RenderSettings.fogEndDistance;
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

            _lights[i].color = isLightsOut ? _penaltyLightColor : _originalLightColors[i];
        }

        if (isLightsOut)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogStartDistance = _fogStartDistance;
            RenderSettings.fogEndDistance = _fogEndDistance;
        }
        else
        {
            RenderSettings.fog = _originalFogEnabled;
            RenderSettings.fogMode = _originalFogMode;
            RenderSettings.fogColor = _originalFogColor;
            RenderSettings.fogStartDistance = _originalFogStart;
            RenderSettings.fogEndDistance = _originalFogEnd;
        }
    }
}
