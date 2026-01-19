using UnityEngine;

namespace RPG.Core
{
    /// <summary>
    /// Интерфейс для всех движущихся объектов в игре.
    /// Определяет контракт для управления движением.
    /// Следуя принципу Interface Segregation (SOLID), 
    /// интерфейс содержит только методы, связанные с движением.
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Переместить объект в указанном направлении
        /// </summary>
        /// <param name="direction">Нормализованный вектор направления движения</param>
        void Move(Vector2 direction);

        /// <summary>
        /// Скорость движения объекта
        /// </summary>
        float MoveSpeed { get; set; }

        /// <summary>
        /// Проверка, движется ли объект в данный момент
        /// </summary>
        bool IsMoving { get; }
    }
}
