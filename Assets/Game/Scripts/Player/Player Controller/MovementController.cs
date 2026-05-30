using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Performance")]
    [SerializeField] private InteractionSystemController interactionSystemController;

    [SerializeField] private bool limitFPS = true;

    private Rigidbody2D rigidBody;
    private PlayerInput playerInput;
    
    private PlayerMovementView view;

    private Vector2 moveInput;

    private bool canMove = true;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        
        view = new PlayerMovementView(rigidBody);
    }

    private void Start()
    {
        SetupFPS();
    }

    private void FixedUpdate()
    {
        view.Move(moveInput.x, moveSpeed);
    } 

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Interact"].performed += OnInteract;
    }
    
    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Interact"].performed -= OnInteract;
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;

            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        interactionSystemController.InteractableObjectSearch(gameObject.transform);
    }

    private void SetupFPS()
    {
        if (limitFPS)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }
        else
        {
            Application.targetFrameRate = -1;
        }
    }
}