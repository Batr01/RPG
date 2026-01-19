using UnityEngine;
using RPG.Input;

namespace RPG.UI
{
    /// <summary>
    /// Мост между UI джойстиком/кнопками и InputManager.
    /// Преобразует ввод с виртуальных элементов управления в игровые действия.
    /// </summary>
    public class MobileInputBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MobileJoystick joystick;
        [SerializeField] private AttackButton attackButton;

        private InputManager _inputManager;
        private RPG.Player.PlayerCombat _playerCombat;

        private void Start()
        {
            _inputManager = InputManager.Instance;

            // Найти игрока для отображения комбо
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerCombat = player.GetComponent<RPG.Player.PlayerCombat>();
            }
        }

        private void Update()
        {
            UpdateJoystickInput();
            UpdateComboDisplay();
        }

        /// <summary>
        /// Обновление ввода с джойстика
        /// </summary>
        private void UpdateJoystickInput()
        {
            if (joystick == null || _inputManager == null) return;

            // Джойстик управляет движением напрямую через физический ввод
            // InputManager автоматически получит значения от On-Screen Controls
        }

        /// <summary>
        /// Обновление отображения комбо на кнопке атаки
        /// </summary>
        private void UpdateComboDisplay()
        {
            if (attackButton == null || _playerCombat == null) return;

            attackButton.UpdateComboDisplay(
                _playerCombat.CurrentComboIndex,
                3 // максимум комбо
            );
        }

        /// <summary>
        /// Обработка нажатия кнопки атаки (вызывается из UI Event)
        /// </summary>
        public void OnAttackButtonPressed()
        {
            // Кнопка атаки будет подключена через On-Screen Button к Input System
            // Этот метод можно использовать для дополнительной логики
        }
    }
}
