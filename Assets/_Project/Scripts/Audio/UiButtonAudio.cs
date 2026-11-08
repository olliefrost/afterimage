// UiButtonAudio: on the MenuButton prefab, so every menu button gets a hover tick and a click.
// Plays through the UI pool, which keeps sounding while the game is paused.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameJam.Audio
{
    /// <summary>Plays hover and click sounds for a UI button.</summary>
    [RequireComponent(typeof(Button))]
    public class UiButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private SoundCue _hover;
        [SerializeField] private SoundCue _click;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button.interactable)
            {
                AudioManager.PlayUi(_hover);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_button.interactable)
            {
                AudioManager.PlayUi(_click);
            }
        }
    }
}
