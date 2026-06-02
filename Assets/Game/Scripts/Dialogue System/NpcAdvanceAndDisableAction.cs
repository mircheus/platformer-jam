using UnityEngine;

/// <summary>
/// Утилита-действие для вызова из UnityEvent (например, из
/// <see cref="OneShotDepartureCutscene"/> по завершении ухода). Один публичный
/// метод <see cref="Execute"/> делает две вещи: продвигает прогресс диалога у
/// NPC с заданным ObjectID (<see cref="NPCInteractable.AdvanceProgress"/>) и
/// выключает указанный GameObject.
///
/// Поиск NPC — по ID среди всех NPCInteractable в сцене, включая выключенные.
/// </summary>
public class NpcAdvanceAndDisableAction : MonoBehaviour
{
    [Header("Advance NPC Progress")]
    [Tooltip("ObjectID NPC, чьему диалогу продвигается прогресс. Пусто — шаг пропускается.")]
    [SerializeField] private string npcId;

    [Header("Disable Object")]
    [Tooltip("Объект, который выключается (SetActive(false)). Пусто — шаг пропускается.")]
    [SerializeField] private GameObject objectToDisable;

    /// <summary>
    /// Продвигает прогресс NPC по ID и выключает заданный объект. Каждый шаг
    /// независим: незаполненное поле просто пропускается.
    /// </summary>
    public void Execute()
    {
        AdvanceNpc();
        DisableObject();
    }

    private void AdvanceNpc()
    {
        if (string.IsNullOrEmpty(npcId))
        {
            return;
        }

        NPCInteractable npc = FindNpcById(npcId);

        if (npc == null)
        {
            Debug.LogWarning($"NpcAdvanceAndDisableAction ('{name}'): NPC с ObjectID '{npcId}' не найден — прогресс не продвинут.");
            return;
        }

        npc.AdvanceProgress();

        Debug.Log($"NpcAdvanceAndDisableAction ('{name}'): прогресс NPC '{npcId}' продвинут до {npc.Progress}.");
    }

    private void DisableObject()
    {
        if (objectToDisable == null)
        {
            return;
        }

        objectToDisable.SetActive(false);
    }

    private NPCInteractable FindNpcById(string id)
    {
        // Включая выключенные объекты: NPC мог уже уйти/быть деактивирован к моменту вызова.
        foreach (NPCInteractable npc in FindObjectsByType<NPCInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (npc.ObjectID == id)
            {
                return npc;
            }
        }

        return null;
    }
}
