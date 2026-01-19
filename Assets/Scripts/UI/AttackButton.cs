using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RPG.UI
{
    /// <summary>
    /// Кнопка атаки для мобильных устройств.
    /// Отображает текущий индекс комбо и обрабатывает нажатия.
    /// </summary>
    public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Button Settings")]
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;

        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.gray;
        [SerializeField] private float pressScale = 0.9f;

        [Header("Combo Display")]
        [SerializeField] private bool showComboCounter = true;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private Image comboFillImage;

        private Vector3 _originalScale;
        private bool _isPressed = false;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (buttonImage == null)
                buttonImage = GetComponent<Image>();

            _originalScale = transform.localScale;

            // Настройка начального состояния
            if (buttonImage != null)
                buttonImage.color = normalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            
            // Визуальный feedback
            if (buttonImage != null)
                buttonImage.color = pressedColor;
            
            transform.localScale = _originalScale * pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            
            // Возврат к нормальному состоянию
            if (buttonImage != null)
                buttonImage.color = normalColor;
            
            transform.localScale = _originalScale;
        }

        /// <summary>
        /// Обновить отображение комбо
        /// </summary>
        public void UpdateComboDisplay(int currentCombo, int maxCombo)
        {
            if (!showComboCounter) return;

            // Обновление текста
            if (comboText != null)
            {
                comboText.text = $"{currentCombo + 1}/{maxCombo}";
            }

            // Обновление прогресс-бара
            if (comboFillImage != null)
            {
                float fillAmount = (float)(currentCombo + 1) / maxCombo;
                comboFillImage.fillAmount = fillAmount;
            }
        }

        /// <summary>
        /// Показать/скрыть счетчик комбо
        /// </summary>
        public void SetComboCounterVisible(bool visible)
        {
            showComboCounter = visible;
            
            if (comboText != null)
                comboText.gameObject.SetActive(visible);
            
            if (comboFillImage != null)
                comboFillImage.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Анимация пульсации при успешном комбо
        /// </summary>
        public void PlayComboPulse()
        {
            // Можно добавить анимацию через Animator или DOTween
            StartCoroutine(PulseAnimation());
        }

        private System.Collections.IEnumerator PulseAnimation()
        {
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 1.2f, elapsed / duration);
                transform.localScale = _originalScale * scale;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1.2f, 1f, elapsed / duration);
                transform.localScale = _originalScale * scale;
                yield return null;
            }

            transform.localScale = _originalScale;
        }
    }
}
