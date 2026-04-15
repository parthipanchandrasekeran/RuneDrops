using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Checks if this is the first run and spawns the tutorial overlay.
    /// </summary>
    public class TutorialTrigger : MonoBehaviour
    {
        private void Start()
        {
            Invoke(nameof(CheckTutorial), 0.5f);
        }

        private void CheckTutorial()
        {
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                if (!save.Data.HasCompletedTutorial)
                {
                    var tutorialGO = new GameObject("TutorialOverlay");
                    var tutorial = tutorialGO.AddComponent<TutorialOverlayUI>();
                    tutorial.Initialize();
                }
            }
        }
    }
}
