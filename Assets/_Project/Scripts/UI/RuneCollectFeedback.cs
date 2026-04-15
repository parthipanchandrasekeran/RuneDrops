using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;
using RuneDrop.Runes;

namespace RuneDrop.UI
{
    /// <summary>
    /// Shows floating text feedback when runes are collected or combos activate.
    /// "+Fire!" floats up and fades. "FLAME TRAIL!" shows big and gold for combos.
    /// </summary>
    public class RuneCollectFeedback : MonoBehaviour
    {
        private Canvas _canvas;
        private Transform _ct;

        private void Start()
        {
            _canvas = UIHelper.CreateCanvas(transform, "FeedbackCanvas", 120);
            _ct = UIHelper.GetSafeAreaRoot(_canvas);

            EventBus.Subscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<AnchorUsedEvent>(OnAnchorUsed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<AnchorUsedEvent>(OnAnchorUsed);
        }

        private void OnRuneCollected(RuneCollectedEvent evt)
        {
            var type = (RuneType)evt.Type;
            Color color = type switch
            {
                RuneType.Fire => new Color(1f, 0.6f, 0f),
                RuneType.Wind => new Color(0.3f, 0.95f, 1f),
                RuneType.Shadow => new Color(0.8f, 0.3f, 1f),
                RuneType.Earth => new Color(0.3f, 1f, 0.3f),
                _ => Color.white
            };

            // Short name only — descriptions are in the Powers reference screen
            string desc = type switch
            {
                RuneType.Fire => "+FIRE!",
                RuneType.Wind => "+WIND!",
                RuneType.Shadow => "+SHADOW!",
                RuneType.Earth => "+EARTH!",
                _ => "+RUNE!"
            };

            SpawnFloatingText(desc, 38, color, new Vector2(0.5f, 0.45f), 1.5f);
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            var combo = (ComboType)evt.Combo;
            string name = combo switch
            {
                ComboType.FlameTrail => "FLAME TRAIL!\nRed blocks near you die\n+ you fall slower for 8 sec",
                ComboType.BlinkDash => "BLINK DASH!\nYou teleport down 5 meters\npassing through everything!",
                ComboType.ExplosiveShield => "EXPLOSIVE SHIELD!\nNext hit won't kill you\n+ it destroys all blocks nearby!",
                _ => "COMBO!"
            };
            SpawnFloatingText(name, 34, new Color(1f, 0.8f, 0.2f), new Vector2(0.5f, 0.40f), 3.5f);
        }

        private void OnAnchorUsed(AnchorUsedEvent evt)
        {
            SpawnFloatingText($"ANCHOR ({evt.ChargesRemaining} left)", 30,
                new Color(0.3f, 0.8f, 1f), new Vector2(0.5f, 0.50f), 1f);
        }

        private void SpawnFloatingText(string text, int fontSize, Color color, Vector2 anchor, float duration)
        {
            var go = new GameObject("FloatText");
            go.transform.SetParent(_ct, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(800, 80);

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.font = Resources.Load<Font>("Cinzel");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.9f);
            outline.effectDistance = new Vector2(2, -2);

            // Animate: float up and fade out
            var anim = go.AddComponent<FloatingTextAnim>();
            anim.Duration = duration;
        }
    }

    /// <summary>
    /// Simple animation: float up and fade out, then self-destruct.
    /// </summary>
    public class FloatingTextAnim : MonoBehaviour
    {
        public float Duration = 1.5f;
        private float _elapsed;
        private RectTransform _rect;
        private Text _text;
        private Vector2 _startPos;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
            _text = GetComponent<Text>();
            _startPos = _rect.anchoredPosition;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / Duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Float upward
            _rect.anchoredPosition = _startPos + new Vector2(0, t * 80f);

            // Fade out in last 40%
            if (t > 0.6f)
            {
                var c = _text.color;
                c.a = 1f - ((t - 0.6f) / 0.4f);
                _text.color = c;
            }

            // Scale pop at start
            if (t < 0.1f)
            {
                float scale = 1f + (1f - t / 0.1f) * 0.3f;
                _rect.localScale = Vector3.one * scale;
            }
            else
            {
                _rect.localScale = Vector3.one;
            }
        }
    }
}
