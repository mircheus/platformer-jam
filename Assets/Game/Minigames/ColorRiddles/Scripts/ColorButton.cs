using System;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class ColorButton : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite pressedSprite;

        private int _id;
        private Button _button;
        private Sprite _defaultSprite;

        public event Action<int> ButtonPressedEvent;

        private void OnEnable()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ButtonClicked);
        }

        public void Init(int id)
        {
            _id = id;

            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage != null)
            {
                _defaultSprite = targetImage.sprite;
            }
        }

        public void SetPressed()
        {
            // У кнопки без "нажатого" спрайта (всегда ненажатая) спрайт не меняем.
            if (pressedSprite == null || targetImage == null)
            {
                return;
            }

            targetImage.sprite = pressedSprite;
        }

        public void ResetSprite()
        {
            if (targetImage != null)
            {
                targetImage.sprite = _defaultSprite;
            }
        }

        private void ButtonClicked()
        {
            ButtonPressedEvent?.Invoke(_id);
        }
    }
}
