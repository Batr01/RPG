using UnityEngine;
using RPG.Core;

namespace RPG.Player
{
    /// <summary>
    /// Отвечает за движение персонажа.
    /// Следует принципу Single Responsibility - только управление движением.
    /// Использует CharacterController для физики и коллизий.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour, IMovable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f; // Увеличена скорость поворота для более отзывчивого управления
        [SerializeField] private float gravity = -9.81f;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private Vector2 _currentDirection;

        public float MoveSpeed 
        { 
            get => moveSpeed; 
            set => moveSpeed = value; 
        }

        public bool IsMoving => _currentDirection.sqrMagnitude > 0.01f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ApplyGravity();
            
            // Применяем движение
            if (_currentDirection.sqrMagnitude > 0.01f)
            {
                // Преобразование 2D направления в 3D для top-down вида
                Vector3 moveDirection = new Vector3(_currentDirection.x, 0f, _currentDirection.y).normalized;
                
                // Движение персонажа
                Vector3 move = moveDirection * moveSpeed * Time.deltaTime;
                _characterController.Move(move);

                // Поворот персонажа в сторону движения
                RotateTowardsMovement(moveDirection);
            }
        }

        /// <summary>
        /// Переместить объект в указанном направлении (реализация интерфейса IMovable)
        /// Стандартный подход - просто сохраняем направление каждый кадр
        /// </summary>
        public void Move(Vector2 direction)
        {
            _currentDirection = direction;
        }

        /// <summary>
        /// Плавный поворот персонажа в направлении движения
        /// </summary>
        private void RotateTowardsMovement(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                // Используем более точный расчет угла для плавного поворота
                float angle = Quaternion.Angle(transform.rotation, targetRotation);
                
                // Если угол очень мал, применяем поворот мгновенно для точности
                if (angle < 0.1f)
                {
                    transform.rotation = targetRotation;
                }
                else
                {
                    // Используем Slerp для плавного поворота
                    float rotationStep = Mathf.Min(rotationSpeed * Time.deltaTime, 1f);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        rotationStep
                    );
                }
            }
        }

        /// <summary>
        /// Применение гравитации
        /// </summary>
        private void ApplyGravity()
        {
            if (!_characterController.isGrounded)
            {
                _velocity.y += gravity * Time.deltaTime;
            }
            else if (_velocity.y < 0)
            {
                _velocity.y = -2f; // Небольшое значение для удержания на земле
            }

            _characterController.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Получить текущую скорость движения (для анимации)
        /// </summary>
        public float GetCurrentSpeed()
        {
            return _currentDirection.magnitude * moveSpeed;
        }

        /// <summary>
        /// Получить нормализованную скорость (0-1 для анимации)
        /// </summary>
        public float GetNormalizedSpeed()
        {
            return _currentDirection.magnitude;
        }

        /// <summary>
        /// Получить текущее направление движения (для анимации Blend Tree)
        /// </summary>
        public Vector2 GetMovementDirection()
        {
            return _currentDirection;
        }
    }
}
