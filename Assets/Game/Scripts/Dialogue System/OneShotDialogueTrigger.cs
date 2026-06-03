using UnityEngine;

/// <summary>
/// Разовый триггер-зона: когда игрок входит в коллайдер, запускает заданный диалог.
/// Срабатывает строго один раз.
///
/// В отличие от <see cref="OneShotDialogueEvent"/> (который ждёт ЗАВЕРШЕНИЯ другого
/// диалога), здесь источник срабатывания — физическое касание игроком триггера.
/// Запуск диалога идёт тем же путём, что и у <see cref="DialogueTrigger"/> /
/// <see cref="OneShotEntranceCutscene"/>: через GameContext.DialogueSelector.
///
/// Вешается на объект с Collider2D (isTrigger выставляется автоматически).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class OneShotDialogueTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Диалог, который запускается, когда игрок входит в зону.")]
    [SerializeField] private DialogueData dialogue;

    private bool hasFired;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasFired)
        {
            return;
        }

        // Реагируем только на игрока. Коллайдер может быть на дочернем объекте,
        // поэтому ищем контроллер в родителях (по аналогии с Egg/PlayerCatcher).
        if (other.GetComponentInParent<PlayerMovementController>() == null)
        {
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning($"OneShotDialogueTrigger ('{name}'): dialogue не назначен — диалог не запустится.");
            return;
        }

        hasFired = true;

        // Триггер одноразовый — отключаем коллайдер, чтобы не ловить повторные входы.
        GetComponent<Collider2D>().enabled = false;

        GameContext.Instance.DialogueSelector.SelectDialogue(dialogue);
    }
}
