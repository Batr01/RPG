using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RPG.UI
{
    /// <summary>
    /// Виртуальный джойстик для мобильных устройств.
    /// Обрабатывает касания и передает направление движения в InputManager.
    /// </summary>
    public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Settings")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 50f;
        [SerializeField] private float deadZone = 0.1f;

        [Header("Visual Settings")]
        [SerializeField] private bool returnToCenter = true;
        [SerializeField] private float returnSpeed = 10f;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float activeAlpha = 1f;
        [SerializeField] private float inactiveAlpha = 0.5f;

        private Vector2 _inputDirection = Vector2.zero;
        private Vector2 _joystickCenter;
        private bool _isDragging = false;

        private void Start()
        {
            // Настройка прозрачности
            if (canvasGroup != null)
            {
                canvasGroup.alpha = inactiveAlpha;
            }
        }

        private void Update()
        {
            // Возврат джойстика в центр, когда он не используется
            if (!_isDragging && returnToCenter)
            {
                joystickHandle.anchoredPosition = Vector2.Lerp(
                    joystickHandle.anchoredPosition,
                    Vector2.zero,
                    returnSpeed * Time.deltaTime
                );

                // Обнуление направления при достижении центра
                if (joystickHandle.anchoredPosition.magnitude < 1f)
                {
                    joystickHandle.anchoredPosition = Vector2.zero;
                    _inputDirection = Vector2.zero;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            
            // Визуальный feedback
            if (canvasGroup != null)
            {
                canvasGroup.alpha = activeAlpha;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Получение позиции касания относительно джойстика
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            // Ограничение движения ручки джойстика
            Vector2 direction = localPoint;
            float magnitude = direction.magnitude;
            
            if (magnitude > handleRange)
            {
                direction = direction.normalized * handleRange;
            }

            // Установка позиции ручки
            joystickHandle.anchoredPosition = direction;

            // Вычисление нормализованного направления
            _inputDirection = direction / handleRange;

            // Применение мертвой зоны
            if (_inputDirection.magnitude < deadZone)
            {
                _inputDirection = Vector2.zero;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            
            // Визуальный feedback
            if (canvasGroup != null)
            {
                canvasGroup.alpha = inactiveAlpha;
            }

            if (returnToCenter)
            {
                _inputDirection = Vector2.zero;
            }
        }

        /// <summary>
        /// Получить текущее направление ввода
        /// </summary>
        public Vector2 GetInputDirection()
        {
            return _inputDirection;
        }

        /// <summary>
        /// Получить нормализованное направление
        /// </summary>
        public Vector2 GetNormalizedDirection()
        {
            return _inputDirection.magnitude > deadZone ? _inputDirection.normalized : Vector2.zero;
        }
    }
}
