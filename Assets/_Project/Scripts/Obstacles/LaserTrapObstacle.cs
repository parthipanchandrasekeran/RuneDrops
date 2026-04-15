using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Toggling laser beam. Bright red when active, dim when off.
    /// </summary>
    public class LaserTrapObstacle : ObstacleBase
    {
        private SpriteRenderer _beamRenderer;
        private BoxCollider2D _beamCollider;
        private float _onDuration = 1.5f;
        private float _offDuration = 1.0f;
        private float _warningDuration = 0.5f;
        private float _timer;
        private int _phase; // 0=off, 1=warning, 2=active

        protected override void SetupVisuals()
        {
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateSquareSprite();
            sr.color = Color.clear;

            var beamGO = new GameObject("Beam");
            beamGO.transform.SetParent(transform);
            beamGO.transform.localPosition = Vector3.zero;
            beamGO.layer = 7;

            _beamRenderer = beamGO.AddComponent<SpriteRenderer>();
            _beamRenderer.sprite = CreateSquareSprite();
            _beamRenderer.sortingOrder = 3;
            beamGO.transform.localScale = new Vector3(12f, 0.12f, 1f);

            _beamCollider = beamGO.AddComponent<BoxCollider2D>();
            _beamCollider.isTrigger = true;

            _phase = 0;
            _timer = _offDuration * Random.Range(0.2f, 1f);
            SetBeamState(false);

            _onDuration = Mathf.Lerp(1.5f, 2.5f, Difficulty);
            _offDuration = Mathf.Lerp(1.5f, 0.8f, Difficulty);
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

            if (_phase == 1)
            {
                float flash = Mathf.PingPong(Time.time * 10f, 1f);
                _beamRenderer.color = new Color(1f, 0.1f, 0.1f, flash * 0.4f);
            }
        }

        private void SetBeamState(bool active)
        {
            _beamRenderer.color = active ?
                new Color(1f, 0.1f, 0.1f, 0.85f) :
                new Color(0.3f, 0.05f, 0.05f, 0.1f);
            _beamCollider.enabled = active;
        }
    }
}
