using System;
using System.Collections.Generic;
using UnityEngine;

public class LiftInteractable : IInteractable
{
    [Serializable]
    public class Floor
    {
        [Tooltip("Локация этого этажа. Лифт считает себя на этом этаже, когда активна эта локация.")]
        public string locationId;

        [Tooltip("Опциональный квест-гейт на подъём ВВЕРХ с этого этажа. Если пусто — вверх можно сразу.")]
        public string unlockQuestId;

        [Tooltip("Состояние квеста, при котором подъём вверх с этого этажа становится доступен.")]
        public QuestState requiredState = QuestState.Completed;
    }

    private enum MoveDirection
    {
        Up,
        Down
    }

    [Header("Lift")]
    [SerializeField] private LiftPanelUI liftPanel;

    [Tooltip("Этажи снизу вверх: индекс 0 — самый нижний. Вверх — следующий индекс, вниз — предыдущий.")]
    [SerializeField] private List<Floor> floors = new List<Floor>();

    [Header("Cabin animation")]
    [SerializeField] private Animator cabinAnimator;
    [Tooltip("Transform кабины, к которому парентится игрок на время поездки (движущаяся часть, напр. Elevator_sprite).")]
    [SerializeField] private Transform rideAnchor;
    [Tooltip("Пауза между стартом анимации кабины и началом затемнения.")]
    [SerializeField] private float delayBeforeFade = 0.3f;
    [Tooltip("Пауза после осветления, чтобы анимация прибытия доиграла, перед возвратом управления.")]
    [SerializeField] private float delayAfterArrival = 0.3f;

    [Header("Animation states (общие для всех этажей)")]
    [Tooltip("Стейт анимации отъезда вверх.")]
    [SerializeField] private string upAnimationState = "Up";
    [Tooltip("Стейт анимации прибытия после подъёма.")]
    [SerializeField] private string upArrivalState = "Up_arrive";
    [Tooltip("Стейт анимации отъезда вниз.")]
    [SerializeField] private string downAnimationState = "Down";
    [Tooltip("Стейт анимации прибытия после спуска.")]
    [SerializeField] private string downArrivalState = "Down_arrive";

    protected override void OnInteract()
    {
        liftPanel.Open(this);
    }

    public void TryGoUp() => TryGo(MoveDirection.Up);

    public void TryGoDown() => TryGo(MoveDirection.Down);

    private void TryGo(MoveDirection direction)
    {
        int current = GetCurrentFloorIndex();
        int targetIndex = direction == MoveDirection.Up ? current + 1 : current - 1;

        if (!CanGo(current, targetIndex, direction))
        {
            liftPanel.ShowBrokenMessage();
            return;
        }

        liftPanel.Close();

        bool goingUp = direction == MoveDirection.Up;

        LiftDeparture departure = new LiftDeparture
        {
            animator = cabinAnimator,
            animationState = goingUp ? upAnimationState : downAnimationState,
            arrivalState = goingUp ? upArrivalState : downArrivalState,
            delayBeforeFade = delayBeforeFade,
            delayAfterArrival = delayAfterArrival,
            rideAnchor = rideAnchor
        };

        GameContext.Instance.LocationTransition.Go(floors[targetIndex].locationId, departure);
    }

    private bool CanGo(int current, int targetIndex, MoveDirection direction)
    {
        // Текущий этаж не распознан или целевого этажа физически нет.
        if (current < 0 || targetIndex < 0 || targetIndex >= floors.Count)
            return false;

        // Вниз всегда доступно, если этаж ниже существует.
        if (direction == MoveDirection.Down)
            return true;

        // Вверх: гейт хранится на этаже, с которого поднимаемся.
        Floor from = floors[current];

        if (string.IsNullOrEmpty(from.unlockQuestId))
            return true;

        return GameContext.Instance.QuestSystem.GetQuestState(from.unlockQuestId)
            == from.requiredState;
    }

    /// <summary>
    /// Текущий этаж = индекс этажа, чья локация сейчас активна.
    /// Единый источник правды — LocationTransitionController.CurrentLocation.
    /// </summary>
    private int GetCurrentFloorIndex()
    {
        Location currentLocation = GameContext.Instance.LocationTransition.CurrentLocation;

        if (currentLocation == null)
            return -1;

        for (int i = 0; i < floors.Count; i++)
        {
            if (floors[i].locationId == currentLocation.LocationId)
                return i;
        }

        return -1;
    }
}
