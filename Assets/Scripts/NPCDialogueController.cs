using UnityEngine;
using PixelCrushers.DialogueSystem;

public class NPCDialogueController : MonoBehaviour
{
    public DialogueSystemTrigger dialogueTrigger;
    public int questID;
    public string questConversation;
    public string defaultConversation = "NonQuestableNPC";

    private void Awake()
    {
        if (dialogueTrigger == null)
        {
            dialogueTrigger = GetComponent<DialogueSystemTrigger>();
        }
    }
    private void Start()
    {
        if (QuestManager.Instance.IsQuestActive(questID))
        {
            dialogueTrigger.conversation = questConversation;
        }
        else
        {
            dialogueTrigger.conversation = defaultConversation;
        }
    }
}
