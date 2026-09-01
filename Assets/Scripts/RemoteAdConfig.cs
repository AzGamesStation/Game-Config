using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GameAnalyticsAndAds
{
    /// <summary>
    /// Ad configuration data mapped directly from your GitHub JSON file.
    /// </summary>
    [Serializable]
    public class AdConfigData
    {
        [Tooltip("Master switch: If false, all ads are turned off.")]
        public bool show_ads = true;

        [Tooltip("Controls whether interstitial ads are enabled.")]
        public bool show_interstitial = true;

        [Tooltip("Controls whether rewarded video ads are enabled.")]
        public bool show_rewarded = true;

        [Tooltip("Cooldown period in seconds between showing interstitial ads.")]
        public float interstitial_interval_sec = 45f;

        [Tooltip("Max seconds to wait for an ad to load/respond before continuing safely.")]
        public float ad_fail_timeout = 5f;

        // Convenient PascalCase properties
        public bool ShowAds => show_ads;
        public bool ShowInterstitial => show_interstitial;
        public bool ShowRewarded => show_rewarded;
        public float InterstitialIntervalSec => interstitial_interval_sec;
        public float AdFailTimeout => ad_fail_timeout;

        public override string ToString()
        {
            return $"[AdConfigData] show_ads={show_ads}, show_interstitial={show_interstitial}, " +
                   $"show_rewarded={show_rewarded}, interstitial_interval_sec={interstitial_interval_sec}, " +
                   $"ad_fail_timeout={ad_fail_timeout}";
        }
    }

    /// <summary>
    /// Centralized Zero-Backend Remote Ad Configuration System.
    /// Automatically fetches configs from: https://github.com/AzGamesStation/Game-Config
    /// 
    /// PLUG & PLAY:
    /// - No scene setup needed! Automatically initializes on game launch.
    /// - Use static helpers anywhere: RemoteAdConfig.ShowAds, RemoteAdConfig.ShowInterstitial, etc.
    /// - Subscribe with 1 line: RemoteAdConfig.OnReady(config => { ... });
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class RemoteAdConfig : MonoBehaviour
    {
        #region Static Quick Accessors (Zero Setup Needed)
        /// <summary>
        /// Master switch: Returns false if remote config disabled ads, or true by default.
        /// </summary>
        public static bool ShowAds => Instance != null ? Instance.CurrentConfig.show_ads : true;

        /// <summary>
        /// True if interstitial ads are permitted.
        /// </summary>
        public static bool ShowInterstitial => Instance != null ? Instance.CurrentConfig.show_interstitial : true;

        /// <summary>
        /// True if rewarded video ads are permitted.
        /// </summary>
        public static bool ShowRewarded => Instance != null ? Instance.CurrentConfig.show_rewarded : true;

        /// <summary>
        /// Cooldown in seconds between interstitial ads.
        /// </summary>
        public static float InterstitialInterval => Instance != null ? Instance.CurrentConfig.interstitial_interval_sec : 45f;

        /// <summary>
        /// Ad failure timeout in seconds.
        /// </summary>
        public static float AdFailTimeout => Instance != null ? Instance.CurrentConfig.ad_fail_timeout : 5f;

        /// <summary>
        /// True once the configuration has resolved (remote or safe fallback).
        /// </summary>
        public static bool IsReady => Instance != null && Instance.IsLoaded;

        /// <summary>
        /// Active configuration object.
        /// </summary>
        public static AdConfigData Config => Instance != null ? Instance.CurrentConfig : SafeDefaults;
        #endregion

        #region Singleton & Auto-Init
        public static RemoteAdConfig Instance { get; private set; }

        private static readonly AdConfigData SafeDefaults = new AdConfigData
        {
            show_ads = true,
            show_interstitial = true,
            show_rewarded = true,
            interstitial_interval_sec = 45f,
            ad_fail_timeout = 5f
        };

        // Automatically boots before first scene loads - no need to manually place in any scene!
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("[RemoteAdConfig]");
                Instance = go.AddComponent<RemoteAdConfig>();
                DontDestroyOnLoad(go);
            }
        }
        #endregion

        #region Events
        private static event Action<AdConfigData> OnConfigLoadedInternal;

        /// <summary>
        /// Single-line listener: Automatically calls your callback immediately if config
        /// has already loaded, or registers it to run as soon as download finishes.
        /// </summary>
        public static void OnReady(Action<AdConfigData> callback)
        {
            if (callback == null) return;

            if (IsReady)
            {
                callback.Invoke(Config);
            }
            else
            {
                OnConfigLoadedInternal += callback;
            }
        }
        #endregion

        #region Inspector Settings (Pre-Configured for AzGamesStation/Game-Config)
        [Header("GitHub Repository Configuration")]
        [Tooltip("GitHub username / organization")]
        [SerializeField] private string githubUsername = "AzGamesStation";

        [Tooltip("GitHub repository name")]
        [SerializeField] private string repositoryName = "Game-Config";

        [Tooltip("Branch name (usually 'main' or 'master')")]
        [SerializeField] private string branch = "main";

        [Tooltip("Folder containing the game json files")]
        [SerializeField] private string configsDirectory = "configs";

        [Header("Network Settings")]
        [Tooltip("Seconds before timeout and using fallback defaults")]
        [Range(2, 30)]
        [SerializeField] private int requestTimeoutSeconds = 8;

        [Header("Fallback Defaults")]
        [SerializeField] private AdConfigData defaultFallbackConfig = new AdConfigData();

