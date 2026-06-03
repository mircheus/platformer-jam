using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Разовая вступительная катсцена. Оркеструет открытие игры:
///
///   1. Общий план дерева + вступительный диалог (Space листает реплики сам
///      DialogueSystem).
///   2. После диалога камера блендом спускается к берлоге (фокус перехватывает
///      геймплейная камера слежения за игроком).
///   3. Затемнение, опциональный звук под чёрным экраном, осветление обратно на
///      той же берлоге — и управление возвращается игроку.
///
/// В стиле <see cref="OneShotEntranceCutscene"/>/<see cref="OneShotArrivalCutscene"/>:
/// вешается на отдельный всегда-активный объект-оркестратор, подписывается на
/// обобщённое событие DialogueSystem.DialogueEnded и фильтрует по вступительному
/// диалогу. Ядро (DialogueSystem, камеры) про эту сцену ничего не знает.
///
/// Спуск камеры реализован приоритетами Cinemachine: на старте поднимаем Priority
/// у vcam общего плана ВЫШЕ геймплейной камеры слежения, а при спуске опускаем его
/// НИЖЕ неё — фокус перехватывает камера слежения за игроком, и CinemachineBrain сам
/// делает плавный blend (отдельная vcam берлоги не нужна). Длительность blend задаётся
/// в Brain (Default Blend) — её же нужно проставить в descentDuration, чтобы корутина
/// дождалась окончания перехода перед затемнением.
/// </summary>
public class OneShotIntroCutscene : MonoBehaviour
{
    /// <summary>Текущая фаза катсцены.</summary>
    private enum Phase
    {
        Idle,           // ещё не начали
        IntroDialogue,  // показан общий план, идёт вступительный диалог
        Finishing,      // спуск камеры → затемнение → осветление
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
    [Tooltip("Вступительный диалог, который листается по Space на общем плане.")]
    [SerializeField] private DialogueData introDialogue;

    [Header("2. Camera Descent To Burrow")]
    [Tooltip("Cinemachine-камера общего плана дерева (активна в начале интро).")]
    [SerializeField] private CinemachineCamera wideVcam;
    [Tooltip("Priority общего плана, пока идёт интро. Должен быть ВЫШЕ приоритета геймплейной камеры слежения, чтобы wide гарантированно выигрывал (мини-игровая камера = 100 в интро не активна).")]
    [SerializeField] private int widePriorityActive = 90;
    [Tooltip("Priority, на который опускается общий план при спуске. Должен быть НИЖЕ приоритета геймплейной камеры, чтобы она перехватила фокус и Brain сделал blend к слежению за игроком.")]
    [SerializeField] private int widePriorityInactive = -10;
    [Tooltip("Пауза после диалога перед началом спуска камеры.")]
    [SerializeField] private float delayBeforeDescent = 0.4f;
    [Tooltip("Сколько ждать завершения блenda камеры. Выстави равным длительности Default Blend в CinemachineBrain (на Main Camera).")]
    [SerializeField] private float descentDuration = 2f;
    [Tooltip("Выключить фон общего плана под чёрным экраном. Оставь выключенным, если фон прятать не нужно.")]
    [SerializeField] private bool hideBackgroundAfterDescent = true;

    [Header("3. Fade Out / In")]
    [Tooltip("Опциональный звук под чёрным экраном (например, стук молотков/пиление). Можно оставить пустым.")]
    [SerializeField] private AudioSource audioUnderBlack;
    [Tooltip("Пауза под чёрным экраном (даём звуку/таймскипу прозвучать) перед осветлением.")]
    [SerializeField] private float delayUnderBlack = 1.5f;

    private Phase phase = Phase.Idle;
    private bool subscribed;

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
            StartCoroutine(BeginIntroRoutine());
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

        StartCoroutine(BeginIntroRoutine());
    }

    private IEnumerator BeginIntroRoutine()
    {
        if (phase != Phase.Idle)
        {
            yield break;
        }

        phase = Phase.IntroDialogue;

        SetPlayerBlocked(true);

        // Явно отдаём фокус общему плану ВЫШЕ геймплейной камеры — чтобы на старте
        // не было ничьей с камерой слежения за игроком.
        if (wideVcam != null)
        {
            wideVcam.Priority = widePriorityActive;
        }

        if (introBackground != null)
        {
            introBackground.SetActive(true);
        }

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        if (introDialogue == null)
        {
            Debug.LogWarning($"OneShotIntroCutscene ('{name}'): introDialogue не назначен — пропускаю вступление, сразу к спуску камеры.");
            StartCoroutine(DescendAndFinishRoutine());
            yield break;
        }

        // Дальше ход у игрока: листает реплики по Space. Продолжим в OnDialogueEnded.
        dialogueSystem.StartDialogue(introDialogue);
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
        // Ждём завершения именно вступительного диалога.
        if (phase != Phase.IntroDialogue || dialogue != introDialogue)
        {
            return;
        }

        StartCoroutine(DescendAndFinishRoutine());
    }

    private IEnumerator DescendAndFinishRoutine()
    {
        phase = Phase.Finishing;

        // Триггер одноразовый — отписываемся сразу, чтобы не ловить лишние диалоги.
        if (subscribed && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            subscribed = false;
        }

        // DialogueSystem.EndDialogue вернул управление игроку — снова блокируем,
        // пока идёт спуск камеры и затемнение.
        SetPlayerBlocked(true);

        if (delayBeforeDescent > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDescent);
        }

        // Опускаем приоритет общего плана НИЖЕ геймплейной камеры — фокус честно
        // перехватывает камера слежения за игроком, и Brain плавно сводит wide → слежение.
        // (Если экран «телепортируется» вместо плавного спуска — Default Blend в
        // CinemachineBrain стоит в Cut или 0с; поставь Ease In Out со временем.)
        if (wideVcam != null)
        {
            wideVcam.Priority = widePriorityInactive;
        }
        else
        {
            Debug.LogWarning($"OneShotIntroCutscene ('{name}'): wideVcam не назначена — спуск камеры не произойдёт.");
        }

        if (descentDuration > 0f)
        {
            yield return new WaitForSeconds(descentDuration);
        }

        // Затемнение.
        if (screenFader != null)
        {
            yield return screenFader.FadeOut();
        }

        // Фон общего плана прячем под чёрным экраном — без видимого «прыжка».
        if (hideBackgroundAfterDescent && introBackground != null)
        {
            introBackground.SetActive(false);
        }

        if (audioUnderBlack != null)
        {
            audioUnderBlack.Play();
        }

        if (delayUnderBlack > 0f)
        {
            yield return new WaitForSeconds(delayUnderBlack);
        }

        // Осветление обратно на той же берлоге.
        if (screenFader != null)
        {
            yield return screenFader.FadeIn();
        }

        phase = Phase.Done;

        // Управление — игроку.
        SetPlayerBlocked(false);
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
