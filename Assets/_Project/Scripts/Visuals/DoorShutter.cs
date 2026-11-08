// DoorShutter: makes a door's panel roll up into the top of its frame like a shutter, so an open
// door still shows a door. SetRaised(1) is fully rolled up, 0 is down (closed). If the panel is a
// SpriteFlipbook of slide-up frames (the iso Door prefab), it scrubs those frames; a plain sprite
// panel instead squashes towards its top edge. IntroSequence raises the entrance door's shutter
// while the door stands open and drops it for the slam. Visual only; Door still owns the collider.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Rolls a door panel up towards its top edge, or back down.</summary>
    public class DoorShutter : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The door panel's transform (the Door prefab's Visual child).")]
        [SerializeField] private Transform _panel;

        [Header("Shape")]
        [Tooltip("Height of the panel's top edge above its pivot, in world units, at full size.")]
        [SerializeField] private float _topAbovePivot = 1.5f;
        [Tooltip("How much of the panel stays showing at the top when fully raised (0.15 = 15%).")]
        [Range(0f, 1f)]
        [SerializeField] private float _raisedLeftover = 0.15f;
        [Tooltip("Sorting order while the shutter is not fully down, so it shows over a lit doorway. Fully down it goes back to normal so characters in front still draw over it.")]
        [SerializeField] private int _movingSortingOrder = 2;

        private Vector3 _restPosition;
        private Vector3 _restScale;
        private SpriteRenderer _panelRenderer;
        private SpriteFlipbook _flipbook;
        private int _restSortingOrder;

        private void Awake()
        {
            _restPosition = _panel.localPosition;
            _restScale = _panel.localScale;
            _panelRenderer = _panel.GetComponent<SpriteRenderer>();
            _flipbook = _panel.GetComponent<SpriteFlipbook>();
            _restSortingOrder = _panelRenderer.sortingOrder;
        }

        /// <summary>Sets how far up the shutter is: 0 closed (full panel), 1 rolled up to the leftover strip.</summary>
        public void SetRaised(float amount)
        {
            // Frame-based shutter: its frames already slide up inside the frame and sort by position.
            if (_flipbook != null)
            {
                _flipbook.ShowAt(amount);
                return;
            }

            float height = Mathf.Lerp(1f, _raisedLeftover, Mathf.Clamp01(amount));

            // Squash the panel and lift it so its top edge does not move.
            _panel.localScale = new Vector3(_restScale.x, _restScale.y * height, _restScale.z);
            _panel.localPosition = _restPosition + new Vector3(0f, _topAbovePivot * (1f - height), 0f);
            _panelRenderer.sortingOrder = amount > 0f ? _movingSortingOrder : _restSortingOrder;
        }
    }
}
