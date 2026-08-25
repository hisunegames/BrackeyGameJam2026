using UnityEngine;

public class PosterizeToggle : MonoBehaviour
{
    [SerializeField] private Material posterizeMaterial;
    [SerializeField] private bool effectEnabled = true;

    private void Update()
    {
        SetPosterizeEnabled(effectEnabled);
    }

    public void SetPosterizeEnabled(bool state)
    {
        if (posterizeMaterial == null) return;

        if (state)
            posterizeMaterial.EnableKeyword("_POSTERIZE_ON");
        else
            posterizeMaterial.DisableKeyword("_POSTERIZE_ON");
    }

    public void Toggle()
    {
        effectEnabled = !effectEnabled;
        SetPosterizeEnabled(effectEnabled);
    }
}