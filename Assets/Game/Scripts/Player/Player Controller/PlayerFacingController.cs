using UnityEngine;

/// <summary>
/// Надстройка над Player: разворачивает спрайт по направлению движения и
/// синхронно переставляет Interactable Trigger в нужную сторону, чтобы зона
/// взаимодействия всегда была перед игроком. Ссылки прокидываем полями в
/// инспекторе. При нулевом вводе направление сохраняется (не сбрасывается).
/// </summary>
public class PlayerFacingController : MonoBehaviour
{
    [Tooltip("SpriteRenderer игрока (объект SpriteAnim).")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Трансформ объекта Interactable Trigger.")]
    [SerializeField] private Transform interactableTrigger;

    [Tooltip("Включи, если по умолчанию (flipX=false) спрайт смотрит вправо. " +
             "Сними галочку, если разворот получается зеркальным.")]
    [SerializeField] private bool spriteFacesRight = true;

    private bool facingRight = true;

    /// <summary>
    /// Обновляет направление по знаку горизонтального ввода. Нулевой ввод
    /// (стоим на месте) направление не меняет.
    /// </summary>
    public void SetDirection(float moveX)
    {
        if (Mathf.Abs(moveX) < 0.01f)
            return;

        bool movingRight = moveX > 0f;
        if (movingRight == facingRight)
            return;

        facingRight = movingRight;

        if (spriteRenderer != null)
            spriteRenderer.flipX = facingRight ? !spriteFacesRight : spriteFacesRight;

        if (interactableTrigger != null)
        {
            Vector3 local = interactableTrigger.localPosition;
            local.x = facingRight ? 1f : -1f;
            interactableTrigger.localPosition = local;
        }
    }
}
