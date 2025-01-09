# Jam Starter Kit
This package is meant to include an ever-growing library of scripts, tools, systems & basic textures that help to kick off
projects & Game jams.

> ## Installation Instructions
> To install the Starter kit:
> 
> ![giturl_installation.PNG](Documentation%7E/Images/giturl_installation.PNG)
> 1. Open the Package Window
> 2. Select the **+** button
> 3. Click Add from GitURL
> 4. Paste `https://github.com/abr-designs/jam-starter-package.git` into the text box


### `v0.0.3` - Jan 09, 2025
### Supports Unity 2022

## To Do
- [ ] Add Cinematics Documentation
- [ ] Expand [TransformAnimator.cs](Runtime/Scripts/Utilities/Animations/TransformAnimator.cs) to function in 3D
- [ ] Add missing Gizmos into [WaitForUIMoveAnimations.cs](Runtime/Scripts/Utilities/WaitForAnimations/WaitForUIMoveAnimations.cs)
- [ ] Remove call requirement for [PhysicsLauncher.cs](Runtime/Scripts/Utilities/Physics/PhysicsLauncher.cs)
- [ ] Add Settings menu sample into [MainMenuUI.cs](Samples~/MainMenu/MainMenuUI.cs)
- [ ] Add Character Controller Example
- [ ] Add Text Animators
- [ ] Add Trajectory Line Renderer
- [ ] Add procedural shapes using shaders
- [x] ~~Expand [CollisionChecks.cs](Runtime/Scripts/Utilities/Physics/CollisionChecks.cs) library~~ - As of `v0.0.3`
- [x] ~~Expand [PhysicsLauncher.cs](Runtime/Scripts/Utilities/Physics/PhysicsLauncher.cs) to function in 3D~~ - As of `v0.0.3`
- [x] ~~Implement Reorder List extension into [CollectionExtensions.cs](Runtime/Scripts/Utilities/extensions/CollectionExtensions.cs)~~ - As of `v0.0.3`
- [x] ~~Add Recycling Utility~~ as of `v0.0.2`


## Samples
> To add samples, navigate to the **_Package Manager window → Jam Starter Kit → Samples_**
> 
> ![samples](Documentation%7E/Images/samples_tutorial.PNG)

- ### [VFX Manager](Documentation~/Samples/vfx-manager.md)
- ### [SFX Manager](Documentation~/Samples/sfx-manager.md)
- ### [Music Manager](Documentation~/Samples/music-manager.md)
- ### [Dialog System](Documentation~/Samples/dialog-system.md)
- ### [Game Input](Documentation~/Samples/game-input.md)
- ### [Cinematics - WIP](Documentation~/Samples/cinematics.md)
- ### [MainMenuUI.cs](Samples~/MainMenu/MainMenuUI.cs)

## Features
- ### [Level Loading](Documentation~/Levels.md)
- ### [Interfaces](Documentation~/Interfaces.md)
- ### [Utilities](Documentation~/Utilities.md)
  - #### [Animators](Documentation~/Utilities/utilities-animators.md)
  - #### [Wait-For Animations](Documentation~/Utilities/utilities-wait-animation.md)
  - #### [Physics](Documentation~/Utilities/utilities-physics.md)
  - #### [Extensions](Documentation~/Utilities/utilities-extensions.md)
  - #### [Debugging - Draw.cs](Documentation~/Utilities/utilities-draw.md)
  - #### [Recycling](Documentation~/Utilities/utilities-recycling.md)

---

### Unity packages

| Package          | Version |
|------------------|---------|
| [Unity Feature 2d](https://docs.unity3d.com/6000.0/Documentation/Manual/2DFeature.html) | Default |
| [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html)   | 3.0.9   |
| [Cinemachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.3/manual/CinemachineOverview.html)    | 2.10.1  |
| [New Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/index.html) | 1.7.0   |
| [Newtonsoft JSON](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html) | 3.2.1   |

#### Custom Packages
> _See [AddPackages.cs](Editor/AddPackages.cs)_
- [Naughty Attributes](https://github.com/dbrizov/NaughtyAttributes)
- [Unity Utils](https://github.com/ayellowpaper/SerializedDictionary)
- [Cast Visualizer](https://www.bgtools.de/page/castvisualizer-manual/)
- [Timescale Toolbar](https://assetstore.unity.com/packages/tools/utilities/timescale-toolbar-291564?srsltid=AfmBOorUkK2rYmOyD1nEe3st6saUWgPJvKqNkvB6vEWthLqgC344A70Y)