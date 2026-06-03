using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiftPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Buttons")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button closeButton;

    [Header("Broken direction placeholder")]
    [SerializeField] private GameObject brokenMessage;
    [Tooltip("Текст внутри brokenMessage. Подменяется уникальным сообщением этажа; " +
             "если этаж сообщение не задал — возвращается текст по умолчанию из префаба.")]
    [SerializeField] private TMP_Text brokenMessageText;
    [SerializeField] private float brokenMessageDuration = 2f;

    [Header("Player")]
    [SerializeField] private PlayerMovementController playerController;

    private LiftInteractable currentSource;
    private Coroutine brokenRoutine;
    private string defaultBrokenMessage;

    private void Awake()
    {
        if (brokenMessageText != null)
            defaultBrokenMessage = brokenMessageText.text;

        if (upButton != null)
            upButton.onClick.AddListener(OnUpClicked);

        if (downButton != null)
            downButton.onClick.AddListener(OnDownClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Open(LiftInteractable source)
    {
        currentSource = source;

        HideBrokenMessage();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        playerController.DisableMovement();
        playerController.DisableInteraction();
    }

    public void Close()
    {
        HideBrokenMessage();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        currentSource = null;

        playerController.EnableMovement();
        playerController.EnableInteraction();
    }

    public void ShowBrokenMessage(string message = null)
    {
        if (brokenMessage == null)
            return;

        // Уникальное сообщение этажа подменяет текст; пусто — возвращаем стандартное из префаба.
        if (brokenMessageText != null)
            brokenMessageText.text = string.IsNullOrEmpty(message) ? defaultBrokenMessage : message;

        if (brokenRoutine != null)
            StopCoroutine(brokenRoutine);

        brokenRoutine = StartCoroutine(BrokenMessageRoutine());
    }

    private IEnumerator BrokenMessageRoutine()
    {
        brokenMessage.SetActive(true);
        yield return new WaitForSeconds(brokenMessageDuration);
        brokenMessage.SetActive(false);
        brokenRoutine = null;
    }

    private void HideBrokenMessage()
    {
        if (brokenRoutine != null)
        {
            StopCoroutine(brokenRoutine);
            brokenRoutine = null;
        }

        if (brokenMessage != null)
            brokenMessage.SetActive(false);
    }

    private void OnUpClicked()
    {
        if (currentSource != null)
            currentSource.TryGoUp();
    }

    private void OnDownClicked()
    {
        if (currentSource != null)
            currentSource.TryGoDown();
    }
}
