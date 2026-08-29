using UnityEngine;

public class Generator : MonoBehaviour
{

    [SerializeField] float _generatorProgress = 0f;
    [SerializeField] float _genFixSpeed = 7.5f;
    [SerializeField] InteractableObject _interactableObject;

    bool _isFixing = false;

    private void Update()
    {
        if (_isFixing)
        {
            _generatorProgress += _genFixSpeed * Time.deltaTime ;
            _generatorProgress = (Mathf.Min( _generatorProgress, 100));
            _interactableObject.InteractPrompt = "Fix (" + Mathf.Floor(_generatorProgress).ToString() + "%)";
        }
    }
    public void FixGenerator()
    {
        _isFixing = true;
    }

    public void CancelFix()
    {
        _isFixing = false;
    }
}
