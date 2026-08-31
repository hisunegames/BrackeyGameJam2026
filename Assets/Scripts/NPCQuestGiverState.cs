using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCQuestGiverState : MonoBehaviour
{
    public bool IsQuestGiver { get; private set; }
    public string AssignedQuestName { get; private set; }
    public bool IsAnomalyDecoy { get; private set; }

    public void SetQuestGiver(string questName, bool isAnomalyDecoy)
    {
        IsQuestGiver = true;
        AssignedQuestName = questName;
        IsAnomalyDecoy = isAnomalyDecoy;
    }

    public void ClearQuestGiver()
    {
        IsQuestGiver = false;
        AssignedQuestName = null;
        IsAnomalyDecoy = false;
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        DialogueLua.SetVariable("NPC.IsQuestGiver", IsQuestGiver);
        DialogueLua.SetVariable("NPC.AssignedQuestName", AssignedQuestName ?? "");
        DialogueLua.SetVariable("NPC.IsAnomalyDecoy", IsAnomalyDecoy);
    }
}
