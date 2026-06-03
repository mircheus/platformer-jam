using System.Collections;
using Minigames;
using Minigames.Contract;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Minigames.Skillcheck
{
    public class Skillcheck : MinigameBase
    {
        [SerializeField] private SkillcheckSlider skillcheckSlider;
        [SerializeField] private SkillcheckPlayerMover playerMover;

        [Header("Win Condition")]
        [Tooltip("Сколько успешных нажатий нужно для победы.")]
        [SerializeField] private int requiredHits = 3;

        [Tooltip("Пауза перед завершением игры, чтобы Player успел доехать " +
                 "и переход не был резким, сек.")]
        [SerializeField] private float winDelay = 0.6f;

        private bool _active;
        private int _hits;

        protected override void OnBegin(MinigameContext ctx)
        {
            Debug.Log($"Skillcheck started | source: {ctx.SourceObjectID}");

            _active = true;
            _hits = 0;

            if (playerMover != null)
            {
                playerMover.ResetToStart();
            }

            skillcheckSlider.StartMoving();
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                TryHit();
            }
        }

        private void TryHit()
        {
            if (skillcheckSlider.IsInTargetZone())
            {
                _hits++;

                // Звук на каждое успешное попадание (в т.ч. последнее перед победой).
                PlaySuccessSound();

                Debug.Log($"Skillcheck: HIT! {_hits}/{requiredHits} | value = {skillcheckSlider.Value:F2}");

                // Player делает плавный рывок к следующей точке.
                if (playerMover != null)
                {
                    playerMover.MoveToNext();
                }

                if (_hits >= requiredHits)
                {
                    Win();
                    return;
                }

                // Каждое попадание ускоряет слайдер для следующего нажатия.
                skillcheckSlider.NextSpeed();
            }
            else
            {
                Debug.Log($"Skillcheck: miss | value = {skillcheckSlider.Value:F2}");
            }
        }

        public void Win()
        {
            // Сразу гасим ввод и движение слайдера, но завершение даём с паузой,
            // чтобы Player успел доехать и переход не выглядел резким.
            _active = false;
            skillcheckSlider.StopMoving();

            StartCoroutine(CompleteAfterDelay());
        }

        private IEnumerator CompleteAfterDelay()
        {
            if (winDelay > 0f)
            {
                yield return new WaitForSeconds(winDelay);
            }

            Complete(new MinigameResult());
        }
    }
}
