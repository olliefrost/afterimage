// TitleMenu: the buttons on the title screen (Title.unity, the first scene in Build Settings).
// Continue only shows when a chamber is saved; New Game starts the first chamber; Quit closes the
// game. All scene changes go through SceneFlow.
using GameJam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Wires the title screen buttons to <see cref="SceneFlow"/>.</summary>
    public class TitleMenu : MonoBehaviour
    {
        [Header("UI references")]
        [Tooltip("Hidden when there is no saved chamber to continue from.")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _quitButton;

        private void OnEnable()
        {
            _continueButton.gameObject.SetActive(SaveProgress.HasSave);
            _continueButton.onClick.AddListener(SceneFlow.ContinueGame);
            _newGameButton.onClick.AddListener(SceneFlow.NewGame);
            _quitButton.onClick.AddListener(SceneFlow.QuitGame);
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveListener(SceneFlow.ContinueGame);
            _newGameButton.onClick.RemoveListener(SceneFlow.NewGame);
            _quitButton.onClick.RemoveListener(SceneFlow.QuitGame);
        }
    }
}
