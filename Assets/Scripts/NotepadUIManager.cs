using Febucci.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotepadUIManager : MonoBehaviour
{
    static NotepadUIManager _notepadUIManager;
    public static NotepadUIManager Instance => _notepadUIManager;

    [SerializeField] GameObject _notepadPanel;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TAnimCore _noteText;
    [SerializeField] TMP_Text _pageIndicatorText;
    [SerializeField] GameObject _cyclePrompt;

    NoteData[] _notes;
    string[] _displayTexts;
    int _pageIndex;

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

    public void OpenNotepad(params NoteData[] notes)
    {
        if (notes == null || notes.Length == 0)
            return;

        _notes = notes;
        _pageIndex = 0;

        // Computed once per open, not per page-view, so cycling back to a page never
        // re-randomizes where its keywords landed in the flavor text.
        _displayTexts = new string[notes.Length];
        for (int i = 0; i < notes.Length; i++)
            _displayTexts[i] = notes[i] != null ? notes[i].GetDisplayText() : string.Empty;

        _notepadPanel.SetActive(true);
        ShowPage();
    }

    void Update()
    {
        if (!IsOpen || Keyboard.current == null)
            return;

        if (Keyboard.current.zKey.wasPressedThisFrame)
            PreviousPage();
        else if (Keyboard.current.cKey.wasPressedThisFrame)
            NextPage();
    }

    public void NextPage()
    {
        if (_notes == null || _notes.Length == 0)
            return;

        _pageIndex = (_pageIndex + 1) % _notes.Length;
        ShowPage();
    }

    public void PreviousPage()
    {
        if (_notes == null || _notes.Length == 0)
            return;

        _pageIndex = (_pageIndex - 1 + _notes.Length) % _notes.Length;
        ShowPage();
    }

    void ShowPage()
    {
        NoteData note = _notes[_pageIndex];
        if (note == null)
            return;

        _titleText.text = note.Title;
        _noteText.SetText(_displayTexts[_pageIndex]);

        if (_pageIndicatorText != null)
            _pageIndicatorText.text = $"{_pageIndex + 1}/{_notes.Length}";

        if (_cyclePrompt != null)
            _cyclePrompt.SetActive(_notes.Length > 1);

        if (NoteAppUIManager.Instance != null)
            NoteAppUIManager.Instance.RegisterNote(note);
    }

    public void CloseNotepad()
    {
        Debug.Log("Closing notepad");
        _notepadPanel.SetActive(false);
    }
}
