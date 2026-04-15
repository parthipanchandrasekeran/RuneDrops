using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneDrop.Core
{
    /// <summary>
    /// Root game manager. Singleton with DontDestroyOnLoad.
    /// Orchestrates game state, scene loading, and run lifecycle.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── State ───────────────────────────────────────────────────
        public GameState CurrentState { get; private set; } = GameState.Boot;
        public bool IsInitialized { get; private set; }

        // ── Run Data ────────────────────────────────────────────────
        public float CurrentRunDepth { get; set; }
        public int CurrentRunRunes { get; set; }
        public float CurrentRunDuration { get; set; }
        public int CurrentRunCombos { get; set; }
        public int CurrentRunDecisionRooms { get; set; }

        // ── Scene Names ─────────────────────────────────────────────
        private const string SCENE_BOOT = "Boot";
        private const string SCENE_MAIN_MENU = "MainMenu";
        private const string SCENE_GAMEPLAY = "Gameplay";

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.Register(this);
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;

                IsInitialized = true;
                Debug.Log("[GameManager] Initialized");

                // Transition to MainMenu
                StartCoroutine(LoadSceneAndTransition(SCENE_MAIN_MENU, GameState.MainMenu));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] Initialize CRASHED: {e}");
            }
        }

        // SaveSystem handles its own OnApplicationPause save

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<GameManager>();
                Instance = null;
            }
        }

        // ── State Transitions ───────────────────────────────────────

        private void TransitionTo(GameState newState)
        {
            var oldState = CurrentState;
            CurrentState = newState;
            Debug.Log($"[GameManager] {oldState} -> {newState}");
            EventBus.Publish(new GameStateChangedEvent
            {
                OldState = oldState,
                NewState = newState
            });
        }

        // ── Run Lifecycle ───────────────────────────────────────────

        public void StartRun()
        {
            ResetRunData();

            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                save.Data.TotalRuns++;
                save.Save();
            }

            StartCoroutine(StartRunCoroutine());
        }

        private IEnumerator StartRunCoroutine()
        {
            yield return LoadSceneAndTransition(SCENE_GAMEPLAY, GameState.Playing);
            // Publish AFTER scene is loaded so subscribers exist
            EventBus.Publish(new RunStartedEvent());
        }

        public void EndRun()
        {
            TransitionTo(GameState.Dead);
            EventBus.Publish(new PlayerDiedEvent
            {
                DepthReached = CurrentRunDepth,
                RunesCollected = CurrentRunRunes,
                RunDuration = CurrentRunDuration
            });

            // Update best depth
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                if (CurrentRunDepth > save.Data.BestDepth)
                {
                    save.Data.BestDepth = CurrentRunDepth;
                }
                save.Data.TotalRunesCollected += CurrentRunRunes;
                save.Save();
            }
        }

        public void RevivePlayer()
        {
            TransitionTo(GameState.Reviving);
            // Brief delay then resume
            StartCoroutine(ReviveSequence());
        }

        private IEnumerator ReviveSequence()
        {
            yield return new WaitForSeconds(0.5f);
            TransitionTo(GameState.Playing);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                TransitionTo(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                TransitionTo(GameState.Playing);
            }
        }

        public void EnterDecisionRoom()
        {
            if (CurrentState == GameState.Playing)
            {
                TransitionTo(GameState.DecisionRoom);
                CurrentRunDecisionRooms++;
            }
        }

        public void ExitDecisionRoom()
        {
            if (CurrentState == GameState.DecisionRoom)
            {
                TransitionTo(GameState.Playing);
            }
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneAndTransition(SCENE_MAIN_MENU, GameState.MainMenu));
        }

        // ── Helpers ─────────────────────────────────────────────────

        private void ResetRunData()
        {
            CurrentRunDepth = 0f;
            CurrentRunRunes = 0;
            CurrentRunDuration = 0f;
            CurrentRunCombos = 0;
            CurrentRunDecisionRooms = 0;
        }

        private IEnumerator LoadSceneAndTransition(string sceneName, GameState targetState)
        {
            // Check if scene is already loaded
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == sceneName)
            {
                TransitionTo(targetState);
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[GameManager] Failed to load scene: {sceneName}");
                yield break;
            }

            while (!op.isDone)
            {
                yield return null;
            }

            TransitionTo(targetState);
        }
    }
}
