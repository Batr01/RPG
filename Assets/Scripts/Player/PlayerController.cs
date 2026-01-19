using UnityEngine;
using RPG.Input;

namespace RPG.Player
{
    /// <summary>
    /// Главный контроллер персонажа игрока.
    /// Координирует взаимодействие между компонентами (Movement, Combat, Animation).
    /// Следует принципу Single Responsibility - только координация компонентов.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAnimationController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private PlayerCombat playerCombat;

        private InputManager _inputManager;

        private void Awake()
        {
            // Получение ссылок на компоненты
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();
            
            if (animationController == null)
                animationController = GetComponent<PlayerAnimationController>();
            
            if (playerCombat == null)
                playerCombat = GetComponent<PlayerCombat>();
        }

        private void Start()
        {
            // Получение ссылки на InputManager
            _inputManager = InputManager.Instance;

            if (_inputManager == null)
            {
                Debug.LogError("InputManager не найден в сцене! Добавьте GameObject с InputManager.");
            }
        }

        private void Update()
        {
            if (_inputManager == null) return;

            HandleMovement();
            HandleAttack();
        }

        private void LateUpdate()
        {
            // Обновляем анимацию после всех движений в том же кадре
            // Это обеспечивает синхронизацию анимации с движением без задержки
            UpdateAnimations();
        }

        /// <summary>
        /// Обработка движения персонажа
        /// </summary>
        private void HandleMovement()
        {
            // Запрет на движение во время атаки
            if (playerCombat != null && playerCombat.IsAttacking)
            {
                playerMovement.Move(Vector2.zero);
                return;
            }

            // Стандартный подход - просто передаем направление каждый кадр (как с атаками)
            Vector2 movementDirection = _inputManager.GetMovementDirection();
            playerMovement.Move(movementDirection);
        }

        /// <summary>
        /// Обработка ввода атаки
        /// </summary>
        private void HandleAttack()
        {
            if (_inputManager.AttackPressed && playerCombat != null)
            {
                // Всегда вызываем Attack(), логика проверки внутри метода
                // Это позволяет продолжать комбо во время атаки
                // Защита от случайных атак теперь обрабатывается в InputManager
                playerCombat.Attack();
                
                // Сброс состояния кнопки после обработки
                _inputManager.ResetAttackInput();
            }
        }

        /// <summary>
        /// Обновление параметров анимации
        /// </summary>
        private void UpdateAnimations()
        {
            // Стандартный подход - только скорость и флаг движения
            // Персонаж поворачивается через скрипт, используется одна анимация бега (вперед)
            float normalizedSpeed = playerMovement.GetNormalizedSpeed();
            animationController.SetSpeed(normalizedSpeed);
            animationController.SetMoving(playerMovement.IsMoving);
        }

        /// <summary>
        /// Получить ссылку на компонент движения
        /// </summary>
        public PlayerMovement Movement => playerMovement;

        /// <summary>
        /// Получить ссылку на контроллер анимации
        /// </summary>
        public PlayerAnimationController AnimationController => animationController;

        /// <summary>
        /// Получить ссылку на боевую систему
        /// </summary>
        public PlayerCombat Combat => playerCombat;
    }
}
