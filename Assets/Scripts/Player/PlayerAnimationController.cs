using UnityEngine;

namespace RPG.Player
{
    /// <summary>
    /// Управляет анимациями персонажа.
    /// Отделяет логику анимаций от логики геймплея.
    /// Синхронизирует параметры Animator с состоянием персонажа.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator _animator;

        // Хэши параметров для оптимизации
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        // Параметры сглаживания анимации (в секундах)
        [Header("Animation Smoothing")]
        [SerializeField] private float speedDampTime = 0.1f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Обновить параметр скорости движения с сглаживанием
        /// </summary>
        public void SetSpeed(float speed)
        {
            _animator.SetFloat(SpeedHash, speed, speedDampTime, Time.deltaTime);
        }

        /// <summary>
        /// Установить параметр движения
        /// </summary>
        public void SetMoving(bool isMoving)
        {
            _animator.SetBool(IsMovingHash, isMoving);
        }

        /// <summary>
        /// Триггер атаки
        /// </summary>
        public void TriggerAttack()
        {
            _animator.SetTrigger(AttackHash);
        }

        /// <summary>
        /// Установить индекс комбо-атаки
        /// </summary>
        public void SetComboIndex(int index)
        {
            _animator.SetInteger(ComboIndexHash, index);
        }

        /// <summary>
        /// Сброс триггера атаки
        /// </summary>
        public void ResetAttackTrigger()
        {
            _animator.ResetTrigger(AttackHash);
        }

        /// <summary>
        /// Проверка, проигрывается ли анимация атаки
        /// </summary>
        public bool IsInAttackAnimation()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsTag("Attack");
        }

        /// <summary>
        /// Получить текущий прогресс анимации (0-1)
        /// </summary>
        public float GetCurrentAnimationProgress()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            // Используем normalizedTime % 1.0f для получения прогресса в диапазоне 0-1
            // даже если анимация зациклена или повторяется
            return stateInfo.normalizedTime % 1.0f;
        }

        /// <summary>
        /// Получить прогресс анимации атаки (0-1), если сейчас проигрывается атака
        /// </summary>
        public float GetAttackAnimationProgress()
        {
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
            return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
        }
    }
}
