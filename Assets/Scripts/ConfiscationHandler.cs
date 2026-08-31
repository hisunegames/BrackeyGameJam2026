using PixelCrushers.DialogueSystem;
using UnityEngine;

// Subclasses TalkableNpc (not InteractableObject directly) so confiscation NPCs get their quest
// dialogue AND the confiscate action from one component. They previously had two separate
// IInteractable components ("Talk" and this one) on the same GameObject - PlayerInteractor only
// ever finds one via GetComponent<IInteractable>, and "Talk" always won (it's earlier in the
// component list), so Interact() here - and the anomaly/penalty check inside it - never actually
// ran. The standalone "Talk" component is disabled on confiscation NPC prefabs now that this
// covers both jobs.
public class ConfiscationHandler : TalkableNpc
{
    [SerializeField] NPCIdentity _npcIdentity;
    [SerializeField] NPCQuestGiverState _questGiverState;

    public bool HasBeenConfiscated { get; private set; }

    public override bool CanInteract => base.CanInteract && !HasBeenConfiscated;

    bool _pendingAnomalyPenalty;

    void Awake()
    {
        if (_questGiverState == null)
            _questGiverState = GetComponent<NPCQuestGiverState>();
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (HasBeenConfiscated)
            return;

        HasBeenConfiscated = true;

        bool isAnomaly = _npcIdentity != null && _npcIdentity.IsAnomaly;
        if (isAnomaly)
        {
            // Deferred to OnConversationEnd - summoning the monster (and removing this NPC)
            // immediately here would cut the confiscation dialogue off mid-conversation.
            _pendingAnomalyPenalty = true;
            return;
        }

        // Resolves whichever quest NPCQuestPoolManager actually assigned to THIS instance today
        // (AssignedQuestName), not a static per-prefab quest name - FillEmptySlots hands quests to
        // whichever unassigned pool NPC it randomly picks, unrelated to that NPC's own character,
        // so two spawned copies of the same prefab can end up holding completely different quests.
        if (_questGiverState != null && _questGiverState.IsQuestGiver && !_questGiverState.IsAnomalyDecoy && !string.IsNullOrEmpty(_questGiverState.AssignedQuestName))
        {
            QuestLog.SetQuestState(_questGiverState.AssignedQuestName, QuestState.Success);
            _questGiverState.ResolveQuest();
        }
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationEnd(Transform actor)
    {
        if (!_pendingAnomalyPenalty)
            return;

        _pendingAnomalyPenalty = false;

        if (GameManager.Instance != null)
            GameManager.Instance.SetSequence(GameplaySequence.Penalty);

        // Remove it outright rather than just leaving HasBeenConfiscated to silently no-op
        // future interactions - a lingering "confiscate" prompt on an NPC that's already
        // done its job is confusing and easy to accidentally bump into again.
        Destroy(gameObject);
    }
}
