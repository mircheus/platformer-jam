using Minigames.Contract;
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

        private int _caughtCount;

        public override void Begin(MinigameContext ctx)
        {
            Debug.Log($"CatchEgg started | source: {ctx.SourceObjectID}");

            _caughtCount = 0;

            playerCatcher.EnableControl();
            objectSpawner.EggCaught += OnEggCaught;
            objectSpawner.StartSpawning();
        }

        private void OnEggCaught()
        {
            _caughtCount++;

            Debug.Log($"CatchEgg: caught {_caughtCount}/{targetCatchCount}");

            if (_caughtCount >= targetCatchCount)
            {
                Win();
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
