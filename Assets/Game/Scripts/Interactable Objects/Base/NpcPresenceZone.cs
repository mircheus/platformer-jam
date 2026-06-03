using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Триггер-зона, которая следит за находящимися внутри NPC (<see cref="NPCInteractable"/>).
/// Пока в зоне есть хотя бы один NPC, <see cref="IsOccupied"/> возвращает true.
/// Используется, например, лифтом, чтобы блокировать взаимодействие, пока рядом стоит NPC.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NpcPresenceZone : MonoBehaviour
{
    private readonly HashSet<NPCInteractable> npcsInside = new();

    private Collider2D triggerZone;

    /// <summary>В зоне сейчас есть хотя бы один NPC.</summary>
    public bool IsOccupied => npcsInside.Count > 0;

    private void Awake()
    {
        triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;
    }

    /// <summary>
    /// Перечищает список NPC по коллайдерам, реально перекрывающим зону сейчас. Нужен после
    /// свапа локаций, когда нельзя полагаться на OnTriggerEnter2D/Exit2D (по аналогии с
    /// InteractionSystemController.RefreshInteractables).
    /// </summary>
    public void RefreshOccupants()
    {
        npcsInside.Clear();

        if (triggerZone == null)
            return;

        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        filter.useTriggers = true;

        List<Collider2D> overlaps = new();
        triggerZone.Overlap(filter, overlaps);

        foreach (Collider2D collider in overlaps)
        {
            NPCInteractable npc = collider.GetComponent<NPCInteractable>();

            if (Tracks(npc))
                npcsInside.Add(npc);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NPCInteractable npc = collision.GetComponent<NPCInteractable>();

        if (Tracks(npc))
            npcsInside.Add(npc);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        NPCInteractable npc = collision.GetComponent<NPCInteractable>();

        if (npc != null)
            npcsInside.Remove(npc);
    }

    /// <summary>
    /// Зона отслеживает NPC, только если он существует и не помечен флагом
    /// <see cref="NPCInteractable.IgnoredByNpcPresenceZone"/>.
    /// </summary>
    private static bool Tracks(NPCInteractable npc)
    {
        return npc != null && !npc.IgnoredByNpcPresenceZone;
    }
}
