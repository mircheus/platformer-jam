using UnityEngine;

public class GameContext : MonoBehaviour
{
    public static GameContext Instance { get; private set; }

    [Header("Shared Systems")]
    [SerializeField] private SaveSystemController saveSystem;
    [SerializeField] private QuestSystemController questSystem;
    [SerializeField] private DialogueSelector dialogueSelector;
    [SerializeField] private PlayerInventoryController playerInventory;

    [Header("Player")]
    [SerializeField] private GameObject player;

    public SaveSystemController SaveSystem => saveSystem;
    public QuestSystemController QuestSystem => questSystem;
    public DialogueSelector DialogueSelector => dialogueSelector;
    public PlayerInventoryController PlayerInventory => playerInventory;
    public GameObject Player => player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
