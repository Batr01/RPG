using UnityEngine;
using RPG.Core;
using RPG.Combat;

namespace RPG.Player
{
    /// <summary>
    /// Система боя персонажа с поддержкой комбо-атак.
    /// Управляет цепочками атак, тайминг окнами и состоянием боя.
    /// Следует принципу Single Responsibility - только боевая логика.
    /// </summary>
    public class PlayerCombat : MonoBehaviour, IAttackable
    {
        [Header("Combat Settings")]
        [SerializeField] private int maxComboCount = 3;
        [SerializeField] private float comboResetTime = 1.0f;
        [SerializeField] private float comboWindowStart = 0.5f; // С какого момента анимации можно продолжить комбо (0-1)
        [SerializeField] private float comboWindowEnd = 0.9f;   // До какого момента анимации можно продолжить комбо (0-1)

        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 0.1f;

        [Header("Damage Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float[] comboDamageMultipliers = { 1f, 1.5f, 2f };
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private DamageDealer weaponDamageDealer;

        private PlayerAnimationController _animationController;
        private int _currentComboIndex = 0;
        private float _lastAttackTime = 0f;
        private float _comboResetTimer = 0f;
        private bool _isAttacking = false;
        private bool _hasQueuedAttack = false;

        public bool CanAttack => !_isAttacking && Time.time >= _lastAttackTime + attackCooldown;
        public bool IsAttacking => _isAttacking;
        public int CurrentComboIndex => _currentComboIndex;

        private void Awake()
        {
            _animationController = GetComponent<PlayerAnimationController>();
            
            // Инициализировать DamageDealer если он назначен
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.Initialize(baseDamage, enemyLayer);
            }
            else
            {
                Debug.LogWarning("DamageDealer не назначен в PlayerCombat! Урон не будет наноситься.");
            }
        }

        private void Update()
        {
            UpdateComboState();
            HandleQueuedAttack();
        }

        /// <summary>
        /// Выполнить атаку или продолжить комбо
        /// </summary>
        public void Attack()
        {
            // Если уже атакуем, проверяем возможность продолжения комбо
            if (_isAttacking)
            {
                if (CanContinueCombo())
                {
                    _hasQueuedAttack = true;
                }
                return;
            }

            // Если можем атаковать, начинаем атаку
            if (CanAttack)
            {
                ExecuteAttack();
            }
        }

        /// <summary>
        /// Выполнение атаки
        /// </summary>
        private void ExecuteAttack()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            _comboResetTimer = comboResetTime;

            // Рассчитать урон с учетом комбо-множителя
            float damageMultiplier = GetDamageMultiplier();
            float totalDamage = baseDamage * damageMultiplier;

            // Обновить урон в DamageDealer
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.SetDamage(totalDamage);
            }

            // Установка индекса комбо и триггер анимации
            _animationController.SetComboIndex(_currentComboIndex);
            _animationController.TriggerAttack();

            Debug.Log($"Executing attack - Combo Index: {_currentComboIndex}, Display: {_currentComboIndex + 1}/{maxComboCount}, Damage: {totalDamage} (x{damageMultiplier})");
        }

        /// <summary>
        /// Обработка отложенной атаки в комбо-окне
        /// </summary>
        private void HandleQueuedAttack()
        {
            if (!_hasQueuedAttack || !_isAttacking) return;

            float animProgress = _animationController.GetAttackAnimationProgress();

            // Проверка, находимся ли мы в окне комбо
            if (animProgress >= comboWindowStart && animProgress <= comboWindowEnd)
            {
                // Переход к следующей атаке в комбо
                _currentComboIndex++;
                
                if (_currentComboIndex >= maxComboCount)
                {
                    _currentComboIndex = 0; // Сброс комбо после последнего удара
                }

                Debug.Log($"Continuing combo - New Index: {_currentComboIndex}, Progress: {animProgress:F2}");

                _hasQueuedAttack = false;
                _isAttacking = false;
                
                // Небольшая задержка перед следующей атакой
                _lastAttackTime = Time.time - attackCooldown + 0.05f;
                
                ExecuteAttack();
            }
            else if (animProgress > comboWindowEnd)
            {
                // Окно комбо упущено
                Debug.Log($"Combo window missed - Progress: {animProgress:F2}, Window: {comboWindowStart:F2}-{comboWindowEnd:F2}");
                _hasQueuedAttack = false;
            }
        }

