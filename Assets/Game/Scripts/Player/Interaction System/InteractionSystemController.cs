using System.Collections.Generic;
using UnityEngine;

public class InteractionSystemController : MonoBehaviour
{
    private readonly List<IInteractable> nearbyInteractables = new();

    private bool _canInteract = true;

    private Collider2D triggerZone;

    private void Awake()
    {
        triggerZone = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Очищает список интерактаблов и пересобирает его по коллайдерам, реально
    /// перекрывающим триггер-зону игрока сейчас. Нужен после свапа локаций, когда
    /// нельзя полагаться на OnTriggerEnter2D/Exit2D.
    /// </summary>
    public void RefreshInteractables()
    {
        nearbyInteractables.Clear();

        if (triggerZone == null)
            return;

        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        filter.useTriggers = true;

        List<Collider2D> overlaps = new();
        triggerZone.Overlap(filter, overlaps);

        foreach (Collider2D collider in overlaps)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();

            if (interactable != null && !nearbyInteractables.Contains(interactable))
                nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if (interactable == null)
        {
            return;
        }

        if (nearbyInteractables.Contains(interactable))
        {
            return;
        }

        nearbyInteractables.Add(interactable);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if (interactable == null)
        {
            return;
        }

        nearbyInteractables.Remove(interactable);
    }

    public void InteractableObjectSearch(Transform playerTransform)
    {
        if (!_canInteract)
            return;
        
        if (nearbyInteractables.Count == 0)
        {
            Debug.Log("В непосредственной близости нет интерактивных объектов.");

            return;
        }

        IInteractable closestInteractable = null;

        float closestDistance = Mathf.Infinity;

        foreach (IInteractable interactable in nearbyInteractables)
        {
            MonoBehaviour interactableObject = interactable as MonoBehaviour;

            if (interactableObject == null)
            {
                continue;
            }

            float distance = Vector2.Distance(
                playerTransform.position,
                interactableObject.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable == null)
        {
            Debug.Log("Ближайший интерактивный объект не найден.");

            return;
        }

        Debug.Log(
            $"Ближайший интерактивный объект | " +
            $"Name: {closestInteractable.DisplayName} | " +
            $"ID : {closestInteractable.ObjectID} | " +
            $"Distance: {closestDistance}"
            );

        closestInteractable.Interact();
    }
    
    public void EnableInteraction()
    {
        _canInteract = true;
    }

    public void DisableInteraction()
    {
        _canInteract = false;
    }
}
