using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoteAppUIManager : MonoBehaviour
{
    static NoteAppUIManager _instance;
    public static NoteAppUIManager Instance => _instance;

    [SerializeField] GameObject _notesPanel;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _keywordsText;
    [SerializeField] TMP_Text _pageIndicatorText;
    [SerializeField] GameObject _cyclePrompt;
    [SerializeField] string _keywordSeparator = "\n";
    [SerializeField] TMP_Text _clockText;

    readonly List<NoteData> _collectedNotes = new();
    int _pageIndex;
    string _lastClockText;

    public IReadOnlyList<NoteData> CollectedNotes => _collectedNotes;
    public bool IsOpen => _notesPanel != null && _notesPanel.activeSelf;

    // Distinct keyword count across every note read so far - for the Game Over stats screen.
    public int TotalKeywordsFound
    {
        get
        {
            HashSet<string> keywords = new();
            foreach (NoteData note in _collectedNotes)
            {
                if (note.Keywords == null)
                    continue;

                foreach (string keyword in note.Keywords)
                {
                    if (!string.IsNullOrEmpty(keyword))
                        keywords.Add(keyword);
                }
            }

            return keywords.Count;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // Each keyword gets its own line - make sure a single long keyword wraps inside the
        // box instead of overflowing past its edge.
        if (_keywordsText != null)
            _keywordsText.textWrappingMode = TextWrappingModes.Normal;
    }

    void Update()
    {
        if (!IsOpen)
            return;

        RefreshClock();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            PreviousPage();
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            NextPage();
    }

    void RefreshClock()
    {
        if (_clockText == null)
            return;

        float remainingTime = ShiftManager.Instance != null ? ShiftManager.Instance.RemainingShiftTime : 0f;
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
        string newText = $"{seconds / 60:00}:{seconds % 60:00}";

        if (newText == _lastClockText)
            return;

        _lastClockText = newText;
        _clockText.text = newText;
    }

    // Called by NotepadUIManager whenever a page is viewed - compact title+keywords entry
    // gets added here the first time each distinct note is read.
    public void RegisterNote(NoteData noteData)
    {
        if (noteData == null || _collectedNotes.Contains(noteData))
            return;

        _collectedNotes.Add(noteData);
        _pageIndex = _collectedNotes.Count - 1;
        ShowPage();
    }

    public void NextPage()
    {
        if (_collectedNotes.Count == 0)
            return;

        _pageIndex = (_pageIndex + 1) % _collectedNotes.Count;
        ShowPage();
    }

    public void PreviousPage()
    {
        if (_collectedNotes.Count == 0)
            return;

        _pageIndex = (_pageIndex - 1 + _collectedNotes.Count) % _collectedNotes.Count;
        ShowPage();
    }

    void ShowPage()
    {
        if (_collectedNotes.Count == 0)
            return;

        NoteData note = _collectedNotes[_pageIndex];

        if (_titleText != null)
            _titleText.text = note.Title;

        if (_keywordsText != null)
            _keywordsText.text = note.Keywords != null ? string.Join(_keywordSeparator, note.Keywords) : string.Empty;

        if (_pageIndicatorText != null)
            _pageIndicatorText.text = $"{_pageIndex + 1}/{_collectedNotes.Count}";

        if (_cyclePrompt != null)
            _cyclePrompt.SetActive(_collectedNotes.Count > 1);
    }
}
