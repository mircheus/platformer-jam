using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundTrigger;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Performance")]
    [SerializeField] private InteractionSystemController interactionSystemController;

    [SerializeField] private bool limitFPS = true;

    private Rigidbody2D rigidBody;
    private PlayerInput playerInput;

    private PlayerMovementModel model;
    private PlayerMovementView view;

    private Vector2 moveInput;

    private bool canMove = true;
    private bool isGrounded;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        model = new PlayerMovementModel(moveSpeed, jumpForce);
        view = new PlayerMovementView(rigidBody);
    }

    private void Start()
    {
        SetupFPS();
    }

    private void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        view.Move(moveInput.x, moveSpeed);
    } 

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;

        playerInput.actions["Jump"].performed += OnJump;

        playerInput.actions["Interact"].started += OnInteract;
    }
    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;

        playerInput.actions["Jump"].performed -= OnJump;

        playerInput.actions["Interact"].performed -= OnInteract;
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

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!isGrounded)
        {
            return;
        }

        view.Jump(jumpForce);
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

    private void CheckGround()
    {
        isGrounded = view.CheckGround(groundTrigger, groundLayer, groundCheckRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundTrigger == null)
        {
            return;
        }

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(groundTrigger.position, groundCheckRadius);
    }

    public void EnableMovement()
    {
        canMove = true;
    }
    public void DisableMovement()
    {
        canMove = false;
    }
}