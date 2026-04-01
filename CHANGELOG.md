
# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [0.0.10-preview] - DATE

### Added
- 

### Changed
- 

### Fixed
- 

## [0.0.9] - 2026-04-01

### Added
- Implemented Unit Tests for `LevelLoader.cs` in `LevelLoaderTests.cs`
  - Added check for Duplicate LevelLoader objects
  - Added check for duplicate loaded levels
  - Added check for correct level load flows
- Added `SPACE.cs` enum to allow selection between World & Local spaces
- Added `WaitForRotationAnimation.cs` to adjust tranform rotations easily
- Added `Contributing.md` as our first pass for contribution guidelines
- Added `SimplePathFollow.cs`
  - Added `SimplePathFollowEditor.cs` to manage moving path point handles & drawing line gizmos
  - Added ability for target transform to move along created path
  - Added Documentation & images for `SimplePathFollow.cs`

### Changed
- Updated `LevelLoader.cs` to improve its usability
  - Added `public IReadOnlyList` of levels
  - Added editor only debugging to load specific levels
  - Adjusted `LoadFirstLevel()` to load the debug index when in editor, and the user has enabled debugging
  - Added `LoadLevelAtIndex()` to simplify external access to loading levels
- Added functionality to `WaitForAnimationBase.cs` to be called from Unity Events
  - Added `WaitForAnimationBase.Animate()` to be easily called from Unity Events
  - Added `defaultAnimationTime` to allow `Animate()` to use when calling
  - Added `m_lastAnimationDirection` to allow `Animate()` to ping-pong between positions when calling `Animate()`
- Added Transform Space selection for WaitForAnimations
  - Added SPACE field into `WaitForAnimationBase.AnimationData` class
  - Added SPACE switch into `WaitForMoveAnimations.SetValue()`
- Updated documentation to meet new decided standards
  - `FixedPaletteTool` documentation was moved into the `/Documentation~/` directory
  - `WebGLTemplates` documentation was moved into the `/Documentation~/` directory
  - All images we sub categorized & placed within the `/Images/` directory
  - All documents image links were updated to reflect the new directory structure
  - All documents now include the yaml title tag
  - Updated `/WebsiteDocs/package.json` to remove the now obsolete commands for moving directories
- Updated documentation for `WaitForAnimationBase.cs`

## [0.0.8f1] - 2026-01-23

### Fixed
- Added `Jam-Stater.Runtime.Geodesics.asmdef` to allow awaiting for NaughtyAttributes to be included preventing compilation issue
- Removed #if UNITY_NUGET in favour of using an assembly definition defineConstraints
- Removed the zLinq dependency within `ScriptingDefinitionHelper.cs` causing compilation error

## [0.0.8] - 2025-12-10

### Added
- Added `SPACE.cs` enum, that is used for `PingPongAnimator.cs` & `SimpleSpin.cs` to allow space specific assignments
- Added `TransformExtensions.cs` to provide Setting `position` & `rotation` functions that use `enum SPACE` as a parameter
- Added `TweenToCoroutine()` & `TweenScaleToCoroutine()` variant of the `TweenTo()` library
  - Still operates using the `Update()` loop, but allows yielding
- Added Playmode tests for `ScreenFader.cs`
- Added UniTask to `AddPackages.cs` as a default included package
- Added `AddNuGetPackages.cs` as an automatic implementation of nuget packages
  - Automatically adds the `ZLinq` nuget package
