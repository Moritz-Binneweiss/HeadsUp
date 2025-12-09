# HeadsUp - Project Context for Copilot

## Project Overview
**HeadsUp** is a mobile party game for Android (similar to the popular "Heads Up!" app) built with **Unity 2022.3 LTS**.

**Core Gameplay:**
- Players hold phone to forehead (upside down portrait mode)
- Other players give clues about the word shown on screen
- **Tilt phone DOWN (towards feet)** = Correct answer
- **Tilt phone UP (towards sky)** = Skip word
- Timer-based rounds with score tracking
- Multiple players and customizable round count (1-5 rounds)

**Current Status:** Game logic complete, UI functional, **CRITICAL ISSUE: Accelerometer not working on Google Pixel 9 Pro (Android 15)**

---

## Critical Issue: Accelerometer on Android 15

### The Problem
Unity's `Input.acceleration` and `Input.gyro` are **completely broken** on Android 15 (Google Pixel 9 Pro):
- `Input.acceleration` returns `(0, 0, 0)` despite device having sensor
- `Input.gyro.enabled` stays `false` even when set to `true`
- `SystemInfo.supportsAccelerometer` correctly returns `true`
- This is a **known Unity 2022.3 LTS bug** with Android 15's sensor API changes

### Current Solution Attempt
**Native Android SensorManager Implementation** via AndroidJavaProxy:
- File: `Assets/Scripts/Managers/AndroidAccelerometer.cs`
- Uses JNI bridge to access Android's SensorManager directly
- Implements `SensorEventListener` to receive sensor callbacks
- Fallback system: Try Unity's Input.acceleration first, then native if magnitude < 0.1

### Testing Status
- ❌ Unity's Input.acceleration: NOT working (returns zeros)
- ❌ Unity's Input.gyro: NOT working (can't be enabled)
- ⏳ Native AndroidAccelerometer: **Implemented but not yet confirmed working**
  - Code is complete with detailed logging
  - Needs fresh APK build and testing on device
  - Should see logs like `[AndroidAccel] ✓ Registration: true` and `[SensorListener] ✓ First callback received!`

---

## Project Structure

### Key Scripts

#### Managers/
- **GameManager.cs** - Core game state, player management, scoring, round logic
  - `totalRounds` - Number of rounds to play (1-5)
  - `GetCurrentRound()` - Current round number
  - `GetCurrentRoundPlayer()` - Active player for current round
  - `MarkCorrect()` / `MarkSkipped()` - Score tracking

- **UIManager.cs** - Screen navigation, UI updates, button setup
  - Manages all screen GameObjects (PlayerSetup, Ready, Gameplay, Results)
  - Round selection UI with highlight buttons (1-5)
  - `ShowScreen()` - Screen switching logic

- **CategoryLoader.cs** - Loads word categories from JSON
  - Loads from `Resources/Categories/` folder
  - Available categories: Animals.json (49 words), Food.json (58 words)
  - Used by UIManager to populate category selection

- **AndroidAccelerometer.cs** ⚠️ **CRITICAL** ⚠️
  - Native Android sensor access when Unity's API fails
  - Uses `AndroidJavaProxy` to implement `SensorEventListener`
  - Singleton pattern, persists across scenes
  - **STATUS: Needs testing on device**
  - Detailed logging for debugging sensor initialization

- **AccelerometerDebug.cs** - On-screen sensor value display
  - Shows Unity acceleration (X, Y, Z) with visual bars
  - Shows Native acceleration status
  - Displays which system is active: "Using: UNITY" or "Using: NATIVE"
  - **Use this to verify sensor functionality on device**

#### Controllers/
- **GameplayController.cs** - Gameplay loop, tilt detection, timer
  - `DetectTilt()` - Checks Y-axis for tilt gestures
    - Y > `tiltThreshold` (0.3) = Correct (tilt down)
    - Y < `-tiltThreshold` = Skip (tilt up)
  - Fallback chain: Unity Input.acceleration → AndroidAccelerometer → Input.gyro.gravity
  - Timer countdown, word display logic
  - Keyboard controls for Editor testing: Up Arrow = Skip, Down Arrow = Correct