        /// <summary>
        /// Обновление состояния комбо
        /// </summary>
        private void UpdateComboState()
        {
            // Проверка завершения анимации атаки
            if (_isAttacking && !_animationController.IsInAttackAnimation())
            {
                _isAttacking = false;
                
                // Если нет отложенной атаки, начинаем отсчет для сброса комбо
                if (!_hasQueuedAttack)
                {
                    _comboResetTimer -= Time.deltaTime;
                    
                    if (_comboResetTimer <= 0f)
                    {
                        ResetCombo();
                    }
                }
            }

            // Сброс комбо, если анимация атаки завершилась и нет продолжения
            if (!_isAttacking && !_hasQueuedAttack)
            {
                _comboResetTimer -= Time.deltaTime;
                
                if (_comboResetTimer <= 0f && _currentComboIndex > 0)
                {
                    ResetCombo();
                }
            }
        }

        /// <summary>
        /// Проверка возможности продолжения комбо
        /// </summary>
        private bool CanContinueCombo()
        {
            if (_currentComboIndex >= maxComboCount - 1)
            {
                Debug.Log("Cannot continue combo - reached max");
                return false; // Достигнут максимум комбо
            }

            float animProgress = _animationController.GetAttackAnimationProgress();
            // Разрешаем установку отложенной атаки, если мы еще не прошли окно комбо
            // Это позволит игроку нажать кнопку заранее, и атака выполнится в нужный момент
            bool canContinue = animProgress < comboWindowEnd;
            
            if (canContinue)
            {
                Debug.Log($"Queueing combo attack - Current Index: {_currentComboIndex}, Progress: {animProgress:F2}");
            }
            
            return canContinue;
        }

        /// <summary>
        /// Сброс комбо
        /// </summary>
        private void ResetCombo()
        {
            _currentComboIndex = 0;
            _hasQueuedAttack = false;
            _animationController.SetComboIndex(0);
            Debug.Log("Combo reset");
        }

        /// <summary>
        /// Принудительная остановка атаки (для внешних событий)
        /// </summary>
        public void StopAttack()
        {
            _isAttacking = false;
            _hasQueuedAttack = false;
            ResetCombo();
        }

        /// <summary>
        /// Получить прогресс текущего комбо (0-1)
        /// </summary>
        public float GetComboProgress()
        {
            return (float)_currentComboIndex / maxComboCount;
        }

        /// <summary>
        /// Получить множитель урона для текущего комбо
        /// </summary>
        private float GetDamageMultiplier()
        {
            if (comboDamageMultipliers == null || comboDamageMultipliers.Length == 0)
            {
                return 1f;
            }

            // Убедиться, что индекс в пределах массива
            int index = Mathf.Min(_currentComboIndex, comboDamageMultipliers.Length - 1);
            return comboDamageMultipliers[index];
        }

        #region Animation Events

        /// <summary>
        /// Вызывается из Animation Event - начало активной фазы удара
        /// </summary>
        public void OnAttackStart()
        {
            if (weaponDamageDealer != null)
            {
                weaponDamageDealer.Activate();
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
            }
        }

        #endregion

        #region Debug
        
        private void OnGUI()
        {
            // Отладочная информация (можно отключить в релизе)
            if (Debug.isDebugBuild)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                GUILayout.Label($"Combo: {_currentComboIndex + 1}/{maxComboCount}");
                GUILayout.Label($"Is Attacking: {_isAttacking}");
                GUILayout.Label($"Can Attack: {CanAttack}");
                GUILayout.Label($"Queued Attack: {_hasQueuedAttack}");
                GUILayout.Label($"Combo Reset Timer: {_comboResetTimer:F2}");
                GUILayout.EndArea();
            }
        }
        
        #endregion
    }
}
