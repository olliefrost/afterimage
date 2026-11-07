// CloneIconView: one clone icon in the HUD's right-hand column. Shows the clone's colour and number
// key, a faint ghost look for the empty new-clone slot, a framed row and pointer for the clone the
// player is controlling, a blinking outline for the clone picked to control next, and a dead look
// (shake, then red with a skull). Clicking it picks that clone. Driven by CloneColumn; visual only.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>A single clone icon: tinted body and glow, key badge, controlled frame and pointer, pick outline and dead skull.</summary>
    public class CloneIconView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Child that holds every visual, so it can shake without fighting the layout group.")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _body;
        [SerializeField] private Image _glow;
        [SerializeField] private Image _outline;
        [SerializeField] private Image _skull;
        [Tooltip("Padlock shown on the slot just past the chamber's clone limit.")]
        [SerializeField] private Image _lock;
        [SerializeField] private Text _keyLabel;
        [SerializeField] private Button _button;
        [Tooltip("Framed panel behind the whole row, shown on the controlled clone.")]
        [SerializeField] private Image _rowFrame;
        [Tooltip("Arrow beside the row, shown on the controlled clone.")]
        [SerializeField] private Image _pointer;

        [Header("Look")]
        [SerializeField] private Color _deadColour = new Color(0.85f, 0.12f, 0.1f);
        [Tooltip("Glow opacity behind a normal icon.")]
        [Range(0f, 1f)]
        [SerializeField] private float _glowAlpha = 0.35f;
        [Tooltip("Opacity of the empty new-clone slot's icon.")]
        [Range(0f, 1f)]
        [SerializeField] private float _ghostAlpha = 0.3f;
        [Tooltip("Body colour of a locked slot (past the chamber's clone limit).")]
        [SerializeField] private Color _lockedColour = new Color(0.5f, 0.55f, 0.6f, 0.25f);

        [Header("Controlled")]
        [SerializeField] private float _pulseSpeed = 4f;
        [Tooltip("How far the pointer bobs sideways, in canvas pixels.")]
        [SerializeField] private float _pointerBob = 5f;

        [Header("Picked next")]
        [Tooltip("Outline blinks per second while this clone is picked to control next.")]
        [SerializeField] private float _blinkRate = 3f;

        [Header("Death shake")]
        [SerializeField] private float _shakeDuration = 0.4f;
        [Tooltip("Furthest the icon moves sideways while shaking, in canvas pixels.")]
        [SerializeField] private float _shakeDistance = 6f;
        [SerializeField] private float _shakeSpeed = 70f;

        private int _slot;
        private Color _slotColour = Color.white;
        private Color _bodyColour;
        private bool _isControlled;
        private bool _isPending;
        private float _shakeTimeLeft;
        private Vector2 _pointerRest;

        /// <summary>Raised with this icon's slot index when the player clicks it.</summary>
        public event Action<int> Clicked;

        private void Awake()
        {
            _pointerRest = _pointer.rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClicked);
            _shakeTimeLeft = 0f;
            _content.anchoredPosition = Vector2.zero;
        }

        private void Update()
        {
            // Purely visual, so unscaled real time is fine here.
            float time = Time.unscaledTime;

            // 1. Controlled: the glow breathes and the pointer bobs towards the icon.
            if (_isControlled)
            {
                float wave = 0.5f + 0.5f * Mathf.Sin(time * _pulseSpeed);
                Color glow = _bodyColour;
                glow.a = Mathf.Lerp(_glowAlpha, 1f, wave);
                _glow.color = glow;
                _pointer.rectTransform.anchoredPosition = _pointerRest + new Vector2(wave * _pointerBob, 0f);
            }

            // 2. Picked next: the outline blinks on and off.
            if (_isPending)
            {
                _outline.enabled = Mathf.Repeat(time * _blinkRate, 1f) < 0.6f;
            }

            // 3. Just died: a short sideways shake that settles.
            float offset = 0f;
            if (_shakeTimeLeft > 0f)
            {
                _shakeTimeLeft -= Time.unscaledDeltaTime;
                float fade = Mathf.Clamp01(_shakeTimeLeft / _shakeDuration);
                offset = Mathf.Sin(time * _shakeSpeed) * _shakeDistance * fade;
            }

            _content.anchoredPosition = new Vector2(offset, 0f);
        }

        /// <summary>Binds the icon to a slot and its colour.</summary>
        public void Init(int slot, Color slotColour)
        {
            _slot = slot;
            _slotColour = slotColour;
        }

        /// <summary>Shows the key that picks this clone on the badge (1-5 unless the player rebinds it).</summary>
        public void SetKeyLabel(string key)
        {
            _keyLabel.text = key;
        }

        /// <summary>
        /// Shows the icon's current look. A saved clone is solid (red with a skull if dead); the empty
        /// new-clone slot is a faint ghost. The controlled and picked-next markers sit on top.
        /// </summary>
        public void Show(bool isSaved, bool isDead, bool isControlled, bool isPending)
        {
            _isControlled = isControlled;
            _isPending = isPending;

            // 1. Body: slot colour, red when dead, faint for an empty slot.
            _bodyColour = isDead ? _deadColour : _slotColour;
            Color body = _bodyColour;
            body.a = isSaved ? 1f : _ghostAlpha;
            _body.color = body;
            _skull.enabled = isDead;
            _lock.enabled = false;

            Color glow = _bodyColour;
            glow.a = isSaved ? _glowAlpha : _glowAlpha * _ghostAlpha;
            _glow.color = glow;

            // 2. Controlled: framed row and pointer in the slot colour.
            _rowFrame.enabled = isControlled;
            _rowFrame.color = _slotColour;
            _pointer.gameObject.SetActive(isControlled);
            _pointer.color = _slotColour;

            // 3. Picked next: outline (blinking in Update).
            _outline.enabled = isPending;
            _outline.color = Color.Lerp(_slotColour, Color.white, 0.5f);
        }

        /// <summary>
        /// Shows the slot as locked: a faint grey body with a padlock and no markers, telling the
        /// player this chamber allows no more clones.
        /// </summary>
        public void ShowLocked()
        {
            _isControlled = false;
            _isPending = false;
            _body.color = _lockedColour;
            _glow.color = Color.clear;
            _skull.enabled = false;
            _lock.enabled = true;
            _rowFrame.enabled = false;
            _pointer.gameObject.SetActive(false);
            _outline.enabled = false;
        }

        /// <summary>Shakes the icon briefly, used the moment its clone dies.</summary>
        public void Shake()
        {
            _shakeTimeLeft = _shakeDuration;
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(_slot);
        }
    }
}
