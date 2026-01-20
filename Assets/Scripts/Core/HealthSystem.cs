using System;
using UnityEngine;

namespace RPG.Core
{
    /// <summary>
    /// Универсальная система здоровья для любых объектов (игрок, враги, NPC).
    /// Реализует IDamageable и предоставляет события для UI и других систем.
    /// Следует принципу Single Responsibility - только управление здоровьем.
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        
        private float currentHealth;
        private bool isDead = false;

        #region Events
        
        /// <summary>
        /// Событие изменения здоровья (текущее, максимальное)
        /// </summary>
        public event Action<float, float> OnHealthChanged;
        
        /// <summary>
        /// Событие получения урона (количество урона)
        /// </summary>
        public event Action<float> OnDamageTaken;
        
        /// <summary>
        /// Событие смерти
        /// </summary>
        public event Action OnDeath;
        
        #endregion

        #region IDamageable Implementation

        /// <summary>
        /// Текущее здоровье
        /// </summary>
        public float CurrentHealth => currentHealth;

        /// <summary>
        /// Максимальное здоровье
        /// </summary>
        public float MaxHealth => maxHealth;

        /// <summary>
        /// Проверка, жив ли объект
        /// </summary>
        public bool IsAlive => !isDead && currentHealth > 0;

        /// <summary>
        /// Получить урон
        /// </summary>
        /// <param name="damage">Количество урона</param>
        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            // Вызов событий
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"{gameObject.name} получил {damage} урона. Осталось HP: {currentHealth}/{maxHealth}");

            // Проверка смерти
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Восстановить здоровье
        /// </summary>
        /// <param name="amount">Количество восстанавливаемого здоровья</param>
        public void Heal(float amount)
        {
            if (isDead || amount <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"{gameObject.name} восстановил {amount} HP. Текущее HP: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Получить процент здоровья (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0;
        }

        /// <summary>
        /// Сбросить здоровье до максимума (для возрождения)
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"{gameObject.name} здоровье восстановлено до {maxHealth}");
        }

        /// <summary>
        /// Установить максимальное здоровье (для левел-апа)
        /// </summary>
        public void SetMaxHealth(float newMaxHealth, bool healToMax = false)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            
            if (healToMax)
            {
                currentHealth = maxHealth;
            }
            else
            {
                // Сохранить процент здоровья
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Инициализация здоровья
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // Уведомить подписчиков о начальном состоянии
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Обработка смерти
        /// </summary>
        private void Die()
        {
            isDead = true;
            currentHealth = 0;

            Debug.Log($"{gameObject.name} умер!");

            // Вызов события смерти
            OnDeath?.Invoke();
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        /// <summary>
        /// Визуализация в редакторе
        /// </summary>
        private void OnValidate()
        {
            // Убедиться, что максимальное здоровье положительное
            maxHealth = Mathf.Max(1, maxHealth);
        }
        #endif

        #endregion
    }
}
