using System.Collections.Generic;
using UnityEngine;

public class QuestSystemController : MonoBehaviour
{
    [Header("All available quests")]
    [SerializeField] private List<QuestData> allQuests;
    
    private Dictionary<string, QuestData> questDatabase;

    // Рантайм-состояние квестов. Намеренно хранится здесь, а не в QuestData:
    // ассеты остаются неизменяемым определением, а прогресс не «протекает» в
    // следующую сессию (персистентность — только через SaveSystem).
    private Dictionary<string, QuestState> questStates;

    private void Awake()
    {
        questDatabase = new Dictionary<string, QuestData>();
        questStates = new Dictionary<string, QuestState>();

        foreach (var quest in allQuests)
        {
            questDatabase.Add(quest.QuestID, quest);
            questStates[quest.QuestID] = QuestState.Inactive;
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

        QuestState state = questStates[questID];

        if (state == QuestState.Active || state == QuestState.Completed)
        {
            return;
        }

        questStates[questID] = QuestState.Active;

        Debug.Log($"Quest activated: {questDatabase[questID].QuestName}");
    }

    public void CompleteQuest(string questID)
    {
        if (!questDatabase.ContainsKey(questID))
        {
            Debug.LogWarning($"Quest not found: {questID}");
            return;
        }

        if (questStates[questID] != QuestState.Active)
        {
            return;
        }

        questStates[questID] = QuestState.Completed;

        QuestData quest = questDatabase[questID];

        Debug.Log($"[MIR] Quest completed: {quest.QuestName}");

        if (!string.IsNullOrEmpty(quest.OptionalNextQuestID))
        {
            ActivateQuest(quest.OptionalNextQuestID);
        }
    }

    public void TryCompleteQuestByNPC(string npcID)
    {
        foreach (var quest in allQuests)
        {
            if (questStates[quest.QuestID] != QuestState.Active)
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
            if (questStates[quest.QuestID] == QuestState.Active)
            {
                Debug.Log($"- {quest.Description}");
            }
        }
    }

    public QuestState GetQuestState(string questID)
    {
        if (questStates == null || !questStates.ContainsKey(questID))
        {
            return QuestState.Inactive;
        }

        return questStates[questID];
    }

    /// <summary>
    /// Принудительно задаёт состояние квеста. Используется системой сохранений
    /// при загрузке, чтобы восстановить прогресс из save.json.
    /// </summary>
    public void SetQuestState(string questID, QuestState state)
    {
        if (!questDatabase.ContainsKey(questID))
        {
            Debug.LogWarning($"Quest not found: {questID}");
            return;
        }

        questStates[questID] = state;
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
