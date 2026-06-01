using UnityEngine;

/// <summary>
/// Данные «отъезда» лифта, которые лифт передаёт в LocationTransitionController.
/// Контроллер не знает про конкретный клип/кабину — только проигрывает и ждёт по этим данным.
/// </summary>
public class LiftDeparture
{
    /// <summary>Animator кабины. Если null — анимация пропускается.</summary>
    public Animator animator;

    /// <summary>Имя стейта анимации отъезда для выбранного направления (напр. "Up"/"Down").</summary>
    public string animationState;

    /// <summary>Пауза между стартом анимации и началом затемнения.</summary>
    public float delayBeforeFade;

    /// <summary>Имя стейта анимации прибытия (напр. "Up_arrive"/"Down_arrive"). Если пусто — пропускается.</summary>
    public string arrivalState;

    /// <summary>Пауза после осветления, чтобы анимация прибытия доиграла, перед возвратом управления.</summary>
    public float delayAfterArrival;

    /// <summary>
    /// Transform кабины, к которому на время поездки парентится игрок, чтобы ехать вместе с анимацией.
    /// Обычно это движущаяся часть (напр. Elevator_sprite), а не корень кабины.
    /// Если null — парентинг пропускается, игрок просто стоит на месте под чёрным экраном.
    /// </summary>
    public Transform rideAnchor;
}
