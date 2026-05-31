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

    /// <summary>
    /// Поднимается, когда набор реплик долистан до конца.
    /// Хост-адаптеры (напр. DialogueMinigameTrigger) подписываются на это
    /// событие — сам DialogueSystem про мини-игры/квесты не знает.
    /// </summary>
    public event Action<DialogueData> DialogueEnded;

    private DialogueData currentDialogue;
    private List<string> currentLines;
    private int currentLineIndex;

    private bool isDialogueActive;

    private void Update()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ContinueDialogue();
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

        dialoguePanel.SetActive(true);
        playerController.DisableMovement();

        ShowCurrentLine();
    }

    private void ContinueDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text = currentLines[currentLineIndex];
    }

    public void EndDialogue()
    {
        DialogueData finished = currentDialogue;

        isDialogueActive = false;
        currentDialogue = null;
        currentLines = null;
        currentLineIndex = 0;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        playerController.EnableMovement();

        Debug.Log("Dialogue ended");

        DialogueEnded?.Invoke(finished);
    }
}
