using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
/// Это ФИНАЛЬНАЯ катсцена игры: после завершения последнего диалога (приездного,
/// либо стартового, если приездного нет) выжидает <see cref="endingDelay"/> секунд,
/// затемняет экран через <see cref="ScreenFader"/> в полную темноту и плавно
/// проявляет надпись «Конец» (<see cref="endScreen"/>). Затем — после паузы
/// <see cref="creditsDelay"/> — мягко гасит «Конец» и проявляет опциональные
/// статичные титры (<see cref="creditsScreen"/>). Длительность фейда надписей —
/// <see cref="labelFadeDuration"/>. После титров (пауза <see cref="returnToMenuDelay"/>)
/// автоматически загружает сцену главного меню <see cref="mainMenuSceneName"/>.
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

    [Header("Ending")]
    [Tooltip("Пауза (сек) после завершения последнего диалога перед затемнением экрана.")]
    [SerializeField] private float endingDelay = 3f;
    [Tooltip("ScreenFader, которым экран затемняется в полную темноту в финале катсцены.")]
    [SerializeField] private ScreenFader screenFader;
    [Tooltip("Выровнять игрока по X в момент затемнения (под чёрным экраном), чтобы " +
             "камера встала ровно к показу «Конца» и дёрганья не было видно.")]
    [SerializeField] private bool alignPlayerXOnFade = true;
    [Tooltip("Значение X, на которое ставится игрок при выравнивании под затемнением.")]
    [SerializeField] private float alignedPlayerX = 0f;
    [Tooltip("CanvasGroup надписи «Конец», которая появляется после затемнения и плавно " +
             "гаснет перед показом титров.")]
    [SerializeField] private CanvasGroup endScreen;
    [Tooltip("Пауза (сек) после показа надписи «Конец» перед переходом к титрам.")]
    [SerializeField] private float creditsDelay = 3f;
    [Tooltip("CanvasGroup статичных титров, которые плавно проявляются после надписи «Конец». Пусто — титры не показываются.")]
    [SerializeField] private CanvasGroup creditsScreen;
    [Tooltip("Длительность (сек) мягкого фейда надписей: затухание «Конца» и проявление титров.")]
    [SerializeField] private float labelFadeDuration = 1f;
    [Tooltip("Пауза (сек) после показа титров перед автоматической загрузкой главного меню.")]
    [SerializeField] private float returnToMenuDelay = 5f;
    [Tooltip("Имя сцены главного меню (как в Build Settings), в которую возвращаемся после титров.")]
    [SerializeField] private string mainMenuSceneName = "_Menu";

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

        // Катсцена подъёма разовая — здесь же плавно поднимаем тему концовки
        // поверх геймплейной.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEndingMusic();

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

        // Нет реплики на приезд — стартовый диалог и был последним, сразу к финалу.
        if (arrivalDialogue == null)
        {
            StartEnding();
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
        // Приездной диалог был последним — запускаем финал.
        StartEnding();
    }

    /// <summary>
    /// Финал катсцены (и игры): ждём <see cref="endingDelay"/> секунд, затемняем
    /// экран в полную темноту и включаем надпись «Конец». Вызывается после
    /// завершения последнего диалога катсцены. isRunning держим до самого конца
    /// последовательности — катсцена считается идущей, пока не показан «Конец».
    /// </summary>
    private void StartEnding()
    {
        // Это финал: предыдущий диалог/приезд лифта только что ВЕРНУЛ игроку
        // управление (EnableMovement/EnableInteraction). Отбираем его окончательно
        // прямо здесь — синхронно, без зазора в кадр, — чтобы во время endingDelay,
        // фейда и титров персонаж был полностью обездвижен и больше не оживал.
        DisablePlayerControl();

        StartCoroutine(EndingRoutine());
    }

    /// <summary>
    /// Окончательно блокирует управление игроком на финал катсцены: движение,
    /// взаимодействие и подсветку интерактаблов. Обратно управление не возвращаем —
    /// после титров сцена выгружается в главное меню.
    /// </summary>
    private void DisablePlayerControl()
    {
        if (GameContext.Instance == null)
            return;

        if (GameContext.Instance.Player != null
            && GameContext.Instance.Player.TryGetComponent(out PlayerMovementController player))
        {
            player.DisableMovement();
            player.DisableInteraction();

            Rigidbody2D rb = GameContext.Instance.Player.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        // Гасим ещё и подсветку интерактаблов: DisableMovement.canInteract уже
        // блокирует ввод, но pop-up прячет именно InteractionSystem.
        if (GameContext.Instance.InteractionSystem != null)
            GameContext.Instance.InteractionSystem.DisableInteraction();
    }

    /// <summary>
    /// Ставит игрока на <see cref="alignedPlayerX"/> по X (Y/Z сохраняются), чтобы
    /// выровнять кадр перед показом финальных надписей. Зовётся ТОЛЬКО под полностью
    /// затемнённым экраном — иначе будет виден рывок камеры за игроком.
    /// </summary>
    private void AlignPlayerX()
    {
        if (GameContext.Instance == null || GameContext.Instance.Player == null)
            return;

        Transform playerTransform = GameContext.Instance.Player.transform;
        Vector3 position = playerTransform.position;
        position.x = alignedPlayerX;
        playerTransform.position = position;
    }

    private IEnumerator EndingRoutine()
    {
        yield return new WaitForSeconds(endingDelay);

        if (screenFader != null)
            yield return screenFader.FadeOut();
        else
            Debug.LogWarning($"{name}: не назначен screenFader — экран не затемнится.");

        // Экран уже полностью чёрный — телепорт игрока по X и резкий сдвиг камеры
        // за ним скрыты темнотой. К показу «Конца» камера встанет ровно.
        if (alignPlayerXOnFade)
            AlignPlayerX();

        // Надпись «Конец» плавно проявляется на затемнённом экране.
        if (endScreen != null)
            yield return FadeLabel(endScreen, 0f, 1f);
        else
            Debug.LogWarning($"{name}: не назначен endScreen — надпись «Конец» не покажется.");

        // Статичные титры после надписи «Конец». Опционально — если объект не назначен,
        // на «Конце» всё и заканчивается.
        if (creditsScreen != null)
        {
            yield return new WaitForSeconds(creditsDelay);

            // Сначала мягко гасим «Конец», затем проявляем титры.
            if (endScreen != null)
            {
                yield return FadeLabel(endScreen, 1f, 0f);
                endScreen.gameObject.SetActive(false);
            }

            yield return FadeLabel(creditsScreen, 0f, 1f);
        }

        // Вся катсцена полностью завершена.
        isRunning = false;

        // Автовозврат в главное меню после титров. LoadScene уничтожит эту сцену
        // (и объект), поэтому это последний шаг последовательности.
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            yield return new WaitForSeconds(returnToMenuDelay);
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning($"{name}: не задан mainMenuSceneName — автовозврат в меню не выполнен.");
        }
    }

    /// <summary>
    /// Мягкий фейд альфы <paramref name="group"/> от <paramref name="from"/> к
    /// <paramref name="to"/> за <see cref="labelFadeDuration"/> секунд. Перед стартом
    /// включает объект и выставляет начальную альфу, так что годится и для проявления
    /// (0→1), и для затухания (1→0).
    /// </summary>
    private IEnumerator FadeLabel(CanvasGroup group, float from, float to)
    {
        group.alpha = from;
        group.gameObject.SetActive(true);

        if (labelFadeDuration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < labelFadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / labelFadeDuration);
            yield return null;
        }

        group.alpha = to;
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
