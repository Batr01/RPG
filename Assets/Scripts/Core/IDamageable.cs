namespace RPG.Core
{
    /// <summary>
    /// Интерфейс для всех объектов, которые могут получать урон.
    /// Будет использоваться в будущем для системы здоровья.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Получить урон
        /// </summary>
        /// <param name="damage">Количество урона</param>
        void TakeDamage(float damage);

        /// <summary>
        /// Текущее здоровье
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Максимальное здоровье
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Проверка, жив ли объект
        /// </summary>
        bool IsAlive { get; }
    }
}
