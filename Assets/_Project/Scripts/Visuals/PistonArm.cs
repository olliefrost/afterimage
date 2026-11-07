// PistonArm: draws the arm of Chamber 3's hydraulic piston as a line from its fixed base to the
// moving block, so the block reads as pushed out by a mechanism. Visual only: HydraulicBlock moves the block.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Stretches a line between a piston's base and its block every frame.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public class PistonArm : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Where the arm leaves the base.")]
        [SerializeField] private Transform _base;
        [Tooltip("The block the arm pushes.")]
        [SerializeField] private Transform _block;
        [Tooltip("Raises the arm off the floor to the middle of the block.")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.2f, 0f);

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.positionCount = 2;
        }

        private void LateUpdate()
        {
            _line.SetPosition(0, _base.position + _offset);
            _line.SetPosition(1, _block.position + _offset);
        }
    }
}
