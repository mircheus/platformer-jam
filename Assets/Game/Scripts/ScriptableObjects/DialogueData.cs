using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;

    public List<DialogueLine> lines;
}

[Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;

    [Tooltip("Если задано — после этой реплики диалог встанет на паузу, хост-адаптер запустит мини-игру, и по её завершении диалог продолжится со следующей реплики. Пусто — обычная реплика.")]
    public MinigameData minigameAfter;

    /// <summary>
    /// Нужно ли остановить диалог после этой реплики и ждать внешнего Resume().
    /// DialogueSystem смотрит только на этот булев флаг и про мини-игры не знает.
    /// </summary>
    public bool PausesAfter => minigameAfter != null;
}
