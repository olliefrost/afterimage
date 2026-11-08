// CameraPullBack: the Ending's slow zoom-out. Play takes the camera off CameraFollow and eases its
// position and orthographic size to a new view, ignoring the room bounds so the whole room (and the
// observation window) can be shown. EndingSequence waits on the returned coroutine. One-way: it
// leaves CameraFollow (and so camera shake) switched off, which is fine for the final scene. Visual only.
using System.Collections;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Eases the main camera to a wider view, freeing it from CameraFollow.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraPullBack : MonoBehaviour
    {
        [Tooltip("The follow script, switched off while the pull-back runs so it stops fighting the camera.")]
        [SerializeField] private CameraFollow _follow;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        /// <summary>Sets the zoom straight away (used when a cutscene is skipped).</summary>
        public void SnapZoom(float size)
        {
            _camera.orthographicSize = size;
        }

        /// <summary>
        /// Eases only the zoom to <paramref name="size"/>, leaving CameraFollow running, so the camera
        /// keeps gliding to its target (and stays inside the room bounds) while it zooms.
        /// </summary>
        public IEnumerator ZoomTo(float size, float seconds)
        {
            float startSize = _camera.orthographicSize;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                _camera.orthographicSize = Mathf.Lerp(startSize, size, Mathf.SmoothStep(0f, 1f, t / seconds));
                yield return null;
            }

            _camera.orthographicSize = size;
        }

        /// <summary>Eases to <paramref name="point"/> (x, y) at <paramref name="size"/> over <paramref name="seconds"/>.</summary>
        public IEnumerator Play(Vector2 point, float size, float seconds)
        {
            // 1. Take over from the follow camera, starting from where it is now.
            _follow.enabled = false;
            Vector3 startPosition = transform.position;
            float startSize = _camera.orthographicSize;
            Vector3 endPosition = new Vector3(point.x, point.y, startPosition.z);

            // 2. Slow in and out, so the reveal feels heavy.
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / seconds);
                transform.position = Vector3.Lerp(startPosition, endPosition, k);
                _camera.orthographicSize = Mathf.Lerp(startSize, size, k);
                yield return null;
            }

            transform.position = endPosition;
            _camera.orthographicSize = size;
        }
    }
}
