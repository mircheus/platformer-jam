using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Minigames
{
    public class ObjectSpawner : MonoBehaviour
    {
        /// <summary>Поднимается, когда игрок поймал объект.</summary>
        public event Action EggCaught;

        [Header("Objects")]
        [Tooltip("Готовые объекты, заранее размещённые внутри префаба игры. " +
                 "Спавнер активирует их по очереди и деактивирует после падения/поимки.")]
        [SerializeField] private Egg[] eggs;

        [Header("Spawn Area")]
        [Tooltip("Y, на которой появляются объекты (верх поля).")]
        [SerializeField] private float spawnY = 6f;
        [Tooltip("Y, на которой объект считается упущенным (низ поля).")]
        [SerializeField] private float despawnY = -6f;
        [Tooltip("Горизонтальный разброс точек появления.")]
        [SerializeField] private float minX = -7f;
        [SerializeField] private float maxX = 7f;

        [Header("Timing")]
        [SerializeField] private float spawnInterval = 1f;
        [Tooltip("Случайное время падения (мин..макс) в секундах.")]
        [SerializeField] private Vector2 fallDurationRange = new Vector2(1.5f, 3f);

        [Header("Ease")]
        [SerializeField] private EaseType ease = EaseType.Linear;

        // Деактивированные объекты, готовые к запуску.
        private readonly Queue<Egg> _available = new Queue<Egg>();
        private Coroutine _spawnRoutine;

        private void Awake()
        {
            foreach (Egg egg in eggs)
            {
                if (egg == null)
                {
                    continue;
                }

                egg.Finished += OnEggFinished; // подписка один раз на старте, не на каждую активацию
                egg.gameObject.SetActive(false);
                _available.Enqueue(egg);
            }
        }

        private void OnDestroy()
        {
            foreach (Egg egg in eggs)
            {
                if (egg != null)
                {
                    egg.Finished -= OnEggFinished;
                }
            }
        }

        public void StartSpawning()
        {
            if (_spawnRoutine == null)
            {
                _spawnRoutine = StartCoroutine(SpawnLoop());
            }
        }

        public void StopSpawning()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        private IEnumerator SpawnLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(spawnInterval);

            while (true)
            {
                SpawnOne();

                yield return wait;
            }
        }

        private void SpawnOne()
        {
            // Все объекты сейчас в падении — пропускаем тик, дождёмся освобождения.
            if (_available.Count == 0)
            {
                return;
            }

            Egg egg = _available.Dequeue();
            egg.gameObject.SetActive(true);

            float x = Random.Range(minX, maxX);
            float duration = Random.Range(fallDurationRange.x, fallDurationRange.y);

            egg.Launch(new Vector2(x, spawnY), despawnY, duration, ease);
        }

        private void OnEggFinished(Egg egg, bool caught)
        {
            egg.gameObject.SetActive(false);
            _available.Enqueue(egg);

            if (caught)
            {
                EggCaught?.Invoke();
            }
        }
    }
}
