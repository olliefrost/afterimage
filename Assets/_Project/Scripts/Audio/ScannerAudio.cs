// ScannerAudio: on Chamber 4's Scanner. Plays a confirmation when a scan completes.
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a sound when the scanner finishes a scan.</summary>
    public class ScannerAudio : MonoBehaviour
    {
        [SerializeField] private Scanner _scanner;
        [SerializeField] private SoundCue _done;

        private void OnEnable()
        {
            _scanner.Scanned += HandleScanned;
        }

        private void OnDisable()
        {
            _scanner.Scanned -= HandleScanned;
        }

        private void HandleScanned()
        {
            AudioManager.Play(_done);
        }
    }
}
