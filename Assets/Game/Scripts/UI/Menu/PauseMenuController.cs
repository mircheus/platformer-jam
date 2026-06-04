using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Меню паузы в игровой сцене. Кнопка настроек открывает панель и ставит всю игру
/// на паузу (Time.timeScale = 0). Внутри панели две кнопки:
///  • «Сохранить» — фиксирует настройки в PlayerPrefs и снимает паузу (закрывает панель);
///  • «Выйти»     — возвращает в главное меню.
///
/// Слайдеры громкости применяются вживую через AudioVolumeController, поэтому
/// «Сохранить» лишь сбрасывает значения на диск.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Volume (optional)")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioVolumeController volumeController;

    [Header("Buttons")]
    [Tooltip("Кнопка в HUD, открывающая меню паузы.")]
    [SerializeField] private Button settingsButton;
    [Tooltip("Сохранить настройки и закрыть меню (снять паузу).")]
    [SerializeField] private Button saveButton;
    [Tooltip("Выйти в главное меню.")]
    [SerializeField] private Button exitButton;

    [Header("Scene")]
    [Tooltip("Имя сцены главного меню (как в файле .unity и в Build Settings).")]
    [SerializeField] private string mainMenuSceneName = "_Menu";

    [Header("Transition (optional)")]
    [SerializeField] private ScreenFader screenFader;

    private bool isExiting;

    private void Awake()
    {
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveAndClose);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMainMenu);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // Стартуем с закрытой панелью на случай, если её забыли выключить в сцене.
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>Открыть панель настроек и поставить игру на паузу.</summary>
    public void OpenSettings()
    {
        // Заполняем слайдеры текущими значениями прямо при открытии: панель обычно
        // выключена, и инициализировать её в Start было бы поздно/бессмысленно.
        // SetValueWithoutNotify — чтобы синхронизация не вызвала onValueChanged
        // и не перезаписала только что прочитанное значение.
        if (volumeController != null)
        {
            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(volumeController.MusicVolume);

            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(volumeController.SFXVolume);
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OnMusicChanged(float value)
    {
        if (volumeController != null)
            volumeController.SetMusicVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        if (volumeController != null)
            volumeController.SetSFXVolume(value);
    }

    /// <summary>Сохранить настройки и закрыть панель (снять паузу).</summary>
    public void SaveAndClose()
    {
        // Громкость уже применена слайдерами вживую — здесь просто фиксируем на диск.
        PlayerPrefs.Save();
        CloseSettings();
    }

    private void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ExitToMainMenu()
    {
        if (isExiting)
            return;

        isExiting = true;

        // Снимаем паузу ДО загрузки: Time.timeScale переносится между сценами, и без
        // сброса главное меню (а заодно и фейд ниже) останется замороженным.
        Time.timeScale = 1f;

        StartCoroutine(ExitRoutine());
    }

    private IEnumerator ExitRoutine()
    {
        if (screenFader != null)
            yield return screenFader.FadeOut();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Страховка: если объект уничтожат/выключат, пока стоит пауза, не оставляем
    // timeScale в нуле — иначе следующая сцена окажется замороженной.
    private void OnDisable()
    {
        if (!isExiting)
            Time.timeScale = 1f;
    }
}
