
# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased] - yyyy-mm-dd

### Added
- 

### Changed
- 

### Fixed
-

## [0.0.5] - yyyy-mm-dd

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

## [0.0.1] - 2024-12-15

### Added
- All documentation of included scripts & Samples
- All Samples formalized

### Changed


### Fixed
