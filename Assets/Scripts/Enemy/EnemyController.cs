using UnityEngine;
using RPG.Core;

namespace RPG.Enemy
{
    /// <summary>
    /// Главный контроллер врага - координирует все компоненты.
    /// Реализует паттерны Facade и Mediator.
    /// Управляет взаимодействием между AI, движением, боем и анимациями.
    /// </summary>
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(EnemyCombat))]
    [RequireComponent(typeof(EnemyAnimationController))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Компоненты
        private EnemyMovement _movement;
        private EnemyAI _ai;
        private EnemyCombat _combat;
        private EnemyAnimationController _animationController;
        private HealthSystem _healthSystem;

        // Состояние
        private bool _isDead = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Получить все компоненты
            _movement = GetComponent<EnemyMovement>();
            _ai = GetComponent<EnemyAI>();
            _combat = GetComponent<EnemyCombat>();
            _animationController = GetComponent<EnemyAnimationController>();
            _healthSystem = GetComponent<HealthSystem>();

            // Проверить, что все компоненты найдены
            ValidateComponents();
        }

        private void OnEnable()
        {
            // Подписаться на события здоровья
            if (_healthSystem != null)
            {
                _healthSystem.OnDeath += OnDeath;
                _healthSystem.OnDamageTaken += OnDamageTaken;
            }
        }

        private void OnDisable()
        {
            // Отписаться от событий
            if (_healthSystem != null)
            {
                _healthSystem.OnDeath -= OnDeath;
                _healthSystem.OnDamageTaken -= OnDamageTaken;
            }
        }

        private void Update()
        {
            if (_isDead) return;

            // Координация компонентов
            CoordinateComponents();
        }

        #endregion

        #region Coordination Logic

        /// <summary>
        /// Координация всех компонентов врага
        /// </summary>
        private void CoordinateComponents()
        {
            // Обновить анимацию скорости
            UpdateMovementAnimation();

            // Обработать поведение в зависимости от состояния AI
            switch (_ai.CurrentState)
            {
                case EnemyAI.EnemyState.Idle:
                    HandleIdleBehavior();
                    break;

                case EnemyAI.EnemyState.Chasing:
                    HandleChasingBehavior();
                    break;

                case EnemyAI.EnemyState.Attacking:
                    HandleAttackingBehavior();
                    break;
            }
        }

        /// <summary>
        /// Поведение в состоянии Idle
        /// </summary>
        private void HandleIdleBehavior()
        {
            // Остановить движение
            _movement.Stop();
        }

        /// <summary>
        /// Поведение в состоянии Chasing (преследование)
        /// </summary>
        private void HandleChasingBehavior()
        {
            if (_ai.Target == null) return;

            // Двигаться к цели
            _movement.MoveTowards(_ai.Target.position);
        }

        /// <summary>
        /// Поведение в состоянии Attacking (атака)
        /// </summary>
        private void HandleAttackingBehavior()
        {
            if (_ai.Target == null) return;

            // Остановить движение во время атаки
            if (_combat.IsAttacking)
            {
                _movement.Stop();
            }
            else
            {
                // Повернуться к цели даже если не атакуем
                Vector3 directionToTarget = (_ai.Target.position - transform.position).normalized;
                directionToTarget.y = 0;

                if (directionToTarget.magnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                }

                // Попытаться атаковать
                if (_combat.CanAttack)
                {
                    _combat.Attack();
                }
            }
        }

        /// <summary>
        /// Обновить анимацию движения
        /// </summary>
        private void UpdateMovementAnimation()
        {
            // Получить нормализованную скорость (0-1)
            float normalizedSpeed = _movement.GetNormalizedSpeed();
            
            // Обновить параметр Speed в Animator
            _animationController.SetSpeed(normalizedSpeed);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Обработчик смерти
        /// </summary>
        private void OnDeath()
        {
            if (_isDead) return;

            _isDead = true;

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} умер!");
            }

            // Остановить все активности
            _movement.Stop();
            _combat.StopAttack();
            _ai.ResetAI();

            // Запустить анимацию смерти
            _animationController.TriggerDeath();

            // Отключить компоненты
            _movement.enabled = false;
            _combat.enabled = false;
            _ai.enabled = false;

            // Опционально: уничтожить объект через несколько секунд
            Destroy(gameObject, 5f);
        }

        /// <summary>
        /// Обработчик получения урона
        /// </summary>
        private void OnDamageTaken(float damage)
        {
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} получил {damage} урона! Осталось: {_healthSystem.CurrentHealth}/{_healthSystem.MaxHealth}");
            }

            // Можно добавить визуальные эффекты, звуки и т.д.
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Проверить наличие всех компонентов
        /// </summary>
        private void ValidateComponents()
        {
            if (_movement == null)
                Debug.LogError($"EnemyMovement не найден на {gameObject.name}!");
            
            if (_ai == null)
                Debug.LogError($"EnemyAI не найден на {gameObject.name}!");
            
            if (_combat == null)
                Debug.LogError($"EnemyCombat не найден на {gameObject.name}!");
            
            if (_animationController == null)
                Debug.LogError($"EnemyAnimationController не найден на {gameObject.name}!");
            
            if (_healthSystem == null)
                Debug.LogError($"HealthSystem не найден на {gameObject.name}!");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Получить компонент здоровья
        /// </summary>
        public HealthSystem GetHealthSystem() => _healthSystem;

        /// <summary>
        /// Получить компонент AI
        /// </summary>
        public EnemyAI GetAI() => _ai;

        /// <summary>
        /// Получить компонент боя
        /// </summary>
        public EnemyCombat GetCombat() => _combat;

        /// <summary>
        /// Проверить, мертв ли враг
        /// </summary>
        public bool IsDead => _isDead;

        #endregion

        #region Debug

        #if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
            
            if (screenPos.z > 0)
            {
                GUILayout.BeginArea(new Rect(screenPos.x - 75, Screen.height - screenPos.y, 150, 80));
                GUILayout.Label($"HP: {_healthSystem.CurrentHealth:F0}/{_healthSystem.MaxHealth:F0}");
                GUILayout.Label($"State: {_ai.CurrentState}");
                GUILayout.Label($"Attacking: {_combat.IsAttacking}");
                GUILayout.Label($"Dead: {_isDead}");
                GUILayout.EndArea();
            }
        }
        #endif

        #endregion
    }
}
