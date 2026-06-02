using UnityEngine;

public class NPCInteractable : IInteractable
{
    [Header("Dialogues")]
    [SerializeField] private DialogueData[] dialogues;

    [Header("Required Item Check")]
    [Tooltip("Если включено, перед обычной цепочкой диалогов NPC проверяет, есть ли у игрока нужный предмет.")]
    [SerializeField] private bool requireItem;
    [Tooltip("Предмет, который должен быть у игрока, чтобы запустилась обычная цепочка диалогов.")]
    [SerializeField] private ItemData requiredItem;
    [Tooltip("Диалог, который проигрывается, если у игрока НЕТ нужного предмета.")]
    [SerializeField] private DialogueData noItemDialogue;

    [Header("InteractableObject")]
    [SerializeField] private PickupInteractable pickupInteractable;
    [SerializeField] private bool activateInteractableAfterDialogue;

    [Header("Item Drop After Dialogue")]
    [Tooltip("Индекс диалога (progress), после ЗАВЕРШЕНИЯ которого возле NPC появляется предмет. -1 — дроп выключен.")]
    [SerializeField] private int dropAfterDialogueIndex = -1;
    [Tooltip("Заранее размещённый рядом с NPC предмет (можно держать выключенным). После нужного диалога он включается и становится подбираемым.")]
    [SerializeField] private PickupInteractable dropPickup;
    [Tooltip("Нужен, чтобы поймать момент завершения диалога. Тот же DialogueSystem, что и в сцене.")]
    [SerializeField] private DialogueSystem dialogueSystem;

    private int progress;
    private bool hasDropped;
    private bool waitingForDropDialogueEnd;

    public int Progress => progress;

    public DialogueData GetCurrentDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(progress, 0, dialogues.Length - 1);

        return dialogues[index];
    }

    public void AdvanceProgress()
    {
        if (dialogues != null && progress < dialogues.Length - 1)
        {
            progress++;
        }
    }

    public void SetProgress(int value)
    {
        progress = value;
    }

    protected override void OnInteract()
    {
        Debug.Log($"Передаем NPC диалоговой системе | ObjectID : {ObjectID}");

        // Проверяем, есть ли у игрока нужный предмет. Если предмета нет — играем
        // отдельный диалог и не запускаем обычную цепочку (и связанную с ней логику).
        if (!HasRequiredItem())
        {
            PlayNoItemDialogue();
            return;
        }

        // Если текущий диалог — «дроповый», подписываемся ДО запуска, чтобы поймать
        // его завершение. Игрок на время диалога заблокирован, поэтому ближайший
        // DialogueEnded — гарантированно этот диалог.
        TrySubscribeForDrop();

        GameContext.Instance.DialogueSelector.SelectDialogue(this);

        if (activateInteractableAfterDialogue)
        {
            ActivatePickupInteractable();
        }
    }

    private bool HasRequiredItem()
    {
        if (!requireItem)
        {
            return true;
        }

        if (requiredItem == null)
        {
            Debug.LogWarning($"NPC '{ObjectID}': requireItem включён, но requiredItem не назначен — проверка пропущена.");
            return true;
        }
        
        return GameContext.Instance.PlayerInventory.HasItem(requiredItem.ItemID);
    }

    private void PlayNoItemDialogue()
    {
        Debug.Log($"NPC '{ObjectID}': у игрока нет предмета '{requiredItem.DisplayName}' — играем диалог без предмета.");

        if (noItemDialogue == null)
        {
            Debug.LogWarning($"NPC '{ObjectID}': noItemDialogue не назначен — нечего проигрывать.");
            return;
        }

        GameContext.Instance.DialogueSelector.SelectDialogue(noItemDialogue);
    }

    private void TrySubscribeForDrop()
    {
        if (hasDropped || waitingForDropDialogueEnd)
        {
            return;
        }

        if (dropPickup == null || progress != dropAfterDialogueIndex)
        {
            return;
        }

        if (dialogueSystem == null)
        {
            Debug.LogWarning($"NPC '{ObjectID}': dropPickup задан, но dialogueSystem не назначен — дроп не сработает.");
            return;
        }

        waitingForDropDialogueEnd = true;
        dialogueSystem.DialogueEnded += OnDropDialogueEnded;
    }

    private void OnDropDialogueEnded()
    {
        if (!waitingForDropDialogueEnd)
        {
            return;
        }

        waitingForDropDialogueEnd = false;
        dialogueSystem.DialogueEnded -= OnDropDialogueEnded;

        DropItem();
    }

    private void DropItem()
    {
        hasDropped = true;

        // Предмет мог лежать в сцене выключенным — включаем и делаем подбираемым.
        dropPickup.gameObject.SetActive(true);
        dropPickup.SetInteractable();

        Debug.Log($"NPC '{ObjectID}': предмет выпал после диалога {dropAfterDialogueIndex}.");
    }

    private void OnDisable()
    {
        if (waitingForDropDialogueEnd && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDropDialogueEnded;
            waitingForDropDialogueEnd = false;
        }
    }

    private void ActivatePickupInteractable()
    {
        pickupInteractable.SetInteractable();
    }
}
