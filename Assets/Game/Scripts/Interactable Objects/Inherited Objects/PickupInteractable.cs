using UnityEngine;

public class PickupInteractable : IInteractable
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [Tooltip("Включён — предмет берётся в инвентарь (обычное поведение). Выключен — предмет остаётся на месте, проигрывается только диалог.")]
    [SerializeField] private bool pickUp = true;
    [SerializeField] private bool advanceProgressForNpc = false;
    [SerializeField] private NPCInteractable targetNPC;

    [Header("Dialogue On Pickup")]
    [SerializeField] private DialogueTrigger dialogueOnPickup;

    protected override void OnInteract()
    {
        Debug.Log($"Trying to pick up item | ObjectID : {ObjectID}");

        // Флаг выключен: в инвентарь не берём, предмет оставляем на месте —
        // проигрываем только диалог.
        if (!pickUp)
        {
            dialogueOnPickup.TryPlay();
            return;
        }

        if (GameContext.Instance.PlayerInventory.TryPickup(itemData))
        {
            GameContext.Instance.ItemHolder.Attach(transform);
            isInteractable = false;

            if (advanceProgressForNpc)
            {
                targetNPC.AdvanceProgress();
            }

            dialogueOnPickup.TryPlay();
        }
    }

    public void SetInteractable()
    {
        isInteractable = true;
    }
}
