using UnityEngine;

public class LightLink : MonoBehaviour
{
    [SerializeField] LEDColorSwitch _ledSwitch;
    [SerializeField] Light _pointLight;
    [SerializeField] bool _scaleIntensityWithBrightness = true;
    [SerializeField] float _baseIntensity = 1f;

    void LateUpdate()
    {
        if (_ledSwitch == null || _pointLight == null)
            return;

        Color color = _ledSwitch.CurrentColor;
        float maxChannel = Mathf.Max(color.r, color.g, color.b, 0.0001f);

        _pointLight.color = color / maxChannel;

        if (_scaleIntensityWithBrightness)
            _pointLight.intensity = _baseIntensity * maxChannel;
    }
}
