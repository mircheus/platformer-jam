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

    [Tooltip("Опциональная зона: пока в ней находится NPC, лифт заблокирован для взаимодействия.")]
    [SerializeField] private NpcPresenceZone npcBlockZone;

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

    public override bool CanInteract()
    {
        // Пока в зоне стоит NPC — лифт недоступен.
        if (npcBlockZone != null && npcBlockZone.IsOccupied)
            return false;

        return base.CanInteract();
    }

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

        GameContext.Instance.LocationTransition.Go(floors[targetIndex].locationId, BuildDeparture(direction));
    }

    /// <summary>
    /// Принудительно отправляет лифт на нижний этаж (индекс 0) с анимацией спуска.
    /// Тонкая обёртка над <see cref="GoToFloor"/>.
    /// </summary>
    public void GoToGroundFloor() => GoToFloor(0, goingUp: false);

    /// <summary>
    /// Принудительно отправляет лифт на этаж <paramref name="targetIndex"/> с анимацией
    /// в заданном направлении, минуя панель, промежуточные этажи и квест-гейты. Нужно
    /// скриптовым надстройкам (напр. <see cref="ScriptedLiftInteractable"/> /
    /// <see cref="ScriptedLiftAscentInteractable"/>), чтобы переиспользовать единый
    /// конфиг кабины и список этажей вместо дублирования.
    /// </summary>
    public void GoToFloor(int targetIndex, bool goingUp)
    {
        if (targetIndex < 0 || targetIndex >= floors.Count)
        {
            Debug.LogWarning($"{name}: нет этажа с индексом {targetIndex} — лифту некуда ехать.");
            return;
        }

        MoveDirection direction = goingUp ? MoveDirection.Up : MoveDirection.Down;
        GameContext.Instance.LocationTransition.Go(floors[targetIndex].locationId, BuildDeparture(direction));
    }

    private LiftDeparture BuildDeparture(MoveDirection direction)
    {
        bool goingUp = direction == MoveDirection.Up;

        return new LiftDeparture
        {
            animator = cabinAnimator,
            animationState = goingUp ? upAnimationState : downAnimationState,
            arrivalState = goingUp ? upArrivalState : downArrivalState,
            delayBeforeFade = delayBeforeFade,
            delayAfterArrival = delayAfterArrival,
            rideAnchor = rideAnchor
        };
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
