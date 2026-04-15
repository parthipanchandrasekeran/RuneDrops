using UnityEngine;
using System;
using RuneDrop.Core;
#if RUNEDROP_ADMOB
using GoogleMobileAds.Api;
#endif

namespace RuneDrop.Monetization
{
    /// <summary>
    /// AdMob integration for Rune Drop.
    /// Simulated mode by default — add RUNEDROP_ADMOB scripting define + Google Mobile Ads
    /// plugin to enable real ads.
    ///
    /// Ad placements:
    /// 1. Rewarded: revive after death (1 per run)
    /// 2. Rewarded: double soul shards on death
    /// 3. Interstitial: every 3rd death
    ///
    /// AdMob Account: ca-app-pub-6500938855258542
    /// Create new ad units for RuneDrop in AdMob console, then replace test IDs below.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        // ── Ad Unit IDs ─────────────────────────────────────────────
        // TODO: Replace with real RuneDrop ad unit IDs from AdMob console
        [Header("Ad Unit IDs (Android) — replace with real IDs")]
        [SerializeField] private string _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // TEST
        [SerializeField] private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // TEST

        // ── Frequency Caps ──────────────────────────────────────────
        [Header("Frequency Caps")]
        [SerializeField] private float _rewardedCooldown = 30f;
        [SerializeField] private int _maxRevivesPerRun = 1;
        [SerializeField] private int _interstitialEveryNDeaths = 3;

        // ── Anti-Exploit Caps ───────────────────────────────────────
        [Header("Anti-Exploit Caps")]
        [SerializeField] private int _maxRewardedAdsPerDay = 10;
        [SerializeField] private int _maxDoubleShardsPerSession = 3;

        // ── State ───────────────────────────────────────────────────
        private bool _adsRemoved;
        private bool _isRewardedLoaded;
        private bool _isInterstitialLoaded;
        private bool _isInitialized;
        private float _lastRewardedTime = -999f;
        private int _revivesThisRun;
        private int _deathsSinceLastInterstitial;
        private int _rewardedAdsToday;
        private int _doubleShardsThisSession;
        private int _rewardedAdsThisSession;

        private Action _onRewardedComplete;
        private Action _onRewardedFailed;

#if RUNEDROP_ADMOB
        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;
#endif

        // ── Properties ──────────────────────────────────────────────
        public bool AdsRemoved => _adsRemoved;
        public bool IsRewardedReady => _isRewardedLoaded
            && Time.time - _lastRewardedTime >= GetEscalatingCooldown()
            && _rewardedAdsToday < _maxRewardedAdsPerDay;
        public bool CanRevive => _revivesThisRun < _maxRevivesPerRun && IsRewardedReady;
        public bool CanDoubleShards => _doubleShardsThisSession < _maxDoubleShardsPerSession && IsRewardedReady;

        /// <summary>Cooldown escalates: 30s, 45s, 67s, 101s... per session</summary>
        private float GetEscalatingCooldown()
        {
            return _rewardedCooldown * Mathf.Pow(1.5f, Mathf.Min(_rewardedAdsThisSession, 5));
        }

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
                _adsRemoved = save.Data.AdsRemoved;

            // Load daily ad counter
            string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            string lastAdDate = PlayerPrefs.GetString("LastAdDate", "");
            if (lastAdDate != today)
            {
                _rewardedAdsToday = 0;
                PlayerPrefs.SetString("LastAdDate", today);
                PlayerPrefs.SetInt("RewardedAdsToday", 0);
                PlayerPrefs.Save();
            }
            else
            {
                _rewardedAdsToday = PlayerPrefs.GetInt("RewardedAdsToday", 0);
            }

