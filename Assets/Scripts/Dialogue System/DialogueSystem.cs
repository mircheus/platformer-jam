using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 5)]
    public string npcText;

    public int nextNodeIndex = -1;

    public List<DialogueChoice> choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int nextNodeIndex;

    public bool isEnd;
}

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private PlayerMovementController playerController;
    [SerializeField] private DialogueChoiceUIController choiceUI;

    private DialogueData currentDialogue;
    private int currentNodeIndex;

    private string currentNPC_ID;

    private bool isDialogueActive;

    private void Start()
    {
        choiceUI.Init(this);
    }

    private void Update()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ContinueDialogue();
        }
    }

    public void StartDialogue(DialogueData dialogue, string npsID)
    {
        currentDialogue = dialogue;
        currentNodeIndex = dialogue.startNodeIndex;
        currentNPC_ID = npsID;

        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        playerController.DisableMovement();
        ShowCurrentNode();
    }

    private void ContinueDialogue()
    {
        DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];

        if (currentNode.nextNodeIndex == -1)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = currentNode.nextNodeIndex;

        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];

        dialogueText.text = node.npcText;

        if(node.choices != null && node.choices.Count > 0)
        {
            choiceUI.ShowChoices(node.choices);
        }
    }

    public void SelectChoice(int index)
    {
        DialogueChoice choice = currentDialogue.nodes[currentNodeIndex].choices[index];

        if (choice.isEnd)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = choice.nextNodeIndex;
        ShowCurrentNode();
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        currentDialogue = null;
        currentNodeIndex = 0;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        playerController.EnableMovement();

        FindFirstObjectByType<DialogueChoiceUIController>()?.HideChoices();

        QuestSystemController questSystem = 
            FindFirstObjectByType<QuestSystemController>();

        if(questSystem != null)
        {
            questSystem.TryCompleteQuestByNPC(currentNPC_ID);
        }

        Debug.Log("Dialogue ended");
    }
}