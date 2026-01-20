using UnityEngine;

namespace RPG.Enemy
{
    /// <summary>
    /// Управляет анимациями врага.
    /// Отделяет логику анимаций от логики геймплея.
    /// Синхронизирует параметры Animator с состоянием врага.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimationController : MonoBehaviour
    {
        private Animator _animator;

        // Хэши параметров для оптимизации (избегаем поиска по строке каждый кадр)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        [Header("Animation Smoothing")]
        [SerializeField] private float speedDampTime = 0.1f;

        #region Unity Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError($"Animator не найден на {gameObject.name}!");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Обновить параметр скорости движения с сглаживанием
        /// </summary>
        /// <param name="speed">Скорость (0-1 или абсолютное значение)</param>
        public void SetSpeed(float speed)
        {
            if (_animator == null) return;
            
            _animator.SetFloat(SpeedHash, speed, speedDampTime, Time.deltaTime);
        }

        /// <summary>
        /// Триггер атаки
        /// </summary>
        public void TriggerAttack()
        {
            if (_animator == null) return;
            
            _animator.SetTrigger(AttackHash);
        }

        /// <summary>
        /// Установить состояние смерти
        /// </summary>
        public void TriggerDeath()
        {
            if (_animator == null) return;
            
            _animator.SetBool(IsDeadHash, true);
        }

        /// <summary>
        /// Сбросить триггер атаки (для отмены)
        /// </summary>
        public void ResetAttackTrigger()
        {
            if (_animator == null) return;
            
            _animator.ResetTrigger(AttackHash);
        }

        /// <summary>
        /// Проверка, проигрывается ли анимация атаки
        /// </summary>
        public bool IsInAttackAnimation()
        {
            if (_animator == null) return false;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsTag("Attack");
        }

        /// <summary>
        /// Получить текущий прогресс анимации (0-1)
        /// </summary>
        public float GetCurrentAnimationProgress()
        {
            if (_animator == null) return 0f;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            // Используем % 1.0f для получения прогресса в диапазоне 0-1
            return stateInfo.normalizedTime % 1.0f;
        }

        /// <summary>
        /// Получить прогресс анимации атаки (0-1), если сейчас проигрывается атака
        /// </summary>
        public float GetAttackAnimationProgress()
        {
            if (_animator == null) return 0f;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsTag("Attack"))
            {
                return stateInfo.normalizedTime % 1.0f;
            }
            return 0f;
        }

        /// <summary>
        /// Проверка завершения текущей анимации
        /// </summary>
        public bool IsAnimationFinished()
        {
            if (_animator == null) return true;

            return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
        }

        /// <summary>
        /// Получить ссылку на Animator (для расширенного использования)
        /// </summary>
        public Animator GetAnimator()
        {
            return _animator;
        }

        #endregion
    }
}
