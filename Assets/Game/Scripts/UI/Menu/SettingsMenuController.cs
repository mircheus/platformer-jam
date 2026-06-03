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
        if (volumeController != null)
        {
            // SetValueWithoutNotify — чтобы инициализация слайдера не вызвала
            // onValueChanged и не перезаписала только что прочитанное значение.
            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(volumeController.MusicVolume);

            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(volumeController.SFXVolume);
        }

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
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
