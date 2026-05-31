using UnityEngine;

public class Location : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string locationId;

    [Header("Transition (optional)")]
    [Tooltip("Если задан — при переходе игрок переносится сюда. Если null — игрок остаётся на месте.")]
    [SerializeField] private Transform playerSpawnPoint;

    [Tooltip("Границы Cinemachine Confiner для этой локации. Можно не заполнять, если конфайнер общий.")]
    [SerializeField] private Collider2D cameraConfinerBounds;

    public string LocationId => locationId;
    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Collider2D CameraConfinerBounds => cameraConfinerBounds;
}
