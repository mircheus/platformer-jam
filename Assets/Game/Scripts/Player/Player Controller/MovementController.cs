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

    [Header("Audio")]
    [Tooltip("Зацикленный звук шагов: играет, пока игрок движется по земле.")]
    [SerializeField] private AudioClip moveLoopClip;

    [Header("Animation")]
    [SerializeField] private PlayerWalkAnimator walkAnimator;

    [Header("Performance")]
    [SerializeField] private InteractionSystemController interactionSystemController;

    [SerializeField] private bool limitFPS = true;

    private Rigidbody2D rigidBody;
    private PlayerInput playerInput;

    private PlayerMovementView view;

    private Vector2 moveInput;

    private bool canMove = true;
    private bool canInteract = true;

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
        // Пока движение заблокировано (например, на время диалога) — гасим
        // горизонтальную скорость. Иначе зажатая клавиша оставляет moveInput
        // ненулевым, и игрок продолжает ехать сквозь диалог.
        view.Move(canMove ? moveInput.x : 0f, moveSpeed);

        // Прыжка нет, поэтому опору не проверяем: идём = есть ввод по X и движение
        // разрешено. Этот же признак питает и анимацию, и звук шагов.
        bool walking = canMove && Mathf.Abs(moveInput.x) > 0.01f;

        if (walkAnimator != null)
            walkAnimator.SetWalking(walking);

        UpdateMoveLoop(walking);
    }

    /// <summary>
    /// Включает/выключает звук шагов: когда игрок реально едет (есть ввод и
    /// движение разрешено). AudioManager сам гасит лишние повторные вызовы и
    /// сглаживает старт/стоп микрофейдом.
    /// </summary>
    private void UpdateMoveLoop(bool walking)
    {
        if (moveLoopClip == null || AudioManager.Instance == null)
            return;

        if (walking)
            AudioManager.Instance.StartMoveLoop(moveLoopClip);
        else
            AudioManager.Instance.StopMoveLoop();
    }

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Interact"].started += OnInteract;
    }
    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
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

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!canInteract)
        {
            return;
        }

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

    public void EnableInteraction()
    {
        canInteract = true;
    }
    public void DisableInteraction()
    {
        canInteract = false;
    }
}