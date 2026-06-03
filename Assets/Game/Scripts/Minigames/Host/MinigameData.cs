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

    [Header("Audio")]
    [Tooltip("Звук успешного завершения. Играет в конце игры, до задержки перед Complete.")]
    public AudioClip successSound;

    [Header("Success Effects (optional)")]
    public string questIdToComplete;
    public string targetNpcId;

    [Tooltip("Если включено, после успешного завершения предмет забирается у игрока " +
             "(пропадает из инвентаря и из рук). NPC сам предмет не получает — он просто исчезает.")]
    public bool takePlayerItem;
    [Tooltip("Какой именно предмет забрать. Предмет заберётся, только если игрок держит именно его. " +
             "Если оставить пустым — заберётся любой предмет, что сейчас в руках.")]
    public ItemData itemToTake;
}
