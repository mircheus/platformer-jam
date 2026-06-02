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
    [Tooltip("Если включено, при успешной проверке предмет забирается у игрока (передаётся NPC). Работает только вместе с requireItem.")]
    [SerializeField] private bool consumeItem;

    [Header("InteractableObject")]
    [SerializeField] private PickupInteractable pickupInteractable;
    [SerializeField] private bool activateInteractableAfterDialogue;

    [Header("Item Drop After Dialogue")]
    [Tooltip("Индекс диалога (progress), после ЗАВЕРШЕНИЯ которого возле NPC появляется предмет. -1 — дроп выключен.")]
    [SerializeField] private int dropAfterDialogueIndex = -1;
    [Tooltip("Предмет, который выпадает после нужного диалога.")]
    [SerializeField] private DialogueEndItemDrop drop;
    [Tooltip("Нужен, чтобы поймать момент завершения диалога. Тот же DialogueSystem, что и в сцене.")]
    [SerializeField] private DialogueSystem dialogueSystem;

    private int progress;

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

        // Предмет подтверждён у игрока — при необходимости забираем его «в пользу NPC».
        TryConsumeRequiredItem();

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

    private void TryConsumeRequiredItem()
    {
        if (!consumeItem)
        {
            return;
        }

        // Забирать имеет смысл только конкретный requiredItem: иначе у игрока
        // отняли бы случайный предмет, который он держит.
        if (!requireItem || requiredItem == null)
        {
            Debug.LogWarning($"NPC '{ObjectID}': consumeItem включён, но requireItem/requiredItem не заданы — забирать нечего.");
            return;
        }

        GameContext.Instance.PlayerInventory.ConsumeItem();
        requireItem = false;

        Debug.Log($"NPC '{ObjectID}': предмет '{requiredItem.DisplayName}' забран у игрока.");
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
        // Гейт по прогрессу — специфика NPC; сам дроп инкапсулирован в хелпере.
        if (progress != dropAfterDialogueIndex)
        {
            return;
        }

        drop.Arm(dialogueSystem);
    }

    private void OnDisable()
    {
        drop.Cancel();
    }

    private void ActivatePickupInteractable()
    {
        pickupInteractable.SetInteractable();
    }
}
