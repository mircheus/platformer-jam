using UnityEngine;
using UnityEngine.InputSystem;

namespace Minigames
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerCatcher : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;

        [Header("Movement Bounds")]
        [Tooltip("Границы движения по горизонтали (в мировых координатах X).")]
        [SerializeField] private float minX = -7f;
        [SerializeField] private float maxX = 7f;

        private Rigidbody2D rigidBody;
        private bool active;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();

            // Dynamic-тело, чтобы естественно упираться в стены-коллайдеры,
            // но без гравитации и вращения — движение только по горизонтали.
            rigidBody.bodyType = RigidbodyType2D.Dynamic;
            rigidBody.gravityScale = 0f;
            rigidBody.constraints = RigidbodyConstraints2D.FreezePositionY |
                                    RigidbodyConstraints2D.FreezeRotation;
            rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void EnableControl()
        {
            active = true;
        }

        public void DisableControl()
        {
            active = false;
            rigidBody.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!active)
            {
                return;
            }

            float dir = ReadHorizontal();

            // Гасим движение наружу за границу, чтобы игрок не уходил бесконечно
            // влево/вправо. Внутрь границ движение остаётся свободным.
            float posX = rigidBody.position.x;
            if ((posX <= minX && dir < 0f) || (posX >= maxX && dir > 0f))
            {
                dir = 0f;
            }

            rigidBody.linearVelocity = new Vector2(dir * moveSpeed, 0f);

            // Подстраховка: если тело уже вылетело за границу (толчок, спавн),
            // возвращаем его обратно в допустимый диапазон.
            if (posX < minX || posX > maxX)
            {
                rigidBody.position = new Vector2(Mathf.Clamp(posX, minX, maxX), rigidBody.position.y);
            }
        }

        // Прямой опрос A/D через Input System. Не конфликтует с PlayerMovementController:
        // во время мини-игры он уже выключен через DisableMovement() в MinigameSystemController.
        private float ReadHorizontal()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return 0f;
            }

            float dir = 0f;

            if (keyboard.aKey.isPressed)
            {
                dir -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                dir += 1f;
            }

            return dir;
        }
    }
}
