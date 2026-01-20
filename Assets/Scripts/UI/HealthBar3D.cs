using UnityEngine;
using UnityEngine.UI;
using RPG.Core;

namespace RPG.UI
{
    /// <summary>
    /// 3D полоска здоровья, которая отображается над объектом (World Space Canvas).
    /// Автоматически поворачивается к камере (billboard эффект).
    /// Обновляется при изменении здоровья через события HealthSystem.
    /// </summary>
    public class HealthBar3D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Transform billboard; // Canvas для поворота к камере
        
        [Header("Settings")]
        [SerializeField] private bool hideWhenFull = false;
        [SerializeField] private bool hideWhenDead = true;
        [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);

        [Header("Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float criticalThreshold = 0.3f; // 30% HP
        [SerializeField] private float damagedThreshold = 0.7f; // 70% HP

        private HealthSystem _healthSystem;
        private UnityEngine.Camera _mainCamera;
        private Canvas _canvas;

        #region Unity Lifecycle

        private void Awake()
        {
            _mainCamera = UnityEngine.Camera.main;
            _canvas = GetComponent<Canvas>();

            // Найти HealthSystem в родителе или себе
            _healthSystem = GetComponentInParent<HealthSystem>();

            if (_healthSystem == null)
            {
                Debug.LogError($"HealthSystem не найден для {gameObject.name}! HealthBar3D не будет работать.");
                enabled = false;
                return;
            }

            if (fillImage == null)
            {
                Debug.LogError($"Fill Image не назначен в {gameObject.name}!");
                enabled = false;
                return;
            }

            // Если billboard не назначен, использовать сам transform
            if (billboard == null)
            {
                billboard = transform;
            }
        }

        private void OnEnable()
        {
            if (_healthSystem != null)
            {
                // Подписаться на события здоровья
                _healthSystem.OnHealthChanged += OnHealthChanged;
                _healthSystem.OnDeath += OnDeath;

                // Обновить начальное состояние
                OnHealthChanged(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                // Отписаться от событий
                _healthSystem.OnHealthChanged -= OnHealthChanged;
                _healthSystem.OnDeath -= OnDeath;
            }
        }

        private void LateUpdate()
        {
            // Billboard эффект - всегда смотреть на камеру
            LookAtCamera();
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

            // Обновить fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = healthPercentage;

                // Изменить цвет в зависимости от процента здоровья
                UpdateHealthColor(healthPercentage);
            }

            // Скрыть/показать полоску
            UpdateVisibility(healthPercentage);
        }

        /// <summary>
        /// Обработчик смерти
        /// </summary>
        private void OnDeath()
        {
            if (hideWhenDead && _canvas != null)
            {
                _canvas.enabled = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Повернуть полоску к камере (billboard)
        /// </summary>
        private void LookAtCamera()
        {
            if (_mainCamera == null || billboard == null) return;

            // Повернуть к камере
            billboard.rotation = Quaternion.LookRotation(billboard.position - _mainCamera.transform.position);
        }

        /// <summary>
        /// Обновить цвет полоски здоровья
        /// </summary>
        private void UpdateHealthColor(float healthPercentage)
        {
            if (fillImage == null) return;

            if (healthPercentage <= criticalThreshold)
            {
                fillImage.color = criticalColor;
            }
            else if (healthPercentage <= damagedThreshold)
            {
                // Плавный переход от красного к желтому
                float t = (healthPercentage - criticalThreshold) / (damagedThreshold - criticalThreshold);
                fillImage.color = Color.Lerp(criticalColor, damagedColor, t);
            }
            else
            {
                // Плавный переход от желтого к зеленому
                float t = (healthPercentage - damagedThreshold) / (1f - damagedThreshold);
                fillImage.color = Color.Lerp(damagedColor, healthyColor, t);
            }
        }

        /// <summary>
        /// Обновить видимость полоски
        /// </summary>
        private void UpdateVisibility(float healthPercentage)
        {
            if (_canvas == null) return;

            if (hideWhenFull && healthPercentage >= 0.99f)
            {
                _canvas.enabled = false;
            }
            else
            {
                _canvas.enabled = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Установить смещение полоски относительно объекта
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
            transform.localPosition = offset;
        }

        /// <summary>
        /// Принудительно обновить полоску
        /// </summary>
        public void ForceUpdate()
        {
            if (_healthSystem != null)
            {
                OnHealthChanged(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
            }
        }

        #endregion
    }
}
