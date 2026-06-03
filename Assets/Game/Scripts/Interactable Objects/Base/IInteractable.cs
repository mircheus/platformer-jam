using UnityEngine;
using UnityEngine.Events;

public abstract class IInteractable : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string objectID;
    [SerializeField] private string displayName;
    [SerializeField] private string promptText = "Interact";

    [Header("State")]
    [SerializeField] protected bool isInteractable = true;

    [Header("Prompt")]
    [Tooltip("Спрайт/панель pop-up, который подсвечивается, когда игрок рядом и может " +
             "взаимодействовать. Включается и выключается автоматически.")]
    [SerializeField] private GameObject interactionPrompt;

    public string ObjectID => objectID;
    public string DisplayName => displayName;
    public string PromptText => promptText;

    /// <summary>У объекта назначен pop-up, который можно показывать.</summary>
    public bool HasPrompt => interactionPrompt != null;

    protected virtual void Awake()
    {
        // На случай, если в сцене pop-up оставили включённым — прячем на старте.
        HidePrompt();
    }

    public virtual bool CanInteract()
    {
        return isInteractable;
    }

    /// <summary>Показать pop-up (если он назначен).</summary>
    public void ShowPrompt()
    {
        if (interactionPrompt != null && !interactionPrompt.activeSelf)
            interactionPrompt.SetActive(true);
    }

    /// <summary>Спрятать pop-up (если он назначен).</summary>
    public void HidePrompt()
    {
        if (interactionPrompt != null && interactionPrompt.activeSelf)
            interactionPrompt.SetActive(false);
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        OnInteract();
    }

    protected abstract void OnInteract();
}