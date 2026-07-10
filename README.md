# vrgameyay

silli little gorilla tag fangame i made with Claude.
i used ai :(
sorry

A Unity VR project (Unity **6000.3.11f1**) implementing Gorilla-Tag-style, hand-over-hand climbing locomotion for the Meta Quest, built with XR Interaction Toolkit + OpenXR and URP.

## Building the APK

1. Open the project in Unity Hub with editor version `6000.3.11f1` (must match `ProjectSettings/ProjectVersion.txt`).
2. `File > Build Settings`, select the **Meta Quest** build profile.
3. Make sure the scene you want to play is added to the build (only `Assets/Scenes/SampleScene.unity` is included by default — for climbing locomotion, add `Assets/Scenes/Gorilla/Gorilla Locomotion.unity`).
4. Click **Build** (or **Build and Run** with the headset connected) to produce the APK.

## Installing on your Quest

**Option A — adb**
1. Enable Developer Mode on the headset (Meta Horizon mobile app → Devices → your headset → Developer Mode).
2. Connect the Quest to your PC via USB-C and accept the "Allow USB debugging?" prompt in the headset.
3. Install [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools), then run:
   ```
   adb install path\to\your.apk
   ```
4. In the headset, go to **Apps → menu (top right) → Unknown Sources** and launch it.

**Option B — SideQuest**
1. Install [SideQuest](https://sidequestvr.com/setup-howto) and enable Developer Mode + connect via USB as above.
2. Drag-and-drop the APK onto the SideQuest window (or use the "Install APK" button).
3. Launch it from **Unknown Sources** in the headset's app library.

## Controls

Grip and move your controllers like climbing holds — grab a surface with either hand to pull yourself along it, Gorilla Tag-style. Releasing while moving carries your momentum into a jump/throw.
