using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

namespace RPG.Combat
{
    /// <summary>
    /// Управляет VFX эффектами атаки мечом через Visual Effect Graph.
    /// Активирует эффекты синхронно с анимацией атаки.
    /// </summary>
    public class SwordAttackVFX : MonoBehaviour
    {
        [Header("VFX References")]
        [SerializeField] private VisualEffect visualEffect; // Visual Effect Graph
        [SerializeField] private TrailRenderer swordTrail;
        
        [Header("Settings")]
        [SerializeField] private bool playOnAttackStart = true;
        [SerializeField] private bool stopOnAttackEnd = true;
        [SerializeField] private float trailDuration = 0.3f;

        private bool _isTrailActive = false;
        private bool _isInitialized = false;

        private void Awake()
        {
            InitializeComponents();
        }

        /// <summary>
        /// Инициализировать компоненты VFX
        /// </summary>
        private void InitializeComponents()
        {
            if (_isInitialized) return;

            // Получить Visual Effect, если не назначен
            if (visualEffect == null)
            {
                visualEffect = GetComponent<VisualEffect>();
            }

            if (visualEffect != null)
            {
                // Убедиться, что эффект не запускается автоматически
                // Остановить эффект, если он был запущен при старте
                try
                {
                    // Сначала остановить все события
                    visualEffect.Stop();
                    // Очистить все частицы и переинициализировать
                    visualEffect.Reinit();
                    // Убедиться, что эффект полностью остановлен
                    Debug.Log("SwordAttackVFX: Visual Effect остановлен при инициализации. Эффект будет запускаться только при атаке.");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SwordAttackVFX: Не удалось остановить Visual Effect при инициализации: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("SwordAttackVFX: Visual Effect не найден! Добавьте компонент Visual Effect на этот объект.");
            }

            // Получить Trail Renderer, если не назначен
            if (swordTrail == null)
            {
                swordTrail = GetComponent<TrailRenderer>();
            }

            if (swordTrail != null)
            {
                // Отключить Trail по умолчанию
                swordTrail.enabled = false;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Активировать эффект атаки
        /// Вызывается из Animation Event или PlayerCombat
        /// </summary>
        public void PlayAttackEffect()
        {
            if (!_isInitialized)
            {
                InitializeComponents();
            }

            // Запустить Visual Effect Graph
            if (visualEffect != null && playOnAttackStart)
            {
                try
                {
                    // Остановить эффект, если он уже запущен (на случай предыдущей атаки)
                    visualEffect.Stop();
                    // Переинициализировать для чистого старта
                    visualEffect.Reinit();
                    // Запустить эффект через событие OnPlay
                    // Используем SendEvent для явного запуска события
                    // Если события нет, SendEvent просто не выполнит ничего, поэтому используем Play() как fallback
                    try
                    {
                        visualEffect.SendEvent("OnPlay");
                        Debug.Log("SwordAttackVFX: Visual Effect Graph запущен через событие OnPlay");
                    }
                    catch
                    {
                        // Если события нет, используем Play()
                        visualEffect.Play();
                        Debug.Log("SwordAttackVFX: Visual Effect Graph запущен через Play()");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SwordAttackVFX: Не удалось запустить Visual Effect: {e.Message}");
                }
            }
            else if (visualEffect == null)
            {
                Debug.LogWarning("SwordAttackVFX: visualEffect не назначен! Проверьте ссылку в Inspector.");
            }

            // Включить Trail Renderer
            if (swordTrail != null)
            {
                swordTrail.enabled = true;
                _isTrailActive = true;
            }
        }

        /// <summary>
        /// Остановить эффект атаки
        /// Вызывается из Animation Event или PlayerCombat
        /// </summary>
        public void StopAttackEffect()
        {
            // Остановить Visual Effect Graph
            if (visualEffect != null && stopOnAttackEnd)
            {
                try
                {
                    visualEffect.Stop();
                    Debug.Log("SwordAttackVFX: Visual Effect Graph остановлен");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SwordAttackVFX: Не удалось остановить Visual Effect: {e.Message}");
                }
            }

            // Отключить Trail Renderer с задержкой
            if (swordTrail != null && _isTrailActive)
            {
                StartCoroutine(DisableTrailAfterDelay());
            }
        }

        /// <summary>
        /// Отключить Trail после завершения анимации
        /// </summary>
        private IEnumerator DisableTrailAfterDelay()
        {
            yield return new WaitForSeconds(trailDuration);
            
            if (swordTrail != null)
            {
                swordTrail.enabled = false;
                _isTrailActive = false;
            }
        }

        /// <summary>
        /// Установить цвет эффекта (для разных типов атак)
        /// </summary>
        public void SetEffectColor(Color color)
        {
            if (!_isInitialized)
            {
                InitializeComponents();
            }

            // Установить цвет для Visual Effect Graph (если есть параметр "Color")
            if (visualEffect != null)
            {
                try
                {
                    // Попробуем разные варианты названий параметра цвета
                    if (visualEffect.HasVector4("Color"))
                    {
                        visualEffect.SetVector4("Color", new Vector4(color.r, color.g, color.b, color.a));
                    }
                    else if (visualEffect.HasVector3("Color"))
                    {
                        visualEffect.SetVector3("Color", new Vector3(color.r, color.g, color.b));
                    }
                    else if (visualEffect.HasFloat("ColorR") && visualEffect.HasFloat("ColorG") && visualEffect.HasFloat("ColorB"))
                    {
                        visualEffect.SetFloat("ColorR", color.r);
                        visualEffect.SetFloat("ColorG", color.g);
                        visualEffect.SetFloat("ColorB", color.b);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SwordAttackVFX: Не удалось установить цвет Visual Effect: {e.Message}. Убедитесь, что в VFX Graph есть параметр 'Color'.");
                }
            }

            // Установить цвет для Trail Renderer
            if (swordTrail != null)
            {
                swordTrail.startColor = color;
                swordTrail.endColor = new Color(color.r, color.g, color.b, 0f);
            }
        }

        /// <summary>
        /// Установить интенсивность эффекта (через параметр VFX Graph)
        /// </summary>
        public void SetIntensity(float intensity)
        {
            if (!_isInitialized)
            {
                InitializeComponents();
            }

            if (visualEffect != null)
            {
                try
                {
                    // Попробуем установить параметр интенсивности
                    if (visualEffect.HasFloat("Intensity"))
                    {
                        visualEffect.SetFloat("Intensity", intensity);
                    }
                    else if (visualEffect.HasFloat("ParticleCount"))
                    {
                        visualEffect.SetFloat("ParticleCount", intensity * 30f);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SwordAttackVFX: Не удалось установить интенсивность: {e.Message}. Добавьте параметр 'Intensity' в VFX Graph.");
                }
            }
        }

        /// <summary>
        /// Установить цвет эффекта в зависимости от индекса комбо
        /// </summary>
        public void SetComboColor(int comboIndex)
        {
            Color[] comboColors = {
                Color.white,           // 1-й удар
                Color.yellow,           // 2-й удар
                Color.red              // 3-й удар
            };

            int colorIndex = Mathf.Clamp(comboIndex, 0, comboColors.Length - 1);
            SetEffectColor(comboColors[colorIndex]);
        }
    }
}
