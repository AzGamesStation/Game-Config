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

Inside your repository, create a `configs/` folder and add a `.json` file named after each game's **Game ID** (e.g. `az-1012.json`, `sd-2021.json`) or package name:

```
Game-Config/
└── configs/
    ├── az-1012.json
    ├── sd-2021.json
    ├── com.az2.game3.json
    └── com.azgame.gameone.json
```

**Sample JSON file content:**
```json
{
  "GameId": "az-1012",
  "ProjectName": "com.az2.game3",
  "HideAllAds": false,
  "CanShowAppOpen": true,
  "CanShowAppOpenOnStart": true,
  "CanShowAppOpenOnResume": true,
  "CanShowAppOpenAfterInterstitial": false,
  "CanShowAppOpenAfterInterstitial2": false,
  "CanShowBanner": true,
  "CanShowInterstitial": true,
  "CanShowRewarded": true,
  "CanShowRectBanner": true,
  "MissionDelay": 60
}
```

---

### Step 3: Link in Your Splash Screen (Single Script & Runs Once)

1. Drop **[`RemoteAdConfig.cs`](file:///Assets/Scripts/RemoteAdConfig.cs)** into your Unity project's `Assets` folder.
2. In your **Splash Screen** scene:
   - Attach `RemoteAdConfig` to a GameObject.
   - Enter your **Game Code** in the Inspector (e.g. `az-1012` or `sd-2021`). If left empty, it will automatically look up via your package name (`Application.identifier`).
   - It runs **once**, caches results, and persists across scenes (`DontDestroyOnLoad`).

#### Dual Lookup & Fallback Hierarchy:
- **Game Code Provided:** Fetches `configs/{GameCode}.json`. If not found, automatically falls back to search through `configs/{PackageName}.json`. If still not found, logs:
  `Game ID '{GameCode}' not found, and no json file found with package name '{PackageName}'. Using fallback defaults.`
- **Game Code Empty:** Logs `No Game ID given. Searching through package name '{PackageName}'...`. If not found, logs:
  `No ID given, and no json file found with package name '{PackageName}'. Using fallback defaults.`

---

## 🎮 How to Use in Splash Screen & Game Code

### 1. Splash Screen Controller
Use [`SplashScreenController.cs`](file:///Assets/Scripts/SplashScreenController.cs) on your Splash Screen manager:

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameAnalyticsAndAds;

public class MySplashScreen : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 1. Wait for RemoteAdConfig to resolve
        yield return new WaitUntil(() => RemoteAdConfig.IsReady);

        // 2. Control behavior accordingly
        if (RemoteAdConfig.AdsEnabled && RemoteAdConfig.CanShowAppOpen && RemoteAdConfig.CanShowAppOpenOnStart)
        {
            // Show App Open Ad on launch
        }

        // 3. Proceed to Main Menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
```

### 2. Query Ad Rules Anywhere Across Your Game (1 line)
```csharp
// Check if ads are enabled and interstitial is allowed
if (RemoteAdConfig.AdsEnabled && RemoteAdConfig.CanShowInterstitial)
{
    // Show interstitial
}

// Check App Open rules
if (RemoteAdConfig.CanShowAppOpen && RemoteAdConfig.CanShowAppOpenOnResume)
{
    // Show App Open on resume
}

// Get the mission delay
int delay = RemoteAdConfig.MissionDelay; // e.g. 60 seconds

// Access Game ID & Project Name
string id = RemoteAdConfig.GameId; // e.g. "az-1012"
```

---

## 🌐 Web Dashboard Features
Open [`admin.html`](file:///admin.html) in any web browser:
- **Instant Search:** Search across all games simultaneously by **Game ID** (`az-1012`) or **Project Name** (`com.azgame`).
- **All 11 Keys Supported:** Visual toggles and delay inputs with clean cards.
- **Works Without Token:** Public read mode lets anyone inspect configs. Connect a GitHub Personal Access Token (`repo` scope) to save and publish live updates in 1 click!
