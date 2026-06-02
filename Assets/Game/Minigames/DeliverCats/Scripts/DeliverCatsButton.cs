using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.DeliverCats
{
    // Одна из кнопок игры. Хранит свой индекс (0 или 1) и сообщает о нажатии.
    [RequireComponent(typeof(Button))]
    public class DeliverCatsButton : MonoBehaviour
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
