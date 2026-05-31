using Minigames.Contract;
using UnityEngine;

namespace Minigames
{
    // Пример-заглушка. Корень префаба мини-игры несёт этот скрипт.
    // Зависит ТОЛЬКО от Minigames.Contract — игровые системы здесь недоступны по дисциплине.
    public class PuzzleMinigame : MinigameBase
    {
        public override void Begin(MinigameContext ctx)
        {
            Debug.Log($"PuzzleMinigame started | source: {ctx.SourceObjectID}");
            // TODO: своя логика и UI. При выполнении условия победы вызвать Complete(...).
        }

        // Повесить на кнопку/условие победы.
        public void Win()
        {
            Complete(new MinigameResult());
        }
    }
}
