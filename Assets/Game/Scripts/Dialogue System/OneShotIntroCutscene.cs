using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Разовая вступительная катсцена. Оркеструет открытие игры цепочкой камер и
/// диалогов:
///
///   1. Общий план дерева (wideVcam) + вступительный диалог introDialogue
///      (Space листает реплики сам DialogueSystem).
///   2. Камера спуска (downVcam) перехватывает приоритет ВЫШЕ общего плана — Brain
///      плавным blend'ом опускает кадр к берлоге; затем диалог downDialogue.
///   3. Затемнение на пару секунд; под чёрным экраном переключаемся на камеру паука
///      (spiderVcam); осветление — и диалог spiderDialogue.
///   4. Финальное затемнение; под чёрным экраном Default Blend в Brain переключается
///      в Cut и приоритет отдаётся основной (геймплейной) камере слежения — переход
///      мгновенный, без видимого «проезда». Default Blend восстанавливается,
///      опциональный звук, осветление — и управление возвращается игроку.
///
/// В стиле <see cref="OneShotEntranceCutscene"/>/<see cref="OneShotArrivalCutscene"/>:
/// вешается на отдельный всегда-активный объект-оркестратор, подписывается на
/// обобщённое событие DialogueSystem.DialogueEnded. Ядро (DialogueSystem, камеры)
/// про эту сцену ничего не знает.
///
/// Вся последовательность живёт в одной мастер-корутине: каждый диалог стартуется и
/// «ожидается» через флаг, выставляемый в OnDialogueEnded. Переключения камер — через
/// приоритеты Cinemachine (камера-победитель = наибольший Priority среди активных).
/// Финальный переход к геймплейной камере делается под чёрным экраном с временной
/// сменой Default Blend на Cut, чтобы не было видимого проезда.
/// </summary>
public class OneShotIntroCutscene : MonoBehaviour
{
    /// <summary>Текущая фаза катсцены.</summary>
    private enum Phase
    {
        Idle,     // ещё не начали
        Running,  // идёт последовательность
        Done
    }

    [Header("Core References")]
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private ScreenFader screenFader;

    [Header("Start")]
    [Tooltip("Запустить катсцену автоматически в Start. Выключи, если хочешь дёргать BeginIntro() извне.")]
    [SerializeField] private bool playOnStart = true;
    [Tooltip("Пауза перед самым началом катсцены (даёт сцене/системам прогрузиться).")]
    [SerializeField] private float startDelay = 0.25f;

    [Header("1. Intro — Wide Shot + Dialogue")]
    [Tooltip("Опциональный объект фона (общий план дерева, рамка с текстом и т.п.). Включается на старте.")]
    [SerializeField] private GameObject introBackground;
    [Tooltip("Cinemachine-камера общего плана дерева (активна в начале интро).")]
    [SerializeField] private CinemachineCamera wideVcam;
    [Tooltip("Priority общего плана, пока идёт интро. Должен быть ВЫШЕ приоритета геймплейной камеры слежения, чтобы wide гарантированно выигрывал.")]
    [SerializeField] private int widePriorityActive = 90;
    [Tooltip("Priority, на который опускается общий план под финальным чёрным экраном (cut к геймплейной камере). Должен быть НИЖЕ приоритета геймплейной камеры слежения.")]
    [SerializeField] private int widePriorityInactive = -10;
    [Tooltip("Вступительный диалог, который листается по Space на общем плане.")]
    [SerializeField] private DialogueData introDialogue;

    [Header("2. Camera Descent (Wide → Down) + Dialogue")]
    [Tooltip("Cinemachine-камера спуска к берлоге. На неё Brain плавно сводит кадр с общего плана.")]
    [SerializeField] private CinemachineCamera downVcam;
    [Tooltip("Priority камеры спуска во время спуска. Должен быть ВЫШЕ widePriorityActive, чтобы Brain сделал blend wide → down.")]
    [SerializeField] private int downPriorityActive = 100;
    [Tooltip("Priority камеры спуска под финальным чёрным экраном (cut к геймплейной камере). Должен быть НИЖЕ приоритета геймплейной камеры слежения.")]
    [SerializeField] private int downPriorityInactive = -10;
    [Tooltip("Пауза после вступительного диалога перед началом спуска камеры.")]
    [SerializeField] private float delayBeforeDescent = 0.4f;
    [Tooltip("Сколько ждать завершения блenda камеры wide → down. Выстави равным длительности Default Blend в CinemachineBrain (на Main Camera).")]
    [SerializeField] private float descentDuration = 2f;
    [Tooltip("Диалог на камере спуска (берлога). Можно оставить пустым — тогда шаг просто пропускается.")]
    [SerializeField] private DialogueData downDialogue;

