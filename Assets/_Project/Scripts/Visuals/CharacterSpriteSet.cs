// CharacterSpriteSet: data asset holding a character's 8-direction idle sprites, run and jump frames,
// plus climb frames (one set: on a ladder the character always faces the wall).
// CharacterSpriteAnimator reads it, so the Player and Clone prefabs share one asset. Build it from a
// PixelLab character folder with Assets > Create > GameJam > Character Sprite Set From Folder.
using System;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Idle sprite and run frames for one facing direction.</summary>
    [Serializable]
    public struct DirectionSprites
    {
        [Tooltip("Shown while standing still facing this way.")]
        public Sprite Idle;
        [Tooltip("Looped while moving this way. Leave empty to show Idle with a walk bob instead.")]
        public Sprite[] Run;

        [Tooltip("Played once across a jump (Chamber 4 onward). Leave empty to show Idle on the arc.")]
        public Sprite[] Jump;
    }

    /// <summary>Eight facing directions, counter-clockwise from east: E, NE, N, NW, W, SW, S, SE.</summary>
    [CreateAssetMenu(menuName = "GameJam/Character Sprite Set", fileName = "CharacterSpriteSet")]
    public class CharacterSpriteSet : ScriptableObject
    {
        public const int DirectionCount = 8;

        /// <summary>Direction names in index order, matching PixelLab's folder names.</summary>
        public static readonly string[] DirectionNames =
        {
            "east", "north-east", "north", "north-west", "west", "south-west", "south", "south-east",
        };

        [Tooltip("Index order: E, NE, N, NW, W, SW, S, SE.")]
        [SerializeField] private DirectionSprites[] _directions = new DirectionSprites[DirectionCount];

        [Tooltip("Run frames shown per second.")]
        [SerializeField] private float _runFramesPerSecond = 10f;

        [Tooltip("Looped while climbing a ladder, facing the wall. Leave empty to show the idle sprite with a bob.")]
        [SerializeField] private Sprite[] _climb;

        [Tooltip("Climb frames shown per second.")]
        [SerializeField] private float _climbFramesPerSecond = 8f;

        public float RunFramesPerSecond => _runFramesPerSecond;

        /// <summary>Climb frames, looped while moving on a ladder. May be empty.</summary>
        public Sprite[] Climb => _climb;

        public float ClimbFramesPerSecond => _climbFramesPerSecond;

        /// <summary>Sprites for a direction index (0 = east, counting counter-clockwise).</summary>
        public DirectionSprites Get(int direction)
        {
            return _directions[direction];
        }

        /// <summary>Replaces the climb frames; used by the editor builder.</summary>
        public void SetClimb(Sprite[] climb)
        {
            _climb = climb;
        }

        /// <summary>Replaces all directions; used by the editor builder.</summary>
        public void SetDirections(DirectionSprites[] directions)
        {
            _directions = directions;
        }
    }
}
