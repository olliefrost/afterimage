// LaserBeamVisuals: draws a laser's shot as pixel-art beam segments laid end to end from the barrel
// to whatever stopped it, with an impact burst at the far end. Each segment sprite is one half-cell
// step along the 2:1 iso diagonal, so the beam tiles pixel-exactly with no rotated pixels. Frames
// play across the beam ticks (a hot blast fading to a thin trace). Sits on the Laser prefab's Beam
// child and reads LaserEmitter; visual only. A beam lifted onto a raised level can sort each piece on
// the floor plan (_floorPlanDrop), so characters in front of or behind the beam overlap it correctly.
using GameJam.Hazards;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameJam.Visuals
{
    /// <summary>Lays out and animates the beam and impact sprites while the emitter is firing.</summary>
    public class LaserBeamVisuals : MonoBehaviour
    {
        // One segment spans a 32x16 px step of the iso diagonal at 64 PPU: (0.5, 0.25) world units.
        private static readonly Vector2 SegmentStep = new Vector2(0.5f, 0.25f);

        [Header("References")]
        [SerializeField] private LaserEmitter _emitter;
        [Tooltip("Shows the burst where the beam stops.")]
        [SerializeField] private SpriteRenderer _impact;

        [Header("Frames")]
        [Tooltip("Beam segment frames, drawn running up-right; played across the beam ticks.")]
        [SerializeField] private Sprite[] _beamFrames;
        [Tooltip("Impact burst frames, played across the beam ticks.")]
        [SerializeField] private Sprite[] _impactFrames;

        [Header("Look")]
        [Tooltip("Most segments a beam can use; longer beams are cut short. 48 covers a 26-unit beam.")]
        [SerializeField] private int _maxSegments = 48;
        [SerializeField] private string _sortingLayer = "Default";
        [Tooltip("Draws over floors and characters, like the old line beam.")]
        [SerializeField] private int _sortingOrder = 5;
        [Tooltip("Unlit, so the beam glows in the dark room.")]
        [SerializeField] private Material _material;
        [Tooltip("0 = sort the beam as drawn. Above 0: the beam is drawn this far above the floor, and each piece sorts with characters at its floor-plan point instead (outside any SortingGroup the laser sits in).")]
        [Min(0f)]
        [SerializeField] private float _floorPlanDrop;

        private SpriteRenderer[] _segments;
        private Transform[] _anchors;
        private Transform _impactAnchor;
        private Transform _floorRoot;
        private bool _shown;

        private void Awake()
        {
            // Build the segment pool once, so firing never allocates.
            _segments = new SpriteRenderer[_maxSegments];
            _anchors = new Transform[_maxSegments];
            for (int i = 0; i < _maxSegments; i++)
            {
                var segment = new GameObject("Segment").AddComponent<SpriteRenderer>();
                _anchors[i] = Anchor(segment.transform, "SegmentAnchor");
                segment.sharedMaterial = _material;
                segment.sortingLayerName = _sortingLayer;
                segment.sortingOrder = _sortingOrder;
                segment.enabled = false;
                _segments[i] = segment;
            }

            _impact.sortingOrder = _sortingOrder + 1;
            _impactAnchor = Anchor(_impact.transform, "ImpactAnchor");
            _impact.enabled = false;
        }

        private void OnDisable()
        {
            // Floor-plan pieces sit outside this object, so they would stay lit if it switched off mid-shot.
            // On a scene unload Unity may destroy them first; then there is nothing left to hide.
            if (_shown && (_floorPlanDrop <= 0f || _floorRoot != null))
            {
                Hide();
            }
        }

        private void OnDestroy()
        {
            // The floor-plan pieces live outside this object, so take them with it.
            if (_floorRoot != null)
            {
                Destroy(_floorRoot.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (_emitter.Phase != LaserPhase.Firing)
            {
                if (_shown)
                {
                    Hide();
                }

                return;
            }

            Show(_emitter.PhaseProgress);
        }

        /// <summary>Places enough segments to cover the beam and shows this moment's frame on each.</summary>
        private void Show(float progress)
        {
            Vector2 start = _emitter.BeamStart;
            Vector2 end = _emitter.BeamEnd;
            Vector2 delta = end - start;

            // 1. Mirror the up-right art for beams heading left, and step along the matching diagonal.
            bool left = delta.x < 0f;
            Vector2 step = new Vector2(left ? -SegmentStep.x : SegmentStep.x, delta.y < 0f ? -SegmentStep.y : SegmentStep.y);
            int count = Mathf.Clamp(Mathf.CeilToInt(delta.magnitude / step.magnitude), 0, _segments.Length);

            // 2. Lay segments centre to centre from the barrel; the last may overrun slightly, under the impact.
            Sprite beam = Pick(_beamFrames, progress);
            for (int i = 0; i < _segments.Length; i++)
            {
                SpriteRenderer segment = _segments[i];
                bool on = i < count;
                segment.enabled = on;
                if (on)
                {
                    segment.sprite = beam;
                    segment.flipX = left;
                    Place(_anchors[i], segment.transform, start + step * (i + 0.5f));
                }
            }

            // 3. Burst where the beam stopped.
            _impact.sprite = Pick(_impactFrames, progress);
            Place(_impactAnchor, _impact.transform, end);
            _impact.enabled = true;
            _shown = true;
        }

        /// <summary>
        /// Gives a beam sprite the object it sorts by: itself, or (with a floor-plan drop) a SortingGroup
        /// parent that sits on the floor plan under it.
        /// </summary>
        private Transform Anchor(Transform sprite, string name)
        {
            if (_floorPlanDrop <= 0f)
            {
                sprite.SetParent(transform, false);
                return sprite;
            }

            if (_floorRoot == null)
            {
                // A scene-root holder, so the pieces sort with characters rather than inside the laser's group.
                _floorRoot = new GameObject(gameObject.name + " (beam on floor plan)").transform;
            }

            var anchor = new GameObject(name).transform;
            anchor.SetParent(_floorRoot, false);
            anchor.gameObject.AddComponent<SortingGroup>().sortingLayerName = _sortingLayer;
            sprite.SetParent(anchor, false);
            sprite.localPosition = new Vector3(0f, _floorPlanDrop, 0f);
            return anchor;
        }

        /// <summary>Puts a beam sprite at <paramref name="drawnAt"/>, moving its anchor to the floor plan beneath it.</summary>
        private void Place(Transform anchor, Transform sprite, Vector2 drawnAt)
        {
            if (anchor == sprite)
            {
                sprite.position = drawnAt;
                return;
            }

            anchor.position = drawnAt - new Vector2(0f, _floorPlanDrop);
        }

        /// <summary>Hides every beam sprite once the shot is over.</summary>
        private void Hide()
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                _segments[i].enabled = false;
            }

            _impact.enabled = false;
            _shown = false;
        }

        /// <summary>Picks the frame a fraction of the way through a list, so playback follows the ticks.</summary>
        private static Sprite Pick(Sprite[] frames, float progress)
        {
            int index = Mathf.Min(frames.Length - 1, Mathf.FloorToInt(progress * frames.Length));
            return frames[index];
        }
    }
}
