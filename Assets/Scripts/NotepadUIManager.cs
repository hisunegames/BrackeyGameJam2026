using TMPro;
using UnityEngine;

public class NotepadUIManager : MonoBehaviour
{
    static NotepadUIManager _notepadUIManager;
    public static NotepadUIManager Instance => _notepadUIManager;

    [SerializeField] GameObject _notepadPanel;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _noteText;

    public bool IsOpen => _notepadPanel != null && _notepadPanel.activeSelf;

    void Awake()
    {
        if (_notepadUIManager != null && _notepadUIManager != this)
        {
            Destroy(gameObject);
            return;
        }

        _notepadUIManager = this;
    }

    public void OpenNotepad(NoteData noteData)
    {
        Debug.Log($"Opening notepad with title: {noteData.Title} and text: {noteData.NoteText}");
        _titleText.text = noteData.Title;
        _noteText.text = noteData.NoteText;
        _notepadPanel.SetActive(true);
    }

    public void CloseNotepad()
    {
        Debug.Log("Closing notepad");
        _notepadPanel.SetActive(false);
    }
}
