namespace RPG.Core
{
    /// <summary>
    /// Глобальные константы игры.
    /// Централизованное хранение игровых параметров для удобной настройки.
    /// </summary>
    public static class GameConstants
    {
        #region Player Constants
        
        /// <summary>
        /// Тег игрока для поиска через GameObject.FindGameObjectWithTag
        /// </summary>
        public const string PLAYER_TAG = "Player";
        
        /// <summary>
        /// Слой игрока
        /// </summary>
        public const string PLAYER_LAYER = "Player";
        
        #endregion

        #region Combat Constants
        
        /// <summary>
        /// Максимальное количество ударов в комбо по умолчанию
        /// </summary>
        public const int DEFAULT_MAX_COMBO = 3;
        
        /// <summary>
        /// Время сброса комбо по умолчанию (секунды)
        /// </summary>
        public const float DEFAULT_COMBO_RESET_TIME = 1.0f;
        
        /// <summary>
        /// Тег анимации атаки для Animator
        /// </summary>
        public const string ATTACK_ANIMATION_TAG = "Attack";
        
        #endregion

        #region Animation Parameters
        
        /// <summary>
        /// Имена параметров Animator (для хэширования)
        /// </summary>
        public static class AnimatorParameters
        {
            public const string SPEED = "Speed";
            public const string IS_MOVING = "IsMoving";
            public const string ATTACK = "Attack";
            public const string COMBO_INDEX = "ComboIndex";
            public const string IS_DEAD = "IsDead";
            public const string TAKE_DAMAGE = "TakeDamage";
            public const string HORIZONTAL = "Horizontal";
            public const string VERTICAL = "Vertical";
        }
        
        #endregion

        #region Input Constants
        
        /// <summary>
        /// Мертвая зона для джойстика (минимальное значение для регистрации ввода)
        /// </summary>
        public const float JOYSTICK_DEAD_ZONE = 0.1f;
        
        #endregion

        #region Camera Constants
        
        /// <summary>
        /// Угол камеры для top-down вида по умолчанию
        /// </summary>
        public const float DEFAULT_CAMERA_ANGLE = 50f;
        
        /// <summary>
        /// Время сглаживания следования камеры
        /// </summary>
        public const float DEFAULT_CAMERA_SMOOTH_TIME = 0.2f;
        
        #endregion

        #region Movement Constants
        
        /// <summary>
        /// Скорость движения игрока по умолчанию
        /// </summary>
        public const float DEFAULT_MOVE_SPEED = 5f;
        
        /// <summary>
        /// Скорость поворота игрока по умолчанию
        /// </summary>
        public const float DEFAULT_ROTATION_SPEED = 10f;
        
        /// <summary>
        /// Значение гравитации
        /// </summary>
        public const float GRAVITY = -9.81f;
        
        #endregion

        #region UI Constants
        
        /// <summary>
        /// Время анимации UI (секунды)
        /// </summary>
        public const float UI_ANIMATION_DURATION = 0.2f;
        
        /// <summary>
        /// Альфа-канал для активного UI
        /// </summary>
        public const float UI_ACTIVE_ALPHA = 1f;
        
        /// <summary>
        /// Альфа-канал для неактивного UI
        /// </summary>
        public const float UI_INACTIVE_ALPHA = 0.5f;
        
        #endregion

        #region Scene Names
        
        /// <summary>
        /// Имена сцен в проекте
        /// </summary>
        public static class Scenes
        {
            public const string MAIN_MENU = "MainMenu";
            public const string GAME = "Game";
            public const string LOADING = "Loading";
        }
        
        #endregion

        #region Layers
        
        /// <summary>
        /// ID слоев Unity
        /// </summary>
        public static class Layers
        {
            public const int DEFAULT = 0;
            public const int PLAYER = 8;
            public const int ENEMY = 9;
            public const int GROUND = 10;
            public const int INTERACTABLE = 11;
        }
        
        #endregion
    }
}
