using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Экран настроек: слайдеры громкости музыки и эффектов + кнопка «Назад».
/// Слайдеры инициализируются текущими (сохранёнными) значениями и при изменении
/// сразу применяют/сохраняют громкость через AudioVolumeController.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("References")]
    [SerializeField] private AudioVolumeController volumeController;
    [SerializeField] private MainMenuController mainMenu;

    private void Awake()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    // Инициализацию слайдеров делаем в Start, а не в Awake: значения громкости
    // заполняются в Awake самого AudioVolumeController, а порядок Awake между
    // объектами не гарантирован. Все Awake завершаются до любого Start, поэтому
    // здесь volumeController уже прочитал сохранённые значения.
    private void Start()
    {
        if (volumeController == null)
            return;

        // SetValueWithoutNotify — чтобы инициализация слайдера не вызвала
        // onValueChanged и не перезаписала только что прочитанное значение.
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(volumeController.MusicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(volumeController.SFXVolume);
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

    private void OnBackClicked()
    {
        if (mainMenu != null)
            mainMenu.ShowMainMenu();
    }
}
