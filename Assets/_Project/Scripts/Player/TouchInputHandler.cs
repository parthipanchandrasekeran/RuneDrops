using System;
using UnityEngine;

namespace RuneDrop.Player
{
    /// <summary>
    /// Processes raw touch/mouse input into abstract drag and tap events.
    /// Drag-to-follow: finger X position = target player X position.
    /// Tap: quick touch with minimal movement = anchor trigger.
    /// </summary>
    public class TouchInputHandler : MonoBehaviour
    {
        // ── Configuration ───────────────────────────────────────────
        [SerializeField] private float _tapTimeThreshold = 0.2f;
        [SerializeField] private float _tapDistanceThreshold = 0.15f;

        // ── Events ──────────────────────────────────────────────────
        public Action<float> OnDragPosition;
        public Action OnTap;
        public Action OnTouchBegan;
        public Action OnTouchEnded;

        // ── State ───────────────────────────────────────────────────
        private Camera _camera;
        private bool _isTouching;
        private float _touchStartTime;
        private Vector2 _touchStartScreenPos;

        public bool IsTouching => _isTouching;

        // ── Lifecycle ───────────────────────────────────────────────

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

#if UNITY_EDITOR
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        // ── Touch Input (Mobile) ────────────────────────────────────

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0)
            {
                if (_isTouching)
                {
                    _isTouching = false;
                    OnTouchEnded?.Invoke();
                }
                return;
            }

            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _isTouching = true;
                    _touchStartTime = Time.time;
                    _touchStartScreenPos = touch.position;
                    OnTouchBegan?.Invoke();
                    EmitDragPosition(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    EmitDragPosition(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // Check if this was a tap (quick + minimal movement)
                    float duration = Time.time - _touchStartTime;
                    float distance = Vector2.Distance(touch.position, _touchStartScreenPos);

                    if (duration < _tapTimeThreshold && distance < _tapDistanceThreshold * Screen.dpi)
                    {
                        OnTap?.Invoke();
                    }

                    _isTouching = false;
                    OnTouchEnded?.Invoke();
                    break;
            }
        }

        // ── Mouse Input (Editor) ────────────────────────────────────

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isTouching = true;
                _touchStartTime = Time.time;
                _touchStartScreenPos = Input.mousePosition;
                OnTouchBegan?.Invoke();
                EmitDragPosition(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                EmitDragPosition(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                float duration = Time.time - _touchStartTime;
                float distance = Vector2.Distance((Vector2)Input.mousePosition, _touchStartScreenPos);

                if (duration < _tapTimeThreshold && distance < _tapDistanceThreshold * 96f)
                {
                    OnTap?.Invoke();
                }

                _isTouching = false;
                OnTouchEnded?.Invoke();
            }
        }

        // ── Helpers ─────────────────────────────────────────────────

        private void EmitDragPosition(Vector2 screenPos)
        {
            screenPos = RuneDrop.Core.ScreenSetup.FixTouchPos(screenPos);
            var worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            OnDragPosition?.Invoke(worldPos.x);
        }
    }
}
