using UnityEngine;

/// <summary>
/// Надстройка над Player: переключает анимацию ходьбы/покоя.
/// В аниматоре нет параметров и переходов — только состояния Player_Idle и
/// Player_Walk, поэтому переключаем их напрямую через Animator.Play.
/// Ссылку на Animator прокидываем вручную полем в инспекторе.
/// </summary>
public class PlayerWalkAnimator : MonoBehaviour
{
    private static readonly int IdleState = Animator.StringToHash("Player_Idle");
    private static readonly int WalkState = Animator.StringToHash("Player_Walk");

    [Tooltip("Animator со спрайтом игрока (объект SpriteAnim, висит дочерним).")]
    [SerializeField] private Animator animator;

    [Tooltip("Дочерний AttachPoint — опускается вместе со спрайтом во время ходьбы.")]
    [SerializeField] private Transform attachPoint;

    [Tooltip("На сколько опустить спрайт по Y во время ходьбы (в юнитах).")]
    [SerializeField] private float walkDropDistance = 0.1f;

    private Transform spriteTransform;
    private Vector3 baseSpriteLocalPosition;
    private Vector3 baseAttachLocalPosition;

    private bool isWalking;
    private bool initialized;

    private void Awake()
    {
        if (animator != null)
        {
            // Animator висит на SpriteAnim — его же трансформ и опускаем.
            spriteTransform = animator.transform;
            baseSpriteLocalPosition = spriteTransform.localPosition;
        }

        if (attachPoint != null)
            baseAttachLocalPosition = attachPoint.localPosition;
    }

    /// <summary>
    /// Включает анимацию ходьбы или покоя. Состояние меняется только при смене
    /// флага, чтобы не перезапускать клип каждый кадр. Во время ходьбы спрайт
    /// слегка опускается, в покое возвращается на место.
    /// </summary>
    public void SetWalking(bool walking)
    {
        if (animator == null)
            return;

        if (initialized && isWalking == walking)
            return;

        initialized = true;
        isWalking = walking;

        animator.Play(walking ? WalkState : IdleState);

        Vector3 drop = walking ? new Vector3(0f, walkDropDistance, 0f) : Vector3.zero;

        spriteTransform.localPosition = baseSpriteLocalPosition - drop;

        if (attachPoint != null)
            attachPoint.localPosition = baseAttachLocalPosition - drop;
    }
}
