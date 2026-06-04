using UnityEngine;

/// <summary>
/// Скриптовая надстройка над <see cref="LiftInteractable"/>. При взаимодействии
/// проигрывает заданный набор реплик, а по ЗАВЕРШЕНИИ диалога принудительно
/// отправляет лифт на нижний этаж (локацию 0), переиспользуя список этажей и
/// конфиг анимации кабины самого LiftInteractable.
///
/// Задумано как разовая замена обычного поведения лифта на определённом отрезке
/// игры: держится выключенным (объект SetActive(false)), а в нужный момент
/// включается вместо LiftInteractable.
///
/// ВАЖНО про переключение: InteractionSystem берёт интерактабл через
/// GetComponent&lt;IInteractable&gt;() — первый компонент этого типа на объекте,
/// без учёта enabled. Поэтому этот скрипт и LiftInteractable должны жить на
/// РАЗНЫХ GameObject со своими коллайдерами, а переключение делать через
/// SetActive (выключить объект лифта / включить объект надстройки), а не enabled.
/// </summary>
public class ScriptedLiftInteractable : IInteractable
{
    [Header("Lift")]
    [Tooltip("Лифт, чьё поведение замещаем. Переиспользуем его список этажей и анимацию кабины.")]
    [SerializeField] private LiftInteractable lift;

    [Header("Dialogue")]
    [Tooltip("Набор реплик, который проигрывается перед спуском лифта.")]
    [SerializeField] private DialogueData dialogue;
    [Tooltip("Тот же DialogueSystem, что и в сцене — нужен, чтобы поймать завершение диалога.")]
    [SerializeField] private DialogueSystem dialogueSystem;

    // Защита от повторного запуска, пока реплики идут и лифт ещё не уехал.
    private bool isRunning;

    // Катсцена разовая: после первого прохода скрипт больше не отрабатывает.
    private bool hasPlayed;

    public override bool CanInteract()
    {
        return base.CanInteract() && !hasPlayed;
    }

    protected override void OnInteract()
    {
        if (isRunning || hasPlayed)
            return;

        if (lift == null || dialogue == null || dialogueSystem == null)
        {
            Debug.LogWarning($"{name}: не назначены lift/dialogue/dialogueSystem — нечего проигрывать.");
            return;
        }

        isRunning = true;

        // Финал игры: с этим лифтом игрок взаимодействует один раз (катсцена разовая),
        // поэтому здесь же плавно глушим всю музыку.
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        // Подписываемся ДО старта: игрок на время диалога заблокирован, поэтому
        // ближайший DialogueEnded — гарантированно наш. Фильтр по ссылке — на всякий.
        dialogueSystem.DialogueEnded += OnDialogueEnded;

        GameContext.Instance.DialogueSelector.SelectDialogue(dialogue);
    }

    private void OnDialogueEnded(DialogueData ended)
    {
        // Не наш диалог (на сцене мог завершиться другой) — ждём дальше.
        if (ended != dialogue)
            return;

        dialogueSystem.DialogueEnded -= OnDialogueEnded;
        isRunning = false;
        hasPlayed = true;

        // EndDialogue уже вернул управление игроку; LocationTransition.Go снова
        // его заблокирует на время поездки.
        lift.GoToGroundFloor();
    }

    private void OnDisable()
    {
        // Объект могут выключить посреди диалога — снимаем подписку, чтобы не
        // словить событие в неактивном состоянии. hasPlayed не трогаем: если
        // катсцена уже отработала, при повторном включении она не запустится.
        if (isRunning && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            isRunning = false;
        }
    }
}
