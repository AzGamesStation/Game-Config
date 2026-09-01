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
        [Tooltip("Game ID string format (e.g. az-1012 or sd-2021).")]
        public string GameId = "";

        [Tooltip("Optional project / package name (Application.identifier).")]
        public string ProjectName = "";

        [Tooltip("Master kill switch: If true, all ads are completely disabled across the game.")]
        public bool HideAllAds = false;

        [Tooltip("Master switch for App Open ads.")]
        public bool CanShowAppOpen = true;

        [Tooltip("Allow showing App Open ads on application cold start / launch.")]
        public bool CanShowAppOpenOnStart = true;

        [Tooltip("Allow showing App Open ads when resuming from background.")]
        public bool CanShowAppOpenOnResume = true;

        [Tooltip("Allow showing App Open ads after an interstitial ad completes.")]
        public bool CanShowAppOpenAfterInterstitial = false;

        [Tooltip("Alternative/secondary rule for App Open after interstitial.")]
        public bool CanShowAppOpenAfterInterstitial2 = false;

        [Tooltip("Allow showing standard Banner ads.")]
        public bool CanShowBanner = true;

        [Tooltip("Allow showing Interstitial ads.")]
        public bool CanShowInterstitial = true;

        [Tooltip("Allow showing Rewarded video ads.")]
        public bool CanShowRewarded = true;

        [Tooltip("Allow showing Rectangular / MREC banner ads.")]
        public bool CanShowRectBanner = true;

        [Tooltip("Delay in seconds before missions/gameplay can trigger ads.")]
        public int MissionDelay = 60;

        // Convenient helpers
        public bool AdsEnabled => !HideAllAds;

        // Backwards compatibility helpers
        public bool show_ads => !HideAllAds;
        public bool show_interstitial => CanShowInterstitial;
        public bool show_rewarded => CanShowRewarded;

        public override string ToString()
        {
            return $"[AdConfigData] HideAllAds={HideAllAds}, CanShowAppOpen={CanShowAppOpen}, " +
                   $"CanShowAppOpenOnStart={CanShowAppOpenOnStart}, CanShowAppOpenOnResume={CanShowAppOpenOnResume}, " +
                   $"CanShowAppOpenAfterInterstitial={CanShowAppOpenAfterInterstitial}, " +
                   $"CanShowAppOpenAfterInterstitial2={CanShowAppOpenAfterInterstitial2}, " +
                   $"CanShowBanner={CanShowBanner}, CanShowInterstitial={CanShowInterstitial}, " +
                   $"CanShowRewarded={CanShowRewarded}, CanShowRectBanner={CanShowRectBanner}, " +
                   $"MissionDelay={MissionDelay}";
        }
    }

    /// <summary>
    /// Centralized Zero-Backend Remote Ad Configuration System.
    /// Automatically fetches configs from: https://github.com/AzGamesStation/Game-Config
    /// 
    /// PLUG & PLAY:
    /// - No scene setup needed! Automatically initializes on game launch.
    /// - Use static helpers anywhere: RemoteAdConfig.HideAllAds, RemoteAdConfig.CanShowInterstitial, etc.
    /// - Subscribe with 1 line: RemoteAdConfig.OnReady(config => { ... });
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class RemoteAdConfig : MonoBehaviour
    {
        #region Static Quick Accessors (Zero Setup Needed)
        /// <summary>
        /// Remote Game ID code (e.g. az-1012, sd-2021).
        /// </summary>
        public static string GameId => Instance != null ? Instance.CurrentConfig.GameId : "";

        /// <summary>
        /// Remote Project Name (Application.identifier).
        /// </summary>
        public static string ProjectName => Instance != null ? Instance.CurrentConfig.ProjectName : "";

        /// <summary>
        /// Master switch: Returns true if all ads are hidden/disabled.
        /// </summary>
        public static bool HideAllAds => Instance != null ? Instance.CurrentConfig.HideAllAds : false;

        /// <summary>
        /// True if ads are enabled (!HideAllAds).
        /// </summary>
        public static bool AdsEnabled => !HideAllAds;

        /// <summary>
        /// Master switch for App Open ads.
        /// </summary>
        public static bool CanShowAppOpen => Instance != null ? (Instance.CurrentConfig.CanShowAppOpen && !Instance.CurrentConfig.HideAllAds) : true;

        /// <summary>
        /// True if App Open can be shown on start.
        /// </summary>
        public static bool CanShowAppOpenOnStart => Instance != null ? Instance.CurrentConfig.CanShowAppOpenOnStart : true;

        /// <summary>
        /// True if App Open can be shown on resume.
        /// </summary>
        public static bool CanShowAppOpenOnResume => Instance != null ? Instance.CurrentConfig.CanShowAppOpenOnResume : true;

        /// <summary>
        /// True if App Open can be shown after interstitial.
        /// </summary>
        public static bool CanShowAppOpenAfterInterstitial => Instance != null ? Instance.CurrentConfig.CanShowAppOpenAfterInterstitial : false;

        /// <summary>
        /// True if App Open can be shown after interstitial (rule 2).
        /// </summary>
        public static bool CanShowAppOpenAfterInterstitial2 => Instance != null ? Instance.CurrentConfig.CanShowAppOpenAfterInterstitial2 : false;

        /// <summary>
        /// True if banner ads are permitted.
        /// </summary>
        public static bool CanShowBanner => Instance != null ? (Instance.CurrentConfig.CanShowBanner && !Instance.CurrentConfig.HideAllAds) : true;

        /// <summary>
        /// True if rectangular/MREC banner ads are permitted.
        /// </summary>
        public static bool CanShowRectBanner => Instance != null ? (Instance.CurrentConfig.CanShowRectBanner && !Instance.CurrentConfig.HideAllAds) : true;

        /// <summary>
        /// True if interstitial ads are permitted.
        /// </summary>
        public static bool CanShowInterstitial => Instance != null ? (Instance.CurrentConfig.CanShowInterstitial && !Instance.CurrentConfig.HideAllAds) : true;

        /// <summary>
        /// True if rewarded video ads are permitted.
        /// </summary>
        public static bool CanShowRewarded => Instance != null ? (Instance.CurrentConfig.CanShowRewarded && !Instance.CurrentConfig.HideAllAds) : true;

        /// <summary>
        /// Mission delay in seconds before gameplay/missions can show ads.
        /// </summary>
        public static int MissionDelay => Instance != null ? Instance.CurrentConfig.MissionDelay : 60;

        /// <summary>
        /// Backwards compatibility helper for ShowAds.
        /// </summary>
        public static bool ShowAds => AdsEnabled;

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
            HideAllAds = false,
            CanShowAppOpen = true,
            CanShowAppOpenOnStart = true,
            CanShowAppOpenOnResume = true,
            CanShowAppOpenAfterInterstitial = false,
            CanShowAppOpenAfterInterstitial2 = false,
            CanShowBanner = true,
            CanShowInterstitial = true,
            CanShowRewarded = true,
            CanShowRectBanner = true,
            MissionDelay = 60
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

        #region Inspector Settings
        [Header("Game Identification")]
        [Tooltip("Game ID string format (e.g. az-1012, sd-2021). If left blank, RemoteAdConfig will fetch using Application.identifier.")]
        [SerializeField] private string gameCode = "";

        [Header("Splash Screen & Lifecycle")]
        [Tooltip("Keep GameObject persistent between scenes.")]
        [SerializeField] private bool persistAcrossScenes = true;

        [Tooltip("Automatically start fetching on Awake/Start (runs once).")]
        [SerializeField] private bool autoFetchOnStart = true;

        [Tooltip("UnityEvent fired as soon as configuration is ready (convenient for Splash Screen UI/scenes).")]
        [SerializeField] private UnityEvent<AdConfigData> onConfigLoaded;

        [Header("GitHub Repository Configuration")]
        [Tooltip("GitHub username / organization")]
        [SerializeField] private string githubUsername = "AzGamesStation";

        [Tooltip("GitHub repository name")]
        [SerializeField] private string repositoryName = "Game-Config";

        [Tooltip("Branch name (usually 'main')")]
        [SerializeField] private string branch = "main";

        [Tooltip("Folder containing the game json files")]
        [SerializeField] private string configsDirectory = "configs";

        [Header("Network Settings")]
        [Tooltip("Seconds before timeout for each request")]
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

        private bool isFetching = false;
        private bool hasFetched = false;
        #endregion

        public string GameCode
        {
            get => gameCode;
            set => gameCode = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            CurrentConfig = defaultFallbackConfig ?? SafeDefaults;

            if (autoFetchOnStart)
            {
                FetchRemoteConfig();
            }
        }

        /// <summary>
        /// Fetches remote config. Runs once; ignores repeat calls unless forceRefresh is true.
        /// Ideal to link in Splash Screen.
        /// </summary>
        public void FetchRemoteConfig(bool forceRefresh = false)
        {
            if (hasFetched && !forceRefresh)
            {
                NotifyLoaded();
                return;
            }

            if (isFetching) return;

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

        private string BuildRawGitHubUrl(string fileIdentifier, long cacheBust)
        {
            string folder = string.IsNullOrEmpty(configsDirectory) ? "" : $"{configsDirectory.Trim('/')}/";
            return $"https://raw.githubusercontent.com/{githubUsername.Trim()}/{repositoryName.Trim()}/{branch.Trim()}/{folder}{fileIdentifier.Trim()}.json?t={cacheBust}";
        }

        private IEnumerator FetchConfigRoutine()
        {
            isFetching = true;
            string code = gameCode?.Trim();
            string packageId = GetPackageIdentifier();
            long cacheBust = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool loaded = false;

            // Scenario 1: Game Code is provided
            if (!string.IsNullOrEmpty(code))
            {
                string codeUrl = BuildRawGitHubUrl(code, cacheBust);
                TargetRawUrl = codeUrl;
                Debug.Log($"[RemoteAdConfig] Loading config using Game Code '{code}' from: {codeUrl}");

                yield return DownloadAndParseJson(codeUrl, success => loaded = success);

                if (loaded)
                {
                    Debug.Log($"[RemoteAdConfig] Successfully loaded remote config for Game ID: '{code}'");
                }
                else
                {
                    Debug.LogWarning($"[RemoteAdConfig] No json file found for Game ID '{code}'. Searching through package name '{packageId}'...");

                    string pkgUrl = BuildRawGitHubUrl(packageId, cacheBust);
                    TargetRawUrl = pkgUrl;
                    yield return DownloadAndParseJson(pkgUrl, success => loaded = success);

                    if (loaded)
                    {
                        Debug.Log($"[RemoteAdConfig] Successfully loaded remote config using fallback Package Name: '{packageId}'");
                    }
                    else
                    {
                        Debug.LogError($"[RemoteAdConfig] Game ID '{code}' not found, and no json file found with package name '{packageId}'. Using fallback defaults.");
                    }
                }
            }
            // Scenario 2: Game Code is NOT provided (empty string)
            else
            {
                Debug.Log($"[RemoteAdConfig] No Game ID given. Searching through package name '{packageId}'...");

                string pkgUrl = BuildRawGitHubUrl(packageId, cacheBust);
                TargetRawUrl = pkgUrl;
                yield return DownloadAndParseJson(pkgUrl, success => loaded = success);

                if (loaded)
                {
                    Debug.Log($"[RemoteAdConfig] Successfully loaded remote config using Package Name: '{packageId}'");
                }
                else
                {
                    Debug.LogError($"[RemoteAdConfig] No ID given, and no json file found with package name '{packageId}'. Using fallback defaults.");
                }
            }

            if (!loaded)
            {
                CurrentConfig = defaultFallbackConfig ?? SafeDefaults;
                IsLoaded = true;
                IsRemoteLoaded = false;
                Debug.Log($"[RemoteAdConfig] Using safe fallback config: {CurrentConfig}");
            }

            hasFetched = true;
            isFetching = false;
            NotifyLoaded();
        }

        private IEnumerator DownloadAndParseJson(string url, Action<bool> onComplete)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
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
                        Debug.LogWarning($"[RemoteAdConfig] JSON parse error: {ex.Message}");
                    }

                    if (parsed != null)
                    {
                        CurrentConfig = parsed;
                        IsLoaded = true;
                        IsRemoteLoaded = true;
                        onComplete?.Invoke(true);
                        yield break;
                    }
                }

                onComplete?.Invoke(false);
            }
        }

        private void NotifyLoaded()
        {
            try
            {
                onConfigLoaded?.Invoke(CurrentConfig);
                OnConfigLoadedInternal?.Invoke(CurrentConfig);
                OnConfigLoadedInternal = null; // Unregister one-shot subscribers
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RemoteAdConfig] Exception in config ready callback: {ex}");
            }
        }
    }
}
