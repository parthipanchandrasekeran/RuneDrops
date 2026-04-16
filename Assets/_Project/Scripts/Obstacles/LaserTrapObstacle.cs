using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Ancient rune trap — beam with emitter endpoints, glow layer, and warning pulse.
    /// </summary>
    public class LaserTrapObstacle : ObstacleBase
    {
        private SpriteRenderer _beamRenderer;
        private SpriteRenderer _beamGlow;
        private BoxCollider2D _beamCollider;
        private SpriteRenderer _emitterL, _emitterR;
        private float _onDuration = 1.5f;
        private float _offDuration = 1.0f;
        private float _warningDuration = 0.5f;
        private float _timer;
        private int _phase;

        protected override void SetupVisuals()
        {
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateSquareSprite();
            sr.color = Color.clear;

            float beamWidth = 6f;

            // Beam glow (wider, softer behind beam)
            var glowGO = new GameObject("BeamGlow");
            glowGO.transform.SetParent(transform);
            glowGO.transform.localPosition = Vector3.zero;
            glowGO.layer = 7;
            _beamGlow = glowGO.AddComponent<SpriteRenderer>();
            _beamGlow.sprite = CreateSquareSprite();
            _beamGlow.sortingOrder = 2;
            _beamGlow.color = Color.clear;
            glowGO.transform.localScale = new Vector3(beamWidth, 0.3f, 1f);

            // Beam core (thin, bright)
            var beamGO = new GameObject("Beam");
            beamGO.transform.SetParent(transform);
            beamGO.transform.localPosition = Vector3.zero;
            beamGO.layer = 7;
            _beamRenderer = beamGO.AddComponent<SpriteRenderer>();
            _beamRenderer.sprite = CreateSquareSprite();
            _beamRenderer.sortingOrder = 3;
            beamGO.transform.localScale = new Vector3(beamWidth, 0.07f, 1f);
            _beamCollider = beamGO.AddComponent<BoxCollider2D>();
            _beamCollider.isTrigger = true;

            // Emitter endpoints
            _emitterL = CreateEmitter("EmitterL", new Vector3(-beamWidth / 2f, 0, 0));
            _emitterR = CreateEmitter("EmitterR", new Vector3(beamWidth / 2f, 0, 0));

            _phase = 0;
            _timer = _offDuration * Random.Range(0.2f, 1f);
            SetBeamState(false);

            _onDuration = Mathf.Lerp(1.5f, 2.5f, Difficulty);
            _offDuration = Mathf.Lerp(1.5f, 0.8f, Difficulty);
        }

        private SpriteRenderer CreateEmitter(string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.transform.localPosition = localPos;
            var emSR = go.AddComponent<SpriteRenderer>();
            emSR.sprite = CreateCircleSprite();
            emSR.color = new Color(0.4f, 0.1f, 0.1f, 0.3f);
            emSR.sortingOrder = 4;
            go.transform.localScale = Vector3.one * 0.2f;
            return emSR;
        }

        protected override void SetupCollider() { }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _phase = (_phase + 1) % 3;
                switch (_phase)
                {
                    case 0: _timer = _offDuration; SetBeamState(false); break;
                    case 1: _timer = _warningDuration; _beamCollider.enabled = false; break;
                    case 2: _timer = _onDuration; SetBeamState(true); break;
                }
            }

            // Warning flash
            if (_phase == 1)
            {
                float flash = Mathf.PingPong(Time.time * 10f, 1f);
                _beamRenderer.color = new Color(1f, 0.15f, 0.1f, flash * 0.35f);
                _beamGlow.color = new Color(1f, 0.1f, 0.08f, flash * 0.1f);
                float emScale = 0.2f + flash * 0.15f;
                if (_emitterL != null) _emitterL.transform.localScale = Vector3.one * emScale;
                if (_emitterR != null) _emitterR.transform.localScale = Vector3.one * emScale;
            }
        }

        private void SetBeamState(bool active)
        {
            if (active)
            {
                _beamRenderer.color = new Color(1f, 0.2f, 0.15f, 0.85f);
                _beamGlow.color = new Color(1f, 0.12f, 0.08f, 0.12f);
                _beamCollider.enabled = true;
                SetEmitterColor(new Color(1f, 0.3f, 0.2f, 0.65f), 0.25f);
            }
            else
            {
                _beamRenderer.color = new Color(0.3f, 0.05f, 0.05f, 0.08f);
                _beamGlow.color = Color.clear;
                _beamCollider.enabled = false;
                SetEmitterColor(new Color(0.4f, 0.1f, 0.1f, 0.25f), 0.18f);
            }
        }

        private void SetEmitterColor(Color color, float scale)
        {
            if (_emitterL != null) { _emitterL.color = color; _emitterL.transform.localScale = Vector3.one * scale; }
            if (_emitterR != null) { _emitterR.color = color; _emitterR.transform.localScale = Vector3.one * scale; }
        }
    }
}
