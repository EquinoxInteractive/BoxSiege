<div align="center">

# BoxSiege

**A 2D Local Multiplayer Fighting Game Built with Unity**

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-black)](https://unity.com)
[![Language](https://img.shields.io/badge/Language-C%23-blueviolet)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](https://unity.com/download)
[![Players](https://img.shields.io/badge/Players-2P%20%7C%203P%20%7C%204P-orange)](https://github.com)
[![Status](https://img.shields.io/badge/Status-Active%20Development-brightgreen)](https://github.com)
[![Input System](https://img.shields.io/badge/Input-Unity%20New%20Input%20System-blue)](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

</div>

---

## Table of Contents

- [Overview](#overview)
- [Gameplay](#gameplay)
- [Features](#features)
- [Project Structure](#project-structure)
- [Characters](#characters)
- [Maps](#maps)
- [Power-Ups](#power-ups)
- [Round System](#round-system)
- [Sudden Death](#sudden-death)
- [Input System](#input-system)
- [Audio System](#audio-system)
- [Graphics Settings](#graphics-settings)
- [Code Reference](#code-reference)
- [Scene Flow](#scene-flow)
- [Installation](#installation)
- [Build Instructions](#build-instructions)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

BoxSiege is a local multiplayer 2D fighting game built in Unity, supporting 2, 3, or 4 players simultaneously on a single machine. Players choose their characters and weapons, select a map, and compete across multiple rounds to determine the ultimate winner. The game features a complete round win system, time-based gameplay, power-up pickups, an audio manager, and a Sudden Death tiebreaker mechanic.

The project uses Unity's New Input System for fully rebindable controls, supports keyboard and controller input per player, and includes a graphics settings manager with persistent preferences via PlayerPrefs.

---

## Gameplay

Each match consists of multiple rounds played on a selected map. A player wins a round by eliminating all other players. The first player to reach the win threshold wins the entire match. If the round timer expires and the win threshold has not been reached, a Sudden Death round is triggered under specific tie conditions.

**Win thresholds by player count:**

| Players | Rounds to Win Match |
|---------|---------------------|
| 2P      | 3 wins (max 5 rounds) |
| 3P      | 3 wins (max 7 rounds) |
| 4P      | 3 wins (max 9 rounds) |

---

## Features

- Local multiplayer for 2, 3, or 4 players on one machine
- Per-player character selection with unique alive and death sprites
- Per-player weapon selection from a shared weapon pool
- 4 selectable maps with individual scene loading
- 4 collectible power-up types with timed effects
- Complete round tracking with visual round box indicators per player
- Sudden Death tiebreaker round with knife-only movement
- Fully rebindable controls using Unity's New Input System
- AudioManager with per-category sound effects and background music
- Graphics settings with resolution, fullscreen, and quality persistence via PlayerPrefs
- Pause menu with resume, restart, and main menu navigation
- Healthbar system for all 4 players (P1, P2, P3, P4 each have dedicated components)

---

## Project Structure

```
BoxSiege/
├── Karakter/               -- Character ScriptableObjects, sprites, and selection logic
│   ├── CharacterData.cs
│   ├── CharacterSelection.cs
│   ├── GameData.cs
│   ├── P1 Sprite/
│   ├── P2 Sprite/
│   ├── P3 Sprite/
│   └── P4 Sprite/
├── Main Menu/              -- Main menu scene controller
│   └── Main Menu.cs
├── Maps/                   -- Map selector UI and scene loading
│   └── MapSelector.cs
├── Pause Script/           -- Core game manager and pause/options menu
│   ├── GameManger.cs
│   └── PauseOption.cs
├── PowerUp/                -- Power-up types, effects, manager, and timer UI
│   ├── PowerUp.cs
│   ├── PowerUpManager.cs
│   ├── PlayerPowerUpEffects.cs
│   ├── PowerUpTimerUI.cs
│   └── Prefab/
├── Resolution Setting/     -- Graphics and quality settings manager
│   ├── GraphicsSettings.cs
│   └── SettingsUI.cs
├── Script Controller/      -- Player movement, jumping, shooting, and bullet logic
│   ├── PlayerController.cs
│   ├── PlayerMovement.cs
│   ├── PlayeJump2D.cs
│   ├── PlayerShooting.cs
│   └── Bullet.cs
├── Health/                 -- Health component for P1
│   ├── Health.cs
│   └── Healthbar.cs
├── HealthP2/               -- Health component for P2
├── HealthP3/               -- Health component for P3
├── HealthP4/               -- Health component for P4
├── Timer Script/           -- Round timer and time selection UI
│   ├── Timer.cs
│   └── TimeSelection.cs
├── Round/                  -- Round indicator sprites and assets
├── Script Audio/           -- AudioManager script
├── Scripts/                -- Keybind rebinding UI scripts
│   └── Rebinding UI/
└── Scenes/
    ├── MainMenu.unity
    ├── 2PCharacterSelection.unity
    ├── 3PCharacterSelection.unity
    ├── 4PCharacterSelection.unity
    ├── TheDessert.unity
    ├── TheEarth.unity
    ├── TheHell.unity
    └── TheSnow.unity
```

---

## Characters

Characters are defined using Unity ScriptableObjects (`CharacterData.cs`). Each character holds an alive sprite, a death sprite, and a display name. All assets are assigned via the Inspector and are picked up at runtime by the `CharacterSelection` script.

```csharp
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    public Sprite aliveSprite;   // Sprite when the character is alive
    public Sprite deathSprite;   // Sprite when the character is eliminated
    public string characterName; // Display name shown in character selection UI
}
```

**Available characters per player slot:**

| Player | Characters |
|--------|-----------|
| P1 | Angry, Bandel, Melow, Ninja |
| P2 | Calm, Silat, Mime, Baik |
| P3 | Blind, Drunk, Joker, Spiderweaver |
| P4 | Bald-man, Deadswim, Savior, Spiderblack |

Each character slot has its own sprite folder (P1 Sprite, P2 Sprite, P3 Sprite, P4 Sprite) with alive and die variants.

---

## Maps

Maps are loaded by scene name using Unity's `SceneManager`. The `MapSelector` script manages preview images, display names, and navigation buttons in the UI. Map data is serialized as an array of `MapData` structs assigned via the Inspector.

```csharp
[System.Serializable]
public class MapData
{
    public string mapName;       // Scene name passed to SceneManager.LoadScene
    public string displayName;   // Name shown in the map selection UI
    public Sprite previewSprite; // Preview thumbnail sprite
}
```

**Available maps:**

| Scene Name    | Display Name |
|---------------|-------------|
| TheDessert    | The Dessert  |
| TheEarth      | The Earth    |
| TheHell       | The Hell     |
| TheSnow       | The Snow     |

Navigation between maps uses circular wrap-around (next from last returns to first, back from first returns to last).

---

## Power-Ups

Four power-up types are available, spawned at runtime by `PowerUpManager`. Each power-up type has a dedicated sprite and prefab. Power-ups do not apply to dead or eliminated players.

```csharp
public enum PowerUpType { Health, Shield, JumpBoost, SpeedBoost }
```

| Type       | Effect                                  | Duration    |
|------------|-----------------------------------------|-------------|
| Health     | Restores 1 HP to the collecting player  | Instant     |
| Shield     | Grants 2 shield charges                 | Charge-based|
| JumpBoost  | Multiplies jump height by 1.5x          | 10 seconds  |
| SpeedBoost | Multiplies movement speed by 1.5x       | 10 seconds  |

Power-up effects are applied through `PlayerPowerUpEffects`, which is a component attached to each player GameObject. Timed effects (JumpBoost, SpeedBoost) display a countdown timer in the UI via `PowerUpTimerUI`.

**Dead player check before applying:**

```csharp
bool playerIsDead = (playerHealth   != null && playerHealth.IsDead())   ||
                    (playerHealthP2 != null && playerHealthP2.IsDead()) ||
                    (playerHealthP3 != null && playerHealthP3.IsDead()) ||
                    (playerHealthP4 != null && playerHealthP4.IsDead());

if (playerIsDead) return; // Power-up stays in the world, not destroyed
```

Power-ups do not spawn during Sudden Death rounds.

---

## Round System

The round system is managed entirely by `GameManager`. Round win counts are tracked per player and displayed as filled color boxes in the UI. Round box colors differ between win state and default state and are configurable via the Inspector.

**Round flow:**

1. Round starts — "FIGHT!" text animates in and fades out
2. Players fight until one remains or the timer expires
3. If a winner is found, their win count increments and their round box fills
4. If a player reaches the win threshold, the match ends and the winner UI displays
5. If the timer expires and no tiebreak condition is met, the round is replayed

**Win threshold logic:**

```csharp
// 2P → need 3 wins; 3P and 4P → also 3 wins by default
// Max rounds act as a safety ceiling, not the primary win condition
private const int WINS_TO_WIN = 3;
private const int SUDDEN_DEATH_WINS = 2;
```

---

## Sudden Death

Sudden Death is triggered at timer expiry when two or more players are exactly tied at `SUDDEN_DEATH_WINS` (2 wins) and share the same highest HP value. Players not participating in Sudden Death are eliminated: their sprites switch to the death sprite, HP is set to 0, and movement is disabled.

**Sudden Death rules:**

- Participating players have HP reset to 3
- Only knife (melee) attacks are allowed — shooting is disabled
- Timer runs infinitely until a Sudden Death winner is found
- Power-ups do not spawn
- The round text displays "Sudden Death"
- The winner of the Sudden Death round wins the entire match

**Participation check:**

```csharp
// Only players with exactly SUDDEN_DEATH_WINS and the highest tied HP qualify
// Players with different win counts are excluded regardless of HP
```

---

## Input System

BoxSiege uses Unity's New Input System with one `.inputactions` asset per player. Controls are fully rebindable at runtime via the `Rebinding UI` scripts.

| Player | Input Asset File                      |
|--------|---------------------------------------|
| P1     | PlayerController.inputactions         |
| P2     | PlayerControllerP2.inputactions       |
| P3     | PlayerControllerP3.inputactions       |
| P4     | PlayerControllerP4.inputactions       |

Each player's actions are independently bound and stored. `KeybindResetButton.cs` provides a one-button reset to default bindings from the settings UI. Keyboard and gamepad (Xbox and PlayStation) are both supported with controller icon assets included (`XboxControllerIcons.asset`, `PSControllerIcons.asset`).

---

## Audio System

All sound effects and music are routed through a centralized `AudioManager` singleton. Scripts that need to play audio call `AudioManager` via `FindObjectOfType<AudioManager>()` and invoke `PlaySFX(AudioClip)`.

**AudioManager fields referenced in `PowerUp.cs`:**

```csharp
audioManager.healthPowerUp    // SFX for Health power-up pickup
audioManager.shieldPowerUp    // SFX for Shield power-up pickup
audioManager.jumpBoostPowerUp // SFX for JumpBoost power-up pickup
audioManager.speedBoostPowerUp// SFX for SpeedBoost power-up pickup
audioManager.jump             // Fallback SFX if specific clip is unassigned
```

The AudioManager is expected to be present in every gameplay scene. A missing AudioManager logs a `Debug.LogError` to prevent silent failures during development.

---

## Graphics Settings

`GraphicsSettings.cs` manages resolution, fullscreen mode, and quality level. All settings are persisted between sessions using `PlayerPrefs`. The settings UI (`SettingsUI.cs`) is accessible from both the main menu and the pause menu.

**Persistent keys used:**

```csharp
PlayerPrefs.GetInt("ResolutionIndex", defaultIndex);
PlayerPrefs.GetInt("FullscreenMode", 1);
PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
```

Changes apply immediately via `Screen.SetResolution` and `QualitySettings.SetQualityLevel`.

---

## Code Reference

### GameManager.cs (Pause Script/GameManger.cs)

The central script for the entire match. Handles:

- Player count detection (2P, 3P, 4P)
- Round start/end sequencing with coroutines
- Win count tracking and UI round box updates
- Match winner detection and winner UI display
- Sudden Death eligibility check and activation
- P3/P4 UI toggling based on player count
- PowerUp spawning enable/disable per round phase

**Key public fields:**

```csharp
// Win UI — assign in Inspector
public GameObject player1WinsUI;
public GameObject player2WinsUI;
public GameObject player3WinsUI;
public GameObject player4WinsUI;
public GameObject drawUI;

// Round UI
public GameObject      roundTextUI;
public TextMeshProUGUI roundText;
public GameObject      fightTextUI;
public GameObject      suddenDeathTextUI;

// Health references
public Health   player1Health;
public HealthP2 player2Health;
public HealthP3 player3Health;
public HealthP4 player4Health;

// Shooting references (disabled during Sudden Death)
public PlayerShooting player1Shooting;
public PlayerShooting player2Shooting;
public PlayerShooting player3Shooting;
public PlayerShooting player4Shooting;

// Round box images per player (filled on win)
public Image[] player1RoundBoxes;
public Image[] player2RoundBoxes;
public Image[] player3RoundBoxes;
public Image[] player4RoundBoxes;
```

---

### CharacterSelection.cs (Karakter/CharacterSelection.cs)

Handles character and weapon browsing for all player slots. Shared across three scenes by setting `numberOfPlayersInScene` in the Inspector (2, 3, or 4). P3 UI is hidden in 2P scenes; P4 UI is hidden in 2P and 3P scenes.

**GameData persistence:**

Selected character and weapon data are written to `GameData` (a static or persistent data container) so the gameplay scene can read the chosen assets at load time.

---

### PowerUpManager.cs (PowerUp/PowerUpManager.cs)

Manages power-up spawn points and spawn timing. During Sudden Death, the manager disables all spawning. Spawn intervals and maximum simultaneous power-ups are configurable via the Inspector.

---

### Timer.cs (Timer Script/Timer.cs)

Countdown timer that fires an event or calls a callback when it expires. The `GameManager` subscribes to the expiry event to evaluate Sudden Death conditions or end the round. During Sudden Death, the timer is suspended (infinite time).

---

### Health.cs / HealthP2.cs / HealthP3.cs / HealthP4.cs

Each player uses a dedicated health component to avoid cross-references in multiplayer. All expose `TakeDamage(float amount)` and `IsDead()` methods. Negative damage values in `TakeDamage` are used for healing (Health power-up passes `-1f`).

```csharp
// Heal by 1 HP using TakeDamage with a negative value
playerHealth.TakeDamage(-1f);
```

---

## Scene Flow

```
MainMenu.unity
    |
    |-- (2 Players) --> 2PCharacterSelection.unity
    |-- (3 Players) --> 3PCharacterSelection.unity
    |-- (4 Players) --> 4PCharacterSelection.unity
                            |
                            v
                    MapSelector UI (within CharacterSelection scene)
                            |
                            v
              TheDessert / TheEarth / TheHell / TheSnow
                            |
                            v
                    GameManager runs match
                            |
                        Match ends
                            |
                    Return to MainMenu.unity
```

---

## Installation

**Requirements:**

- Unity 2022.3 LTS or later (recommended)
- Unity Input System package (`com.unity.inputsystem`) version 1.5.1 or later
- TextMeshPro package (`com.unity.textmeshpro`)
- No external third-party plugins required

**Steps:**

1. Clone or download this repository.
2. Open Unity Hub and click **Open Project**.
3. Navigate to the project root folder and confirm.
4. Unity will import all assets automatically. Wait for the import to complete.
5. Open `Scenes/MainMenu.unity` as the starting scene.
6. Press **Play** to run the game in the Editor.

---

## Build Instructions

1. Go to **File > Build Settings**.
2. Add all scenes in this order:
   - `Scenes/MainMenu.unity`
   - `Scenes/2PCharacterSelection.unity`
   - `Scenes/3PCharacterSelection.unity`
   - `Scenes/4PCharacterSelection.unity`
   - `Scenes/TheDessert.unity`
   - `Scenes/TheEarth.unity`
   - `Scenes/TheHell.unity`
   - `Scenes/TheSnow.unity`
3. Select target platform (Windows, Linux, or macOS).
4. Click **Build** and choose an output folder.

**Recommended Player Settings:**

| Setting            | Value              |
|--------------------|--------------------|
| Default Resolution | 1920 x 1080        |
| Fullscreen Mode    | Fullscreen Window  |
| Target Frame Rate  | 60                 |
| Color Space        | Linear             |
| Input System       | New Input System   |

---

## Contributing

Contributions are welcome. To contribute:

1. Fork this repository.
2. Create a new branch: `git checkout -b feature/your-feature-name`
3. Make your changes and test them in the Unity Editor.
4. Commit with a clear message: `git commit -m "Add: description of change"`
5. Push your branch: `git push origin feature/your-feature-name`
6. Open a Pull Request with a description of what was changed and why.

**Coding conventions used in this project:**

- All scripts use PascalCase for class names and public fields
- Header attributes are used to group Inspector fields by category
- Debug.Log and Debug.LogError are used during development — strip or disable these in release builds
- Each player's health, UI, and audio components are kept separate to avoid tight coupling

---

## License

This project is currently unlicensed. All rights are retained by the original author unless otherwise specified. Contact the project maintainer before using assets or code from this project in other works.

---

<div align="center">

**BoxSiege — Built with Unity**

*A 2D local multiplayer fighting game for 2 to 4 players*

</div>