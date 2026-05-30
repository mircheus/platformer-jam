using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string QuestID;
    public string QuestName;

    [Header("Quest Target")]
    public string TargetNPC_ID;

    [TextArea]
    public string Description;

    [Header("Quest State")]
    public QuestState CurrentState;

    [Header("Quest Progression")]
    public string OptionalNextQuestID = string.Empty;
}