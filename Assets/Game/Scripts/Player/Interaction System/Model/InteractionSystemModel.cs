using System.Collections.Generic;

public class InteractionSystemModel
{
    public List<IInteractable> nearbyInteractables = new();
    public IInteractable currentInteractable;

    public List<IInteractable> GetNearbyInteractables()
    {
        return nearbyInteractables;
    }
    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }

    public void UpdateCurrentInteractable(IInteractable currentInteractable)
    {
        this.currentInteractable = currentInteractable;
    }

    public void AddNearbyInteractables(IInteractable child)
    {
        nearbyInteractables.Add(child);
    }
    public void RemoveChildNearbyInteractables(IInteractable child)
    {
        nearbyInteractables.Remove(child);
    }
}