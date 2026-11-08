// TitleCameraDrift: slowly drifts the title screen's camera back and forth between two points over the
// backdrop chamber (Title.unity), easing at each end so it never stops abruptly. Visual only.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Eases the camera between two world points on a slow loop.</summary>
    public class TitleCameraDrift : MonoBehaviour
    {
        [Tooltip("The two ends of the drift, in world space (z keeps the camera's own).")]
        [SerializeField] private Vector2 _from = new Vector2(-1f, 0f);
        [SerializeField] private Vector2 _to = new Vector2(1f, 0.5f);
        [Tooltip("Seconds for one pass from one end to the other.")]
        [SerializeField] private float _passSeconds = 20f;

        private void LateUpdate()
        {
            // A cosine wave eases in and out at both ends.
            float t = 0.5f - 0.5f * Mathf.Cos(Time.unscaledTime * Mathf.PI / _passSeconds);
            Vector2 p = Vector2.Lerp(_from, _to, t);
            transform.position = new Vector3(p.x, p.y, transform.position.z);
        }
    }
}