#if UNITY_EDITOR
        [Header("Editor Debugging")]
        [Tooltip("Optional: Test a specific package name in the Unity Editor. Leave empty to use Application.identifier.")]
        [SerializeField] private string editorPackageNameOverride = "";
#endif
        #endregion

        #region State
        public bool IsLoaded { get; private set; }
        public bool IsRemoteLoaded { get; private set; }
        public AdConfigData CurrentConfig { get; private set; }
        public string TargetRawUrl { get; private set; }
        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentConfig = defaultFallbackConfig ?? SafeDefaults;
            FetchRemoteConfig();
        }

        public void FetchRemoteConfig()
        {
            StartCoroutine(FetchConfigRoutine());
        }

        public string GetPackageIdentifier()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(editorPackageNameOverride))
            {
                return editorPackageNameOverride.Trim();
            }
#endif
            return Application.identifier;
        }

        public string BuildRawGitHubUrl()
        {
            string packageId = GetPackageIdentifier();
            string folder = string.IsNullOrEmpty(configsDirectory) ? "" : $"{configsDirectory.Trim('/')}/";
            return $"https://raw.githubusercontent.com/{githubUsername.Trim()}/{repositoryName.Trim()}/{branch.Trim()}/{folder}{packageId}.json";
        }

        private IEnumerator FetchConfigRoutine()
        {
            TargetRawUrl = BuildRawGitHubUrl();
            string packageId = GetPackageIdentifier();

            // Cache-busting query parameter (?t=timestamp) prevents CDN from caching stale config
            long cacheBust = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string finalUrl = $"{TargetRawUrl}?t={cacheBust}";

            Debug.Log($"[RemoteAdConfig] Loading config for '{packageId}' from: {finalUrl}");

            using (UnityWebRequest request = UnityWebRequest.Get(finalUrl))
            {
                request.timeout = requestTimeoutSeconds;
                request.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                request.SetRequestHeader("Pragma", "no-cache");
                request.SetRequestHeader("Expires", "0");

                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isError = request.result != UnityWebRequest.Result.Success;
#else
                bool isError = request.isNetworkError || request.isHttpError;
#endif

                if (!isError && request.responseCode == 200)
                {
                    string json = request.downloadHandler.text;
                    AdConfigData parsed = null;

                    try
                    {
                        parsed = JsonUtility.FromJson<AdConfigData>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[RemoteAdConfig] JSON parse error: {ex.Message}. Falling back to default settings.");
                    }

                    if (parsed != null)
                    {
                        CurrentConfig = parsed;
                        IsLoaded = true;
                        IsRemoteLoaded = true;
                        Debug.Log($"[RemoteAdConfig] Successfully loaded remote config: {CurrentConfig}");
                        NotifyLoaded();
                        yield break;
                    }
                }
                else
                {
                    string reason = request.responseCode == 404
                        ? $"404 Not Found. Make sure 'configs/{packageId}.json' exists in repo 'https://github.com/{githubUsername}/{repositoryName}' and repository is PUBLIC."
                        : $"HTTP {request.responseCode}: {request.error}";

                    Debug.LogWarning($"[RemoteAdConfig] Could not fetch remote config ({reason}). Falling back to safe defaults.");
                }
            }

            // Safe fallback path (offline, 404, or parse error)
            CurrentConfig = defaultFallbackConfig ?? SafeDefaults;
            IsLoaded = true;
            IsRemoteLoaded = false;
            Debug.Log($"[RemoteAdConfig] Using safe fallback config: {CurrentConfig}");
            NotifyLoaded();
        }

        private void NotifyLoaded()
        {
            try
            {
                OnConfigLoadedInternal?.Invoke(CurrentConfig);
                OnConfigLoadedInternal = null; // Unregister one-shot subscribers
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RemoteAdConfig] Exception in OnReady callback: {ex}");
            }
        }
    }
}