    [Header("3. Fade → Spider Cam + Dialogue")]
    [Tooltip("Cinemachine-камера паука. Переключение на неё происходит под чёрным экраном.")]
    [SerializeField] private CinemachineCamera spiderVcam;
    [Tooltip("Priority камеры паука, пока идёт её диалог. Должен быть ВЫШЕ wide/down, чтобы она выиграла фокус.")]
    [SerializeField] private int spiderPriorityActive = 110;
    [Tooltip("Priority камеры паука под финальным чёрным экраном (cut к геймплейной камере). Должен быть НИЖЕ приоритета геймплейной камеры слежения.")]
    [SerializeField] private int spiderPriorityInactive = -10;
    [Tooltip("Сколько держать чёрный экран при переключении на камеру паука. Должно быть НЕ МЕНЬШЕ длительности Default Blend, чтобы переход успел пройти под чёрным.")]
    [SerializeField] private float blackHoldBeforeSpider = 2f;
    [Tooltip("Выключить фон общего плана под чёрным экраном перед камерой паука. Оставь выключенным, если фон прятать не нужно.")]
    [SerializeField] private bool hideBackgroundAfterDescent = true;
    [Tooltip("Диалог на камере паука. Можно оставить пустым — тогда шаг просто пропускается.")]
    [SerializeField] private DialogueData spiderDialogue;

    [Header("4. Fade → Cut To Gameplay → Fade In")]
    [Tooltip("CinemachineBrain (обычно на Main Camera). Под чёрным экраном его Default Blend временно ставится в Cut, чтобы переход к геймплейной камере был мгновенным. Если пусто — попробуем найти на Camera.main.")]
    [SerializeField] private CinemachineBrain brain;
    [Tooltip("Опциональный звук под финальным чёрным экраном (например, стук молотков/пиление). Можно оставить пустым.")]
    [SerializeField] private AudioSource audioUnderBlack;
    [Tooltip("Пауза под финальным чёрным экраном (даём звуку/таймскипу прозвучать) перед осветлением.")]
    [SerializeField] private float delayUnderBlack = 1.5f;

    private Phase phase = Phase.Idle;
    private bool subscribed;

    // Ожидание завершения конкретного диалога внутри мастер-корутины.
    private DialogueData awaitedDialogue;
    private bool awaitedDialogueEnded;

    private void Start()
    {
        if (dialogueSystem == null)
        {
            Debug.LogWarning($"OneShotIntroCutscene ('{name}'): dialogueSystem не назначен — катсцена не запустится.");
            return;
        }

        dialogueSystem.DialogueEnded += OnDialogueEnded;
        subscribed = true;

        if (playOnStart)
        {
            StartCoroutine(IntroRoutine());
        }
    }

    private void OnDisable()
    {
        if (subscribed && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            subscribed = false;
        }
    }

    /// <summary>
    /// Внешний вход, если playOnStart выключен (например, после собственного
    /// фейд-ина из главного меню).
    /// </summary>
    public void BeginIntro()
    {
        if (phase != Phase.Idle)
        {
            return;
        }

        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        if (phase != Phase.Idle)
        {
            yield break;
        }

        phase = Phase.Running;

        SetPlayerBlocked(true);

        // Расставляем стартовые приоритеты: общий план — наверху, остальные камеры
        // катсцены пока внизу, чтобы не было ничьей с геймплейной камерой слежения.
        SetPriority(wideVcam, widePriorityActive);
        SetPriority(downVcam, downPriorityInactive);
        SetPriority(spiderVcam, spiderPriorityInactive);

        if (introBackground != null)
        {
            introBackground.SetActive(true);
        }

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // --- Шаг 1: общий план + вступительный диалог ---
        yield return PlayDialogueAndWait(introDialogue);

        // --- Шаг 2: спуск wide → down + диалог берлоги ---
        if (delayBeforeDescent > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDescent);
        }

        // Поднимаем приоритет камеры спуска ВЫШЕ общего плана — Brain плавно сводит
        // кадр с wide на down (Default Blend, Ease In Out).
        // (Если экран «телепортируется» вместо плавного спуска — Default Blend в Brain
        // стоит в Cut или 0с; поставь Ease In Out со временем.)
        if (downVcam != null)
        {
            downVcam.Priority = downPriorityActive;
        }
        else
        {
            Debug.LogWarning($"OneShotIntroCutscene ('{name}'): downVcam не назначена — спуск камеры не произойдёт.");
        }

        if (descentDuration > 0f)
        {
            yield return new WaitForSeconds(descentDuration);
        }

        yield return PlayDialogueAndWait(downDialogue);

        // --- Шаг 3: затемнение → камера паука под чёрным → осветление + диалог ---
        if (screenFader != null)
        {
            yield return screenFader.FadeOut();
        }

        // Фон общего плана прячем под чёрным экраном — без видимого «прыжка».
        if (hideBackgroundAfterDescent && introBackground != null)
        {
            introBackground.SetActive(false);
        }

