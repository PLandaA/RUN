# Run

**Unity 6 (URP) + XR Interaction Toolkit 3 — with $P gesture recognition**

**[▶ Play it on itch.io](https://andralandev.itch.io/run)**

A single-player VR horror game: trapped in a dark facility, you must find the red key that opens the exit door while a ghoul hunts you through the halls. Your flashlight battery is a resource — and one of the ways to recharge it is drawing a magic gesture in the air with your hand.

This repository showcases the VR interaction and gameplay systems I built: physics-based climbing, gesture-driven mechanics on top of the $P Point-Cloud Recognizer, and a resource-management horror loop. Recently migrated and hardened for **Unity 6 / XR Interaction Toolkit 3.3 / OpenXR**.

Third-party assets (models, animations, environment art and Asset Store packs) are **not included** — see [External Assets Required](#external-assets-required).

<!-- Add a short gameplay GIF here — it makes a huge difference:
![Gameplay](Docs/gameplay.gif) -->

## What I Built

### Gesture-Driven Mechanics ($P Point-Cloud Recognizer)
- Hand strokes are sampled in world space from the controller, projected to screen space and classified with the **$P point-cloud algorithm** — stroke order and direction don't matter, so gestures stay robust in VR.
- Two gestures are wired into gameplay: **"walk"** (one forward step per recognized stroke — an alternative locomotion mode) and **"lamp"** (recharges the flashlight battery, *Lumos*-style).
- Gesture templates ship with the project in `StreamingAssets/Gestures`; locally-trained gestures (persistent data path) are merged on top. A dev-only *creation mode* records new templates at runtime.
- Recognition is gated by a configurable confidence threshold and guarded against empty training sets.

### Physics-Based Climbing
- Grab any ladder rung with the direct interactors and pull yourself up: the `CharacterController` moves opposite to the real tracked hand velocity (`deviceVelocity` from the XR input subsystem).
- Climbing takes over locomotion cleanly — continuous movement (and its gravity) is suspended while a hand is latched, and restored on release.
- Migrated from the deprecated `XRController` API to XRI 3's interactor `handedness` model.

### Flashlight as a Resource
- ~2 minutes of battery; an emissive indicator on the flashlight body shifts green → yellow → red as it drains, and the light intensity dims in three tiers.
- Material/light updates only fire on tier *changes* (not per frame), and the shared material asset is restored on exit so Play Mode never permanently stains project assets.
- The "lamp" gesture is the recharge mechanic — light management becomes a skill.

### Enemy AI
- The ghoul chases the player with **NavMesh**-driven pursuit (AI Navigation package) and animation-state control.
- Additional creatures (troll, rhinoceros) use patrol / flee / seek behaviors built over a shared path-search layer.

### VR Locomotion Suite & Interaction
- Continuous movement (head-oriented, capsule follows the headset with ground spherecasts) and snap turn.
- Grabbable keys, hinge-jointed physical doors, flashlight and melee weapon via **XR Grab Interactables**; the exit door validates the *correct* key on contact and gives world-space UI feedback.
- VR-ready menus (world-space canvas + XR ray interactors + `XRUIInputModule`).

## Architecture Notes

| Concern | Approach |
|---|---|
| Gesture classification | $P point-cloud matching over screen-projected strokes, threshold-gated |
| Gesture persistence | Bundled templates in `StreamingAssets` + user templates in persistent data path |
| Climbing | Hand `deviceVelocity` → inverse `CharacterController.Move`, per-hand latch via interactor `handedness` |
| Locomotion handover | Continuous movement component toggled off while climbing owns the body |
| Scene flow | Scene loads by *name* (robust against build-index reshuffles) |
| Editor hygiene | Shared material colors restored on destroy; no permanent Play Mode side effects |

## Built With

| Technology | Version |
|---|---|
| Unity | 6 (6000.x), URP 17.3 |
| XR Interaction Toolkit | 3.3.1 |
| OpenXR Plugin | 1.17.1 |
| AI Navigation (NavMesh) | 2.0.12 |
| $P Point-Cloud Recognizer | PDollar (Unity port, New BSD License — included) |

## Project Structure (my code)

```
Assets/Scripts/
  Locomotion/  ContinuosMovement (head-oriented movement + gravity),
               Climber + ClimbInteractable (physics climbing), LocomotionController
  Gestures/    MovementRecognizer ($P capture, classification, gameplay hooks)
  Gameplay/    GameManager (flashlight economy, game state), OpenDoor (key validation),
               Gun, FollowPlayer
  Player/      HandPresence (hand models/animation)
Assets/Agents/Scripts/   Creature AI: Zombie (chase/attack), Troll (patrol/flee),
                         Rhino (patrol/charge) over a shared path-search layer
Assets/Scenes/           Menu (entry), Game (main level)
Assets/StreamingAssets/Gestures/   Trained $P gesture templates (XML)
```

## External Assets Required

This repo intentionally excludes licensed content (Asset Store packs and 3D models that are not mine). All art, models and animations belong to their respective creators. To run the project with full visuals, import the following into their original folders — **scenes will show missing references until then**:

| Asset pack | Import to | Purpose |
|---|---|---|
| [Flashlight](https://assetstore.unity.com/packages/3d/props/electronics/flashlight-18972) (RRFreelance) | `Assets/_Flashlight PRO/` | Flashlight model + toggle system |
| Troll Cannibal | `Assets/Agents/Animations/Troll_Сannibal/` | Troll creature |
| Creature: Rhinoceros | `Assets/Agents/Animations/Creature_Rhinoceros/` | Rhino creature |
| Ghoul + animations | `Assets/Agents/Animations/GhoulZAnim/` | Main monster |
| [Key and Lock](https://assetstore.unity.com/packages/3d/props/furniture/key-and-lock-193317) | `Assets/key_lock/` | Keys / lock |
| Builder's Torch | `Assets/BuildersTorch/` | Torch prop |
| 10 Skyboxes Pack: Day - Night | `Assets/Day-Night Skyboxes/` | Skyboxes |
| Doors pack | `Assets/Doors/` | Physical doors |
| Free SteelLadder Pack | `Assets/Free SteelLadder Pack/` | Ladders |
| Simple Scene Loader | `Assets/HeneGames/` | Loading screens |
| [PBR RPG/FPS Game Assets — Industrial Set](https://assetstore.unity.com/packages/3d/environments/industrial/pbr-rpg-fps-game-assets-industrial-set-v3-151332) (Dmitrii Kutsenko) | `Assets/PBR_RPG_FPS_Game_Assets_industrial/` | Environment props |
| SDF Basic Training Stage | `Assets/SDF_BasicTrainingStage/` | Environment / stage |
| Oculus sample hands ([Meta XR All-in-One SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657), originally from the deprecated Oculus Integration package) | `Assets/Oculus Hands/` | Hand models + animations |

**Included on purpose:** the [$P Point-Cloud Recognizer](https://depts.washington.edu/acelab/proj/dollar/pdollar.html) (`Assets/PDollar/`) is distributed under the **New BSD License**, which permits redistribution with attribution — so the repo compiles out of the box.

## Setup

1. Clone and open with **Unity 6000.x** (Unity 6).
2. Import the external assets listed above into their original folders.
3. In **Edit → Project Settings → XR Plug-in Management**, enable **OpenXR** for your platform and add your headset's interaction profile.
4. Open `Assets/Scenes/Menu.unity` and press **Play** with your headset connected (Link / SteamVR / AirLink), or build to device.
5. Gestures: draw the stroke while holding the mapped button (primary = walk, secondary = lamp). Templates load from `StreamingAssets/Gestures`.

## Credits

- Game by **AndraLanDev** and **Meme Vega** — gameplay, VR interaction, gesture integration and AI behaviors.
- Unity 6 / XRI 3 migration and polish: **AndraLanDev**.
- **$P Point-Cloud Recognizer** by Radu-Daniel Vatavu, Lisa Anthony & Jacob O. Wobbrock (New BSD License), Unity port by [Da Viking Code](http://davikingcode.com) ([GitHub](https://github.com/DaVikingCode/PDollar-Unity)) — included in `Assets/PDollar/`, attribution required by its license.
- **Music** ©2020 [Joshua McLean](https://joshua-mclean.itch.io) — licensed under [Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/); included in `Assets/Music/`.
- VR rig and interaction foundations built following Valem's VR tutorials on YouTube (heavily extended).

## License

Original code in `Assets/Scripts/` and `Assets/Agents/Scripts/` is provided for portfolio and educational purposes. Third-party content remains under its respective licenses.
