namespace RPG.Core
{
    /// <summary>
    /// Интерфейс для всех объектов, способных атаковать.
    /// Определяет контракт для боевой системы.
    /// Следуя принципу Interface Segregation (SOLID),
    /// интерфейс содержит только методы, связанные с атакой.
    /// </summary>
    public interface IAttackable
    {
        /// <summary>
        /// Выполнить атаку
        /// </summary>
        void Attack();

        /// <summary>
        /// Проверка, может ли объект атаковать в данный момент
        /// </summary>
        bool CanAttack { get; }

        /// <summary>
        /// Проверка, находится ли объект в процессе атаки
        /// </summary>
        bool IsAttacking { get; }
    }
}
