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

        [Header("UI")]
        [Tooltip("Текст счётчика пойманных объектов.")]
        [SerializeField] private TMP_Text counterText;

        private int _caughtCount;

        public override void Begin(MinigameContext ctx)
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

            Complete(new MinigameResult());
        }
    }
}
