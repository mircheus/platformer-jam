using UnityEngine;

public class InteractionSystemController : MonoBehaviour
{
    private InteractionSystemModel model;

    private void Awake()
    {
        model = new InteractionSystemModel();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if(interactable == null)
        {
            return;
        }

        if (model.GetNearbyInteractables().Contains(interactable))
        {
            return;
        }

        model.AddNearbyInteractables(interactable);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if(interactable == null)
        {
            return;
        }

        if (!model.GetNearbyInteractables().Contains(interactable))
        {
            return;
        }

        model.RemoveChildNearbyInteractables(interactable);
    }

    public void InteractableObjectSearch(Transform playerTransform)
    {
        if(model.GetNearbyInteractables().Count == 0)
        {
            Debug.Log("В непосредственной близости нет интерактивных объектов.");

            return;
        }

        IInteractable closestInteractable = null;

        float closestDistabce = Mathf.Infinity;

        foreach(IInteractable interactable in model.GetNearbyInteractables())
        {
            MonoBehaviour interactableObject = 
                interactable as MonoBehaviour;

            if(interactableObject == null)
            {
                continue;
            }

            float distance = Vector2.Distance(
                playerTransform.position,
                interactableObject.transform.position
                );

            if(distance < closestDistabce)
            {
                closestDistabce = distance;
                closestInteractable = interactable;
            }

            if(closestInteractable == null)
            {
                Debug.Log("Ближайший интерактивный объект не найден.");

                return;
            }

            model.UpdateCurrentInteractable(closestInteractable);

            Debug.Log(
                $"Ближайший интерактивный объект | " +
                $"Name: {closestInteractable.DisplayName} | " +
                $"ID : {closestInteractable.ObjectID} | " +
                $"Distance: {closestDistabce}"
                );

            closestInteractable.Interact();
        }
    }
}