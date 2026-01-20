using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG.Core;

namespace RPG.UI
{
    /// <summary>
    /// UI компонент для отображения здоровья игрока на экране.
    /// Обновляет полоску здоровья и текст при изменении HP.
    /// Подписывается на события HealthSystem для автоматического обновления.
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private HealthSystem playerHealthSystem;

        [Header("Settings")]
        [SerializeField] private bool showHealthText = true;
        [SerializeField] private string healthTextFormat = "{0} / {1}"; // Current / Max

        [Header("Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float criticalThreshold = 0.25f; // 25% HP
        [SerializeField] private float damagedThreshold = 0.5f; // 50% HP

        [Header("Animation")]
        [SerializeField] private bool smoothTransition = true;
        [SerializeField] private float transitionSpeed = 5f;

        private float _targetFillAmount = 1f;
        private float _currentFillAmount = 1f;

        #region Unity Lifecycle

        private void Awake()
        {
            // Попытаться найти HealthSystem игрока, если не назначен
            if (playerHealthSystem == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(GameConstants.PLAYER_TAG);
                if (player != null)
                {
                    playerHealthSystem = player.GetComponent<HealthSystem>();
                }
            }

            if (playerHealthSystem == null)
            {
                Debug.LogError("HealthSystem игрока не найден для PlayerHealthUI!");
                enabled = false;
                return;
            }

            if (healthBarFill == null)
            {
                Debug.LogError("Health Bar Fill не назначен в PlayerHealthUI!");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (playerHealthSystem != null)
            {
                // Подписаться на события здоровья
                playerHealthSystem.OnHealthChanged += OnHealthChanged;
                playerHealthSystem.OnDeath += OnDeath;

                // Обновить начальное состояние
                OnHealthChanged(playerHealthSystem.CurrentHealth, playerHealthSystem.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (playerHealthSystem != null)
            {
                // Отписаться от событий
                playerHealthSystem.OnHealthChanged -= OnHealthChanged;
                playerHealthSystem.OnDeath -= OnDeath;
            }
        }

        private void Update()
        {
            // Плавное обновление полоски здоровья
            if (smoothTransition && Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.001f)
            {
                _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, transitionSpeed * Time.deltaTime);
                
                if (healthBarFill != null)
                {
                    healthBarFill.fillAmount = _currentFillAmount;
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Обработчик изменения здоровья
        /// </summary>
        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            if (maxHealth <= 0) return;

            // Вычислить процент здоровья
            float healthPercentage = currentHealth / maxHealth;
            _targetFillAmount = healthPercentage;

            // Если не используем плавный переход, обновить сразу
            if (!smoothTransition && healthBarFill != null)
            {
                _currentFillAmount = _targetFillAmount;
                healthBarFill.fillAmount = _currentFillAmount;
            }

            // Обновить цвет полоски
            UpdateHealthColor(healthPercentage);

            // Обновить текст
            UpdateHealthText(currentHealth, maxHealth);
        }

        /// <summary>
        /// Обработчик смерти
        /// </summary>
        private void OnDeath()
        {
            // Можно добавить визуальные эффекты смерти
            Debug.Log("Игрок умер!");
            
            // Например, мигание или изменение цвета
            if (healthBarFill != null)
            {
                healthBarFill.color = Color.black;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Обновить цвет полоски здоровья
        /// </summary>
        private void UpdateHealthColor(float healthPercentage)
        {
            if (healthBarFill == null) return;

            Color targetColor;

            if (healthPercentage <= criticalThreshold)
            {
                targetColor = criticalColor;
            }
            else if (healthPercentage <= damagedThreshold)
            {
                // Плавный переход от критического к поврежденному
                float t = (healthPercentage - criticalThreshold) / (damagedThreshold - criticalThreshold);
                targetColor = Color.Lerp(criticalColor, damagedColor, t);
            }
            else
            {
                // Плавный переход от поврежденного к здоровому
                float t = (healthPercentage - damagedThreshold) / (1f - damagedThreshold);
                targetColor = Color.Lerp(damagedColor, healthyColor, t);
            }

            healthBarFill.color = targetColor;
        }

        /// <summary>
        /// Обновить текст здоровья
        /// </summary>
        private void UpdateHealthText(float currentHealth, float maxHealth)
        {
            if (!showHealthText || healthText == null) return;

            // Форматировать текст
            string formattedText = string.Format(healthTextFormat, 
                Mathf.CeilToInt(currentHealth), 
                Mathf.CeilToInt(maxHealth));

            healthText.text = formattedText;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Установить ссылку на HealthSystem игрока
        /// </summary>
        public void SetHealthSystem(HealthSystem healthSystem)
        {
            // Отписаться от старого
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnHealthChanged -= OnHealthChanged;
                playerHealthSystem.OnDeath -= OnDeath;
            }

            // Установить новый
            playerHealthSystem = healthSystem;

            // Подписаться на новый
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnHealthChanged += OnHealthChanged;
                playerHealthSystem.OnDeath += OnDeath;

                // Обновить UI
                OnHealthChanged(playerHealthSystem.CurrentHealth, playerHealthSystem.MaxHealth);
            }
        }

        /// <summary>
        /// Принудительно обновить UI
        /// </summary>
        public void ForceUpdate()
        {
            if (playerHealthSystem != null)
            {
                OnHealthChanged(playerHealthSystem.CurrentHealth, playerHealthSystem.MaxHealth);
            }
        }

        #endregion
    }
}
