using UnityEngine;

namespace RPG.Camera
{
    /// <summary>
    /// Камера, следующая за игроком с фиксированным углом (top-down вид).
    /// Обеспечивает плавное следование и настраиваемые параметры обзора.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayer = true;

        [Header("Camera Position")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -7f);
        [SerializeField] private float cameraAngle = 50f;

        [Header("Follow Settings")]
        [SerializeField] private float followSmoothTime = 0.2f;
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = false;
        [SerializeField] private bool followZ = true;

        [Header("Zoom Settings")]
        [SerializeField] private bool enableZoom = false;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float zoomSpeed = 2f;

        [Header("Boundaries")]
        [SerializeField] private bool useBoundaries = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
        [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

        private Vector3 _velocity = Vector3.zero;
        private float _currentZoom;

        private void Start()
        {
            // Автоматический поиск игрока
            if (autoFindPlayer && target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    Debug.LogWarning("PlayerCamera: Игрок с тегом 'Player' не найден!");
                }
            }

            // Инициализация зума
            _currentZoom = offset.y;

            // Установка начального угла камеры
            transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleZoom();
            FollowTarget();
        }

        /// <summary>
        /// Плавное следование за целью
        /// </summary>
        private void FollowTarget()
        {
            Vector3 targetPosition = target.position + offset;

            // Применение зума
            if (enableZoom)
            {
                targetPosition.y = _currentZoom;
                // Корректировка Z с учетом зума для сохранения угла
                float zoomRatio = _currentZoom / offset.y;
                targetPosition.z = target.position.z + (offset.z * zoomRatio);
            }

            // Применение ограничений по осям
            Vector3 currentPos = transform.position;
            if (!followX) targetPosition.x = currentPos.x;
            if (!followY) targetPosition.y = currentPos.y;
            if (!followZ) targetPosition.z = currentPos.z;

            // Применение границ
            if (useBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
            }

            // Плавное движение камеры
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                followSmoothTime
            );
        }

        /// <summary>
        /// Обработка зума камеры (для будущего расширения)
        /// </summary>
        private void HandleZoom()
        {
            if (!enableZoom) return;

            // Можно добавить управление зумом через Input System
            // Например, колесико мыши или жесты на мобильных устройствах
            
            _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
        }

        /// <summary>
        /// Установить новую цель для камеры
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Мгновенная телепортация камеры к цели
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            transform.position = targetPosition;
            _velocity = Vector3.zero;
        }

        /// <summary>
        /// Установить смещение камеры
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// Установить границы камеры
        /// </summary>
        public void SetBoundaries(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBoundaries = true;
        }

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            // Визуализация границ камеры
            if (useBoundaries)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3(
                    (minBounds.x + maxBounds.x) / 2f,
                    0f,
                    (minBounds.y + maxBounds.y) / 2f
                );
                Vector3 size = new Vector3(
                    maxBounds.x - minBounds.x,
                    0.1f,
                    maxBounds.y - minBounds.y
                );
                Gizmos.DrawWireCube(center, size);
            }

            // Визуализация связи с целью
            if (target != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(target.position, 0.5f);
            }
        }

        #endregion
    }
}
