using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameAnalyticsAndAds
{
    /// <summary>
    /// Attach this script to your Splash Screen Manager in your first scene.
    /// It links with RemoteAdConfig to ensure ad rules and Game ID are resolved
    /// before transitioning into the main menu or gameplay.
    /// </summary>
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [Tooltip("Name of the scene to load once splash and remote config are ready.")]
        [SerializeField] private string nextSceneName = "MainMenu";

        [Tooltip("Minimum duration in seconds to display the splash screen.")]
        [SerializeField] private float minimumSplashSeconds = 2.0f;

        [Header("App Open Ad on Start")]
        [Tooltip("Wait for App Open ad to dismiss before loading the next scene.")]
        [SerializeField] private bool waitForAppOpenAd = false;

        private float startTime;
        private bool isAdFinished = false;

        private void Start()
        {
            startTime = Time.time;
            StartCoroutine(SplashScreenFlowRoutine());
        }

        private IEnumerator SplashScreenFlowRoutine()
        {
            Debug.Log("[SplashScreen] Waiting for RemoteAdConfig to resolve...");

            // 1. Wait for RemoteAdConfig to finish fetching from GitHub (runs once)
            yield return new WaitUntil(() => RemoteAdConfig.IsReady);

            Debug.Log($"[SplashScreen] Config ready! GameId: '{RemoteAdConfig.GameId}', HideAllAds: {RemoteAdConfig.HideAllAds}, CanShowAppOpenOnStart: {RemoteAdConfig.CanShowAppOpenOnStart}");

            // 2. Control behavior according to remote config
            if (RemoteAdConfig.AdsEnabled)
            {
                // Check if App Open Ad is permitted on start
                if (RemoteAdConfig.CanShowAppOpen && RemoteAdConfig.CanShowAppOpenOnStart)
                {
                    Debug.Log("[SplashScreen] Triggering App Open Ad on start...");
                    ShowAppOpenAdOnStart();

                    if (waitForAppOpenAd)
                    {
                        yield return new WaitUntil(() => isAdFinished);
                    }
                }
            }
            else
            {
                Debug.Log("[SplashScreen] All ads are disabled by remote kill switch (HideAllAds=true).");
            }

            // 3. Ensure minimum splash screen display time
            float elapsed = Time.time - startTime;
            if (elapsed < minimumSplashSeconds)
            {
                yield return new WaitForSeconds(minimumSplashSeconds - elapsed);
            }

            // 4. Proceed to main game / menu
            ProceedToNextScene();
        }

        private void ShowAppOpenAdOnStart()
        {
            // Call your ad mediation SDK (e.g. Google Mobile Ads / AppLovin MAX / LevelPlay)
            // Example:
            // GoogleMobileAdsAppOpenManager.Instance.ShowAdIfAvailable(() => isAdFinished = true);
            isAdFinished = true;
        }

        private void ProceedToNextScene()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[SplashScreen] Loading next scene: '{nextSceneName}'");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
