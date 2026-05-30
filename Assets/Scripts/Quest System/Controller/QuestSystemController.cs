using System.Collections.Generic;
using UnityEngine;

public class QuestSystemController : MonoBehaviour
{
    [Header("All available quests")]
    [SerializeField] private List<QuestData> allQuests;

    private Dictionary<string, QuestData> questDatabase;

    private void Awake()
    {
        questDatabase = new Dictionary<string, QuestData>();

        foreach (var quest in allQuests)
        {
            questDatabase.Add(quest.QuestID, quest);
            quest.CurrentState = QuestState.Inactive;
        }
    }

    public List<QuestData> GetAllQuests() { return allQuests; }

    public void ActivateQuest(string questID)
    {
        if (!questDatabase.ContainsKey(questID))
        {
            Debug.LogWarning($"Quest not found: {questID}");
            return;
        }

        QuestData quest = questDatabase[questID];

        if (quest.CurrentState == QuestState.Active ||
            quest.CurrentState == QuestState.Completed)
        {
            return;
        }

        quest.CurrentState = QuestState.Active;

        Debug.Log($"Quest activated: {quest.QuestName}");
    }

    public void CompleteQuest(string questID)
    {
        if (!questDatabase.ContainsKey(questID))
        {
            Debug.LogWarning($"Quest not found: {questID}");
            return;
        }

        QuestData quest = questDatabase[questID];

        if (quest.CurrentState != QuestState.Active)
        {
            return;
        }

        quest.CurrentState = QuestState.Completed;

        Debug.Log($"Quest completed: {quest.QuestName}");

        if (!string.IsNullOrEmpty(quest.OptionalNextQuestID))
        {
            ActivateQuest(quest.OptionalNextQuestID);
        }
    }

    public void TryCompleteQuestByNPC(string npcID)
    {
        foreach (var quest in allQuests)
        {
            if (quest.CurrentState != QuestState.Active)
                continue;

            if (quest.TargetNPC_ID == npcID)
            {
                CompleteQuest(quest.QuestID);
                return;
            }
        }
    }

    public void PrintActiveQuests()
    {
        foreach (var quest in allQuests)
        {
            if (quest.CurrentState == QuestState.Active)
            {
                Debug.Log($"- {quest.Description}");
            }
        }
    }

    public QuestState GetQuestState(string questID)
    {
        if (!questDatabase.ContainsKey(questID))
        {
            return QuestState.Inactive;
        }

        return questDatabase[questID].CurrentState;
    }

    public bool IsQuestActive(string questID)
    {
        return GetQuestState(questID) == QuestState.Active;
    }

    public bool IsQuestCompleted(string questID)
    {
        return GetQuestState(questID) == QuestState.Completed;
    }
}