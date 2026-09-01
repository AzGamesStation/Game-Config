using System;
using UnityEngine;

namespace GameAnalyticsAndAds
{
    /// <summary>
    /// Example showing how clean and simple it is to use RemoteAdConfig in any game.
    /// </summary>
    public class AdManagerExample : MonoBehaviour
    {
        private float lastInterstitialTime = -999f;

        private void Start()
        {
            // 1-LINE SETUP:
            // Automatically fires when config is ready (or immediately if already downloaded).
            RemoteAdConfig.OnReady(config =>
            {
                Debug.Log($"[AdManager] Config loaded! HideAllAds: {config.HideAllAds}, MissionDelay: {config.MissionDelay}s, CanShowBanner: {config.CanShowBanner}");

                if (RemoteAdConfig.AdsEnabled)
                {
                    InitializeAdNetworks();

                    if (RemoteAdConfig.CanShowAppOpen && RemoteAdConfig.CanShowAppOpenOnStart)
                    {
                        ShowAppOpenAd();
                    }

                    if (RemoteAdConfig.CanShowBanner)
                    {
                        ShowBannerAd();
                    }
                }
            });
        }

        private void InitializeAdNetworks()
        {
            // Example: Initialize Google Mobile Ads / AppLovin MAX / LevelPlay
            Debug.Log("[AdManager] Initializing Ad Networks...");
        }

        public void ShowAppOpenAd()
        {
            if (RemoteAdConfig.CanShowAppOpen)
            {
                Debug.Log("[AdManager] Showing App Open Ad!");
            }
        }

        public void ShowBannerAd()
        {
            if (RemoteAdConfig.CanShowBanner)
            {
                Debug.Log("[AdManager] Showing Banner Ad!");
            }
        }

        public void ShowRectBannerAd()
        {
            if (RemoteAdConfig.CanShowRectBanner)
            {
                Debug.Log("[AdManager] Showing Rectangular (MREC) Banner Ad!");
            }
        }

        /// <summary>
        /// Call whenever an interstitial ad spot appears (e.g., Level Complete, Game Over).
        /// </summary>
        public void ShowInterstitial()
        {
            // Quick 1-line check using static properties anywhere in your project!
            if (!RemoteAdConfig.CanShowInterstitial)
            {
                Debug.Log("[AdManager] Interstitials currently disabled.");
                return;
            }

            // Show Ad
            Debug.Log("[AdManager] Showing Interstitial Ad!");
            lastInterstitialTime = Time.time;

            if (RemoteAdConfig.CanShowAppOpenAfterInterstitial)
            {
                ShowAppOpenAd();
            }
        }

        /// <summary>
        /// Call when player taps a "Watch Ad for Reward" button.
        /// </summary>
        public void ShowRewarded(Action onRewardEarned)
        {
            // Quick 1-line check
            if (!RemoteAdConfig.CanShowRewarded)
            {
                Debug.Log("[AdManager] Rewarded ads currently disabled.");
                return;
            }

            // Show Rewarded Ad
            Debug.Log("[AdManager] Showing Rewarded Video Ad!");
            onRewardEarned?.Invoke();
        }
    }
}
