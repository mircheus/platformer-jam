using System;
using UnityEngine;

namespace Minigames
{
    /// <summary>
    /// Падающий объект мини-игры. Летит строго по вертикали из стартовой Y в конечную Y
    /// за заданное время с выбранным Ease. По достижении низа или поимке игроком
    /// поднимает событие Finished — спавнер возвращает его в пул.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Egg : MonoBehaviour
    {
        /// <summary>(egg, caught) — caught=true, если объект поймал игрок.</summary>
        public event Action<Egg, bool> Finished;

        private float startY;
        private float endY;
        private float duration;
        private EaseType ease;

        private float elapsed;
        private bool falling;

        private void Awake()
        {
            // Kinematic + триггерный коллайдер: двигаем через transform,
            // а ловец (Dynamic Rigidbody2D) надёжно генерирует OnTriggerEnter2D.
            Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            rigidBody.gravityScale = 0f;

            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Launch(Vector2 startPosition, float endY, float duration, EaseType ease)
        {
            transform.position = startPosition;

            startY = startPosition.y;
            this.endY = endY;
            this.duration = Mathf.Max(0.0001f, duration);
            this.ease = ease;

            elapsed = 0f;
            falling = true;
        }

        private void Update()
        {
            if (!falling)
            {
                return;
            }

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Easing.Evaluate(ease, t);

            Vector3 position = transform.position;
            position.y = Mathf.LerpUnclamped(startY, endY, eased); // X не трогаем — движение только вертикальное
            transform.position = position;

            if (t >= 1f)
            {
                Stop(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!falling)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerCatcher>() == null)
            {
                return;
            }

            Stop(true);
        }

        private void Stop(bool caught)
        {
            falling = false;
            Finished?.Invoke(this, caught);
        }
    }
}
