using UnityEngine;

namespace RPG.Enemy
{
    /// <summary>
    /// Искусственный интеллект врага.
    /// Управляет поведением через конечный автомат (State Machine).
    /// Обнаруживает игрока, преследует и переходит в режим атаки.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private float attackRadius = 2f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Behavior Settings")]
        [SerializeField] private float chaseUpdateInterval = 0.2f;
        [SerializeField] private float loseTargetDistance = 15f; // Дистанция потери цели

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private Transform _player;
        private EnemyState _currentState = EnemyState.Idle;
        private float _nextChaseUpdate = 0f;

        /// <summary>
        /// Состояния врага
        /// </summary>
        public enum EnemyState
        {
            Idle,       // Ожидание
            Chasing,    // Преследование
            Attacking   // В зоне атаки
        }

        #region Properties

        /// <summary>
        /// Текущее состояние AI
        /// </summary>
        public EnemyState CurrentState => _currentState;

        /// <summary>
        /// Цель (игрок)
        /// </summary>
        public Transform Target => _player;

        /// <summary>
        /// Может ли атаковать (находится в зоне атаки)
        /// </summary>
        public bool CanAttack => _currentState == EnemyState.Attacking && _player != null;

        /// <summary>
        /// Должен ли двигаться к цели
        /// </summary>
        public bool ShouldChase => _currentState == EnemyState.Chasing && _player != null;

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!enabled) return;

            UpdateState();
        }

        #endregion

        #region State Machine

        /// <summary>
        /// Обновление состояния AI
        /// </summary>
        private void UpdateState()
        {
            switch (_currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;

                case EnemyState.Chasing:
                    HandleChasingState();
                    break;

                case EnemyState.Attacking:
                    HandleAttackingState();
                    break;
            }
        }

        /// <summary>
        /// Обработка состояния Idle
        /// </summary>
        private void HandleIdleState()
        {
            // Попытаться найти игрока
            if (_player == null)
            {
                _player = FindPlayer();
            }

            // Если игрок найден и в радиусе обнаружения
            if (_player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

                if (distanceToPlayer <= detectionRadius)
                {
                    ChangeState(EnemyState.Chasing);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"{gameObject.name}: Игрок обнаружен! Начинаю преследование.");
                    }
                }
            }
        }

        /// <summary>
        /// Обработка состояния Chasing
        /// </summary>
        private void HandleChasingState()
        {
            if (_player == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            // Обновление преследования с интервалами (оптимизация)
            if (Time.time >= _nextChaseUpdate)
            {
                _nextChaseUpdate = Time.time + chaseUpdateInterval;

                float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

                // Проверить, достиг ли радиуса атаки
                if (distanceToPlayer <= attackRadius)
                {
                    ChangeState(EnemyState.Attacking);
                }
                // Проверить, не слишком ли далеко игрок (потеря цели)
                else if (distanceToPlayer > loseTargetDistance)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"{gameObject.name}: Игрок слишком далеко. Возврат в Idle.");
                    }
                    
                    _player = null;
                    ChangeState(EnemyState.Idle);
                }
            }
        }

        /// <summary>
        /// Обработка состояния Attacking
        /// </summary>
        private void HandleAttackingState()
        {
            if (_player == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // Если игрок вышел из радиуса атаки
            if (distanceToPlayer > attackRadius)
            {
                // Вернуться к преследованию
                ChangeState(EnemyState.Chasing);
            }
        }

        /// <summary>
        /// Изменить состояние
        /// </summary>
        private void ChangeState(EnemyState newState)
        {
            if (_currentState == newState) return;

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name}: Смена состояния {_currentState} → {newState}");
            }

            _currentState = newState;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Найти игрока в сцене
        /// </summary>
        private Transform FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(Core.GameConstants.PLAYER_TAG);
            return playerObject != null ? playerObject.transform : null;
        }

        /// <summary>
        /// Получить расстояние до игрока
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (_player == null) return float.MaxValue;
            return Vector3.Distance(transform.position, _player.position);
        }

        /// <summary>
        /// Принудительно сбросить AI (при смерти)
        /// </summary>
        public void ResetAI()
        {
            _currentState = EnemyState.Idle;
            _player = null;
            enabled = false;
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Радиус обнаружения
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Радиус атаки
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);

            // Радиус потери цели
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, loseTargetDistance);

            // Линия к игроку
            if (_player != null && Application.isPlaying)
            {
                Gizmos.color = _currentState == EnemyState.Attacking ? Color.red : Color.yellow;
                Gizmos.DrawLine(transform.position, _player.position);
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Показать состояние над врагом
            Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
            
            if (screenPos.z > 0)
            {
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 20), 
                    $"State: {_currentState}");
            }
        }
        #endif

        #endregion
    }
}
