using System;
using System.Collections;
using System.Collections.Generic;
using Minigames.Contract;
using TMPro;
using UnityEngine;

namespace Minigames
{
    public class ColorRiddles : MinigameBase
    {
        [SerializeField] private ColorButton[] colorButtons;
        [SerializeField] private List<string> npcLines;
        [SerializeField] private string npcMistakeLine;
        [SerializeField] private List<int> correctSequence = new List<int>();
        [SerializeField] private TMP_Text npcCurrentLine;

        private int _currentIndex = 0;
        private Coroutine _mistakeLineCoroutine;
        
        protected override void OnBegin(MinigameContext ctx)
        {
            Debug.Log($"ColorRiddles started | source: {ctx.SourceObjectID}");

            for (var i = 0; i < colorButtons.Length; i++)
            {
                var colorButton = colorButtons[i];
                colorButton.Init(i);
                colorButton.ButtonPressedEvent += ColorButtonOnButtonPressedEvent;
            }
            
            TriggerNextNpcLine();
        }

        private void ColorButtonOnButtonPressedEvent(int buttonIndex)
        {
            if (buttonIndex == correctSequence[_currentIndex])
            {
                if (_currentIndex >= correctSequence.Count - 1)
                {
                    Win();
                    return;
                }
                
                _currentIndex++;
                TriggerNextNpcLine();
            }
            else
            {
                _currentIndex = 0;
                TriggerMistakeLine();

                if (_mistakeLineCoroutine != null)
                {
                    StopCoroutine(_mistakeLineCoroutine);
                }
                
                _mistakeLineCoroutine = StartCoroutine(TriggerLineWithDelay());
            }
                
        }
        
        private void TriggerNextNpcLine()
        {
            npcCurrentLine.text = npcLines[_currentIndex];
        }

        private void TriggerMistakeLine()
        {
            npcCurrentLine.text  = npcMistakeLine;
        }

        private IEnumerator TriggerLineWithDelay()
        {
            yield return new WaitForSeconds(1f);
            TriggerNextNpcLine();
        }

        // Повесить на кнопку/условие победы.
        public void Win()
        {
            PlaySuccessSound();

            Complete(new MinigameResult());
        }
    }
}
