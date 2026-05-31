using System;
using UnityEngine;

public class LiftInteractable : IInteractable
{
    [Serializable]
    public class Direction
    {
        [Tooltip("Куда едет лифт в этом направлении. Если пусто — направление физически отсутствует.")]
        public string targetLocationId;

        [Tooltip("Опциональный квест-гейт. Если пусто — направление доступно сразу.")]
        public string unlockQuestId;

        [Tooltip("Состояние квеста, при котором направление становится рабочим.")]
        public QuestState requiredState = QuestState.Completed;
    }

    [Header("Lift")]
    [SerializeField] private LiftPanelUI liftPanel;

    [Header("Directions")]
    [SerializeField] private Direction up;
    [SerializeField] private Direction down;

    protected override void OnInteract()
    {
        liftPanel.Open(this);
    }

    public void TryGoUp() => TryGo(up);

    public void TryGoDown() => TryGo(down);

    private void TryGo(Direction direction)
    {
        if (CanGo(direction))
        {
            liftPanel.Close();
            GameContext.Instance.LocationTransition.Go(direction.targetLocationId);
        }
        else
        {
            liftPanel.ShowBrokenMessage();
        }
    }

    private bool CanGo(Direction direction)
    {
        if (string.IsNullOrEmpty(direction.targetLocationId))
            return false;

        if (string.IsNullOrEmpty(direction.unlockQuestId))
            return true;

        return GameContext.Instance.QuestSystem.GetQuestState(direction.unlockQuestId)
            == direction.requiredState;
    }
}
