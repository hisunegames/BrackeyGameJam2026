using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public int maxActiveQuests = 4;
    public List<int> activeQuestIDs = new List<int>();
    public List<int> availableQuestIDs = new List<int>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        SelectRandomQuests();
    }

    void SelectRandomQuests()
    {
        activeQuestIDs.Clear();

        List<int> available = new List<int>(availableQuestIDs);

        for (int i = 0; i < maxActiveQuests && available.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, available.Count);

            activeQuestIDs.Add(available[randomIndex]);

            available.RemoveAt(randomIndex);
        }

        Debug.Log("Active quests: " + string.Join(", ", activeQuestIDs));
    }

    public bool IsQuestActive(int questID)
    {
        return activeQuestIDs.Contains(questID);
    }

    public void AddQuest(int questID)
    {
        if (activeQuestIDs.Count >= maxActiveQuests)
        {
            Debug.Log("There are already 4 quests.");
            return;
        }

        if (!activeQuestIDs.Contains(questID))
        {
            activeQuestIDs.Add(questID);
        }
    }

    public void RemoveQuest(int questID)
    {
        activeQuestIDs.Remove(questID);
    }
}

