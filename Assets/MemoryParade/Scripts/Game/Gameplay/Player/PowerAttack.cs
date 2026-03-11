using UnityEngine;
using UnityEngine.UI;

namespace Assets.MemoryParade.Scripts.Game.Gameplay.Player
{
    public class PowerAttack : MonoBehaviour
    {
        private BattleSystem _battleSystem;
        private Button _button;
        private Sprite _startSprite;

        [SerializeField] private Sprite activeSprite;

        public bool click = false;

        private void Start()
        {
            _battleSystem = FindAnyObjectByType<BattleSystem>();
            _button = GetComponent<Button>();
            _startSprite = _button.image.sprite;

            RefreshButtonView();
        }

        private void Update()
        {
            if (_battleSystem == null || _button == null)
                return;

            RefreshButtonView();

            if (click)
            {
                _battleSystem.attackCount = 0;
                click = false;
                RefreshButtonView();
            }
        }

        private void RefreshButtonView()
        {
            bool isAvailable = _battleSystem.attackCount >= 3;

            _button.interactable = isAvailable;
            _button.image.sprite = isAvailable && activeSprite != null
                ? activeSprite
                : _startSprite;
        }

        public void OnClick()
        {
            if (_battleSystem == null)
                return;

            if (_battleSystem.attackCount >= 3)
            {
                click = true;
                _battleSystem.PlayerPowerAttack();
            }
        }
    }
}