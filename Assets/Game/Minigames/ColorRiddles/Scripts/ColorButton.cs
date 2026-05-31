using System;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class ColorButton : MonoBehaviour
    {
        private int _id;
        private Button _button;
        
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
        }

        private void ButtonClicked()
        {
            ButtonPressedEvent?.Invoke(_id);
        }
    }
}