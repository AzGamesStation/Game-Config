# Zero-Backend Remote Ad Configuration for Unity

Configured for **`https://github.com/AzGamesStation/Game-Config`**

A plug-and-play, zero-backend ad configuration system for all your Unity games. 

---

## ⚡ 3-Step Setup (As Simple As It Gets)

### Step 1: Ensure Your GitHub Repository is Public
Make sure **`https://github.com/AzGamesStation/Game-Config`** is set to **Public** on GitHub (Settings > General > Change repository visibility > Make public).
*(GitHub Raw CDN only serves files without authentication if the repo is public).*

---

### Step 2: Add Game Configs to GitHub

Inside your repository, create a `configs/` folder and add a `.json` file named strictly after each game's package name (`Application.identifier`):

```
Game-Config/
└── configs/
    ├── com.company.game1.json
    └── com.azgames.yourgame.json
```

**Sample JSON file content:**
```json
{
  "show_ads": true,
  "show_interstitial": true,
  "show_rewarded": true,
  "interstitial_interval_sec": 45.0,
  "ad_fail_timeout": 5.0
}
```

---

### Step 3: Drop `RemoteAdConfig.cs` into Unity

1. Copy **[`RemoteAdConfig.cs`](file:///c:/Users/Zayn%20Iftikhar/Documents/antigravity/proud-carson/Assets/Scripts/RemoteAdConfig.cs)** into your Unity project's `Assets` folder.
2. **You're done!** 
   - No need to create any GameObject in the scene.
   - It automatically starts on game launch before the first scene loads (`[RuntimeInitializeOnLoadMethod]`).
   - Automatically detects your package name (`Application.identifier`) and downloads `configs/<package>.json` from GitHub.
   - Automatically handles offline mode & 404s by falling back to safe local defaults.

---

## 🎮 How to Use in Your Game Code

### 1. Wait for Config on Launch (1 line)
In your game manager or ad manager:

```csharp
using UnityEngine;
using GameAnalyticsAndAds;

public class MyAdManager : MonoBehaviour
{
    private void Start()
    {
        RemoteAdConfig.OnReady(config =>
        {
            if (config.ShowAds)
            {
                // Initialize AdMob, AppLovin MAX, LevelPlay, etc.
            }
        });
    }
}
```

### 2. Query Ad Rules Anywhere (1 line)
You can access remote ad flags directly from any script across your entire game:

```csharp
// Check if ads or interstitials are enabled
if (RemoteAdConfig.ShowAds && RemoteAdConfig.ShowInterstitial)
{
    // Show interstitial
}

// Get the cooldown timer
float cooldown = RemoteAdConfig.InterstitialInterval; // e.g. 45.0 seconds
```

---

## 🛠️ Testing in the Unity Editor

In the Unity Editor, `Application.identifier` may be `com.DefaultCompany.MyGame`.

If you want to test a specific game's config in the Editor without changing your Unity Player Settings:
1. Create an empty GameObject in your scene and attach `RemoteAdConfig`.
2. Under **Editor Debugging**, set **Editor Package Name Override** to your game's package name (e.g. `com.azgames.yourgame`).
3. Press Play!
