using System;
using System.Collections;
using Minigames;
using UnityEngine;

namespace Game.Minigames.Skillcheck
{
    /// <summary>
    /// Продвигает Player по заранее расставленным Transform-точкам строго
    /// слева направо и только по горизонтали (меняется лишь X, Y и Z
    /// сохраняются). Каждый вызов <see cref="MoveToNext"/> делает один рывок
    /// к следующей точке с easing-ускорением, чтобы движение не выглядело
    /// линейным и деревянным.
    /// </summary>
    public class SkillcheckPlayerMover : MonoBehaviour
    {
        [SerializeField] private Transform player;

        [Tooltip("Точки по порядку слева направо. Player стартует в первой.")]
        [SerializeField] private Transform[] waypoints;

        [Header("Dash")]
        [Tooltip("Длительность одного рывка к следующей точке, сек.")]
        [SerializeField] private float moveDuration = 0.4f;

        [Tooltip("Кривая рывка. OutBack/OutCubic дают резкий старт с плавным гашением.")]
        [SerializeField] private EaseType ease = EaseType.OutBack;

        private int _currentIndex;
        private Coroutine _moveRoutine;

        /// <summary>Вызывается, когда Player доехал до последней точки.</summary>
        public event Action ReachedEnd;

        /// <summary>true, если ещё есть точки впереди.</summary>
        public bool HasNext => waypoints != null && _currentIndex < waypoints.Length - 1;

        /// <summary>Ставит Player в первую точку и сбрасывает прогресс.</summary>
        public void ResetToStart()
        {
            StopMoveRoutine();
            _currentIndex = 0;

            if (player != null && HasWaypoint(0))
            {
                player.position = WithWaypointX(0, player.position);
            }
        }

        /// <summary>
        /// Рывок к следующей точке. Возвращает false, если точек больше нет.
        /// </summary>
        public bool MoveToNext()
        {
            if (player == null || !HasNext)
            {
                return false;
            }

            _currentIndex++;
            StopMoveRoutine();
            _moveRoutine = StartCoroutine(MoveRoutine(_currentIndex));
            return true;
        }

        private IEnumerator MoveRoutine(int targetIndex)
        {
            float startX = player.position.x;
            float targetX = waypoints[targetIndex].position.x;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = moveDuration > 0f ? elapsed / moveDuration : 1f;
                float x = Mathf.LerpUnclamped(startX, targetX, Easing.Evaluate(ease, t));

                // Двигаем строго по горизонтали: Y и Z берём текущие.
                Vector3 p = player.position;
                player.position = new Vector3(x, p.y, p.z);

                yield return null;
            }

            // Точная фиксация в конечном X.
            player.position = WithWaypointX(targetIndex, player.position);
            _moveRoutine = null;

            if (!HasNext)
            {
                ReachedEnd?.Invoke();
            }
        }

        private void StopMoveRoutine()
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }
        }

        private bool HasWaypoint(int index)
        {
            return waypoints != null && index >= 0 && index < waypoints.Length && waypoints[index] != null;
        }

        private Vector3 WithWaypointX(int index, Vector3 source)
        {
            return new Vector3(waypoints[index].position.x, source.y, source.z);
        }
    }
}
