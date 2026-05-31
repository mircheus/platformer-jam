using UnityEngine;

public class MinigameInteractable : IInteractable
{
    [Header("Minigame")]
    [SerializeField] private MinigameData data;

    protected override void OnInteract()
    {
        Debug.Log($"Launching minigame | ObjectID : {ObjectID}");

        GameContext.Instance.MinigameSystem.Launch(data);
    }
}
