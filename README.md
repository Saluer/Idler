# Roguelike

A wave-based roguelike arena shooter built in Unity. Fight through escalating enemy waves, buy upgrades and buffs between rounds, and survive as long as you can.

**Style**: Mid-poly, Madness Combat / Megabonk inspired.

## Prerequisites

- **Unity 6000.3.0f1** (Unity 6)
- **Blender 3.x+** (only for hero model generation)
- Git

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/Saluer/Idler.git
   cd Roguelike
   ```

2. Open the project in Unity Hub — select folder and confirm the editor version.

3. Open the main scene: `Assets/_Game/Scenes/SampleScene.unity`

4. Press Play.

## Project Structure

```
Assets/
├── _Game/
│   ├── Configs/            # ScriptableObject configs (enemy levels, arena)
│   │   ├── Arena/          # Arena module configs
│   │   └── Enemy/          # Enemy level configs (1-4 + Boss 1)
│   ├── Materials/
│   ├── Models/             # FBX models (Bandit, Bazooka, Shotgun)
│   ├── Prefabs/            # Gameplay prefabs
│   │   ├── Weapon/         # Sword, Pistol, Shotgun, Rocket Launcher
│   │   ├── Enemy/          # Enemy variants + boss
│   │   └── Buffs/          # Buff chests (speed, attack speed)
│   ├── Resources/          # Runtime-loaded assets (animations, icons)
│   ├── Scenes/
│   │   ├── Entring scene.unity   # Main menu
│   │   └── SampleScene.unity     # Gameplay scene
│   ├── Scripts/            # All game code
│   │   ├── Enemy/          # Enemy scripts (base + variants)
│   │   ├── Weapon/         # Weapon system (IWeapon + implementations)
│   │   └── Сonfig/         # Wave modifier config
│   └── Settings/           # URP render pipeline settings
├── Tests/
│   └── EditMode/           # NUnit edit-mode tests
└── _ThirdParty/            # Third-party asset packs
```

## Core Systems

### Game Loop (GameManager)

Singleton-based round manager. Each round:
1. Roll 1-2 random **wave modifiers** (announced on countdown screen)
2. 60-second countdown
3. Enemies spawn via `SpawnerScript` (adaptive spawn rate — accelerates as enemies die)
4. Kill all enemies to end the round
5. Shop phase — spend gold/diamonds on buffs, weapons, abilities

**Economy**: Enemies drop gold; mines produce diamonds; gold can be converted to diamonds.

### Weapons (IWeapon)

All weapons implement `IWeapon` and are auto-attacked against the closest enemy each frame.

| Weapon | Type | Notes |
|--------|------|-------|
| Sword | Melee | Hitbox-based |
| Pistol | Ranged | Single shot |
| Shotgun | Ranged | Multi-pellet spread |
| Rocket Launcher | Ranged | Explosive projectiles |

Weapons have tiered upgrades (`WeaponUpgradeManager`) purchased with diamonds. Each tier applies damage/cooldown multipliers and bonus pellets.

### Enemies

**Base** (`EnemyScript`) — rigidbody movement toward player, health/damage/speed/gold, wave modifier support, 15% chest drop chance.

| Variant | Behaviour |
|---------|-----------|
| EnemyRanged | Maintains distance, shoots projectiles |
| EnemyChampion | Aura-buffs nearby enemies, spawns 2-4 small copies on death |
| EnemyDodger | Sine-wave strafing, 1.5x speed |
| EnemyBuffer | Shields/buffs nearby enemies |

### Wave Modifiers (WaveModifierManager)

Random effects applied to enemy waves. Multipliers stack multiplicatively.

| Modifier | Effect |
|----------|--------|
| Ant Invasion | 0.4x scale, 3x count, 0.5x health |
| Matryoshka | Split into 2 smaller copies on death (up to 2 generations) |
| Thicc Boys | 2x scale, 2x health, 0.5x speed, 2x gold |
| Explosive Finale | Enemies explode on death |
| Speed Dating | 3x speed, 0.01x health |
| Bouncers | 5x knockback |
| Jackpot | 3x gold, 2x health |
| Ghost Protocol | Semi-transparent enemies |

### Buffs (BuffHub)

Permanent upgrades purchased with diamonds (20 each, Chain Reaction 25):

- **Vampire** — heal HP per kill
- **Thorns** — reflect damage when hit
- **Chain Reaction** — 10% chance per level for kill-explosion
- **Giant Slayer** — +50% damage per level vs enemies scaled > 1.05x
- **Fire Touch** — damage on enemy collision

### Active Abilities (ActiveAbilityManager)

Charge-based abilities activated with hotkeys 1/2/3:

| Key | Ability | Cost | Effect |
|-----|---------|------|--------|
| 1 | Frost | 15 gold | Freeze all enemies for 4s |
| 2 | Explosion | 20 gold | AOE damage (8 radius, 3 damage) |
| 3 | Convert | 10 gold | 10 gold → 1 diamond |

## Controls

- **WASD** — Move
- **Mouse** — Look
- **Space** — Jump
- **1 / 2 / 3** — Active abilities

## Running Tests

Tests use NUnit via Unity Test Framework (edit-mode only).

### From Unity Editor

1. Open **Window > General > Test Runner**
2. Select the **EditMode** tab
3. Click **Run All**

### From Command Line

```bash
Unity -runTests -batchmode -projectPath . -testResults results.xml -testPlatform EditMode
```

Or on macOS with the default install path:

```bash
/Applications/Unity/Hub/Editor/6000.3.0f1/Unity.app/Contents/MacOS/Unity \
  -runTests -batchmode -nographics \
  -projectPath "$(pwd)" \
  -testResults results.xml \
  -testPlatform EditMode
```

### Test Coverage

| File | Covers |
|------|--------|
| `BuffHubTests.cs` | Buff purchases, Giant Slayer damage scaling, stacking, insufficient funds |
| `WaveModifierManagerTests.cs` | Modifier rolling, multiplier stacking, boolean flags, announcements |
| `WeaponUpgradeTests.cs` | Tier tracking, diamond costs, upgrade application, multi-weapon separation |
| `EnemyMechanicsTests.cs` | Enemy modifier configs, Chain Reaction chances, Giant Slayer combos |
| `TestHelpers.cs` | Shared setup/teardown utilities, reflection-based manager creation |

### Adding New Tests

1. Create a new `.cs` file in `Assets/Tests/EditMode/`
2. Reference the `Game` assembly (already configured in `Tests.EditMode.asmdef`)
3. Use `[SetUp]` / `[TearDown]` with helpers from `TestHelpers.cs`:
   ```csharp
   [SetUp]
   public void SetUp()
   {
       TestHelpers.ResetBuffHub();
       _gm = TestHelpers.CreateGameManager();
   }

   [TearDown]
   public void TearDown()
   {
       TestHelpers.Cleanup();
   }
   ```

## Hero Model Generation

The `generate_hero.py` script procedurally creates the bandit hero character in Blender.

```bash
blender --background --python generate_hero.py
```

This exports `Hero.fbx` to `Assets/_Game/Models/`. You can also run it interactively from Blender's Scripting tab.
