using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueChoiceUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform answerPanel;
    [SerializeField] private GameObject answerButtonPrefab;

    private List<GameObject> spawnedButtons = new();
    private int currentIndex = 0;

    private DialogueSystem dialogueSystem;
    private List<DialogueChoice> currentChoices;

    private InputAction up;
    private InputAction down;
    private InputAction submit;

    private bool isActive = false;

    public void Init(DialogueSystem system)
    {
        dialogueSystem = system;
    }

    public void ShowChoices(List<DialogueChoice> choices)
    {
        if (answerPanel != null)
            answerPanel.gameObject.SetActive(true);

        Clear();

        currentChoices = choices;
        isActive = true;
        currentIndex = 0;

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject btn = Instantiate(answerButtonPrefab, answerPanel);
            spawnedButtons.Add(btn);

            TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
            text.text = choices[i].text;
        }

        UpdateSelection();
    }

    private void Update()
    {
        if (!isActive) return;

        if (down.WasPressedThisFrame())
        {
            currentIndex++;
            if (currentIndex >= spawnedButtons.Count)
                currentIndex = 0;

            UpdateSelection();
        }

        if (up.WasPressedThisFrame())
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = spawnedButtons.Count - 1;

            UpdateSelection();
        }

        if (submit.WasPressedThisFrame())
        {
            Confirm();
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            var button = spawnedButtons[i];

            CanvasGroup cg = button.GetComponent<CanvasGroup>();

            if (cg == null)
                cg = button.AddComponent<CanvasGroup>();

            if (i == currentIndex)
                cg.alpha = 0.85f;
            else
                cg.alpha = 1f;

            button.transform.localScale =
                (i == currentIndex) ? Vector3.one * 1.05f : Vector3.one;
        }
    }

    private void Confirm()
    {
        isActive = false;

        dialogueSystem.SelectChoice(currentIndex);

        HideChoices();
    }

    private void Clear()
    {
        foreach (var obj in spawnedButtons)
            Destroy(obj);

        spawnedButtons.Clear();
    }

    public void HideChoices()
    {
        Clear();

        isActive = false;

        if (answerPanel != null)
            answerPanel.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        up = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/upArrow");
        down = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/downArrow");
        submit = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/enter");

        up.Enable();
        down.Enable();
        submit.Enable();
    }
}