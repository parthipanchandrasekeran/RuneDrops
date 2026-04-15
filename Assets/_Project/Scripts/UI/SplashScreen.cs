using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Custom splash screen: "Parthipan Gaming" logo/text.
    /// Shows for 2 seconds then transitions to MainMenu.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        private Text _studioText;
        private Text _presentsText;
        private Image _fadeOverlay;

        private IEnumerator Start()
        {
            CreateUI();

            // Fade in
            yield return FadeOverlay(1f, 0f, 0.5f);

            // Hold
            yield return new WaitForSeconds(1.5f);

            // Fade out
            yield return FadeOverlay(0f, 1f, 0.5f);

            // Load main menu
            SceneManager.LoadScene("MainMenu");
        }

        private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                var c = _fadeOverlay.color;
                c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                _fadeOverlay.color = c;

                // Also fade texts
                float textAlpha = 1f - Mathf.Abs(Mathf.Lerp(fromAlpha, toAlpha, t));
                var sc = _studioText.color; sc.a = textAlpha; _studioText.color = sc;
                var pc = _presentsText.color; pc.a = textAlpha; _presentsText.color = pc;

                yield return null;
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "SplashCanvas", 1000);
            var ct = canvas.transform;

            // Black background
            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one,
                new Color(0.01f, 0.005f, 0.03f));

            // Subtle accent
            UIHelper.MakePanel(ct, "Accent",
                new Vector2(0.3f, 0.48f), new Vector2(0.7f, 0.485f),
                new Color(0.5f, 0.3f, 0.7f, 0.4f));

            // Studio name
            _studioText = UIHelper.MakeText(ct, "Studio", new Vector2(0.5f, 0.55f),
                "PARTHIPAN GAMING", 48, new Color(0.8f, 0.7f, 0.95f));

            // Presents
            _presentsText = UIHelper.MakeText(ct, "Presents", new Vector2(0.5f, 0.45f),
                "presents", 28, UIHelper.TextDim);

            // Fade overlay (on top of everything)
            var fadeGO = UIHelper.MakePanel(ct, "Fade", Vector2.zero, Vector2.one, Color.black);
            _fadeOverlay = fadeGO.GetComponent<Image>();
        }
    }
}
