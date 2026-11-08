// ButtonJuice: makes a menu button feel physical. It grows a little while hovered or selected with
// the keyboard, dips while held down and springs back on release. On the MenuButton prefab, so every
// menu button (title, pause, settings) gets it. Real time, so it works in the paused menu.
// Visual only: the Button component still does the clicking.
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameJam.UI
{
    /// <summary>Hover, select and press scale feedback for a uGUI button.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Scale")]
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressedScale = 0.94f;
        [Tooltip("How quickly it moves to the new size. Higher is snappier.")]
        [SerializeField] private float _sharpness = 18f;
        [Tooltip("Extra size for a moment on release, before settling (0.06 = 6%).")]
        [SerializeField] private float _releasePop = 0.06f;

        private RectTransform _rect;
        private bool _hovered;
        private bool _selected;
        private bool _pressed;
        private float _scale = 1f;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void OnDisable()
        {
            _hovered = false;
            _selected = false;
            _pressed = false;
            _scale = 1f;
            _rect.localScale = Vector3.one;
        }

        private void Update()
        {
            // Only write the scale while it is changing, so other UI animations (UiPopIn) can run.
            float target = _pressed ? _pressedScale : (_hovered || _selected ? _hoverScale : 1f);
            if (Mathf.Abs(_scale - target) < 0.001f)
            {
                return;
            }

            _scale = Mathf.Lerp(_scale, target, 1f - Mathf.Exp(-_sharpness * Time.unscaledDeltaTime));
            if (Mathf.Abs(_scale - target) < 0.001f)
            {
                _scale = target;
            }

            _rect.localScale = new Vector3(_scale, _scale, 1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            _scale += _releasePop;
        }

        public void OnSelect(BaseEventData eventData)
        {
            _selected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _selected = false;
        }
    }
}
