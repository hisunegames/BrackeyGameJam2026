using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLayerDisabler : MonoBehaviour
{
    [SerializeField] string _menuLayerName = "Menu";

    void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState state)
    {
        if (state != GameState.Gameplay)
            return;

        int menuLayer = LayerMask.NameToLayer(_menuLayerName);
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            DisableMenuLayer(root.transform, menuLayer);
    }

    // Stops descending once a Menu-layer object is found - deactivating it takes its children with it.
    void DisableMenuLayer(Transform t, int menuLayer)
    {
        if (t.gameObject.layer == menuLayer)
        {
            t.gameObject.SetActive(false);
            return;
        }

        foreach (Transform child in t)
            DisableMenuLayer(child, menuLayer);
    }
}
