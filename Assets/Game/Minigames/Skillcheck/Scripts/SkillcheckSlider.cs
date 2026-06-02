using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Skillcheck
{
    /// <summary>
    /// Двигает UI Slider от 0 к 1 с заданной скоростью и хранит целевой
    /// диапазон, в котором нажатие считается успешным.
    /// Поддерживает два режима движения, переключаемых булевым флагом:
    ///   - PingPong: дойдя до 1, слайдер идёт обратно к 0 с той же скоростью;
    ///   - Reset:    дойдя до 1, слайдер мгновенно сбрасывается в 0.
    /// </summary>
    public class SkillcheckSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;

        [Header("Movement")]
        [Tooltip("Скорости движения слайдера (единиц значения 0..1 в секунду).\n" +
                 "С каждым попаданием берётся следующее значение по порядку.\n" +
                 "После последнего значение остаётся максимальным (последним в массиве).")]
        [SerializeField] private float[] speeds = { 1f };

        [Tooltip("ON — после достижения 1 слайдер идёт обратно к 0 (ping-pong).\n" +
                 "OFF — после достижения 1 слайдер сбрасывается в 0 и стартует заново.")]
        [SerializeField] private bool pingPong = true;

        [Header("Target Zone")]
        [Tooltip("Нижняя граница целевого диапазона (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float targetMin = 0.4f;

        [Tooltip("Верхняя граница целевого диапазона (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float targetMax = 0.6f;

        // +1 — движемся к 1, -1 — движемся к 0 (используется в режиме ping-pong).
        private int _direction = 1;
        private bool _running;
        private int _speedIndex;

        public float Value => slider != null ? slider.value : 0f;
        public float TargetMin => Mathf.Min(targetMin, targetMax);
        public float TargetMax => Mathf.Max(targetMin, targetMax);
        public float CurrentSpeed => (speeds != null && speeds.Length > 0) ? speeds[_speedIndex] : 0f;

        private void Awake()
        {
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
        }

        public void StartMoving()
        {
            if (slider != null)
            {
                slider.value = 0f;
            }

            _direction = 1;
            _speedIndex = 0;
            _running = true;
        }

        public void StopMoving()
        {
            _running = false;
        }

        /// <summary>
        /// Переключает слайдер на следующую скорость из массива.
        /// На последнем элементе скорость дальше не растёт.
        /// </summary>
        public void NextSpeed()
        {
            if (speeds == null || speeds.Length == 0)
            {
                return;
            }

            _speedIndex = Mathf.Min(_speedIndex + 1, speeds.Length - 1);
        }

        private void Update()
        {
            if (!_running || slider == null)
            {
                return;
            }

            float value = slider.value + _direction * CurrentSpeed * Time.deltaTime;

            if (pingPong)
            {
                // Отражаем движение от границ 0 и 1.
                if (value >= 1f)
                {
                    value = 1f;
                    _direction = -1;
                }
                else if (value <= 0f)
                {
                    value = 0f;
                    _direction = 1;
                }
            }
            else
            {
                // Дойдя до 1 — мгновенно начинаем заново с 0.
                if (value >= 1f)
                {
                    value = 0f;
                }
            }

            slider.value = value;
        }

        /// <summary>
        /// true, если текущее значение слайдера находится в целевом диапазоне.
        /// </summary>
        public bool IsInTargetZone()
        {
            float value = Value;
            return value >= TargetMin && value <= TargetMax;
        }
    }
}
