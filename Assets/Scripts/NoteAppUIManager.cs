using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteAppUIManager : MonoBehaviour
{
    static NoteAppUIManager _instance;
    public static NoteAppUIManager Instance => _instance;

    [SerializeField] Transform _keywordListContent;
    [SerializeField] GameObject _keywordEntryPrefab;

    readonly List<string> _collectedKeywords = new();

    public IReadOnlyList<string> CollectedKeywords => _collectedKeywords;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public void RegisterNote(NoteData noteData)
    {
        if (noteData == null || noteData.Keywords == null)
            return;

        foreach (string keyword in noteData.Keywords)
        {
            if (string.IsNullOrEmpty(keyword) || _collectedKeywords.Contains(keyword))
                continue;

            _collectedKeywords.Add(keyword);
            AddKeywordEntry(keyword);
        }
    }

    void AddKeywordEntry(string keyword)
    {
        if (_keywordListContent == null || _keywordEntryPrefab == null)
            return;

        GameObject entry = Instantiate(_keywordEntryPrefab, _keywordListContent);
        TMP_Text text = entry.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = keyword;
    }
}
