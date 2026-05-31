using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LocationTransitionController : MonoBehaviour
{
    [Header("Locations")]
    [SerializeField] private Location[] locations;
    [SerializeField] private Location startLocation;

    [Header("References")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private PlayerMovementController playerController;

    [Header("Camera (optional)")]
    [SerializeField] private CinemachineCamera gameplayVcam;
    [SerializeField] private CinemachineConfiner2D confiner;

    public Location CurrentLocation { get; private set; }
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        foreach (Location location in locations)
        {
            if (location == null)
                continue;

            location.gameObject.SetActive(location == startLocation);
        }

        CurrentLocation = startLocation;
    }

    /// <summary>
    /// Запускает переход в локацию с указанным id. Игнорируется, если переход уже идёт
    /// или целевая локация не найдена.
    /// </summary>
    public void Go(string targetLocationId)
    {
        if (IsRunning)
        {
            Debug.LogWarning($"Location transition already running, ignoring: {targetLocationId}");
            return;
        }

        Location target = FindLocation(targetLocationId);

        if (target == null)
        {
            Debug.LogWarning($"Location not found: {targetLocationId}");
            return;
        }

        StartCoroutine(TransitionRoutine(target));
    }

    private IEnumerator TransitionRoutine(Location target)
    {
        IsRunning = true;

        // 2. Заглушить игрока.
        playerController.DisableMovement();
        playerController.DisableInteraction();

        // 3. Обнулить остаточную скорость.
        Rigidbody2D rb = GetPlayerRigidbody();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // 4. Затемнение.
        yield return screenFader.FadeOut();

        // 5. Свап локаций.
        CurrentLocation.gameObject.SetActive(false);
        target.gameObject.SetActive(true);

        // 6. Репозиция игрока (опц.).
        Vector3 playerDelta = Vector3.zero;

        if (target.PlayerSpawnPoint != null && rb != null)
        {
            Vector2 oldPosition = rb.position;
            Vector2 spawnPosition = target.PlayerSpawnPoint.position;

            rb.position = spawnPosition;
            playerDelta = spawnPosition - oldPosition;
        }

        // 7. Синхронизация физики.
        Physics2D.SyncTransforms();

        // 8. Пересбор интерактаблов под новую локацию.
        GameContext.Instance.InteractionSystem.RefreshInteractables();

        // 9. Камера.
        if (playerDelta != Vector3.zero && gameplayVcam != null)
        {
            gameplayVcam.OnTargetObjectWarped(GameContext.Instance.Player.transform, playerDelta);
        }

        if (target.CameraConfinerBounds != null && confiner != null)
        {
            confiner.BoundingShape2D = target.CameraConfinerBounds;
            confiner.InvalidateBoundingShapeCache();
        }

        // 10. Осветление.
        yield return screenFader.FadeIn();

        // 11. Вернуть управление.
        playerController.EnableMovement();
        playerController.EnableInteraction();

        // 12. Финализация.
        CurrentLocation = target;
        IsRunning = false;
    }

    private Location FindLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
            return null;

        foreach (Location location in locations)
        {
            if (location != null && location.LocationId == locationId)
                return location;
        }

        return null;
    }

    private Rigidbody2D GetPlayerRigidbody()
    {
        GameObject player = GameContext.Instance.Player;

        return player != null ? player.GetComponent<Rigidbody2D>() : null;
    }
}
