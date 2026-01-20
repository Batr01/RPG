using UnityEngine;
using RPG.Core;
using RPG.Combat;

namespace RPG.Enemy
{
    /// <summary>
    /// Боевая система врага.
    /// Управляет атаками, уроном и координирует с DamageDealer.
    /// Реализует IAttackable для совместимости с общей архитектурой.
    /// </summary>
    public class EnemyCombat : MonoBehaviour, IAttackable
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackDamage = 15f;
        
        [Header("References")]
        [SerializeField] private DamageDealer weaponDamageDealer;
        [SerializeField] private LayerMask playerLayer;

        private EnemyAnimationController _animationController;
        private float _lastAttackTime = -999f;
        private bool _isAttacking = false;

        #region IAttackable Implementation

        /// <summary>
        /// Может ли атаковать в данный момент
        /// </summary>
        public bool CanAttack => !_isAttacking && Time.time >= _lastAttackTime + attackCooldown;

        /// <summary>
        /// Находится ли в процессе атаки
        /// </summary>
        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// Выполнить атаку
        /// </summary>
        public void Attack()
        {
            if (!CanAttack) return;

            ExecuteAttack();
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _animationController = GetComponent<EnemyAnimationController>();

            // Инициализировать DamageDealer если он назначен
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.Initialize(attackDamage, playerLayer);
            }
            else
            {
                Debug.LogWarning($"DamageDealer не назначен на {gameObject.name}! Урон не будет наноситься.");
            }
        }

        private void Update()
        {
            // Обновить состояние атаки
            UpdateAttackState();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Выполнение атаки
        /// </summary>
        private void ExecuteAttack()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;

            // Запустить анимацию атаки
            if (_animationController != null)
            {
                _animationController.TriggerAttack();
            }

            Debug.Log($"{gameObject.name} атакует! Урон: {attackDamage}");
        }

        /// <summary>
        /// Обновление состояния атаки
        /// </summary>
        private void UpdateAttackState()
        {
            if (!_isAttacking) return;

            // Проверить, завершилась ли анимация атаки
            if (_animationController != null && !_animationController.IsInAttackAnimation())
            {
                _isAttacking = false;
            }
        }

        /// <summary>
        /// Принудительно остановить атаку
        /// </summary>
        public void StopAttack()
        {
            _isAttacking = false;
            
            if (_animationController != null)
            {
                _animationController.ResetAttackTrigger();
            }

            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.Deactivate();
            }
        }

        #endregion

        #region Animation Events

        /// <summary>
        /// Вызывается из Animation Event - начало активной фазы удара
        /// </summary>
        public void OnAttackStart()
        {
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.Activate();
                Debug.Log($"{gameObject.name}: Коллайдер оружия активирован");
            }
        }

        /// <summary>
        /// Вызывается из Animation Event - конец активной фазы удара
        /// </summary>
        public void OnAttackEnd()
        {
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.Deactivate();
                Debug.Log($"{gameObject.name}: Коллайдер оружия деактивирован");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Установить урон (для вариативности врагов)
        /// </summary>
        public void SetDamage(float damage)
        {
            attackDamage = Mathf.Max(0, damage);
            
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.SetDamage(attackDamage);
            }
        }

        /// <summary>
        /// Получить текущий урон
        /// </summary>
        public float GetDamage()
        {
            return attackDamage;
        }

        #endregion
    }
}
