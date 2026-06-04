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
        [SerializeField] private float winDelay = 1f;

        [Tooltip("Звук правильного нажатия кнопки (проигрывается на каждый верный шаг последовательности).")]
        [SerializeField] private AudioClip correctPressClip;

        private int _currentIndex = 0;
        private bool _isWon;
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
            // Игра уже выиграна и ждёт выхода — игнорируем нажатия.
            if (_isWon)
            {
                return;
            }

            if (buttonIndex == correctSequence[_currentIndex])
            {
                colorButtons[buttonIndex].SetPressed();
                PlayCorrectPressSound();

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
                ResetAllButtons();
                TriggerMistakeLine();

                if (_mistakeLineCoroutine != null)
                {
                    StopCoroutine(_mistakeLineCoroutine);
                }
                
                _mistakeLineCoroutine = StartCoroutine(TriggerLineWithDelay());
            }
                
        }
        
        private void PlayCorrectPressSound()
        {
            if (correctPressClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(correctPressClip);
        }

        private void ResetAllButtons()
        {
            foreach (var colorButton in colorButtons)
            {
                colorButton.ResetSprite();
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
            if (_isWon)
            {
                return;
            }

            _isWon = true;
            PlaySuccessSound();

            StartCoroutine(CompleteWithDelay());
        }

        private IEnumerator CompleteWithDelay()
        {
            yield return new WaitForSeconds(winDelay);
            Complete(new MinigameResult());
        }
    }
}
