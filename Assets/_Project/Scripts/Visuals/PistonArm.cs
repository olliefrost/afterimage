// PistonArm: draws the ram of Chamber 3's hydraulic piston between its fixed base and the moving
// block, so the block reads as pushed out by a mechanism. The ram is a row of pixel-art segments
// (one half-cell step each, so they tile exactly along the iso grid); only as many are shown as fit
// between the base and the block. Visual only: HydraulicBlock moves the block.
using System.Collections.Generic;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Extends a segmented ram from a piston's base to its block every frame.</summary>
    public class PistonArm : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Where the ram leaves the base.")]
        [SerializeField] private Transform _base;
        [Tooltip("The block the ram pushes.")]
        [SerializeField] private Transform _block;
        [Tooltip("One ram segment, drawn for exactly one half-cell step along the ram's direction.")]
        [SerializeField] private Sprite _segment;
        [Tooltip("Material for the segments (the lit sprite material, like the rest of the room).")]
        [SerializeField] private Material _material;

        [Header("Shape")]
        [Tooltip("Raises the ram off the floor to the middle of the block.")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.2f, 0f);
        [Tooltip("World length of one segment: a half-cell step along an iso axis is 0.2795 (sqrt(0.25² + 0.125²)).")]
        [SerializeField] private float _segmentLength = 0.2795f;
        [Tooltip("Most segments the ram can show (its longest reach).")]
        [SerializeField] private int _maxSegments = 12;

        private readonly List<SpriteRenderer> _segments = new List<SpriteRenderer>();

        private void Awake()
        {
            // Build the pool once; LateUpdate only moves and toggles them.
            for (int i = 0; i < _maxSegments; i++)
            {
                GameObject piece = new GameObject("RamSegment_" + i);
                piece.transform.SetParent(transform, false);
                SpriteRenderer sprite = piece.AddComponent<SpriteRenderer>();
                sprite.sprite = _segment;
                sprite.sharedMaterial = _material;
                sprite.spriteSortPoint = SpriteSortPoint.Pivot;
                sprite.enabled = false;
                _segments.Add(sprite);
            }
        }

        private void LateUpdate()
        {
            // 1. Lay segments from the base towards the block, one step apart.
            Vector3 start = _base.position + _offset;
            Vector3 toBlock = _block.position + _offset - start;
            float length = toBlock.magnitude;
            Vector3 direction = length > 0.0001f ? toBlock / length : Vector3.zero;
            int shown = Mathf.Min(_segments.Count, Mathf.FloorToInt(length / _segmentLength));

            // 2. Show only the segments that fit; the rest stay hidden inside the base.
            for (int i = 0; i < _segments.Count; i++)
            {
                bool visible = i < shown;
                _segments[i].enabled = visible;
                if (visible)
                {
                    _segments[i].transform.position = start + direction * (_segmentLength * (i + 0.5f));
                }
            }
        }
    }
}
