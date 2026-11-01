---
title: 3D Character Controller
---
# 3D Character Controller

![3d-controller-physics.gif](../Images/Samples/3d-controller-physics.gif)

### Dependencies
- [Game Input Sample](game-input.md) _(optional)_

## Description
Two ready-to-play 3D characters and two cameras, set up in a small test level. Buttons on screen swap between them while the game is running, so each combination can be tried side by side.

### Characters
- **Spring Character Controller**: driven by physics, it runs, jumps & double jumps
  - Gets pushed by and pushes against the world
  - Floats slightly above the ground on a spring, gliding over steps & slopes without getting stuck
  - Stays upright on its own, wobbling a little when bumped before settling back
- **Classic Character Controller**: built on Unity's standard `CharacterController`, it walks & runs with simple, predictable movement
  - Starts & stops instantly, with no momentum
  - Walks up slopes & steps up to a set limit and sticks to the ground on the way down
  - Does not jump

Both characters move relative to the camera, so "forward" is always the direction you are looking. Animations blend from idle to running based on how fast the character moves, and the Spring character also plays a take-off, mid-air & landing animation when jumping.

### Cameras
- **Orbit Camera**: circles the character at a fixed distance, keeping it centred on screen
- **Shoulder Camera**: sits behind & slightly to the side of the character, like an over-the-shoulder action game

## Controls

| Action | Keys |
|---|---|
| Move | `W` `A` `S` `D` |
| Jump & double jump _(Spring character)_ | `Space` |
| Orbit camera | Hold right mouse button & move the mouse |
| Shoulder camera | Move the mouse |
| Swap character or camera | Click the on-screen buttons |

## What's Included
- **Spring Character Controller**: the physics character, using the [Universal Animation Library](https://quaternius.itch.io/universal-animation-library) model & animations
- **Classic Character Controller**: the `CharacterController` based character, using the same model & animations
- **Orbit Camera** & **Shoulder Camera**: drop either in a scene & point it at any object to follow
- **Spring Character Movement Data**: a settings file holding every Spring character movement value, so the feel can be tuned without touching code

## Tuning 

### Spring Character
Select `Spring Character Movement Data` in the Project window to change how the character moves. Changes apply while the game is running.

- **Locomotion**: top speed & how quickly the character speeds up on the ground & in the air
- **Jump**: jump height, time to reach the top, fall speed & the number of jumps allowed in a row
- **Jump Buffer & Coyote Time**: small grace windows that still accept a jump pressed slightly before landing or slightly after running off a ledge
- **Character Balancing**: how high the character floats & how firmly it springs back upright

### Classic Character
Movement speed, turn speed, slope limit & gravity are set on the `Classic Character Controller` itself.

### Cameras
Distance, mouse sensitivity, look-up & look-down limits and smoothing are set on each camera. The Shoulder Camera also has a shoulder offset to move it further to the side.

### Animation
Each character's `Character Animation Controller` has a **Speed Smooth Time** that eases the switch between idle & running. The Classic character uses it to soften its instant starts & stops; the Spring character leaves it at 0 since physics already smooths its speed.

## Choosing the Starting Character & Camera
Select the `Controller Selector` in the scene. It holds a list of characters and a list of cameras; the **Start Character Index** & **Start Camera Index** pick which entry is active when the game begins, counting from 0.

## Adding Your Own Character or Camera
A new character works with the animation & selector once its script implements `IThirdPersonCharacter` (speed, grounded state & which camera to move relative to). A new camera needs `ICameraRig` (which target to follow). Add the new object to the matching list on the `Controller Selector` and hook a button up to `SelectCharacter` or `SelectCamera` with its position in that list.
