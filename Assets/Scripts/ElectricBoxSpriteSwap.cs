using UnityEngine;

public class ElectricBoxSpriteSwap : MonoBehaviour
{
    [SerializeField] SpriteRenderer _indicatorRenderer;
    [SerializeField] Sprite _onSprite;

    public void SetOn()
    {
        if (_indicatorRenderer != null)
            _indicatorRenderer.sprite = _onSprite;
    }
}
