using System;

namespace Minigames.Contract
{
    public interface IMinigame
    {
        event Action<MinigameResult> Completed;

        void Begin(MinigameContext ctx);
    }
}
