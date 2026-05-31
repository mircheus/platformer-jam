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

    public string ObjectID => objectID;
    public string DisplayName => displayName;
    public string PromptText => promptText;

    public virtual bool CanInteract()
    {
        return isInteractable;
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        OnInteract();
    }

    protected abstract void OnInteract();
}