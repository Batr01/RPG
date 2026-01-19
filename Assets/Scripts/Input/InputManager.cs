using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Input
{
    /// <summary>
    /// Централизованный менеджер для обработки всех игровых вводов.
    /// Singleton паттерн для глобального доступа.
    /// Использует новую Input System для кроссплатформенной поддержки.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private PlayerInputActions _inputActions;

        // Публичные свойства для доступа к значениям ввода
        public Vector2 MovementDirection { get; private set; }
        public bool AttackPressed { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInput();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeInput()
        {
            try
            {
                _inputActions = new PlayerInputActions();
                
                // Проверка, что asset был создан
                if (_inputActions?.asset == null)
                {
                    Debug.LogError("InputManager: Не удалось создать InputActionAsset!");
                    return;
                }

                // Получение Player actions (это структура, не может быть null)
                var playerActions = _inputActions.Player;

                // Подписка на события движения (аналогично атаке)
                if (playerActions.Movement != null)
                {
                    playerActions.Movement.performed += OnMovementPerformed;
                    playerActions.Movement.canceled += OnMovementCanceled;
                }
                else
                {
                    Debug.LogError("InputManager: Movement action не найдена!");
                }

                if (playerActions.Attack != null)
                {
                    playerActions.Attack.performed += OnAttackPerformed;
                    playerActions.Attack.canceled += OnAttackCanceled;
                }
                else
                {
                    Debug.LogError("InputManager: Attack action не найдена!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"InputManager: Ошибка при инициализации ввода: {e.Message}");
            }
        }

        private void Update()
        {
            // Обновляем направление движения каждый кадр (стандартный подход как с атаками)
            if (_inputActions != null && _inputActions.Player.Movement != null)
            {
                MovementDirection = _inputActions.Player.Movement.ReadValue<Vector2>();
            }
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void OnDestroy()
        {
            // Отписка от событий
            if (_inputActions != null)
            {
                try
                {
                    var playerActions = _inputActions.Player;
                    
                    if (playerActions.Movement != null)
                    {
                        playerActions.Movement.performed -= OnMovementPerformed;
                        playerActions.Movement.canceled -= OnMovementCanceled;
                    }

                    if (playerActions.Attack != null)
                    {
                        playerActions.Attack.performed -= OnAttackPerformed;
                        playerActions.Attack.canceled -= OnAttackCanceled;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"InputManager: Ошибка при отписке от событий: {e.Message}");
                }
            }
        }

        #region Input Callbacks

        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            // Направление движения обновляется в Update() каждый кадр
            // Здесь можно оставить пустым или использовать для событий
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            // Направление движения обновляется в Update() каждый кадр
            // Здесь можно оставить пустым или использовать для событий
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPressed = true;
        }

        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            AttackPressed = false;
        }

        #endregion

        /// <summary>
        /// Сброс состояния кнопки атаки (вызывается после обработки)
        /// </summary>
        public void ResetAttackInput()
        {
            AttackPressed = false;
        }

        /// <summary>
        /// Получить нормализованное направление движения
        /// </summary>
        public Vector2 GetMovementDirection()
        {
            return MovementDirection.normalized;
        }

        /// <summary>
        /// Проверка, есть ли движение
        /// </summary>
        public bool IsMoving()
        {
            return MovementDirection.sqrMagnitude > 0.01f;
        }
    }
}
