using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Загрузка игровой сцены из меню. Имя сцены вынесено в инспектор —
/// поменяй gameSceneName, когда определишься, какая сцена является «игрой».
/// Сцена должна быть добавлена в Build Settings, иначе загрузка молча провалится.
/// При наличии ScreenFader делает плавное затемнение перед загрузкой.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("Target scene")]
    [Tooltip("Имя игровой сцены (как в файле .unity и в Build Settings).")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Transition (optional)")]
    [SerializeField] private ScreenFader screenFader;

    private bool isLoading;

    public void LoadGame()
    {
        if (isLoading)
            return;

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("SceneLoader: gameSceneName не задан.");
            return;
        }

        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        isLoading = true;

        if (screenFader != null)
            yield return screenFader.FadeOut();

        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);

        if (op == null)
        {
            Debug.LogError($"SceneLoader: не удалось загрузить сцену '{gameSceneName}'. " +
                           "Проверь, что она добавлена в Build Settings.");
            isLoading = false;
            yield break;
        }

        while (!op.isDone)
            yield return null;
    }
}
