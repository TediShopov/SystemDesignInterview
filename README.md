# Introduction

This file helps you navigate and understand the source code of system I have developed. 
For more background on the projects and their systems, see the [project page](https://tedishopov.github.io/TootTheLute.html).
Use **Quick Navigation** to jump to sections. Within each section, files are listed by importance, each with a one-line summary and a direct source link.   
Brief glossaries and context notes clarify naming and intent.

## Quick Navigation

- [Toot The Lute](#toot-the-lute)
  - [Beat/Rhythm System](#beatrhythm-system)
  - [Beat-Triggered Interactables](#beat-triggered-interactables)
- [RatKing](#ratking)
  - [Inventory System](#inventory-system)
  - [Throwing (Exit Point)](#throwing--projectile-simulation-system-exit-point-from-inventory)
  - [Collecting (Entry Point)](#collecting-entry-point-to-inventory)
- [Rollback Netcode Fighting Game](#rollback-netcode-fighting-game)
  - [Rollback System](#rollback-system)
- [Computer Graphics Project](#destructible-terrain---dx11-computer-graphics-project)
  - [Destructible Terrain](#destructible-terrain---dx11-computer-graphics-project)

---

# Toot The Lute

### Legend
- **Crochet** — Seconds per beat: `crochet = 60 / BPM`. Converts beat lengths to seconds.
- **Beat** — Musical event with a `Length` (in beats); real-time length is `Length * crochet`.
- **Reference Target** — Transform where the player should “hit” the beat (timing line).
- **Input Offset** — Calibration to align input with audio timing.
- **Visual Offset** — Calibration to align visuals with audio timing.

## Beat/Rhythm System
Related files live in `Assets/Scripts/BeatSystem`.

- [BeatmapTrack.cs](Assets/Scripts/BeatSystem/BeatmapTrack.cs) — Orchestrates the on-screen track; spawns/recycles beats from the active beatmap; evaluates player timing; maps audio time to track position.
- [BeatObject.cs](Assets/Scripts/BeatSystem/BeatObject.cs) — Visual/interactive unit for a single beat; tracks state (Approaching/Successful/Unsuccessful); raises resolution events; updates post-resolution effects.
- [Conductor.cs](Assets/Scripts/BeatSystem/Conductor.cs) — Time authority; reads the Wwise playhead and publishes `SongPosition` (seconds); exposes Play/Resume/Pause/Stop Wwise events.

**Referenced types (context):** **Beatmap** (flattened beats; holds `Conductor`), **Beat** (timing/length), **BeatmapSource** (Wwise events), **InputCalibration** (audio/visual offsets).

## Beat-Triggered Interactables
Related files live in `Assets/Scripts/Interactables`.

- [InteractableReplenishingHealing.cs](TootTheLute/Assets/Scripts/Interactables/InteractableReplenishingHealing.cs) — Spawns discrete healing charges up to a cap; heals proportional to charges.
- [InteractableObject.cs](TootTheLute/Assets/Scripts/Interactables/InteractableObject.cs) — Attackable node that emits a radial burst of homing projectiles.
- [InteractableMeleeObject.cs](TootTheLute/Assets/Scripts/Interactables/InteractableMeleeObject.cs) — Spawns a short-lived melee strike opposite the player.

---

# RatKing

## Inventory System
Related files live in `Ratking/Assets/Scripts/Inventory`.

- [Inventory.cs](Ratking/Assets/Scripts/Inventory/Inventory.cs) — Core inventory data and rules: item list, placement validation, gold totals, persistence events.
- [IPlayerInventory.cs](Ratking/Assets/Scripts/Inventory/IPlayerInventory.cs) — Interface for inventory access: grid size, item list, placement/selection API, update events.
- [InventoryGridView.cs](Ratking/Assets/Scripts/Inventory/InventoryGridView.cs) — Grid UI/controller: builds cells, previews placement, handles pick/place input, updates labels.
- [InventoryObject.cs](Ratking/Assets/Scripts/Inventory/InventoryObject.cs) — UI for a single item: tracks occupied cells, centers/sizes sprite on the grid, bridges to collectibles.
- [InventoryCell.cs](Ratking/Assets/Scripts/Inventory/InventoryCell.cs) — Visual cell widget; stores default colors; flips between default/valid/invalid during placement.

## Throwing & Projectile Simulation System (Exit Point from Inventory)
- [Thrower.cs](Ratking/Assets/Scripts/Inventory/Thrower.cs) — Bridges inventory to world: simulates/throws selected item, predicts trajectory/sound, raises OnThrow event.

## Collecting (Entry Point to Inventory)
- [Collectible.cs](Ratking/Assets/Scripts/Items/Collectible.cs) — Physical item with inventory metadata; handles breakage threshold; emits impact sounds.

---

# Rollback Netcode Fighting Game

## Rollback System

**How buffers work (live vs. rollback shadows):**
- **Live fighter buffers** — Each fighter has a `FighterBufferMono` that creates/owns an `InputBuffer` in `Awake()`, copies inspector settings (delay/history), and assigns it to `FighterController.InputBuffer`.
  - Local fighter: `CollectInputFromKeyboard = true`; `FixedUpdate()` captures keyboard each fixed tick; enqueues stamped `InputFrame` (`now + DelayInput`).
  - Remote fighter: net layer fills buffer with received `InputFrame`s (stamp, checksum, inputs).
- **Rollback shadow buffers** — Each fighter has a paired RB clone with its own `InputBuffer` (e.g., `PlayerRBBuffer`, `EnemyRBBuffer`). After live enqueue/consume, the same frame is appended to the shadow buffer, maintaining a trailing window of `DelayInput + rollbackWindow` frames. Late inputs trigger `RollbackEnqueue` to replace predicted frames and re-predict following frames.
- **During rollback** — `Restore.Rollback(...)`
  1. Swaps live fighters with their RB clones (saved gameplay state).
  2. Builds a temporary rollback buffer by concatenating the shadow’s buffer with the live buffer’s recent frames to cover the timeline from rollback point to “now.”
  3. Calls `FighterController.Resimulate(...)` to replay deterministically, then returns control to live objects.

**Related files**
- [InputBuffer.cs](RollbackNetcode/Assets/Scripts/Gameplay/Input%20Buffers/InputBuffer.cs) — Defines `InputFrame` (byte-packed inputs, frame stamp, checksum, predicted flag) and the `InputBuffer` (delay, prediction, rollback replacement, pressed-keys history).
- [FighterBufferMono.cs](RollbackNetcode/Assets/Scripts/Gameplay/Input%20Buffers/FighterBufferMono.cs) — Mono bridge capturing local input each fixed frame; enqueues with configured delay; raises events.
- [Restore.cs](RollbackNetcode/Assets/Scripts/Gameplay/Core/Restore.cs) — Resimulates N frames from the rollback shadow state; spawns/destroys projectiles to match authoritative state.
- [FighterController.cs](RollbackNetcode/Assets/Scripts/Gameplay/Fighter/FighterController.cs) — Deterministic fighter logic (FixedMath.NET): movement, jump, facing, block/crouch; consumes stamped (or predicted) `InputFrame`s and updates animator/render state.

**Other notable features:** P2P TCP (Nagle disabled), clock sync, deterministic gameplay (FixedMath.NET) with uncapped rendering, lightweight lobbies & chat, checksum-based desync detection.

---

# Destructible Terrain — DX11 Computer Graphics Project

This system raycasts a heightfield to find an impact point, extracts a peak patch around that hit, cuts it against a plane to create a closed solid, shatters the solid into chunks via multiple procedural plane cuts, and spawns Bullet rigid bodies that blast outward. Inside/outside surfaces use different shaders. A CPU-writable “destroyed mask” texture is copied to an SRV; terrain generation samples this mask so rebuilds reflect the removed region.

**Terrain generation & extraction**
- [Terrain.h](DestructableTerrainDX11/Terrain.h) · [Terrain.cpp](DestructableTerrainDX11/Terrain.cpp) — fBM/extra-noise terrain, destroyed-mask sampling, BFS peak extraction (`extractPeakTerrainSubregion`), mesh building.

**Procedural cutting/shattering**
- [ProceduralDestruction.h](DestructableTerrainDX11/ProceduralDestruction.h) · [ProceduralDestruction.cpp](DestructableTerrainDX11/ProceduralDestruction.cpp) — `cutMeshOpen`/`cutMeshClosed`, triangle-fan caps, `radialPlaneCutsRandom`.

**Gameplay glue (raycast → extract → cut/close → shatter → physics)**
- [DestructableTerrainPeaks.h](DestructableTerrainDX11/DestructableTerrainPeaks.h) · [DestructableTerrainPeaks.cpp](DestructableTerrainDX11/DestructableTerrainPeaks.cpp) — `fireProjectileAt`: raycast to terrain, extract peak, close against plane, shatter, and spawn Bullet bodies.

**Rendering & mask update**
- [TerrainTesselationShader.h](DestructableTerrainDX11/TerrainTesselationShader.h) · [TerrainTesselationShader.cpp](DestructableTerrainDX11/TerrainTesselationShader.cpp) — Shader bindings, CPU staging texture + SRV, `markRegionDestructed`.

**Utilities**
- [Transform.h](DestructableTerrainDX11/Transform.h) · [Transform.cpp](DestructableTerrainDX11/Transform.cpp) — Custom transform class.
- [RenderItemCollection.h](DestructableTerrainDX11/RenderItem.h) · [RenderItemCollection.cpp](DestructableTerrainDX11/RenderItem.cpp) — Collection of mesh instances with per-shader setup/parameters.
- [ShaderParameter.h](DestructableTerrainDX11/ShaderParameter.h) · [ShaderParameter.cpp](DestructableTerrainDX11/ShaderParameter.cpp) — Helpers for initializing shader parameters.

**Other notable features:** Procedural Terrain (fBM), Procedural Destruction, Procedural Gerstner Waves, Compute Shader Buoyancy (Bullet3D integration), Screen-Space Reflections + DDA, Parallel-Split Shadow Maps.
