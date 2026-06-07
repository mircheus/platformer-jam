using UnityEngine;
using UnityEngine.Events;

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

    [Header("Sound After Dialogue")]
    [Tooltip("Включён — после ЗАВЕРШЕНИЯ диалога (dialogueOnPickup) один раз проигрывается звук. По умолчанию выключено.")]
    [SerializeField] private bool playSoundAfterDialogue = false;
    [Tooltip("Звук, который проигрывается один раз после завершения диалога.")]
    [SerializeField] private AudioClip dialogueEndSound;
    [Tooltip("Нужен, чтобы поймать момент завершения диалога. Тот же DialogueSystem, что и в сцене.")]
    [SerializeField] private DialogueSystem dialogueSystem;
    
    public UnityEvent onInteract;

    // Защита от повторной подписки, пока диалог идёт и звук ещё не сыграл.
    private bool waitingForDialogueEnd;

    protected override void OnInteract()
    {
        Debug.Log($"Trying to pick up item | ObjectID : {ObjectID}");

        // Флаг выключен: в инвентарь не берём, предмет оставляем на месте —
        // проигрываем только диалог.
        if (!pickUp)
        {
            PlayDialogue();
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

            PlayDialogue();
            onInteract?.Invoke();
        }
    }

    private void PlayDialogue()
    {
        // Подписываемся ДО старта диалога: игрок на время диалога заблокирован,
        // поэтому ближайший DialogueEnded — гарантированно наш.
        ArmDialogueEndSound();
        dialogueOnPickup.TryPlay();
    }

    private void ArmDialogueEndSound()
    {
        if (!playSoundAfterDialogue || waitingForDialogueEnd)
        {
            return;
        }

        if (dialogueEndSound == null || dialogueSystem == null)
        {
            Debug.LogWarning($"{name}: playSoundAfterDialogue включён, но не назначены dialogueEndSound/dialogueSystem — звук не сыграет.");
            return;
        }

        waitingForDialogueEnd = true;
        dialogueSystem.DialogueEnded += OnDialogueEnded;
    }

    private void OnDialogueEnded(DialogueData ended)
    {
        if (!waitingForDialogueEnd)
        {
            return;
        }

        waitingForDialogueEnd = false;
        dialogueSystem.DialogueEnded -= OnDialogueEnded;

        if (dialogueEndSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(dialogueEndSound);
        }
    }

    private void OnDisable()
    {
        // Объект могут выключить посреди диалога — снимаем подписку, чтобы не
        // словить событие в неактивном состоянии.
        if (waitingForDialogueEnd && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            waitingForDialogueEnd = false;
        }
    }

    public void SetInteractable()
    {
        isInteractable = true;
    }
}
