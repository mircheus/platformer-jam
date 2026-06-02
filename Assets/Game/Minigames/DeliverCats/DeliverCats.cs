using System.Collections;
using Game.Minigames.DeliverCats;
using Minigames;
using Minigames.Contract;
using UnityEngine;
using UnityEngine.UI;

public class DeliverCats : MinigameBase
{
    [Header("Кнопки (индексы 0 и 1)")]
    [SerializeField] private DeliverCatsButton[] buttons;

    [Header("Подсказки (индексы соответствуют кнопкам: 0 и 1)")]
    [SerializeField] private Image[] hints;

    [Tooltip("Правильная последовательность нажатий: каждое число — индекс кнопки.")]
    [SerializeField] private int[] correctSequence = { 0, 1, 0, 1 };

    [Header("Окно ошибки (можно оставить пустым — будет только Debug.Log)")]
    [SerializeField] private GameObject errorWindow;

    [Header("Тайминги")]
    [Tooltip("Пауза между шагами: подсказка скрыта и ввод заблокирован, сек.")]
    [SerializeField] private float pauseBetweenSteps = 0.5f;

    [Tooltip("Сколько секунд показывать окно ошибки, сек.")]
    [SerializeField] private float errorWindowDuration = 1f;

    [Tooltip("Пауза перед завершением игры после последнего шага, сек.")]
    [SerializeField] private float winDelay = 0.6f;

    private int _currentIndex;
    private bool _acceptingInput;
    private Coroutine _errorCoroutine;

    public override void Begin(MinigameContext ctx)
    {
        Debug.Log($"DeliverCats started | source: {ctx.SourceObjectID}");

        _currentIndex = 0;
        _acceptingInput = false;

        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            button.Init(i);
            button.ButtonPressedEvent += OnButtonPressed;
        }

        HideAllHints();
        HideErrorWindow();

        ShowCurrentHint();
    }

    private void OnDestroy()
    {
        // Снимаем подписки, чтобы не ловить нажатия после завершения игры.
        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.ButtonPressedEvent -= OnButtonPressed;
            }
        }
    }

    private void OnButtonPressed(int buttonIndex)
    {
        // Пока подсказка не показана (пауза/ошибка/конец) — нажатия игнорируем.
        if (!_acceptingInput)
        {
            return;
        }

        if (buttonIndex == correctSequence[_currentIndex])
        {
            // Верное нажатие: гасим ввод и текущую подсказку.
            _acceptingInput = false;
            HideAllHints();

            if (_currentIndex >= correctSequence.Length - 1)
            {
                Win();
                return;
            }

            _currentIndex++;
            StartCoroutine(ShowNextHintAfterPause());
        }
        else
        {
            // Неверное нажатие: предупреждаем и ждём правильную кнопку,
            // ничего больше не меняем (шаг и подсказка остаются прежними).
            Debug.Log($"DeliverCats: wrong button {buttonIndex}, " +
                      $"expected {correctSequence[_currentIndex]}");
            ShowError();
        }
    }

    private IEnumerator ShowNextHintAfterPause()
    {
        if (pauseBetweenSteps > 0f)
        {
            yield return new WaitForSeconds(pauseBetweenSteps);
        }

        ShowCurrentHint();
    }

    private void ShowCurrentHint()
    {
        HideAllHints();

        var hintIndex = correctSequence[_currentIndex];
        if (hintIndex >= 0 && hintIndex < hints.Length && hints[hintIndex] != null)
        {
            hints[hintIndex].gameObject.SetActive(true);
        }

        _acceptingInput = true;
    }

    private void HideAllHints()
    {
        foreach (var hint in hints)
        {
            if (hint != null)
            {
                hint.gameObject.SetActive(false);
            }
        }
    }

    private void ShowError()
    {
        if (errorWindow == null)
        {
            return;
        }

        errorWindow.SetActive(true);

        if (_errorCoroutine != null)
        {
            StopCoroutine(_errorCoroutine);
        }

        _errorCoroutine = StartCoroutine(HideErrorWindowAfterDelay());
    }

    private IEnumerator HideErrorWindowAfterDelay()
    {
        if (errorWindowDuration > 0f)
        {
            yield return new WaitForSeconds(errorWindowDuration);
        }

        HideErrorWindow();
    }

    private void HideErrorWindow()
    {
        if (errorWindow != null)
        {
            errorWindow.SetActive(false);
        }
    }

    public void Win()
    {
        _acceptingInput = false;
        HideAllHints();
        HideErrorWindow();

        StartCoroutine(CompleteAfterDelay());
    }

    private IEnumerator CompleteAfterDelay()
    {
        if (winDelay > 0f)
        {
            yield return new WaitForSeconds(winDelay);
        }

        Complete(new MinigameResult());
    }
}