- Added `JamStarter.Editor.NuGet-Packages` assembly definition that will prevent pre-emptive compilation of Nuget code without the package being installed
- Added _**Fixed Color Palette Tool**_
  - Added `FixedPaletteSettingsProvider.cs` to display in Project Settings
  - Added `FixedPaletteDrawer.uitoolkit.cs` & `FixedPaletteDrawer.imgui.cs` to display property in inspectors
    - Added `ColorSelectDropdownWindow.TYPE.cs` Custom dropdown window to display as: List, Grid, Shades Grid
  - Added Build Preprocessor to help ensure that the `FixedPaletteSettings` scriptable object is included as a preloaded asset
  - Added `ColorPaletteImporter.cs` to allow flexible importing of palettes by using `IFixedColorPaletteImporter.cs`
    - Added `HEXFixedColorPaletteImporter.cs` & `PNGFixedColorPaletteImporter.cs` as default importers
  - Added `COLOR.cs` as enum to denot Primary, Secondary etc
  - Added `COLOR_SELECT.cs` to allow denoting how colors are selected
  - Added `ColorData.cs` as transportable color data
  - Added `ColorScriptableObject.cs` as container for the Color Palette
  - Added `FixedPaletteAttribute.cs` as main method of denoting in Monobehaviour that a user wants the color to be selectable
  - Added `FixedPaletteSettings.cs` as main throughway to accessing `ColorScriptableObject.cs`
    - Includes section to automatically include the generated `FixedPaletteSettings` as a pre-loaded assets in PlayerSettings
  - Added `PaletteUtility.cs` as helper class to retrieve colors from the selected `ColorScriptableObject.cs`
    - This includes `Editor Only` code that manually parses `FixedPaletteSettings` to obtain color values during the assembly compilation step
  - Added `UnityPaletteParser.cs` to parse & cache the `FixedPaletteSettings.asset` `yaml` file
    - This includes `YamlDotNet.dll`, which is used in-editor only to enable `UnityPaletteParser.cs`
- Added **_Editor_** class `ScriptingDefinitionHelper.cs` to check for specific classes, adding or removing scripting defines
  - Added check for `GameInputDelegator.cs`, and if it exists it will add `JAM_INPUT_DELEGATOR`
- Added `InputHelper.cs` for function to process axis inputs using the Old Input system `KeyCode`
- Added `AddPrefabIfExists.cs` as utility class that checks for a GUID within the Asset Database, adding it into the scene if it exists
  - Added `--- GAME INPUT PROMISE ---.prefab` as container for the `GameInputDelegator` prefab GUID that will be added into the 2D & 3D Character controller sample scenes
- Added `Geodesics Movement Examples` into Samples
  - Added shared assets such as Character with animations
  - Added `CharacterAnimationController.cs` into Shared assets to allow character animations
  - Added `/Sphere Examples/` sub-directory which contains the scene & `SphereMovementController.cs`
  - Added `/Torus Examples/` sub-directory which contains the scene & `TorusMovementController.cs`
- Added `/Utilities/Geodesics/` directory for specialized maths
  - Added `SphereMaths.cs` as main script used to move an object along a surface of a sphere
  - Added `Torus.cs` as data container to simplify required math calls
  - Added partial class `TorusMaths.base.cs` as main location for all required math calculations to work with a torus
  - Added partial class `TorusMaths.extensions.cs` as container for overloads that utilize `Torus.cs`
  - Added `TorusMeshGenerator.cs` since Unity does not provide a way of generating a torus primitive
