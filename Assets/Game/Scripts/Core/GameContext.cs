using UnityEngine;

public class GameContext : MonoBehaviour
{
    public static GameContext Instance { get; private set; }

    [Header("Shared Systems")]
    [SerializeField] private QuestSystemController questSystem;
    [SerializeField] private DialogueSelector dialogueSelector;
    [SerializeField] private PlayerInventoryController playerInventory;
    [SerializeField] private MinigameSystemController minigameSystem;
    [SerializeField] private LocationTransitionController locationTransition;
    [SerializeField] private InteractionSystemController interactionSystem;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerItemHolder itemHolder;
    
    public QuestSystemController QuestSystem => questSystem;
    public DialogueSelector DialogueSelector => dialogueSelector;
    public PlayerInventoryController PlayerInventory => playerInventory;
    public MinigameSystemController MinigameSystem => minigameSystem;
    public LocationTransitionController LocationTransition => locationTransition;
    public InteractionSystemController InteractionSystem => interactionSystem;
    public GameObject Player => player;
    public PlayerItemHolder ItemHolder => itemHolder;

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