- **ReadyScreenController.cs** - Detects tilt-down to start game
  - Player holds phone to forehead, tilts down when ready
  - Uses same accelerometer fallback logic

#### Models/
- **Player.cs** - Player data (name, correct/skipped counts)
- **Category.cs** - Category data (name, word list)

---

## Unity Configuration

### Player Settings (Important!)
- **Target Platform:** Android
- **Minimum API Level:** Android 7.0 (API 24)
- **Target API Level:** Automatic (highest installed)
- **Scripting Backend:** IL2CPP (recommended for Android)
- **API Compatibility:** .NET Standard 2.1
- **Orientation:** Portrait (locked)
- **Input System:** New Input System (com.unity.inputsystem)

### Critical Settings for Accelerometer:
1. **Player Settings → Other Settings → Accelerometer Frequency:** 60 Hz
2. **Edit → Project Settings → Input System Package:**
   - Motion sensors enabled
   - Update mode: Dynamic Update

### Build Settings:
- **Scene Order:**
  1. MainMenu
  2. (All other scenes auto-loaded)
- **Compression:** LZ4 (faster build, larger size) or Default

---

## Categories System

### JSON Format
Located in `Assets/Resources/Categories/`:

```json
{
  "categoryName": "Animals",
  "words": [
    "Dog",
    "Cat",
    "Elephant",
    ...
  ]
}
```

### Current Categories:
- **Animals.json** - 49 words
- **Food.json** - 58 words

To add new category: Create JSON in `Resources/Categories/`, restart Unity, it auto-loads.

---

## Recent Changes

### Code Optimizations (Completed)
- ✅ Moved CategoryLoader from Controllers/ to Managers/
- ✅ Removed ScriptableObject support (JSON only)
- ✅ Simplified code with modern C# (null-conditional operators `?.`)
- ✅ Removed redundant `GetCurrentPlayer()` method
- ✅ ~15% code reduction overall

### Feature Additions (Completed)
- ✅ Round selection in PlayerSetupScreen (1-5 rounds)
- ✅ English localization (all German text replaced)
- ✅ Editor tools for round selection UI setup
- ✅ Keyboard controls for Editor testing (Up/Down arrows)

### Android Build Fixes (In Progress)
- ✅ APK builds successfully
- ✅ App installs on Pixel 9 Pro
- ✅ UI functional, screens work correctly
- ❌ **Accelerometer not working** (main blocker)
- ⏳ Native Android sensor implementation complete (needs testing)

---

## Testing Checklist

### In Unity Editor (Works ✅)
- Category loading: ✅ Working
- Player setup: ✅ Working
- Round selection: ✅ Working
- Gameplay with keyboard: ✅ Working (Up/Down arrows)
- Screen transitions: ✅ Working
- Score tracking: ✅ Working