- Added **_Thumbnail Studio_** sample
  - Added `Photo Studio Sample Scene` which uses a camera stack to add a background
  - Added `CameraScreenshotTool.cs` as main utility script to convert camera image into a texture saved to the project
  - Added `ScreenshotUtility.cs` as tool to select which prefabs will be screenshotted & where in the scene
  - Added `floor-small-square.fbx & `block-grass-overhang-large.fbx` as scene objects, as well as their associated materials
  - Added `Kenney Mini Character 1` as example collection included with the sample

### Changed
- Updated `PingPongAnimator.cs` to utilize the `TransformExtension.cs` & `enum SPACE` to provide more flexibility on use
  - Added `Assert` to `Start()` to help catch potential issues early
- Updated `SimpleSpin.cs` to utilize the `TransformExtension.cs` & `enum SPACE` to provide more flexibility on use
- Updated `TweenData.SetData()` to use a [`enum SPACE`](../../Runtime/Scripts/Utilities/Enums/SPACE.cs) instead of a `bool` to set how it transforms
- Added `enum TweenController.UPDATE_TYPE` to allow sorting of the `TweenData`
- Added `TweenData.AsCoroutine()` to allow yielding as Coroutine
  - Waits until `Active == false` then executes the callback if there was one set
- Added `TweenData.AsAsncTask()` for future implementation of `async`
- Updated `TweenData.SetTargetPosition()`, `TweenData.SetTargetRotation()`, `TweenData.SetTargetScale()` to return `TweenData`, to better allow chaining
- Updated `TweenTo()` tests to use Coroutine & remove deprecated calls
- Added `defaultFadeTime` to `ScreenFader.cs` to allow setting value in the inspector, with a starting default of `0.5f`
- Added overloads for `ScreenFader.cs` `FadeIn()`, `FadeOut()`, `FadeInOut()` without `float time` parameter to fallback to value set in inspector
- Added Automatic Singleton generation for `ScreenFader.cs` which creates the scene Hierarchy
  - No longer inheriting from `HiddenSingleton<>` in favor of managing locally for lifecycle
- Added Pitch to `SDXExtensions.cs`
- Changed `SFXManager.TryGet3DAudioSource()` to `TryGetAudioSourceInstance()`
- Added optional spatialBlend parameter to `TryGetAudioSourceInstance()`
- Added `SFXManager._PlaySoundWithPitch()` to create 2D AudioSource` instances to allow pitch adjustment
- Added optional parameter pitch to `SFXManager.PlaySound()`
- Refactored `AddPackages.Packages` to merge the Package Ids & Package URLs into a single data field
- Added nugetforunity package to `AddPackages.cs`
- Added Checks for `JAM_INPUT_DELEGATOR` into 2D Character Controller Sample
  - Added `#define OLD_INPUT_SYSTEM` into `CharacterController2D.cs` to allow for `[Conditional]` on new `ProcessInputs()` function
  - Added `CharacterController2D.ProcessInputs()` as old input system fallback incase `GameInputDelegator.cs` doesn't exist
  - Wrapped all calls to `GameInputDelegator.cs` in `CharacterController2D.cs` with `#if JAM_INPUT_DELEGATOR`
  - Wrapped all calls to `GameInputDelegator.cs` in `Character2DVisualizer.cs` with `#if JAM_INPUT_DELEGATOR`
  - Added `Character2DVisualizer.LateUpdate()` fallback if `!JAM_INPUT_DELEGATOR`
- Added Checks for `JAM_INPUT_DELEGATOR` into 3D Character Controller Sample
  - Added `#define OLD_INPUT_SYSTEM` into `CharacterController3D.cs` to allow for `[Conditional]` on new `ProcessInputs()` function
  - Added `CharacterController3D.ProcessInputs()` as old input system fallback incase `GameInputDelegator.cs` doesn't exist
- Changed `Geodesics Movement Examples` to use `InputHelper.cs` to process inputs

### Fixed
- 

## [0.0.7f1] - 2025-10-18
### Fixed
- Resolved crash caused by Testframework Assemblies missing a constraint

## [0.0.7] - 2025-10-17

### Added
- Added `Singleton.cs` as a Singleton behaviour where the instance is publicly visible
- Added TestFramework & NUnit support for some of the built in utilities, with Editmode & playmode tests
  - Added tests for `TweenTo`
  - Added tests for `RaycastHitExtensions.cs`
  - Added tests for `JMath.cs`
  - Added tests for `CollisionChecks.cs`

### Changed
- Set `SFXManager.cs`, `MusicController.cs` & `VFXManager.cs` to each utilize the `HiddenSingleton.cs` as a base class to maintain standard use
  - Added static functions in each of these managers that be called from their relative extension classes to help enforce the `HiddenSingleton` structure
- Updated `MusicController._isReady` to be `private` instead of `public`
- Added `null` catch for `TweenController.cs`
- Added `Debug.LogError()` to `TweenController.InstantTween()` to advise avoiding using a `0s` tween time
- Updated `Pull_request_template.md` to better match use case & include a passed tests check


### Fixed
- Resolved potential race condition with `SFXManager.cs` sample when attempting to call `PlaySound()` on the first frame
- Resolved potential race condition with `MusicController.cs` sample when attempting to call `PlayMusic()` on the first frame
- Resolved divide by `0.0` in `TweenController.cs` that would cause a `NaN` error when setting the tween time to `0.0`
  - This was resolved by auo-completing the tween in the event that the `time` value is `0f` by calling `InstantTween()`
- Resolved `CollectionExtensions.PickRandomElement()` & `CollectionExtensions.Shuffle()` not catching null or empty list cases with clear exception
- Resolved `TweenToController` spamming `Debug.LogError()` due to `InstantTween()` not setting `Active = false`

## [0.0.6f1] - 2025-04-26

### Fixed
- Resolved build crash on Unity `6000.1` caused by `ProjectileMath.cs` use of `Mono.Cecil` 

