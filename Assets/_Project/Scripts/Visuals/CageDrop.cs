// CageDrop: the cage that traps her at the end of the Ending. Drop slams the cage's art down from
// above onto the floor with a small bounce, then raises OnLanded (wired to the shake, dust and
// spotlight). Only the Art child moves, so the cage keeps sorting from its floor position
// (CLAUDE.md, Known issues). Visual only.
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Visuals
{
    /// <summary>Drops the cage art onto the floor and announces the landing.</summary>
    public class CageDrop : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Parent of the cage sprites. It starts raised and hidden, and falls to local zero.")]
        [SerializeField] private Transform _art;

        [Header("Drop")]
        [SerializeField] private float _startHeight = 4f;
        [Tooltip("Seconds to fall.")]
        [SerializeField] private float _fallTime = 0.22f;
        [Tooltip("Height of the small bounce after landing, in world units.")]
        [SerializeField] private float _bounceHeight = 0.12f;
        [SerializeField] private float _bounceTime = 0.18f;

        [Header("Events")]
        [Tooltip("Raised the moment the cage hits the floor.")]
        [SerializeField] private UnityEvent _onLanded = new UnityEvent();

        /// <summary>True once the cage has landed.</summary>
        public bool HasLanded { get; private set; }

        private void Awake()
        {
            _art.localPosition = new Vector3(0f, _startHeight, 0f);
            _art.gameObject.SetActive(false);
        }

        /// <summary>Starts the fall. Safe to call only once.</summary>
        public void Drop()
        {
            if (!HasLanded && !_art.gameObject.activeSelf)
            {
                StartCoroutine(Fall());
            }
        }

        private IEnumerator Fall()
        {
            // 1. Fall with an ease-in so it accelerates like something heavy.
            _art.gameObject.SetActive(true);
            for (float t = 0f; t < _fallTime; t += Time.deltaTime)
            {
                float k = t / _fallTime;
                _art.localPosition = new Vector3(0f, Mathf.Lerp(_startHeight, 0f, k * k), 0f);
                yield return null;
            }

            // 2. Land, announce it, then bounce once.
            _art.localPosition = Vector3.zero;
            HasLanded = true;
            _onLanded.Invoke();
            for (float t = 0f; t < _bounceTime; t += Time.deltaTime)
            {
                float k = t / _bounceTime;
                _art.localPosition = new Vector3(0f, Mathf.Sin(k * Mathf.PI) * _bounceHeight, 0f);
                yield return null;
            }

            _art.localPosition = Vector3.zero;
        }
    }
}
