using System.Collections.Generic;
using UnityEngine;
using RPG.Core;

namespace RPG.Combat
{
    /// <summary>
    /// Компонент нанесения урона для оружия.
    /// Устанавливается на оружие игрока/врага и активируется через Animation Events.
    /// Отслеживает попадания, чтобы избежать множественного урона от одного удара.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DamageDealer : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private float currentDamage;
        private LayerMask targetLayers;
        private bool isActive = false;
        private Collider weaponCollider;
        
        // Список объектов, которые уже получили урон от текущего удара
        private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        #region Unity Lifecycle

        private void Awake()
        {
            weaponCollider = GetComponent<Collider>();
            
            // Убедиться, что коллайдер - триггер
            if (!weaponCollider.isTrigger)
            {
                Debug.LogWarning($"Коллайдер на {gameObject.name} должен быть Trigger! Автоматически исправлено.");
                weaponCollider.isTrigger = true;
            }

            // По умолчанию коллайдер выключен
            weaponCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;

            // Проверить, попадает ли объект в целевые слои
            if (!IsInLayerMask(other.gameObject, targetLayers))
            {
                return;
            }

            // Проверить, не попадали ли мы уже в этот объект
            if (hitTargets.Contains(other.gameObject))
            {
                return;
            }

            // Попытаться найти компонент IDamageable
            IDamageable damageable = other.GetComponent<IDamageable>();
            
            if (damageable == null)
            {
                // Попробовать найти в родителях
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null && damageable.IsAlive)
            {
                // Нанести урон
                damageable.TakeDamage(currentDamage);
                
                // Добавить в список попаданий
                hitTargets.Add(other.gameObject);

                if (showDebugInfo)
                {
                    Debug.Log($"{gameObject.name} нанес {currentDamage} урона объекту {other.gameObject.name}");
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Инициализировать DamageDealer с параметрами урона
        /// </summary>
        /// <param name="damageAmount">Количество урона</param>
        /// <param name="targets">Маска слоев целей</param>
        public void Initialize(float damageAmount, LayerMask targets)
        {
            currentDamage = damageAmount;
            targetLayers = targets;

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} инициализирован: Урон={damageAmount}, Целевые слои={targets.value}");
            }
        }

        /// <summary>
        /// Активировать коллайдер оружия (начало удара)
        /// Вызывается через Animation Event
        /// </summary>
        public void Activate()
        {
            if (!weaponCollider.enabled)
            {
                weaponCollider.enabled = true;
                isActive = true;
                hitTargets.Clear(); // Очистить список попаданий для нового удара

                if (showDebugInfo)
                {
                    Debug.Log($"{gameObject.name} активирован (Урон: {currentDamage})");
                }
            }
        }

        /// <summary>
        /// Деактивировать коллайдер оружия (конец удара)
        /// Вызывается через Animation Event
        /// </summary>
        public void Deactivate()
        {
            if (weaponCollider.enabled)
            {
                weaponCollider.enabled = false;
                isActive = false;

                if (showDebugInfo)
                {
                    Debug.Log($"{gameObject.name} деактивирован (Попаданий: {hitTargets.Count})");
                }

                // Очистить список попаданий
                hitTargets.Clear();
            }
        }

        /// <summary>
        /// Установить урон на лету (для комбо-множителей)
        /// </summary>
        public void SetDamage(float damageAmount)
        {
            currentDamage = damageAmount;
        }

        /// <summary>
        /// Проверить, активен ли DamageDealer
        /// </summary>
        public bool IsActive => isActive;

        #endregion

        #region Private Methods

        /// <summary>
        /// Проверить, находится ли объект в указанной маске слоев
        /// </summary>
        private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            return ((1 << obj.layer) & layerMask) != 0;
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !isActive) return;

            // Визуализировать активную зону поражения
            Gizmos.color = Color.red;
            
            if (weaponCollider != null)
            {
                if (weaponCollider is BoxCollider boxCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                }
                else if (weaponCollider is SphereCollider sphereCollider)
                {
                    Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
                }
                else if (weaponCollider is CapsuleCollider capsuleCollider)
                {
                    // Упрощенная визуализация капсулы
                    Gizmos.DrawWireSphere(transform.position + capsuleCollider.center, capsuleCollider.radius);
                }
            }
        }
        #endif

        #endregion
    }
}
