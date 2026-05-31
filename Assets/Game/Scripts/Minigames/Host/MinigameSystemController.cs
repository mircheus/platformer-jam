using System;
using Minigames;
using Minigames.Contract;
using Unity.Cinemachine;
using UnityEngine;

public class MinigameSystemController : MonoBehaviour
{
    [Header("Render Roots")]
    [SerializeField] private Transform uiRoot;
    [SerializeField] private Transform worldRoot;

    [Header("Player")]
    [SerializeField] private PlayerMovementController playerController;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera minigameVcam;     // отдельный vcam, наведён на worldRoot в инспекторе
    [SerializeField] private int minigameCameraPriority = 100;   // выше геймплейного
    [SerializeField] private int idleCameraPriority = 0;         // ниже геймплейного
    
    private MinigameBase currentInstance;
    private MinigameData currentData;
    private Action onCompleteCallback;

    public bool IsRunning => currentInstance != null;

    /// <summary>
    /// Запускает мини-игру. Возвращает true, если запуск реально начался.
    /// onComplete (опц.) вызывается после завершения и применения эффектов —
    /// напр. чтобы продолжить диалог, поставленный на паузу.
    /// </summary>
    public bool Launch(MinigameData data, Action onComplete = null)
    {
        if (data == null)
        {
            Debug.LogWarning("Launch failed: MinigameData is null");
            return false;
        }

        if (IsRunning)
        {
            Debug.Log($"Minigame already running, ignoring launch: {data.id}");
            return false;
        }

        if (data.prefab == null)
        {
            Debug.LogWarning($"Launch failed: prefab not set on MinigameData '{data.id}'");
            return false;
        }

        currentData = data;
        onCompleteCallback = onComplete;

        Transform root = data.renderType == MinigameRenderType.UI ? uiRoot : worldRoot;

        currentInstance = Instantiate(data.prefab, root);
        currentInstance.Completed += OnMinigameCompleted;

        playerController.DisableMovement();
        playerController.DisableInteraction();

        MinigameContext ctx = new MinigameContext
        {
            SourceObjectID = data.id,
            RenderRoot = root
        };

        currentInstance.Begin(ctx);
        
        if (data.renderType == MinigameRenderType.World)
            FocusCamera();

        Debug.Log($"Minigame launched: {data.displayName} ({data.id})");

        return true;
    }

    private void OnMinigameCompleted(MinigameResult result)
    {
        currentInstance.Completed -= OnMinigameCompleted;

        Destroy(currentInstance.gameObject);
        currentInstance = null;

        if (currentData.renderType == MinigameRenderType.World)
            ReleaseCamera();
        
        playerController.EnableMovement();
        playerController.EnableInteraction();

        ApplySuccess(currentData);

        Debug.Log($"Minigame completed: {currentData.displayName}");

        Action callback = onCompleteCallback;
        onCompleteCallback = null;
        currentData = null;

        // В самом конце — чтобы коллбэк (напр. продолжение диалога) выполнялся
        // уже после полного сброса состояния контроллера.
        callback?.Invoke();
    }

    private void ApplySuccess(MinigameData data)
    {
        if (!string.IsNullOrEmpty(data.questIdToComplete))
        {
            GameContext.Instance.QuestSystem.CompleteQuest(data.questIdToComplete);
        }

        if (!string.IsNullOrEmpty(data.targetNpcId))
        {
            NPCInteractable npc = FindNpcById(data.targetNpcId);

            if (npc != null)
            {
                npc.AdvanceProgress();
            }
            else
            {
                Debug.LogWarning($"NPC not found for AdvanceProgress: {data.targetNpcId}");
            }
        }
    }

    private NPCInteractable FindNpcById(string npcId)
    {
        foreach (var npc in FindObjectsByType<NPCInteractable>(FindObjectsSortMode.None))
        {
            if (npc.ObjectID == npcId)
            {
                return npc;
            }
        }

        return null;
    }
    
    private void FocusCamera()
    {
        minigameVcam.Priority = minigameCameraPriority;
    }

    private void ReleaseCamera()
    {
        minigameVcam.Priority = idleCameraPriority;
    }
}
