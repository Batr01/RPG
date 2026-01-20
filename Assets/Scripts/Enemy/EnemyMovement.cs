using UnityEngine;
using RPG.Core;

namespace RPG.Enemy
{
    /// <summary>
    /// Компонент движения врага к цели.
    /// Реализует IMovable для совместимости с общей архитектурой.
    /// Использует CharacterController для перемещения.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMovement : MonoBehaviour, IMovable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float stoppingDistance = 1.5f;

        [Header("Ground Check")]
        [SerializeField] private float gravity = -9.81f;

        private CharacterController _characterController;
        private Vector3 _currentVelocity;
        private bool _isMoving;

        #region IMovable Implementation

        /// <summary>
        /// Скорость движения
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0, value);
        }

        /// <summary>
        /// Проверка, движется ли враг
        /// </summary>
        public bool IsMoving => _isMoving;

        /// <summary>
        /// Двигаться в направлении (для совместимости с IMovable)
        /// </summary>
        public void Move(Vector2 direction)
        {
            Vector3 moveDirection = new Vector3(direction.x, 0, direction.y);
            Move(moveDirection);
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            // Применить гравитацию
            ApplyGravity();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Двигаться к цели
        /// </summary>
        /// <param name="targetPosition">Позиция цели</param>
        public void MoveTowards(Vector3 targetPosition)
        {
            // Вычислить направление к цели
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Игнорировать вертикальное направление

            // Проверить расстояние до цели
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > stoppingDistance)
            {
                // Двигаться к цели
                Move(direction);
                
                // Повернуться к цели
                RotateTowards(direction);
                
                _isMoving = true;
            }
            else
            {
                // Остановиться, но продолжить поворачиваться к цели
                Stop();
                RotateTowards(direction);
            }
        }

        /// <summary>
        /// Остановить движение
        /// </summary>
        public void Stop()
        {
            _isMoving = false;
        }

        /// <summary>
        /// Проверить, достиг ли враг цели
        /// </summary>
        public bool HasReachedDestination(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            return distance <= stoppingDistance;
        }

        /// <summary>
        /// Получить текущую скорость (для анимаций)
        /// </summary>
        public float GetCurrentSpeed()
        {
            if (!_isMoving) return 0f;
            
            Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
            return horizontalVelocity.magnitude;
        }

        /// <summary>
        /// Получить нормализованную скорость (0-1) для анимаций
        /// </summary>
        public float GetNormalizedSpeed()
        {
            return moveSpeed > 0 ? GetCurrentSpeed() / moveSpeed : 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Двигаться в направлении (3D вектор)
        /// </summary>
        private void Move(Vector3 direction)
        {
            if (direction.magnitude > 0.01f)
            {
                // Вычислить движение
                Vector3 movement = direction * moveSpeed;
                
                // Сохранить текущую скорость для анимаций
                _currentVelocity = movement;
                
                // Применить движение с учетом времени
                _characterController.Move(movement * Time.deltaTime);
            }
        }

        /// <summary>
        /// Повернуться в направлении
        /// </summary>
        private void RotateTowards(Vector3 direction)
        {
            if (direction.magnitude < 0.01f) return;

            // Вычислить целевой поворот
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Плавный поворот
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// Применить гравитацию
        /// </summary>
        private void ApplyGravity()
        {
            if (_characterController.isGrounded)
            {
                // Сброс вертикальной скорости при касании земли
                if (_currentVelocity.y < 0)
                {
                    _currentVelocity.y = -2f; // Небольшое значение для стабильности
                }
            }
            else
            {
                // Применить гравитацию
                _currentVelocity.y += gravity * Time.deltaTime;
            }

            // Применить вертикальное движение
            Vector3 verticalMovement = new Vector3(0, _currentVelocity.y, 0);
            _characterController.Move(verticalMovement * Time.deltaTime);
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Визуализировать дистанцию остановки
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        }
        #endif

        #endregion
    }
}
