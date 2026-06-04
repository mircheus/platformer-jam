using UnityEngine;

/// <summary>
/// Скриптовая надстройка над <see cref="LiftInteractable"/>, аналог
/// <see cref="ScriptedLiftInteractable"/>, но для подъёма ВВЕРХ. При взаимодействии
/// проигрывает заданный набор реплик, а по ЗАВЕРШЕНИИ диалога принудительно
/// поднимает лифт на этаж <see cref="targetFloorIndex"/> (по умолчанию 4), минуя все
/// промежуточные локации и квест-гейты. Переиспользует список этажей и конфиг
/// анимации кабины самого LiftInteractable.
///
/// Опционально по ОКОНЧАНИИ приезда лифта (когда отыграла анимация прибытия и
/// игроку вернули управление) проигрывает второй диалог <see cref="arrivalDialogue"/>.
/// «Лифт доехал» ловится через обобщённое событие
/// <see cref="LocationTransitionController.ArrivedAtLocation"/>.
///
/// Катсцена разовая: после первого прохода скрипт больше не отрабатывает.
///
/// ВАЖНО про переключение: InteractionSystem берёт интерактабл через
/// GetComponent&lt;IInteractable&gt;() — первый компонент этого типа на объекте,
/// без учёта enabled. Поэтому этот скрипт и LiftInteractable должны жить на
/// РАЗНЫХ GameObject со своими коллайдерами, а переключение делать через
/// SetActive (выключить объект лифта / включить объект надстройки), а не enabled.
/// </summary>
public class ScriptedLiftAscentInteractable : IInteractable
{
    [Header("Lift")]
    [Tooltip("Лифт, чьё поведение замещаем. Переиспользуем его список этажей и анимацию кабины.")]
    [SerializeField] private LiftInteractable lift;
    [Tooltip("Индекс этажа, на который поднимается лифт (минуя промежуточные).")]
    [SerializeField] private int targetFloorIndex = 4;

    [Header("Sound")]
    [Tooltip("Звук, проигрываемый разово в момент взаимодействия (старт катсцены подъёма).")]
    [SerializeField] private AudioClip interactSound;

    [Header("Dialogue")]
    [Tooltip("Набор реплик, который проигрывается перед подъёмом лифта.")]
    [SerializeField] private DialogueData dialogue;
    [Tooltip("Опциональный набор реплик, который проигрывается ПОСЛЕ того, как лифт приедет на целевой этаж. Если пусто — после приезда ничего не играется.")]
    [SerializeField] private DialogueData arrivalDialogue;
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

        // Разовый звук в момент взаимодействия (катсцена тоже разовая).
        if (interactSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(interactSound);

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
        hasPlayed = true;
        // isRunning остаётся true: катсцена продолжается — едем и ждём приезда.

        // Ловим ОКОНЧАНИЕ приезда лифта. Игрок заблокирован на всю поездку, а
        // LocationTransition не пускает параллельные переходы (IsRunning), поэтому
        // ближайший ArrivedAtLocation — гарантированно прибытие нашего лифта.
        GameContext.Instance.LocationTransition.ArrivedAtLocation += OnLiftArrived;

        // EndDialogue уже вернул управление игроку; LocationTransition.Go снова
        // его заблокирует на время поездки.
        lift.GoToFloor(targetFloorIndex, goingUp: true);
    }

    private void OnLiftArrived(Location location)
    {
        GameContext.Instance.LocationTransition.ArrivedAtLocation -= OnLiftArrived;

        // Нет реплики на приезд — на этом катсцена завершена.
        if (arrivalDialogue == null)
        {
            isRunning = false;
            return;
        }

        // Игрок только что получил управление обратно (приезд завершился) — диалог
        // снова его заблокирует. isRunning держим до конца этой реплики.
        dialogueSystem.DialogueEnded += OnArrivalDialogueEnded;
        GameContext.Instance.DialogueSelector.SelectDialogue(arrivalDialogue);
    }

    private void OnArrivalDialogueEnded(DialogueData ended)
    {
        // Не наш диалог — ждём дальше.
        if (ended != arrivalDialogue)
            return;

        dialogueSystem.DialogueEnded -= OnArrivalDialogueEnded;
        isRunning = false;
    }

    private void OnDisable()
    {
        // Объект могут выключить посреди катсцены (диалог / поездка / реплика на
        // приезд) — снимаем все подписки, чтобы не словить событие в неактивном
        // состоянии. hasPlayed не трогаем: если катсцена уже отработала, при
        // повторном включении она не запустится.
        if (!isRunning)
            return;

        isRunning = false;

        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            dialogueSystem.DialogueEnded -= OnArrivalDialogueEnded;
        }

        if (GameContext.Instance != null && GameContext.Instance.LocationTransition != null)
        {
            GameContext.Instance.LocationTransition.ArrivedAtLocation -= OnLiftArrived;
        }
    }
}
