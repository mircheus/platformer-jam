using Minigames.Contract;
using UnityEngine;

namespace Minigames
{
    public class CatchEgg : MinigameBase
    {
        [SerializeField] private PlayerCatcher playerCatcher;
        [SerializeField] private ObjectSpawner objectSpawner;
        
        public override void Begin(MinigameContext ctx)
        {
            Debug.Log($"CatchEgg started | source: {ctx.SourceObjectID}");

            playerCatcher.EnableControl();
        }

        public void Win()
        {
            playerCatcher.DisableControl();

            Complete(new MinigameResult());
        }
    }
}