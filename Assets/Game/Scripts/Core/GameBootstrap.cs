using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("GAME BOOTSTRAP START");

        GameContext ctx = GameContext.Instance;

        ctx.SaveSystem.Load(ctx.Player, ctx.QuestSystem);

        InitializeQuests();

        Debug.Log("GAME READY");
    }

    private void InitializeQuests()
    {
        QuestSystemController questSystem = GameContext.Instance.QuestSystem;

        questSystem.ActivateQuest("1");
        questSystem.PrintActiveQuests();
    }

    private void OnApplicationQuit()
    {
        GameContext ctx = GameContext.Instance;

        if (ctx == null)
        {
            return;
        }

        ctx.SaveSystem.Save(ctx.Player, ctx.QuestSystem);
    }
}