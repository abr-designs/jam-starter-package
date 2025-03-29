
# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [0.0.6-preview] - yyyy-mm-dd

### Added
- New PR Template into the `/.github/` directory
- Added [`ProjectileMath.cs`](Runtime/Scripts/Utilities/Physics/ProjectileMath.cs) with various helpers for trajectory calculations
- Added [`Trajectory Sample`](Samples~/Trajectory/) scene, scripts and prefabs for working with trajectory components
- Added [`Trajectory Sample`](Documentation~/Samples/trajectory.md) documentation
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
  - 

### Changed
- Added `HitPoint` property to [`MouseCaster.cs`](Runtime/Scripts/Utilities/MouseCaster.cs)
- Added `ShortestRotation()` to [`JMath.cs`](Runtime/Scripts/Utilities/JMath.cs)

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
