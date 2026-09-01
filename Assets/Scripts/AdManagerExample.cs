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
                Debug.Log($"[AdManager] Config loaded! ShowAds: {config.ShowAds}, Interstitial Cooldown: {config.InterstitialIntervalSec}s");

                if (config.ShowAds)
                {
                    InitializeAdNetworks();
                }
            });
        }

        private void InitializeAdNetworks()
        {
            // Example: Initialize Google Mobile Ads / AppLovin MAX / LevelPlay
            Debug.Log("[AdManager] Initializing Ad Networks...");
        }

        /// <summary>
        /// Call whenever an interstitial ad spot appears (e.g., Level Complete, Game Over).
        /// </summary>
        public void ShowInterstitial()
        {
            // Quick 1-line check using static properties anywhere in your project!
            if (!RemoteAdConfig.ShowAds || !RemoteAdConfig.ShowInterstitial)
            {
                Debug.Log("[AdManager] Interstitials currently disabled.");
                return;
            }

            // Respect cooldown interval
            if (Time.time - lastInterstitialTime < RemoteAdConfig.InterstitialInterval)
            {
                float remaining = RemoteAdConfig.InterstitialInterval - (Time.time - lastInterstitialTime);
                Debug.Log($"[AdManager] Interstitial on cooldown. Wait {remaining:F1}s.");
                return;
            }

            // Show Ad
            Debug.Log("[AdManager] Showing Interstitial Ad!");
            lastInterstitialTime = Time.time;
        }

        /// <summary>
        /// Call when player taps a "Watch Ad for Reward" button.
        /// </summary>
        public void ShowRewarded(Action onRewardEarned)
        {
            // Quick 1-line check
            if (!RemoteAdConfig.ShowAds || !RemoteAdConfig.ShowRewarded)
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
