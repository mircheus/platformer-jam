using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string ItemID;
    public string DisplayName;

    [Header("Audio")]
    [Tooltip("Звук получения предмета. Пусто — сыграет общий звук из AudioManager (если задан вызовом с фоллбэком).")]
    public AudioClip pickupSound;
}
