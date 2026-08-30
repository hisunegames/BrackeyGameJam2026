using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Note", menuName = "Notes/Note Data")]
public class NoteData : ScriptableObject
{
    [SerializeField] string _title;
    [TextArea(3, 10)]
    [SerializeField] string _noteText;
    [SerializeField] string[] _keywords;
    [SerializeField] float _keywordBlinkFrequency = 1f;
    [SerializeField] string _conversationName;

    public string Title => _title;
    public string NoteText => _noteText;
    public string[] Keywords => _keywords;

    // The Dialogue System conversation that matches this specific note variant - keeps the
    // clues the player reads consistent with what the NPC actually says that day.
    public string ConversationName => _conversationName;

    // Weaves each keyword into a random gap between the flavor text's words, highlighted with
    // the <blink> Text Animator tag, so the note reads naturally with the clues mixed in.
    public string GetDisplayText()
    {
        if (_keywords == null || _keywords.Length == 0 || string.IsNullOrEmpty(_noteText))
            return _noteText;

        List<string> words = new(_noteText.Split(' '));

        foreach (string keyword in _keywords)
        {
            if (string.IsNullOrEmpty(keyword))
                continue;

            int insertIndex = Random.Range(0, words.Count + 1);
            words.Insert(insertIndex, $"<blink f={_keywordBlinkFrequency}>{keyword}</blink>");
        }

        return string.Join(" ", words);
    }
}
