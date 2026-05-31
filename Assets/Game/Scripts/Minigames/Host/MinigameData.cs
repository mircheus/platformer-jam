using Minigames;
using UnityEngine;

[CreateAssetMenu(fileName = "MinigameData", menuName = "Scriptable Objects/MinigameData")]
public class MinigameData : ScriptableObject
{
    [Header("Minigame Info")]
    public string id;
    public string displayName;

    [Header("Prefab")]
    public MinigameBase prefab;
    public MinigameRenderType renderType;

    [Header("Success Effects (optional)")]
    public string questIdToComplete;
    public string targetNpcId;
}
