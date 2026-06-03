using System;
using Minigames.Contract;
using UnityEngine;

namespace Minigames
{
    public abstract class MinigameBase : MonoBehaviour, IMinigame
    {
        public event Action<MinigameResult> Completed;

        /// <summary>Контекст запуска (источник, корень рендера, звук успеха). Задаётся в Begin.</summary>
        protected MinigameContext Context { get; private set; }

        // Хост зовёт Begin; база запоминает контекст и передаёт управление в OnBegin.
        public void Begin(MinigameContext ctx)
        {
            Context = ctx;
            OnBegin(ctx);
        }

        protected abstract void OnBegin(MinigameContext ctx);

        protected void Complete(MinigameResult result)
        {
            Completed?.Invoke(result);
        }

        /// <summary>
        /// Звук успешного завершения (из MinigameData). Звать в начале Win() — до задержки
        /// и Complete(), чтобы звук успел заиграть до перехода/закрытия игры.
        /// </summary>
        protected void PlaySuccessSound()
        {
            if (Context != null && Context.SuccessSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(Context.SuccessSound);
        }
    }
}
