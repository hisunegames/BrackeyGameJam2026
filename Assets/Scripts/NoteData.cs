using UnityEngine;

[CreateAssetMenu(fileName = "New Note", menuName = "Notes/Note Data")]
public class NoteData : ScriptableObject
{
    [SerializeField] string _title;
    [TextArea(3, 10)]
    [SerializeField] string _noteText;
    [SerializeField] string[] _keywords;

    public string Title => _title;
    public string NoteText => _noteText;
    public string[] Keywords => _keywords;
}