            InitializeAds();
        }

        private void OnDestroy()
        {
#if RUNEDROP_ADMOB
            _rewardedAd?.Destroy();
            _interstitialAd?.Destroy();
#endif
            if (Instance == this)
            {
                ServiceLocator.Unregister(this);
                Instance = null;
            }
        }

        // ── Initialize ──────────────────────────────────────────────

        private void InitializeAds()
        {
#if RUNEDROP_ADMOB
            MobileAds.Initialize(status =>
            {
                _isInitialized = true;
                Debug.Log("[AdManager] AdMob initialized");
                LoadRewarded();
                if (!_adsRemoved) LoadInterstitial();
            });
#else
            _isInitialized = true;
            _isRewardedLoaded = true;
            _isInterstitialLoaded = true;
            Debug.Log("[AdManager] Simulated mode (add RUNEDROP_ADMOB define for real ads)");
#endif
        }

        // ── Run Lifecycle ───────────────────────────────────────────

        public void OnRunStarted()
        {
            _revivesThisRun = 0;
        }

        public void OnPlayerDied()
        {
            _deathsSinceLastInterstitial++;
        }

        // ── Rewarded: Revive ────────────────────────────────────────

        public void ShowRewardedForRevive(Action onRevived, Action onFailed = null)
        {
            if (!CanRevive) { onFailed?.Invoke(); return; }
            ShowRewarded(() =>
            {
                _revivesThisRun++;
                onRevived?.Invoke();
            }, onFailed);
        }

        // ── Rewarded: Double Shards ─────────────────────────────────

        public void ShowRewardedForDoubleShards(Action onDoubled, Action onFailed = null)
        {
            if (!CanDoubleShards) { onFailed?.Invoke(); return; }
            ShowRewarded(() => { _doubleShardsThisSession++; onDoubled?.Invoke(); }, onFailed);
        }

        // ── Interstitial: On Death ──────────────────────────────────

        public bool TryShowInterstitialOnDeath(Action onClosed = null)
        {
            if (_adsRemoved) return false;
            if (_deathsSinceLastInterstitial < _interstitialEveryNDeaths) return false;

            _deathsSinceLastInterstitial = 0;
            return TryShowInterstitial(onClosed);
        }

        // ── Core Show Methods ───────────────────────────────────────

        private void ShowRewarded(Action onComplete, Action onFailed)
        {
            if (!_isRewardedLoaded) { LoadRewarded(); onFailed?.Invoke(); return; }

            _onRewardedComplete = onComplete;
            _onRewardedFailed = onFailed;

#if RUNEDROP_ADMOB
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show(reward => { OnRewardedComplete(); });
            }
            else
            {
                OnRewardedFailed();
            }
#else
            Debug.Log("[AdManager] SIM: Rewarded ad — granting reward");
            OnRewardedComplete();
#endif
        }

        private bool TryShowInterstitial(Action onClosed = null)
        {
            if (_adsRemoved || !_isInterstitialLoaded) return false;

#if RUNEDROP_ADMOB
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
                _interstitialAd.OnAdFullScreenContentClosed += () => { OnInterstitialClosed(onClosed); };
                return true;
            }
            LoadInterstitial();
            return false;
#else
            Debug.Log("[AdManager] SIM: Interstitial shown");
            OnInterstitialClosed(onClosed);
            return true;
#endif
        }

        // ── Callbacks ───────────────────────────────────────────────

        private void OnRewardedComplete()
        {
            _lastRewardedTime = Time.time;
            _isRewardedLoaded = false;
            _rewardedAdsToday++;
            _rewardedAdsThisSession++;
            PlayerPrefs.SetInt("RewardedAdsToday", _rewardedAdsToday);
            PlayerPrefs.Save();
            _onRewardedComplete?.Invoke();
            _onRewardedComplete = null;
            _onRewardedFailed = null;
            LoadRewarded();
        }

        private void OnRewardedFailed()
        {
            _isRewardedLoaded = false;
            _onRewardedFailed?.Invoke();
            _onRewardedComplete = null;
            _onRewardedFailed = null;
            LoadRewarded();
        }

        private void OnInterstitialClosed(Action onClosed)
        {
            _isInterstitialLoaded = false;
            onClosed?.Invoke();
            LoadInterstitial();
        }

        // ── Loading ─────────────────────────────────────────────────

        private void LoadRewarded()
        {
            if (_isRewardedLoaded) return;
#if RUNEDROP_ADMOB
            RewardedAd.Load(_rewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null) { Debug.LogWarning($"[AdManager] Rewarded load failed: {error}"); return; }
                _rewardedAd = ad;
                _isRewardedLoaded = true;
                _rewardedAd.OnAdFullScreenContentFailed += err => { OnRewardedFailed(); };
            });
#else
            _isRewardedLoaded = true;
#endif
        }

        private void LoadInterstitial()
        {
            if (_adsRemoved || _isInterstitialLoaded) return;
#if RUNEDROP_ADMOB
            InterstitialAd.Load(_interstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null) { Debug.LogWarning($"[AdManager] Interstitial load failed: {error}"); return; }
                _interstitialAd = ad;
                _isInterstitialLoaded = true;
            });
#else
            _isInterstitialLoaded = true;
#endif
        }

        // ── Remove Ads (IAP) ────────────────────────────────────────

        public void OnAdsRemoved()
        {
            _adsRemoved = true;
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                save.Data.AdsRemoved = true;
                save.Save();
            }
            Debug.Log("[AdManager] Ads removed permanently");
        }
    }
}