## [0.0.6] - 2025-04-24

### Added
- New PR Template into the `/.github/` directory
- Added `2D Character Controller` Sample
  - Added `2D Character Controller Sample` scene
  - Added `CharacterController2D.cs`
  - Added `CharacterMovementDataScriptableObject.cs`
  - Added `2D Character Controller` prefab
  - Added `2D Character Movement Data` scriptable object that contains default settings for sample
- Added [`ProjectileMath.cs`](Runtime/Scripts/Utilities/Physics/ProjectileMath.cs) with various helpers for trajectory calculations
- Added [`Trajectory Sample`](Samples~/Trajectory/) scene, scripts and prefabs for working with trajectory components
- Added [`Trajectory Sample`](Documentation~/Samples/trajectory.md) documentation
- Added [`RaycastHitExtensions.cs`](Runtime/Scripts/Utilities/Extensions/RaycastHitExtensions.cs) to add methods to sort through non-alloc `RaycastHit` arrays for specific items
  - Includes `GetNearestHit()` & `GetFurthestHit()`
- Added [`TextureAtlasSlicer.cs`](Editor/KennySpriteSlicer/TextureAtlasSlicer.cs) as tool to help parse [Kenny Asset](https://kenney.nl/data/itch/preview/) sprite sheets
- Added **3D Character Controller Sample**, based on [Making A Physics Based Character Controller In Unity](https://youtu.be/qdskE8PJy6Q?si=yGx9nWuwtoum0v6n)
  - Added models from the [Kenny Prototype Kit](https://kenney.nl/data/itch/preview/Previews/Prototype%20Kit.png)
  - Added [CharacterMovement3DDataScriptableObject.cs](Samples~/3DCharacterController/Scripts/CharacterMovement3DDataScriptableObject.cs) as Data container
  - Added [Character3DBalancer.cs](Samples~/3DCharacterController/Scripts/Character3DBalancer.cs) as script to apply floating forces, checking grounded state & keeping character upright
  - Added [CharacterController3D.cs](Samples~/3DCharacterController/Scripts/CharacterController3D.cs) as movement & jumping source
  - Added [PlayerCapsule prefab](Samples~/3DCharacterController/Prefabs/PlayerCapsule.prefab) which represents a basic implementation of the Character Controller
  - Added Sample Scene with all the 3D Character Controller elements implemented
  - Added Player Capsule variant [Animated Player](Samples~/3DCharacterController/Prefabs/Animated%20Player.prefab) which contains the new animations & the Kenny figurine model
  - Added [FigurineAnimationController](Samples~/3DCharacterController/Animation/FigurineAnimationController.controller) with some pre-set animation states
  - Added [CharacterAnimationController.cs](Samples~/3DCharacterController/Scripts/Animation/CharacterAnimationController.cs) as script in charge of updating its `Animator`
  - Added a **Cinemachine Freelook Camera** into the sample scene
  - Added [LockPlayerMouse.cs](Samples~/3DCharacterController/Scripts/LockPlayerMouse.cs) as a way to prevent the mouse from drifting when moving player camera

### Changed
- Added `HitPoint` property to [`MouseCaster.cs`](Runtime/Scripts/Utilities/MouseCaster.cs)
- Added `ShortestRotation()` to [`JMath.cs`](Runtime/Scripts/Utilities/JMath.cs)
- Added `2D Character Controller` into the Package samples
- Adjusted `GameInput` Sample to replace `GrabItem` with `Jump`
  - This includes a change to `Action<bool>` callback for `OnJumpPressed`

### Fixed
-

## [0.0.5] - 2025-02-24

### Added
- Added missing `Circle2Circle()` function in [`CollisionChecks.cs`](Runtime/Scripts/Utilities/Physics/CollisionChecks.cs)
  - Includes overload for `Vector2` parameters
- Added `/WebGLTemplates~/` for a custom HTML player for WebGL builds
- Added `WebGLEditorWindow.cs` to allow the creation of local WebGL Templates
  - There's a Menu Item at `WebGL/Template Wizard` that will open the WebGL template customizer.
  - The Create Template button will copy the template from the package directory into the Local Assets directory and setup all the appropriate build time variables for customization

### Changed
- 

### Fixed
- Fixed [`Draw.Circle()`](Runtime/Scripts/Utilities/Debugging/Draw.cs) position offset being broken
- Fixed build compilation issue with [MouseCaster.cs](Runtime/Scripts/Utilities/MouseCaster.cs) caused by `DrawRay()` preprocessor directive
- Fixed build Compilation issue with [Draw.cs](Runtime/Scripts/Utilities/Debugging/Draw.cs) caused by `Label()` contents not having preprocessor directive
- Fixed [`TweenTo()` Bug](https://github.com/abr-designs/jam-starter-package/issues/8) caused by clamping the time value from `0 - 1`
- Fixed `TransformTweenExtensions.SetupTweenController()` being able to trigger in Editor causing an error

## [0.0.4] - 2025-01-19

### Added
- Added `/WebsiteDocs~` directory to generate website documentation from `/Documentation~` folder
- Added `TransformTweenExtensions.cs` to be a collection of `Transform` Extensions
  - Adds functionalities to move, rotate & scale a `transform` 
  - Includes `CURVE.cs` which allows us to change how a tween moves from 0 -> 1
  - Added `TweenController.cs` as location where Tweens will be updated
- Added docs for the [Materials Sample](Documentation~/Samples/samples-materials.md)
- Added Settings menu to the Main Menu Sample
  - This includes [`SettingsUI.cs`](Samples%7E/MainMenu/SettingsUI.cs) & [`BaseUIWindow.cs](Samples%7E/MainMenu/BaseUIWindow.cs) to help organize functionality

### Changed
- Replaced calls in `LerpFunctions.cs` to `Mathf.Lerp()` with local `LERP()` to be more performant
- Replaced calls in `LerpFunctions.cs` to `Mathf.Clamp()` or `Mathf.Clamp01()` with more performant calls to `Math.Clamp()`
- Moved `Sprite Color Change Example` sample into the `Materials` sample
- Removed the need for the Audio Controller sample from the Main Menu sample, requiring users to add it

### Fixed
-

## [0.0.3] - 2025-01-09

### Added
- Added random enum selection through `Utilities.EnumExtensions.GetRandomEnum()`
- Added `JMath.cs` to provide various math & number functions
  - Includes option to convert `int` into a roman numeral
- Added `LerpFunctions.cs` as utility for custom lerp behaviours
- Added `PhysicsLauncher3D.cs` as a 3D option for launch objects
  - Includes appropriate Gizmos as well
- Added `MouseCaster.cs` into Utilities

### Changed
- Updated README to include Samples tutorial
- Updated `package.json` to show author information
- Added `Draw.Circle()` with overload that includes the circles normal
- Added `Shuffle()` extension method for Lists & Arrays in `CollectionExtensions.cs`
- Set `CollectionExtensions.cs` to use `System.Random` instead of `UnityEngine.Random`
- Added additional functions to `CollisionChecks.cs`
  - `Line2Rect()` - Checks if a line is interacting a rectangle
  - `Poly2Point()` - Checks if a point is interacting with a polygon 
  - `Poly2Rect()` - Checks if a rectangle is interacting with a polygon
### Fixed
-

## [0.0.2] - 2024-12-29

### Added
- `IRecyclable.cs` as interface to include on classes that need `GameObject` recycled
- `Recycler.cs` as `static class` that can attempt to recycle & retrieve `GameObject`
  - This will also auto-generate a Parent Transform when requested.
- Recycling Sample into Package
  - This includes showcasing the difference of recycle via `Enum` or `IRecyclable`

### Changed
- 

### Fixed
-

## [0.0.1-6000] - 2024-12-16

Updates to main branch to function on **Unity 6**

### Changed
- Updated target Unity version to `6000.0`
- Removed packages that are no longer use with **Unity 6**
  - TextmeshPro
- Updated Cinemachine to `3.1.2`
- Updated InputSystem to `1.11.2`
- 

### Fixed
- Fixed namespace upgrade requirement for Cinemachine: `Cinemachine` -> `Unity.Cinemachine`
- Fixed `CinemachineVirtualCamera` -> `CinemachineCamera`
- Fixed `CinemachineSmoothPath` -> `SplineContainer`
-

## [0.0.1] - 2024-12-15

### Added
- All documentation of included scripts & Samples
- All Samples formalized

### Changed


### Fixed
