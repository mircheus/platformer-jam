using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private SaveSystemController saveSystem;
    [SerializeField] private QuestSystemController questSystem;
    [SerializeField] private GameObject player;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("GAME BOOTSTRAP START");

        saveSystem.Load(player, questSystem);

        InitializeQuests();

        Debug.Log("GAME READY");
    }

    private void InitializeQuests()
    {
        questSystem.ActivateQuest("1");
        questSystem.PrintActiveQuests();
    }

    private void OnApplicationQuit()
    {
        saveSystem.Save(player, questSystem);
    }
}