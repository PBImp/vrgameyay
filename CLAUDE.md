# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

This is a Unity VR project (Unity **6000.3.11f1**) implementing Gorilla-Tag-style, hand-over-hand climbing locomotion for the Meta Quest. It uses:
- **XR Interaction Toolkit 3.3.1** + **OpenXR** for input/tracking (`Assets/XR/Loaders/OpenXRLoader.asset`)
- **Universal Render Pipeline 17.3.0**, with separate pipeline assets for mobile vs. PC (`Assets/Settings/Mobile_RPAsset.asset` for Quest, `Assets/Settings/PC_RPAsset.asset` for editor/PC)
- Build target: Android/Meta Quest via the `Meta Quest` build profile (`Assets/Settings/Build Profiles/Meta Quest.asset`)

There is no README or prior CLAUDE.md in this repo.

## Working with this project (no CLI build/test pipeline)

This is a GUI-driven Unity project, not an npm/make-style repo — there are no shell commands for building, linting, or testing.

- **Build/run**: done from the Unity Editor (Unity Hub, editor version `6000.3.11f1` — must match `ProjectSettings/ProjectVersion.txt`). Use File > Build Settings with the `Meta Quest` build profile for device builds; a prior Android build output already exists at `vrgameyay.apk`.
- **Checking compile errors after editing files on disk**: if the Unity Editor is open, it live-watches the `Assets/` folder and auto-recompiles/reimports on save. To check for compiler or import errors without access to the Editor UI, read `%LOCALAPPDATA%/Unity/Editor/Editor.log` (grep for `error CS` or `Problem detected while importing`) — this is the only feedback mechanism available outside the Editor itself.
- **Tests**: `com.unity.test-framework` is installed as a package, but there is no `Tests` assembly/folder anywhere in `Assets/` — no automated tests currently exist in this project.
- Only `Assets/Scenes/SampleScene.unity` is registered in Build Settings (`ProjectSettings/EditorBuildSettings.asset`); the other scenes below are not currently included in builds.

## Editing Unity YAML assets directly (scenes/prefabs/materials)

Claude Code often needs to hand-edit `.prefab`/`.unity`/`.mat` files as text rather than through the Editor UI. This is fragile — keep in mind:
- Cross-references between GameObjects/Components/scripts are by numeric `fileID` (unique per-file) and asset `guid` (unique per-project, defined in the paired `.meta` file). Renaming a GameObject's `m_Name` is safe; changing/removing a `fileID` that something else references is not — grep the whole file (and other assets) for a `fileID` before deleting it.
- Empty arrays (e.g. `m_Children:`) **must** be written as `m_Children: []`. Writing the key with nothing after it (or a blank line) is parsed as null instead of an empty list, and Unity will log `Restored Transform child parent pointer from NULL` on import — a sign the file needs fixing, not a harmless warning.
- If the Editor is running while you create a **new** asset, write its `.meta` file yourself (with a chosen `guid`) in the same edit as the asset — if you let Unity generate the `.meta` on its own first, it assigns a random guid, and any reference you already wrote elsewhere pointing at your intended guid will silently break. If Unity beats you to it, re-read the generated `.meta` and fix up your references to match its guid instead.
- Built-in primitive meshes (Cube/Sphere/Capsule/Cylinder/etc.) all live under the fixed guid `0000000000000000e000000000000000`, keyed by fileID (Cube=10202, Cylinder=10206, Sphere=10207, Capsule=10208).

## Architecture: locomotion system

The climbing/locomotion logic is a variant of the open-source "GorillaLocomotion" movement script, namespace `GorillaLocomotion`, living in `Assets/Scripts/NewGorillaLocomotionScripts/`:

- **`Player.cs`** — singleton (`GorillaLocomotion.Player.Instance`) that drives all movement. Each `Update()`, for both hands, it:
  1. Reads the tracked controller transform (`leftHandTransform`/`rightHandTransform`, driven live by XR Interaction Toolkit `ActionBasedController`s).
  2. Iteratively sphere-casts from the last resolved hand position toward the new controller-driven position (`IterativeCollisionSphereCast` / `CollisionsSphereCast`) to detect and slide along collisions, respecting `maxArmLength`.
  3. Moves the Rigidbody (the whole rig) based on how much the "stuck" hand(s) moved, then updates `leftHandFollower`/`rightHandFollower` — the visual hand anchors — to the resolved contact point.
  4. Applies a velocity-history-based throw/jump impulse (`velocityHistorySize`, `velocityLimit`, `maxJumpSpeed`, `jumpMultiplier`) when hands release a surface.
  - **Important**: `Player.cs` only ever sets `leftHandFollower.position` / `rightHandFollower.position` — it never touches their rotation.
- **`Surface.cs`** — optional component to put on climbable geometry; overrides the default slide/slip percentage (`defaultSlideFactor`) for that surface.
- **`MatchRotation.cs`** — separate small script (not part of the original GorillaLocomotion source) that copies a source Transform's rotation onto the object each `LateUpdate`, with an adjustable `rotationOffset` (Euler angles) applied on top. This exists specifically to make the visual hand followers rotate with the real controllers, compensating for the fact that a VR controller's tracked "grip" rotation doesn't necessarily point the same direction as where the controller visually looks like it's aiming.

⚠️ There used to be a **second, independently re-downloaded copy** of the GorillaLocomotion `Player`/`Surface` scripts (from a `GorillaLocomotion-main.zip` extraction) living alongside the real one, causing `CS0101`/`CS0111` duplicate-definition compile errors. If similar errors reappear, check for a duplicate `Player.cs`/`Surface.cs` under a different folder (e.g. `Assets/GorillaLocomotion-main/...`) before assuming the existing script is broken — the package manifest's scoped registry entry for `com.anotheraxiom.gorillalocomotion` is a leftover from that; the actual scripts are NOT installed as a package, they live directly under `Assets/Scripts/NewGorillaLocomotionScripts/`.

## Architecture: the rig prefab

- **`Assets/Resources/GorillaPrefabs/Gorilla Rig.prefab`** is the entire player rig: XR Origin components, `Main Camera` (head, with the body's `SphereCollider`/head collider), a `Capsule` body (child of the camera, used as `bodyCollider`), `LeftHand Controller`/`RightHand Controller` (raw `ActionBasedController` transforms driven by XR), and `Left Hand`/`Right Hand` (the visual hand followers referenced by `Player.cs`, each holding a `MatchRotation` component plus a procedural paw mesh built from primitives: a `Palm` cube + 4 `Finger` cylinders, using `GorillaHandMaterial.mat`).
- **`Assets/Resources/GorillaPrefabs/Gorilla Hand.prefab`** is a standalone copy of just the paw geometry, kept as a reusable asset. It is **not** nested-linked to `Gorilla Rig.prefab` (Unity's nested-prefab format needs stripped placeholder objects that are risky to hand-author in YAML) — the two hand hierarchies are duplicated, not shared. If the paw shape needs to change, both prefabs currently need updating.
- The main gameplay scene that uses this rig is **`Assets/Scenes/Gorilla/Gorilla Locomotion.unity`** (a simple test gym of floor + cubes). Other scenes: `Assets/Scenes/SampleScene.unity` (the only one in Build Settings), `Assets/Scenes/VRGAMEE.unity` (XR Interaction Manager + Canvas test scene), and the XR Interaction Toolkit's own sample scene under `Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/DemoScene.unity`.
