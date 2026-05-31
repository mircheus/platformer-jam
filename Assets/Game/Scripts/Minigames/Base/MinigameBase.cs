using System;
using Minigames.Contract;
using UnityEngine;

namespace Minigames
{
    public abstract class MinigameBase : MonoBehaviour, IMinigame
    {
        public event Action<MinigameResult> Completed;

        public abstract void Begin(MinigameContext ctx);

        protected void Complete(MinigameResult result)
        {
            Completed?.Invoke(result);
        }
    }
}
