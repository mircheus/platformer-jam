using UnityEngine;

/// <summary>
/// Утилита-действие для вызова из кода или UnityEvent (по аналогии с
/// <see cref="NpcAdvanceAndDisableAction"/>). Один публичный метод
/// <see cref="Execute"/> выключает все объекты из <see cref="objectsToDisable"/>
/// (SetActive(false)) и включает все объекты из <see cref="objectsToEnable"/>
/// (SetActive(true)).
///
/// Пустые элементы массивов пропускаются, так что дырки в инспекторе безопасны.
/// </summary>
public class ObjectsToggleAction : MonoBehaviour
{
    [Header("Disable Objects")]
    [Tooltip("Объекты, которые выключаются (SetActive(false)).")]
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Enable Objects")]
    [Tooltip("Объекты, которые включаются (SetActive(true)).")]
    [SerializeField] private GameObject[] objectsToEnable;

    /// <summary>
    /// Выключает все <see cref="objectsToDisable"/> и включает все
    /// <see cref="objectsToEnable"/>. Незаполненные элементы пропускаются.
    /// </summary>
    public void Execute()
    {
        SetActiveAll(objectsToDisable, false);
        SetActiveAll(objectsToEnable, true);
    }

    private void SetActiveAll(GameObject[] objects, bool active)
    {
        if (objects == null)
        {
            return;
        }

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
}
