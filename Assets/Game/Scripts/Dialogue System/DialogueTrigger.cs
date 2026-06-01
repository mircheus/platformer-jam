using System;
using UnityEngine;

/// <summary>
/// Переиспользуемый помощник для запуска диалога из интерактивных объектов
/// (подъём предмета, использование требуемого предмета и т.д.). Инкапсулирует
/// ссылку на диалог и логику «проиграть только один раз». Объекты держат его
/// как сериализуемое поле и зовут TryPlay() после успешного действия.
/// </summary>
[Serializable]
public class DialogueTrigger
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool playOnce = true;

    private bool hasPlayed;

    public void TryPlay()
    {
        if (dialogue == null)
        {
            return;
        }

        if (playOnce && hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        GameContext.Instance.DialogueSelector.SelectDialogue(dialogue);
    }
}
