using TMPro;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Prewarm")]
    [Tooltip("Префаб pop-up подсказки (InteractionPrompt). Прогревается на старте, чтобы " +
             "первый показ не фризил: первая активация TextMeshPro компилирует SDF-шейдер " +
             "и генерирует атлас шрифта.")]
    [SerializeField] private GameObject interactionPromptPrefab;

    private void Start()
    {
        InitializeGame();
        PrewarmInteractionPrompt();
    }

    /// <summary>
    /// Убирает разовый фриз при первом показе pop-up. Первая за сессию активация
    /// TextMeshPro компилирует SDF-шейдер и заливает атлас шрифта в видеопамять —
    /// делаем это здесь, на загрузке, а не при первой встрече игрока с NPC.
    /// </summary>
    private void PrewarmInteractionPrompt()
    {
        if (interactionPromptPrefab == null)
            return;

        // Создаём подсказку далеко за экраном, форсим генерацию меша и атласа шрифта,
        // затем сразу удаляем — игрок ничего не увидит.
        GameObject warmup = Instantiate(
            interactionPromptPrefab,
            new Vector3(10000f, 10000f, 0f),
            Quaternion.identity
            );

        foreach (TMP_Text text in warmup.GetComponentsInChildren<TMP_Text>(true))
            text.ForceMeshUpdate();

        Destroy(warmup);
    }

    private void InitializeGame()
    {
        Debug.Log("GAME BOOTSTRAP START");

        GameContext ctx = GameContext.Instance;

        // ctx.SaveSystem.Load(ctx.Player, ctx.QuestSystem);

        InitializeQuests();

        Debug.Log("GAME READY");
    }

    private void InitializeQuests()
    {
        QuestSystemController questSystem = GameContext.Instance.QuestSystem;

        questSystem.ActivateQuest("Pigeon");
        questSystem.ActivateQuest("Krot_1");
        questSystem.ActivateQuest("Ending");
        questSystem.PrintActiveQuests();
    }

    private void OnApplicationQuit()
    {
        GameContext ctx = GameContext.Instance;

        if (ctx == null)
        {
            return;
        }
    }
}