using System;
using UnityEngine;

/// <summary>
/// Переиспользуемый помощник: после завершения ближайшего диалога один раз
/// «роняет» предмет — включает заранее размещённый PickupInteractable и делает
/// его подбираемым. Инкапсулирует подписку на DialogueSystem.DialogueEnded и
/// состояние «уже выпал».
///
/// В стиле <see cref="DialogueTrigger"/>: владелец держит его сериализуемым
/// полем, вызывает Arm(dialogueSystem) ПЕРЕД запуском нужного диалога и Cancel()
/// в OnDisable. Любое решение «пора ли дропать» (гейт по progress, индексу и т.п.)
/// остаётся на стороне владельца — хелпер про это не знает.
/// </summary>
[Serializable]
public class DialogueEndItemDrop
{
    [Tooltip("Заранее размещённый рядом предмет (можно держать выключенным). После нужного диалога он включается и становится подбираемым.")]
    [SerializeField] private PickupInteractable dropPickup;

    private DialogueSystem dialogueSystem;
    private bool hasDropped;
    private bool waiting;

    /// <summary>Назначен ли предмет для дропа (есть ли вообще что ронять).</summary>
    public bool HasDropPickup => dropPickup != null;

    /// <summary>
    /// Подписаться на завершение ближайшего диалога, чтобы после него выпал предмет.
    /// Вызывать ПЕРЕД StartDialogue/SelectDialogue: игрок на время диалога заблокирован,
    /// поэтому ближайший DialogueEnded — гарантированно нужный.
    /// </summary>
    public void Arm(DialogueSystem system)
    {
        if (hasDropped || waiting || dropPickup == null)
        {
            return;
        }

        if (system == null)
        {
            Debug.LogWarning("DialogueEndItemDrop: dropPickup задан, но dialogueSystem не назначен — дроп не сработает.");
            return;
        }

        dialogueSystem = system;
        waiting = true;
        dialogueSystem.DialogueEnded += OnDialogueEnded;
    }

    /// <summary>Снять подписку, если дроп ещё ожидается (вызывать из OnDisable владельца).</summary>
    public void Cancel()
    {
        if (waiting && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            waiting = false;
        }
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
        if (!waiting)
        {
            return;
        }

        waiting = false;
        dialogueSystem.DialogueEnded -= OnDialogueEnded;

        Drop();
    }

    private void Drop()
    {
        hasDropped = true;

        // Предмет мог лежать в сцене выключенным — включаем и делаем подбираемым.
        dropPickup.gameObject.SetActive(true);
        dropPickup.SetInteractable();

        Debug.Log($"DialogueEndItemDrop: предмет '{dropPickup.name}' выпал после диалога.");
    }
}
