
# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased] - yyyy-mm-dd

### Added
- Added WebsiteDocs~ directory to generate website documentation from Documentation~ folder
- 

### Changed
- 

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
