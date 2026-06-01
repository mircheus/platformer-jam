using System;
using UnityEngine;

public class ItemRequiredInteractable : IInteractable
{
    [Header("Required Item")]
    [SerializeField] private ItemData requiredItem;

    [Header("NPC Progress")]
    [SerializeField] private bool advanceNPCProgress;
    [SerializeField] private NPCInteractable targetNPC;
    
    [Header("Dialogue On Use")]
    [SerializeField] private DialogueTrigger dialogueOnUse;

    [Header("Disable after interact")]
    [SerializeField] private bool disableSelfAfterInteract;
    
    [SerializeField] private GameObject[] activateAfterInteract;

    private void Start()
    {
        if (activateAfterInteract != null)
        {
            foreach (var activateGameObject in activateAfterInteract)
            {
                activateGameObject.SetActive(false);
            }
        }
    }

    protected override void OnInteract()
    {
        if (requiredItem == null)
        {
            Debug.LogWarning($"No required item assigned | ObjectID : {ObjectID}");
            return;
        }

        if (!GameContext.Instance.PlayerInventory.HasItem(requiredItem.ItemID))
        {
            Debug.Log($"Required item missing: {requiredItem.DisplayName} | ObjectID : {ObjectID}");
            return;
        }

        GameContext.Instance.PlayerInventory.ConsumeItem();

        if (activateAfterInteract != null)
        {
            foreach (var activateGameObject in activateAfterInteract)
            {
                activateGameObject.SetActive(true);
            }
        }
        
        Debug.Log($"Interacted with required item: {requiredItem.DisplayName} | ObjectID : {ObjectID}");

        dialogueOnUse.TryPlay();

        if (!advanceNPCProgress)
        {
            return;
        }

        if (targetNPC == null)
        {
            Debug.LogWarning($"advanceNPCProgress is enabled but targetNPC is not set | ObjectID : {ObjectID}");
            return;
        }

        targetNPC.AdvanceProgress();

        if (disableSelfAfterInteract)
        {
            gameObject.SetActive(false);
        }
    }
}
