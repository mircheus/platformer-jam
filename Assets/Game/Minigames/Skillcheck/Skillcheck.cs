using Minigames;
using Minigames.Contract;
using UnityEngine;

namespace Game.Minigames.Skillcheck
{
    public class Skillcheck : MinigameBase
    {
        public override void Begin(MinigameContext ctx)
        {
            
        }
        
        public void Win()
        {
            Complete(new MinigameResult());
        }
    }
}