        // Переключаемся на камеру паука под чёрным экраном. Держим чёрный не меньше
        // длительности Default Blend, чтобы переход успел пройти невидимым.
        if (spiderVcam != null)
        {
            spiderVcam.Priority = spiderPriorityActive;
        }
        else
        {
            Debug.LogWarning($"OneShotIntroCutscene ('{name}'): spiderVcam не назначена — переключение на паука не произойдёт.");
        }

        if (blackHoldBeforeSpider > 0f)
        {
            yield return new WaitForSeconds(blackHoldBeforeSpider);
        }

        if (screenFader != null)
        {
            yield return screenFader.FadeIn();
        }

        yield return PlayDialogueAndWait(spiderDialogue);

        // --- Шаг 4: финальное затемнение → cut к геймплейной камере ---
        yield return FinishToGameplayRoutine();

        phase = Phase.Done;

        // Управление — игроку.
        SetPlayerBlocked(false);
    }

    private IEnumerator FinishToGameplayRoutine()
    {
        if (screenFader != null)
        {
            yield return screenFader.FadeOut();
        }

        // Под чёрным экраном переключаемся на геймплейную камеру МГНОВЕННО (cut), чтобы
        // не было видимого «проезда» к игроку. Меняем Default Blend в Brain на Cut,
        // опускаем все катсценовые камеры НИЖЕ геймплейной — фокус перехватывает камера
        // слежения за игроком. Исходный Default Blend восстановим перед осветлением,
        // чтобы остальной геймплей продолжал блендить как обычно.
        CinemachineBrain activeBrain = ResolveBrain();
        bool blendOverridden = false;
        CinemachineBlendDefinition savedBlend = default;

        if (activeBrain != null)
        {
            savedBlend = activeBrain.DefaultBlend;
            activeBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            blendOverridden = true;
        }
        else
        {
            Debug.LogWarning($"OneShotIntroCutscene ('{name}'): CinemachineBrain не найден — переход к геймплейной камере может пройти блендом, а не cut'ом.");
        }

        SetPriority(wideVcam, widePriorityInactive);
        SetPriority(downVcam, downPriorityInactive);
        SetPriority(spiderVcam, spiderPriorityInactive);

        if (audioUnderBlack != null)
        {
            audioUnderBlack.Play();
        }

        if (delayUnderBlack > 0f)
        {
            yield return new WaitForSeconds(delayUnderBlack);
        }

        // Cut уже отработал под чёрным экраном — возвращаем исходный Default Blend,
        // чтобы дальнейшие переключения камер шли плавно.
        if (blendOverridden)
        {
            activeBrain.DefaultBlend = savedBlend;
        }

        // Осветление уже на геймплейной камере слежения за игроком.
        if (screenFader != null)
        {
            yield return screenFader.FadeIn();
        }
    }

    /// <summary>
    /// Стартует диалог и ждёт именно его завершения. null/пустой диалог — шаг
    /// пропускается. После завершения повторно блокирует игрока, т.к.
    /// DialogueSystem.EndDialogue возвращает ему управление.
    /// </summary>
    private IEnumerator PlayDialogueAndWait(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            yield break;
        }

        awaitedDialogue = dialogue;
        awaitedDialogueEnded = false;

        dialogueSystem.StartDialogue(dialogue);

        // Дальше ход у игрока: листает реплики по Space. Ждём флаг из OnDialogueEnded.
        while (!awaitedDialogueEnded)
        {
            yield return null;
        }

        awaitedDialogue = null;

        // EndDialogue вернул управление игроку — снова блокируем до конца катсцены.
        SetPlayerBlocked(true);
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
        if (awaitedDialogue != null && dialogue == awaitedDialogue)
        {
            awaitedDialogueEnded = true;
        }
    }

    private static void SetPriority(CinemachineCamera vcam, int priority)
    {
        if (vcam != null)
        {
            vcam.Priority = priority;
        }
    }

    /// <summary>
    /// Возвращает назначенный Brain, либо пытается найти его на Camera.main.
    /// </summary>
    private CinemachineBrain ResolveBrain()
    {
        if (brain != null)
        {
            return brain;
        }

        if (Camera.main != null)
        {
            brain = Camera.main.GetComponent<CinemachineBrain>();
        }

        return brain;
    }

    private void SetPlayerBlocked(bool blocked)
    {
        if (GameContext.Instance == null)
        {
            return;
        }

        GameObject player = GameContext.Instance.Player;

        if (player != null)
        {
            PlayerMovementController movement = player.GetComponent<PlayerMovementController>();

            if (movement != null)
            {
                if (blocked)
                {
                    movement.DisableMovement();
                    movement.DisableInteraction();
                }
                else
                {
                    movement.EnableMovement();
                    movement.EnableInteraction();
                }
            }
        }

        InteractionSystemController interaction = GameContext.Instance.InteractionSystem;

        if (interaction != null)
        {
            if (blocked)
            {
                interaction.DisableInteraction();
            }
            else
            {
                interaction.EnableInteraction();
            }
        }
    }
}
