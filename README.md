# Introduction


This file is intended to make navigating and understanding the codebase for **Toot The Lute** easier and more intuitive.
If you'd like more background on the project and its systems, check out the [project page](https://tedishopov.github.io/TootTheLute.html).

Use the Quick Navigation to jump between sections. Within each section, files are ordered by importance, each with a concise one-line summary and a direct link to the source. Where helpful, brief glossaries and context notes clarify naming and system intent.

## Quick Navigation


- [Toot The Lute](#toot-the-lute)
  - [Beat/Rhythm System](#beatrhythm-system)
  - [Beat-Triggered Interactables](#beat-triggered-interactables)
- [RatKing](#ratking) 
  - [Inventory System](#inventory-system)  
    - Main Logic
    - [Throwing (Exit Point)](#throwing--projectile-simulation-system-exit-point-from-inventory)  
    - [Collecting (Entry Point)](#collecting-entry-point-to-inventory)  
- [Rollback System](#rollback-system) 
- [Computer Graphcis Project](#destructible-terrain---dx11-computer-graphics-project)
  - [Destructable Terrain](#destructible-terrain---dx11-computer-graphics-project) 

# Toot The Lute

### Legend
**Crochet** - Seconds per beat: Crochet = 60 / BPM. Used to convert beat lengths into seconds.

**Beat** - Data describing a musical event; includes a Length (in beats). Real-time length is Length * Crochet.

**Reference Target** - The Transform at which the player is expected to hit the beat (the timing "hit line").

**Input Offset** - Calibration shift applied to align player input with audio timing.

**Visual Offset** - Calibration shift applied to align visuals with audio timing.

## Beat/Rhythm System
Related files could be found in the _Assets/Scripts/BeatSystem_ directory.

[BeatmapTrack.cs](Assets/Scripts/BeatSystem/BeatmapTrack.cs) 
*   Orchestrates the on-screen track
*   Spawn and recycles beat objects form the active Beatmap
*   Evaluates the correctnes of player input in terms of timing
*   Maps audio times to to an on-screen position on the track

[BeatObject.cs](Assets/Scripts/BeatSystem/BeatObject.cs)
*   Visual/interactive unit for a single beat
*   Tracks its own State (Approaching, Successful, Unsuccesful),
*   Raises events on resolution
*   Updated by BeatmapTrack when on the track, but is responsible for any effects after a Successful or Unsuccesful states

[Conductor.cs](Assets/Scripts/BeatSystem/Conductor.cs)
*   Time authority. Reads the Wwise playhead and publishes SongPosition (seconds). Exposes 
*   Play/Resume/Pause/Stop WWise control events

#### Related types referenced (context): 
*   **Beatmap** (provides flattened beats and holds the Conductor), 
*   **Beat** (timing/length data) 
*   **BeatmapSource** (Wwise events), 
*   **InputCalibration** (audio/visual offsets).



##  Beat-Triggered Interactables

Related files can be found in the Assets/Scripts/Interactables directory.

[InteractableReplenishingHealing.cs](TootTheLute/Assets/Scripts/Interactables/InteractableReplenishingHealing.cs) - Spawns discrete healing charges up to a cap; heals proportional to charges.

[InteractableObject.cs](TootTheLute/Assets/Scripts/Interactables/InteractableObject.cs) - Attackable node that emits a radial burst of homing projectiles.

[InteractableMeleeObject.cs](TootTheLute/Assets/Scripts/Interactables/InterableMeleeObject.cs) - Interactable that spawns a short-lived strike oppossite of the player.

# RatKing
## Inventory System
Related files can be found in the _Assets/Scripts/Inventory_ directory.

[Inventory.cs](Ratking/Assets/Scripts/Inventory/Inventory.cs) - Core inventory data and rules. Manages items, placement validation, gold totals, and persistence events.

[IPlayerInventory.cs](Ratking/Assets/Scripts/Inventory/Inventory.cs) - Interface for inventory access: grid size, item list, placement/selection API, and update events.

[InventoryGridView.cs](Ratking/Assets/Scripts/Inventory/InventoryGridView.cs) - Grid UI/controller. Builds cells, previews placement under the cursor, handles pick/place input, and updates gold labels.

[InventoryObject.cs](Ratking/Assets/Scripts/Inventory/InventoryObject.cs) - UI representation of a single item. Tracks occupied cells, centers/sizes the sprite on the grid, and bridges to physical collectibles.

[InventoryCell.cs](Ratking/Assets/Scripts/Inventory/InventoryCell.cs) - Visual cell widget. Stores default colors and flips between default/valid/invalid states during placement.

## Throwing & Projectile Simulation System (Exit Point from Inventory)
[Thrower.cs](Ratking/Assets/Scripts/Inventory/Thrower.cs) - Bridges inventory to the world: simulates & throws the selected item, predicts trajectory/sound, and raises an event on throw.

## Collecting (Entry Point To Inventory)
[Collectible.cs](Ratking/Assets/Scripts/Items/Collectible.cs) - Physical item carrying inventory metadata; handles breakage threshold and emits impact sounds.


# Rollback Netcode Fighting Game

## Rollback System

Here's how the buffers are arranged and used for the live fighters and their rollback shadows:
* **Live fighter buffers:** Each fighter instance has a FighterBufferMono that creates and owns an InputBuffer in Awake(), copies its inspector settings (delay, pressed-keys history), and assigns it to FighterController.InputBuffer.

The local fighter sets CollectInputFromKeyboard = true, so FighterBufferMono.FixedUpdate() captures the keyboard each fixed tick and enqueues a stamped InputFrame (now + DelayInput).

The remote fighter's buffer is filled by the net layer with received InputFrames (same format: stamp, checksum, inputs).

* **Rollback shadow buffers:** Each fighter has a paired RB (rollback) clone that also holds an InputBuffer (e.g., PlayerRBBuffer, EnemyRBBuffer). After the live fighter enqueues/consumes for the current tick, that same frame is appended to the shadow buffer, keeping a trailing window of DelayInput + rollbackWindow frames. When late inputs arrive for a past stamp, the shadow buffer uses RollbackEnqueue to replace the predicted frame at that stamp and re-predict any following frames so its timeline stays consistent.

* **How they work together during a rollback**  When a late frame requires rewinding, Restore.Rollback(...):
1. Swaps live fighters with their RB clones (the "rollback shadow" = the saved gameplay state).

2. Builds a temporary rollback buffer per side by concatenating the shadow's buffer with the live buffer's recent frames (BufferToRollbackWith(RBBuffer, liveBuffer)), ensuring the full timeline from the rollback point to "now".

3. Feeds that buffer to FighterController.Resimulate(...) to replay inputs up to the present, then hands control back to the live objects.

In short: the live buffer is the real-time input queue used to step gameplay each tick; the rollback shadow buffer is the curated, replaceable history that pairs with the shadow’s saved state so the engine can rewind, apply authoritative inputs, and deterministically catch back up.


## Other notable features:
* Peer-to-peer TCP with Nagle disabled.
* Clock synchronization to align start times.
* Deterministic gameplay using FixedMath.NET (fixed-point) with uncapped rendering.
* Lightweight lobbies and in-app chat.
* Checksum-based desync detection.

Related files live in the Assets/Scripts/... Core, InputBuffer & Core directories.

[InputBuffer.cs](RollbackNetcode/Assets/Scripts/Gameplay/Input%20Buffers/InputBuffer.cs)
* Defines InputFrame (byte-packed inputs, frame stamp, checksum, predicted flag).
* InputBuffer queue: delay, prediction, rollback replacement, pressed-keys history.

[FighterBufferMono.cs](RollbackNetcode/Assets/Scripts/Gameplay/Input%20Buffers/FighterBufferMono.cs)
* Mono bridge that captures local keyboard input each fixed frame.
* Enqueues into InputBuffer with configured delay and raises an event for observers.

[Restore.cs](RollbackNetcode/Assets/Scripts/Gameplay/Core/Restore.cs)
* Resimulates N frames starting from the "Rollback Shadow's" state (A copy of the player character N frame in the past. Meant to be invisible in production)
* Additionally, initializes/destroys projectiles as necessary to match the correct game state.

[FighterController.cs](RollbackNetcode/Assets/Scripts/Gameplay/Fighter/FighterController.cs)
* Deterministic fighter logic (FixedMath.NET): movement, jump, facing, block/crouch.
* Consumes stamped InputFrames (or predicted) and updates animator/render state.


# Destructible Terrain - DX11 Computer Graphics Project 

This system takes a heightfield terrain, raycasts to find an impact point, extracts a peak patch around that hit, cuts it against a plane to create a closed solid, 
shatters the solid into chunks with multiple procedural plane cuts, and then spawns Bullet rigid bodies that blast outward. (Inside and outside mesh surfaces use different shaders).
The rendering side uses a CPU-writable "destroyed mask" texture that's copied to an SRV; terrain generation samples this mask so subsequent rebuilds reflect the missing destroyed region.

Terrain generation & extraction

[Terrain.h](DestructableTerrainDX11/Terrain.h) [Terrain.cpp](DestructableTerrainDX11/Terrain.cpp) - fBM/extra noise terrain, destroyed-mask sampling, BFS peak extraction (extractPeakTerrainSubregion), mesh building.

[ProceduralDestruction.h](DestructableTerrainDX11/ProceduralDestruction.h) [ProceduralDestruction.cpp](DestructableTerrainDX11/ProceduralDestruction.cpp) - cutMeshOpen/cutMeshClosed, triangle-fan caps, radialPlaneCutsRandom.

[DestructableTerrainPeaks.h](DestructableTerrainDX11/DestructableTerrainPeaks.h) [DestructableTerrainPeaks.cpp](DestructableTerrainDX11/DestructableTerrainPeaks.cpp)- raycast -> extract -> cut/close -> shatter -> spawn Bullet bodies (fireProjectileAt).

[TerrainTesselationShader.h](DestructableTerrainDX11/TerrainTesselationShader.h) [TerrainTesselationShader.cpp](DestructableTerrainDX11/TerrainTesselationShader.cpp)- shader bindings, CPU staging texture + SRV, markRegionDestructed.

Utility/Helper classess: 
[Transform.h](DestructableTerrainDX11/Transform.h) [Transform.cpp](DestructableTerrainDX11/Transform.cpp) - custom transform class
[RenderItemCollection.h](DestructableTerrainDX11/RenderItem.h) [RenderItemCollection.cpp](DestructableTerrainDX11/RenderItem.cpp) - a generic collection of mesh instance each having a specific setup and 
parameters for a shader 
[ShaderParameter.h](DestructableTerrainDX11/ShaderParameter.h) [ShaderParameter.cpp](DestructableTerrainDX11/ShaderParameter.cpp) - utility for initializing shader parameters

Other Notable Features: 
Procedural Terrain (fBM),Procedural Destruction,Procedural Gerstner Waves,Compute Shader Buoyancy integrated with physics engine Bullet3D,Screen-Space Reflections + DDA,Parallel-Split Shadow Maps,

