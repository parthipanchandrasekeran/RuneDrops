using UnityEngine;

namespace RuneDrop.UI
{
    /// <summary>
    /// Lightweight entrance animation for runtime-generated UI canvases.
    /// </summary>
    public class UIFXAnimator : MonoBehaviour
    {
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private float fromScale = 0.985f;

        private CanvasGroup _group;
        private RectTransform _rect;
        private float _t;
        private bool _completed;

        public static void Attach(GameObject target, float duration = 0.2f, float fromScale = 0.985f)
        {
            if (target == null) return;
            var fx = target.GetComponent<UIFXAnimator>();
            if (fx == null) fx = target.AddComponent<UIFXAnimator>();
            fx.duration = Mathf.Max(0.01f, duration);
            fx.fromScale = Mathf.Clamp(fromScale, 0.92f, 1f);
            fx.ResetAnim();
        }

        private void Awake()
        {
            _group = gameObject.GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();

            _rect = transform as RectTransform;
        }

        private void OnEnable()
        {
            ResetAnim();
        }

        private void ResetAnim()
        {
            if (_group == null) _group = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _rect = transform as RectTransform;
            _t = 0f;
            _completed = false;
            _group.alpha = 0f;
            if (_rect != null) _rect.localScale = Vector3.one * fromScale;
        }

        private void Update()
        {
            if (_completed) return;
            _t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(_t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            _group.alpha = eased;
            if (_rect != null)
                _rect.localScale = Vector3.one * Mathf.Lerp(fromScale, 1f, eased);

            if (p >= 1f)
            {
                _group.alpha = 1f;
                if (_rect != null) _rect.localScale = Vector3.one;
                _completed = true;
            }
        }
    }
}
