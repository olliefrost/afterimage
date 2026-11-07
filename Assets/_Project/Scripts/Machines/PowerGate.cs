// PowerGate: passes an inspector event on only while the room is powered. Chamber 3's exit uses it
// between the door trigger and Door.Open, so the door stays shut until the generator runs.
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Machines
{
    /// <summary>Forwards <see cref="Pass"/> to its event only while the generator has power.</summary>
    public class PowerGate : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PowerGenerator _generator;

        [Header("Events")]
        [Tooltip("Fired by Pass() while the room is powered.")]
        [SerializeField] private UnityEvent _onPassed = new UnityEvent();

        /// <summary>Fires the event if the room has power; does nothing otherwise.</summary>
        public void Pass()
        {
            if (_generator.IsPowered)
            {
                _onPassed.Invoke();
            }
        }
    }
}
