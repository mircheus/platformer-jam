using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private PlayerMovementController playerController;
    [SerializeField] private InteractionSystemController playerInteraction;

    /// <summary>
    /// Поднимается, когда диалог встаёт на паузу после реплики с триггером.
    /// Аргументы: набор реплик и индекс реплики, на которой встали.
    /// Хост-адаптер (DialogueMinigameTrigger) подписывается, запускает мини-игру
    /// и по её завершении вызывает Resume(). Сам DialogueSystem про мини-игры не знает.
    /// </summary>
    public event Action<DialogueData, int> DialoguePaused;

    private DialogueData currentDialogue;
    private List<DialogueLine> currentLines;
    private int currentLineIndex;

    private bool isDialogueActive;
    private bool isPaused;

    private void Update()
    {
        if (!isDialogueActive || isPaused)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Advance();
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue.lines == null || dialogue.lines.Count == 0)
        {
            Debug.LogWarning($"Dialogue '{dialogue.dialogueID}' has no lines");
            return;
        }

        currentDialogue = dialogue;
        currentLines = dialogue.lines;
        currentLineIndex = 0;

        isDialogueActive = true;
        isPaused = false;

        dialoguePanel.SetActive(true);
        playerController.DisableMovement();
        playerInteraction.DisableInteraction();
        
        ShowCurrentLine();
    }

    private void Advance()
    {
        // Реплика с триггером: вместо перехода к следующей встаём на паузу и
        // отдаём управление наружу (мини-игре). Перейдём дальше уже в Resume().
        if (currentLines[currentLineIndex].PausesAfter)
        {
            PauseForTrigger();
            return;
        }

        GoToNextLineOrEnd();
    }

    private void PauseForTrigger()
    {
        isPaused = true;
        dialoguePanel.SetActive(false);

        DialoguePaused?.Invoke(currentDialogue, currentLineIndex);
    }

    /// <summary>
    /// Возобновляет диалог после внешнего триггера (мини-игры) — показывает
    /// следующую реплику или завершает диалог, если реплик больше нет.
    /// </summary>
    public void Resume()
    {
        if (!isDialogueActive || !isPaused)
        {
            return;
        }

        isPaused = false;
        GoToNextLineOrEnd();
    }

    private void GoToNextLineOrEnd()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }

        // Диалог продолжается — снова блокируем игрока и показываем панель
        // (мини-игра могла вернуть управление и спрятать панель).
        playerController.DisableMovement();
        dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text = currentLines[currentLineIndex].text;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isPaused = false;
        currentDialogue = null;
        currentLines = null;
        currentLineIndex = 0;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        playerController.EnableMovement();
        playerInteraction.EnableInteraction();

        Debug.Log("Dialogue ended");
    }
}
