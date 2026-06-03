using System.Collections.Generic;
using UnityEngine;

public class InteractionSystemController : MonoBehaviour
{
    private readonly List<IInteractable> nearbyInteractables = new();

    private bool _canInteract = true;

    private Collider2D triggerZone;

    // Объект, на котором сейчас горит pop-up (всегда ближайший интерактивный).
    private IInteractable promptedInteractable;

    private void Awake()
    {
        triggerZone = GetComponent<Collider2D>();
    }

    private void Update()
    {
        UpdatePrompt();
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

        IInteractable closestInteractable = FindClosestInteractable(playerTransform.position);

        if (closestInteractable == null)
        {
            Debug.Log("Ближайший интерактивный объект не найден.");

            return;
        }

        Debug.Log(
            $"Ближайший интерактивный объект | " +
            $"Name: {closestInteractable.DisplayName} | " +
            $"ID : {closestInteractable.ObjectID}"
            );

        closestInteractable.Interact();
    }

    /// <summary>
    /// Возвращает ближайший к точке интерактивный объект из находящихся в зоне
    /// (пропуская уже уничтоженные). Один и тот же метод используют и подсветка
    /// pop-up, и само взаимодействие — чтобы они всегда указывали на один объект.
    /// </summary>
    private IInteractable FindClosestInteractable(Vector3 fromPosition)
    {
        IInteractable closest = null;

        float closestDistance = Mathf.Infinity;

        foreach (IInteractable interactable in nearbyInteractables)
        {
            MonoBehaviour interactableObject = interactable as MonoBehaviour;

            if (interactableObject == null)
            {
                continue;
            }

            float distance = Vector2.Distance(fromPosition, interactableObject.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    /// <summary>
    /// Каждый кадр держит pop-up включённым ровно на одном объекте — ближайшем,
    /// с которым прямо сейчас сработает кнопка взаимодействия. Если взаимодействие
    /// выключено, рядом ничего нет или ближайший сейчас не интерактивен — гасит подсветку.
    /// </summary>
    private void UpdatePrompt()
    {
        IInteractable target = null;

        if (_canInteract && nearbyInteractables.Count > 0)
        {
            IInteractable closest = FindClosestInteractable(transform.position);

            if (closest != null && closest.CanInteract())
                target = closest;
        }

        if (ReferenceEquals(target, promptedInteractable))
            return;

        if (promptedInteractable != null)
            promptedInteractable.HidePrompt();

        promptedInteractable = target;

        if (promptedInteractable != null)
            promptedInteractable.ShowPrompt();
    }

    public void EnableInteraction()
    {
        _canInteract = true;
    }

    public void DisableInteraction()
    {
        _canInteract = false;

        // Сразу гасим подсветку, чтобы pop-up не висел во время катсцен/диалогов.
        if (promptedInteractable != null)
            promptedInteractable.HidePrompt();

        promptedInteractable = null;
    }
}