### On Android Device (Google Pixel 9 Pro)
- App installation: ✅ Working
- App launcher icon: ✅ Working (after removing custom AndroidManifest)
- UI display: ✅ Working
- Touch controls: ✅ Working
- Unity Input.acceleration: ❌ NOT working (returns zeros)
- Unity Input.gyro: ❌ NOT working (can't enable)
- Native AndroidAccelerometer: ⏳ **NEEDS TESTING**

---

## Next Steps (For Next Agent/Session)

### Immediate Priority: Fix Accelerometer

1. **Build fresh APK** with latest AndroidAccelerometer code
2. **Test on Pixel 9 Pro:**
   - Install APK
   - Open app
   - Look at AccelerometerDebug display (should show on GameplayScreen)
   - Check if it shows "Using: NATIVE" and non-zero X/Y/Z values

3. **Debug via Logcat** (if still not working):
   ```powershell
   adb logcat -s Unity
   ```
   Look for logs:
   - `[AndroidAccel] Starting initialization...`
   - `[AndroidAccel] Got activity: true`
   - `[AndroidAccel] Got SensorManager: true`
   - `[AndroidAccel] ✓ Registration: true`
   - `[SensorListener] ✓ First callback received!`

4. **If native sensor fails:**
   - Check Logcat for Java exceptions
   - Verify Android permissions (though sensors don't need runtime permissions)
   - Consider alternative: **Unity version upgrade to 2023.2+ LTS** (confirmed Android 15 fix)
   - Last resort: Add **touch buttons** as fallback control method

### Alternative Solutions if Native Fails

**Option A: Upgrade Unity** (Recommended if native fails)
- Unity 2023.2 LTS has Android 15 sensor fixes
- Requires re-testing all scripts
- May need InputSystem package update

**Option B: Touchscreen Fallback**
- Add swipe-down gesture = Correct
- Add swipe-up gesture = Skip
- Keep accelerometer as preferred method
- Show tutorial screen explaining controls

**Option C: Different Sensors**
- Try `Input.gyro.rotationRate` instead of acceleration
- Use `Input.gyro.attitude` (quaternion) for tilt detection
- May work even if acceleration doesn't

---

## Common Issues & Solutions

### "Categories not loading"
- Check `Resources/Categories/` folder exists
- Ensure JSON files have `.json` extension
- Verify JSON format is valid

### "Game doesn't respond to tilt"
- Check AccelerometerDebug display (should show changing values)
- Verify `tiltThreshold` in inspector (default 0.3)
- Try increasing threshold to 0.5 for easier detection
- Check Logcat for sensor errors

### "App not in launcher after build"
- Delete `Assets/Plugins/Android/AndroidManifest.xml` if exists
- Let Unity generate default manifest
- Rebuild APK

### "Build errors"
- Check Android SDK installed (via Unity Hub)
- Verify JDK path in Unity Preferences
- Ensure Android Build Support installed

---

## Code Patterns Used

### Singleton Pattern
```csharp
public static GameManager Instance { get; private set; }
void Awake() {
    if (Instance == null) {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    } else {
        Destroy(gameObject);
    }
}
```

### Modern C# Null-Conditional
```csharp
// Instead of: if (obj != null) obj.Method();
obj?.Method();

// Instead of: var x = (obj != null) ? obj.value : defaultValue;
var x = obj?.value ?? defaultValue;
```

### AndroidJavaProxy for Native Android
```csharp
private class SensorListener : AndroidJavaProxy {
    public SensorListener() : base("android.hardware.SensorEventListener") {}
    
    public void onSensorChanged(AndroidJavaObject sensorEvent) {
        // Called by Android system
    }
}
```

---

## Developer Notes

- **Phone Orientation:** Game designed for portrait mode with phone held upside-down to forehead
- **Accelerometer Axis:** Y-axis is primary (vertical when phone upright), changes to horizontal when phone at forehead
- **Testing Device:** Google Pixel 9 Pro (Android 15) - problematic OS for Unity 2022.3 LTS
- **Unity Versions Affected:** 2022.3 LTS has Android 15 sensor bugs, fixed in 2023.2+
- **Fallback Strategy:** Native Android API via JNI bridge (AndroidJavaProxy)

---

## Resources & References

- **Unity Input System:** https://docs.unity3d.com/Packages/com.unity.inputsystem@latest
- **Android SensorManager:** https://developer.android.com/reference/android/hardware/SensorManager
- **Unity Android 15 Issues:** Known bug in Unity Issue Tracker (search "Android 15 accelerometer")
- **AndroidJavaProxy Docs:** https://docs.unity3d.com/ScriptReference/AndroidJavaProxy.html

---

**Last Updated:** December 9, 2025  
**Unity Version:** 2022.3 LTS  
**Target Device:** Google Pixel 9 Pro (Android 15)  
**Critical Blocker:** Accelerometer sensor access  
**Status:** Native sensor implementation complete, awaiting device testing
