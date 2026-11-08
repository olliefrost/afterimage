// ISceneTransition: what SceneFlow needs from the scene's screen transition (UI/ScreenTransition)
// to cover a scene change, so Core never depends on the UI code. Each scene's transition registers
// itself with SceneFlow.Transition while enabled.
namespace GameJam.Core
{
    /// <summary>Covers the screen, optionally shows a caption on the black, then loads a scene.</summary>
    public interface ISceneTransition
    {
        /// <summary>True while a scene change is already under way, so a second request is ignored.</summary>
        bool IsBusy { get; }

        /// <summary>Covers the screen, shows the caption if it is not empty, then loads the scene.</summary>
        void Play(int buildIndex, string caption);
    }
}
