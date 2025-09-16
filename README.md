# Introduction


This file is intended to make navigating and understanding the codebase for **Toot The Lute** easier and more intuitive.
If you'd like more background on the project and its systems, check out the [project page](https://tedishopov.github.io/TootTheLute.html).

Use the Quick Navigation to jump between sections. Within each section, files are ordered by importance, each with a concise one-line summary and a direct link to the source. Where helpful, brief glossaries and context notes clarify naming and system intent.

## Quick Navigation


- [Toot The Lute](#toot-the-lute)
  - [Beat/Rhythm System](#beatrhythm-system)
    - [Legend](#legend)
  - [Enemies](#enemies)
    - [Attack System](#attack-system)
    - [Boid/Steering System](#boidsteering-system)
    - [Duplucation System](#duplication-system)
  - [Beat-Triggered Interactables](#beat-triggered-interactables)
 - [RatKing](#ratking) 
  - [Inventory System](#inventory-system)  
  - [Throwing (Exit Point)](#throwing--projectile-simulation-system-exit-point-from-inventory)  
  - [Collecting (Entry Point)](#collecting-entry-point-to-inventory)  

# Toot The Lute

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


### Legend
**Crochet** - Seconds per beat: Crochet = 60 / BPM. Used to convert beat lengths into seconds.

**Beat** - Data describing a musical event; includes a Length (in beats). Real-time length is Length * Crochet.

**Reference Target** - The Transform at which the player is expected to hit the beat (the timing "hit line").

**Input Offset** - Calibration shift applied to align player input with audio timing.

**Visual Offset** - Calibration shift applied to align visuals with audio timing.

## Enemies 

### Attack System

Related files could be found in the _Assets/Scripts/Enemies_ directory.

[Enemy.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/Enemy.cs) - Base Enemy controller. Knockback logic. Invulnerability windows.

[Wendigo.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/Wendigo.cs) - Boss controller. Switch-case state machine

[MeleeAttack.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/MeleeAttack.cs) - Scheduled on next beat. Utilized beat events.

[RangedAttack.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/RangedAttack.cs) - Scheduled on the next beat. Utilizes beat events.


### Boid/Steering System

Related files could be found in the _Assets/Scripts/Enemies/SteeringMovement_ directory.
BoidBase.cs

[BoidBase.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/BoidBase.cs)
*   Integrates behaviors: weighted sum → clamp → apply to Rigidbody2D.
*   Exposes velocity, maxSpeed, maxForce, and draws behavior gizmos.

[BoidBaseEditor.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/BoidBaseEditor.cs.cs) - One-click add buttons for common behaviors (Seek, Separation, Flee, ObstacleAvoidance).

[SteeringBehaviour.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/SteeringBehavior.cs) - Base interface for steering modules. Includes a custom inspector drawer to label derived types inline.

[ObstacleAvoidance.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/ObstacleAvoidance.cs) - Cone raycasts to detect obstacles and sum repel forces.

[Separation.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/Separation.cs)

[Seek.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/Seek.cs)

[Pursue.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/SteeringMovement/Pursue.cs)


### Duplication System

Related files can be found in the Assets/Scripts/Enemies/Spawning and Assets/Scripts/Utils directories.

[SlimeSpawner.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/Duplication%20&%20Spawning/SlimeSpawner.cs) - Spawns _SlimeDroplet_  along cone directions at a fixed radius. 

[SpawnEnemiesOnDeath.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/Duplication%20&%20Spawning/SpawnEnemiesOnDeath.cs) - A component invoking slime spawner on enemy death.

[SlimeDroplet.cs](TootTheLute/Assets/Scripts/Enemies%20&%20AI/Duplication%20&%20Spawning/SlimeDroplet.cs) - Timed droplet that scales/arches, then spawns an enemy at impact. 

##  Beat-Triggered Interactables

Related files can be found in the Assets/Scripts/Interactables directory.

[InteractableReplenishingHealing.cs](TootTheLute/Assets/Scripts/Interactables/InteractableReplenishingHealing.cs) - Spawns discrete healing charges up to a cap; heals proportional to charges.

[InteractableObject.cs](TootTheLute/Assets/Scripts/Interactables/InteractableObject.cs) - Attackable node that emits a radial burst of homing projectiles.

[InteractableMeleeObject.cs](TootTheLute/Assets/Scripts/Interactables/InterableMeleeObject.cs) - Interactable that spawns a short-lived strike oppossite of the player.

# RatKing
## Inventory System
Related files can be found in the _Assets/Scripts/Inventory_ directory.

[Inventory.cs](RatKing/Assets/Scripts/Inventory/Inventory.cs) - Core inventory data and rules. Manages items, placement validation, gold totals, and persistence events.

[IPlayerInventory.cs](RatKing/Assets/Scripts/Inventory/Inventory.cs) - Interface for inventory access: grid size, item list, placement/selection API, and update events.

[InventoryGridView.cs](RatKing/Assets/Scripts/Inventory/InventoryGridView.cs) - Grid UI/controller. Builds cells, previews placement under the cursor, handles pick/place input, and updates gold labels.

[InventoryObject.cs](RatKing/Assets/Scripts/Inventory/InventoryObject.cs) - UI representation of a single item. Tracks occupied cells, centers/sizes the sprite on the grid, and bridges to physical collectibles.

[InventoryCell.cs](RatKing/Assets/Scripts/Inventory/InventoryCell.cs) - Visual cell widget. Stores default colors and flips between default/valid/invalid states during placement.

## Throwing & Projectile Simulation System (Exit Point from Inventory)
[Thrower.cs](Assets/Scripts/Player/Thrower.cs) - Bridges inventory to the world: simulates & throws the selected item, predicts trajectory/sound, and raises an event on throw.

## Collecting (Entry Point To Inventory)
[Collectible.cs](Assets/Scripts/Items/Collectible.cs) - Physical item carrying inventory metadata; handles breakage threshold and emits impact sounds.

