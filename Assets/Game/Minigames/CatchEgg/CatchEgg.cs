using System.Collections;
using Minigames.Contract;
using TMPro;
using UnityEngine;

namespace Minigames
{
    public class CatchEgg : MinigameBase
    {
        [SerializeField] private PlayerCatcher playerCatcher;
        [SerializeField] private ObjectSpawner objectSpawner;

        [Header("Win Condition")]
        [Tooltip("Сколько объектов нужно поймать для победы.")]
        [SerializeField] private int targetCatchCount = 5;

        [Tooltip("Пауза после победы перед завершением — звук успеха успевает доиграть, переход не выглядит резким.")]
        [SerializeField] private float winDelay = 0.6f;

        [Header("UI")]
        [Tooltip("Текст счётчика пойманных объектов.")]
        [SerializeField] private TMP_Text counterText;

        private int _caughtCount;

        protected override void OnBegin(MinigameContext ctx)
        {
            Debug.Log($"CatchEgg started | source: {ctx.SourceObjectID}");

            _caughtCount = 0;
            UpdateCounterText();

            playerCatcher.EnableControl();
            objectSpawner.EggCaught += OnEggCaught;
            objectSpawner.StartSpawning();
        }

        private void OnEggCaught()
        {
            _caughtCount++;
            UpdateCounterText();

            // Звук на каждую успешную поимку (в т.ч. последнюю перед победой).
            PlaySuccessSound();

            Debug.Log($"CatchEgg: caught {_caughtCount}/{targetCatchCount}");

            if (_caughtCount >= targetCatchCount)
            {
                Win();
            }
        }

        private void UpdateCounterText()
        {
            if (counterText != null)
            {
                counterText.text = $"{_caughtCount}/{targetCatchCount}";
            }
        }

        public void Win()
        {
            playerCatcher.DisableControl();
            objectSpawner.StopSpawning();
            objectSpawner.EggCaught -= OnEggCaught;

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
