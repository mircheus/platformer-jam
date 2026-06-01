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

        [Tooltip("Имя стейта анимации отъезда для этого направления (напр. \"Up\"/\"Down\").")]
        public string animationState;

        [Tooltip("Имя стейта анимации прибытия для этого направления (напр. \"Up_arrive\"/\"Down_arrive\").")]
        public string arrivalState;
    }

    [Header("Lift")]
    [SerializeField] private LiftPanelUI liftPanel;

    [Header("Cabin animation")]
    [SerializeField] private Animator cabinAnimator;
    [Tooltip("Пауза между стартом анимации кабины и началом затемнения.")]
    [SerializeField] private float delayBeforeFade = 0.3f;
    [Tooltip("Пауза после осветления, чтобы анимация прибытия доиграла, перед возвратом управления.")]
    [SerializeField] private float delayAfterArrival = 0.3f;

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

            LiftDeparture departure = new LiftDeparture
            {
                animator = cabinAnimator,
                animationState = direction.animationState,
                delayBeforeFade = delayBeforeFade,
                arrivalState = direction.arrivalState,
                delayAfterArrival = delayAfterArrival
            };
            
            GameContext.Instance.LocationTransition.Go(direction.targetLocationId, departure);
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
