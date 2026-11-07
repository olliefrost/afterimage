// FuelBelt: Chamber 3's conveyor. It drops a fuel crate at the start on a fixed tick schedule and
// carries each one to the generator. As a crate crosses the laser wall it burns unless the wall is
// blocked; crates that arrive call PowerGenerator.Feed(). Everything is timed in ticks so clones
// see the same crates every attempt.
using System.Collections.Generic;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>Moves a steady stream of fuel crates along the belt to the generator.</summary>
    public class FuelBelt : MonoBehaviour, IResettable
    {
        /// <summary>One crate on the belt and the tick it was dropped on.</summary>
        private struct Crate
        {
            public SpriteRenderer Renderer;
            public int SpawnTick;
        }

        [Header("References")]
        [SerializeField] private TickClock _clock;
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _end;
        [SerializeField] private LaserWall _laserWall;
        [SerializeField] private PowerGenerator _generator;
        [Tooltip("An inactive crate sprite to copy for each crate on the belt.")]
        [SerializeField] private SpriteRenderer _crateTemplate;

        [Header("Timing (ticks, 50 per second)")]
        [Min(0)]
        [SerializeField] private int _firstSpawnTick = 0;
        [Min(1)]
        [SerializeField] private int _spawnIntervalTicks = 60;
        [Tooltip("Ticks a crate takes from the start of the belt to the generator.")]
        [Min(1)]
        [SerializeField] private int _travelTicks = 200;
        [Tooltip("How far along the belt the laser wall sits (0 = start, 1 = generator).")]
        [Range(0f, 1f)]
        [SerializeField] private float _laserPoint = 0.45f;

        private readonly List<Crate> _crates = new List<Crate>();
        private readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();

        private void Awake()
        {
            // Pre-make enough crates for a full belt, so none are created during play.
            int needed = _travelTicks / _spawnIntervalTicks + 2;
            for (int i = 0; i < needed; i++)
            {
                _pool.Push(CreateCrate());
            }
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Clears the belt for the next attempt.</summary>
        public void ResetState()
        {
            for (int i = _crates.Count - 1; i >= 0; i--)
            {
                Release(i);
            }
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            int tick = _clock.CurrentTick;

            // 1. Drop a new crate on schedule.
            if (tick >= _firstSpawnTick && (tick - _firstSpawnTick) % _spawnIntervalTicks == 0)
            {
                SpriteRenderer renderer = _pool.Count > 0 ? _pool.Pop() : CreateCrate();
                renderer.gameObject.SetActive(true);
                _crates.Add(new Crate { Renderer = renderer, SpawnTick = tick });
            }

            // 2. Move every crate, burning it at the laser or feeding the generator at the end.
            for (int i = _crates.Count - 1; i >= 0; i--)
            {
                Crate crate = _crates[i];
                float before = (float)(tick - 1 - crate.SpawnTick) / _travelTicks;
                float now = (float)(tick - crate.SpawnTick) / _travelTicks;

                if (before < _laserPoint && now >= _laserPoint && !_laserWall.IsBlocked)
                {
                    _laserWall.Burn();
                    Release(i);
                    continue;
                }

                if (now >= 1f)
                {
                    _generator.Feed();
                    Release(i);
                    continue;
                }

                crate.Renderer.transform.position = Vector3.Lerp(_start.position, _end.position, now);
            }
        }

        private SpriteRenderer CreateCrate()
        {
            SpriteRenderer renderer = Instantiate(_crateTemplate, transform);
            renderer.gameObject.SetActive(false);
            return renderer;
        }

        private void Release(int index)
        {
            SpriteRenderer renderer = _crates[index].Renderer;
            renderer.gameObject.SetActive(false);
            _pool.Push(renderer);
            _crates.RemoveAt(index);
        }
    }
}
