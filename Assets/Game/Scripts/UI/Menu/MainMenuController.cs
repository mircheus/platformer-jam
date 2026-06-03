using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Главное меню: три кнопки (Играть / Настройки / Выйти) и переключение
/// между панелью меню и панелью настроек. Загрузку сцены делегирует SceneLoader.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("References")]
    [SerializeField] private SceneLoader sceneLoader;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnPlayClicked()
    {
        if (sceneLoader != null)
            sceneLoader.LoadGame();
        else
            Debug.LogError("MainMenuController: не назначен SceneLoader.");
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
