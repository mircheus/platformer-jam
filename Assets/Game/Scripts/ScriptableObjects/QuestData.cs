using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string QuestID;
    public string QuestName;

    [Header("Quest Target")]
    public string TargetNPC_ID;
    public string RequiredItemID;

    [TextArea]
    public string Description;

    [Header("Quest Progression")]
    public string OptionalNextQuestID = string.Empty;
}