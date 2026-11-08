// HoldPlate: a pressure plate that is active only while a living actor stands on it (and, if it has
// a generator, only while the room is powered). Unlike PressButton it does not latch, so a clone has
// to keep standing on it. Chamber 3's hover stairs read IsActive from three of these, the hydraulic
// block from its pedal, and SignalCable shows it through ISignalSource.
using System;
using GameJam.Actors;
using GameJam.Core;
using GameJam.Machines;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>A plate that stays down only while someone stands on it.</summary>
    public class HoldPlate : MonoBehaviour, IResettable, ISignalSource
    {
        [Header("References")]
        [Tooltip("The plate's area; anyone inside counts as standing on it.")]
        [SerializeField] private ActorZone _zone;
        [Tooltip("Optional: the plate does nothing while this generator is unpowered.")]
        [SerializeField] private PowerGenerator _power;
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Visuals")]
        [SerializeField] private Color _unpoweredColor = new Color(0.25f, 0.25f, 0.28f);
        [SerializeField] private Color _idleColor = new Color(0.55f, 0.75f, 0.8f);
        [SerializeField] private Color _activeColor = new Color(0.3f, 1f, 0.55f);

        [Header("Events")]
        [SerializeField] private UnityEvent _onActivated = new UnityEvent();
        [SerializeField] private UnityEvent _onReleased = new UnityEvent();

        /// <summary>True while someone stands on the plate and it has power.</summary>
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public bool IsOn => IsActive;

        /// <summary>Raised when the plate goes down (true) or comes back up (false).</summary>
        public event Action<bool> ActiveChanged;

        private bool IsPowered => _power == null || _power.IsPowered;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Lifts the plate for the next attempt.</summary>
        public void ResetState()
        {
            IsActive = false;
            ApplyVisual();
        }

        private void FixedUpdate()
        {
            bool active = IsPowered && _zone.HasLivingActor;
            if (active != IsActive)
            {
                IsActive = active;
                ActiveChanged?.Invoke(active);
                (active ? _onActivated : _onReleased).Invoke();
            }

            // Power can change without the plate changing, so the colour is refreshed every tick.
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            _renderer.color = !IsPowered ? _unpoweredColor : IsActive ? _activeColor : _idleColor;
        }
    }
}
