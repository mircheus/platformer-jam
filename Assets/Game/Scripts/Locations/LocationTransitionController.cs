using System;
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

    /// <summary>
    /// Поднимается в самом конце перехода, когда игрок уже на новой локации и
    /// управление возвращено. Скриптовые сцены подписываются и сверяют LocationId.
    /// Контроллер про конкретные сцены не знает — отдаёт обобщённое событие.
    /// </summary>
    public event Action<Location> ArrivedAtLocation;

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
    public void Go(string targetLocationId, LiftDeparture departure = null)
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

        StartCoroutine(TransitionRoutine(target, departure));
    }

    private IEnumerator TransitionRoutine(Location target, LiftDeparture departure)
    {
        IsRunning = true;

        // 2. Заглушить игрока.
        playerController.DisableMovement();
        playerController.DisableInteraction();

        // 3. Обнулить остаточную скорость.
        Rigidbody2D rb = GetPlayerRigidbody();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // 3.05 Парентинг игрока к кабине, чтобы он ехал вместе с анимацией.
        Transform playerTransform = GameContext.Instance.Player != null
            ? GameContext.Instance.Player.transform : null;
        Transform originalParent = null;
        bool wasSimulated = false;
        bool parented = false;

        if (departure != null && departure.rideAnchor != null && playerTransform != null)
        {
            originalParent = playerTransform.parent;

            if (rb != null)
            {
                wasSimulated = rb.simulated;
                rb.simulated = false; // заглушить физику, чтобы не бороться с движением родителя
            }

            playerTransform.SetParent(departure.rideAnchor, true);
            parented = true;
        }

        // 3.1 Отъезд: запустить анимацию кабины и выждать паузу перед затемнением.
        if (departure != null && departure.animator != null
            && !string.IsNullOrEmpty(departure.animationState))
        {
            Debug.Log($"departure.animationState {departure.animationState}");
            departure.animator.Play(departure.animationState, 0, 0f);
        }

        if (departure != null && departure.delayBeforeFade > 0f)
            yield return new WaitForSeconds(departure.delayBeforeFade);

        // 4. Затемнение.
        yield return screenFader.FadeOut();

        // 5. Свап локаций.
        CurrentLocation.gameObject.SetActive(false);
        target.gameObject.SetActive(true);

        // 6. Репозиция игрока (опц.). Пропускаем, если игрок едет на кабине —
        //    его позицию задаёт анимация прибытия, репозиция телепортнула бы его со спрайта.
        Vector3 playerDelta = Vector3.zero;

        if (!parented && target.PlayerSpawnPoint != null && rb != null)
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

        // 9.1 Прибытие: запустить анимацию прибытия кабины ещё под чёрным экраном.
        if (departure != null && departure.animator != null
            && !string.IsNullOrEmpty(departure.arrivalState))
        {
            departure.animator.Play(departure.arrivalState, 0, 0f);
        }

        // 10. Осветление (раскрывает прибывающую кабину).
        yield return screenFader.FadeIn();

        // 10.1 Дать анимации прибытия доиграть перед возвратом управления.
        if (departure != null && departure.delayAfterArrival > 0f)
            yield return new WaitForSeconds(departure.delayAfterArrival);

        // 11. Отцепить игрока от кабины (лифт живёт вне локаций, свап его не выключает,
        //     поэтому отцепляемся только здесь) и вернуть управление.
        if (parented)
        {
            playerTransform.SetParent(originalParent, true);

            if (rb != null)
            {
                rb.simulated = wasSimulated;
                rb.position = playerTransform.position; // синхронизировать тело с приехавшей позицией
            }

            Physics2D.SyncTransforms();
        }

        playerController.EnableMovement();
        playerController.EnableInteraction();

        // 12. Финализация.
        CurrentLocation = target;
        IsRunning = false;

        // 13. Сообщить наружу о прибытии (скриптовые сцены и т.п.).
        ArrivedAtLocation?.Invoke(target);
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
