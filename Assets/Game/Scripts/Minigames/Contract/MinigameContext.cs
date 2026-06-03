using UnityEngine;

namespace Minigames.Contract
{
    public class MinigameContext
    {
        public string SourceObjectID;
        public Transform RenderRoot;

        /// <summary>Звук успешного завершения из MinigameData. Может быть null.</summary>
        public AudioClip SuccessSound;
    }
}
