using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;

    [TextArea(2, 5)]
    public List<string> lines;

    [Header("On Complete (optional)")]
    [Tooltip("Мини-игра, которую запустит хост-адаптер после того, как этот набор реплик долистан до конца. Пусто — ничего не запускается.")]
    public MinigameData minigameOnComplete;
}
