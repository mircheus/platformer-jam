using UnityEngine;

/// <summary>
/// Держит подобранные предметы как дочерние объекты точки крепления внутри Player.
/// Предмет не деактивируется визуально — гасится только его физика и коллайдеры.
/// </summary>
public class PlayerItemHolder : MonoBehaviour
{
    [Tooltip("Точка внутри Player, в которую сажается подобранный предмет. " +
             "Если не задана — используется этот объект.")]
    [SerializeField] private Transform attachPoint;

    private Transform AttachPoint => attachPoint != null ? attachPoint : transform;

    private GameObject heldItem;

    public void Attach(Transform item)
    {
        if (item == null)
        {
            return;
        }

        heldItem = item.gameObject;

        // Гасим физику, иначе гравитация утащит предмет из точки крепления.
        if (item.TryGetComponent(out Rigidbody2D body))
        {
            body.simulated = false;
        }

        // Выключаем коллайдеры: предмет больше не толкается и не триггерит взаимодействия.
        // Отключение перекрывающегося коллайдера само вызовет OnTriggerExit2D у игрока,
        // так что предмет уйдёт из nearbyInteractables.
        foreach (Collider2D col in item.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        item.SetParent(AttachPoint, worldPositionStays: false);
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
    }

    /// <summary>Визуально удаляет текущий прикреплённый предмет (вызывается при ConsumeItem).</summary>
    public void ClearHeld()
    {
        if (heldItem == null)
        {
            return;
        }

        Destroy(heldItem);
        heldItem = null;
    }
}
