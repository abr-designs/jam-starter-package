---
title: 3D Character Controller
---
# 3D Character Controller

![3d-controller-physics.gif](../Images/Samples/3d-controller-physics.gif)

### Dependencies
- [Game Input Sample](game-input.md) _(optional)_

## Description
A ready-to-play 3D character that runs, jumps & double jumps around a small test level, with a camera that orbits around it using the mouse.

- The character is driven by physics, so it gets pushed by and pushes against the world
- It floats slightly above the ground on a spring, which lets it glide over steps & slopes without getting stuck
- It stays upright on its own, wobbling a little when bumped before settling back
- Movement follows the camera, so "forward" is always the direction you are looking
- Animations blend from idle to running based on how fast the character moves, and play a take-off, mid-air & landing animation when jumping

## Controls

| Action | Keys |
|---|---|
| Move | `W` `A` `S` `D` |
| Jump & double jump | `Space` |
| Orbit camera | Hold right mouse button & move the mouse |

## What's Included
- **Animated Player**: the playable character, using the [Universal Animation Library](https://quaternius.itch.io/universal-animation-library) model & animations
- **Player Capsule**: a plain capsule version of the same character, useful for testing without animations
- **Freelook Camera**: the orbiting camera; drop it in a scene & point it at any object to follow
- **3D Character Movement Data**: a settings file holding every movement value, so the feel can be tuned without touching code
- **Test level**: built from the [Kenney Prototype Kit](https://kenney.nl/assets/prototype-kit), with stairs, slopes, walls & crates to try the movement on

## Tuning the Feel
Select `3D Character Movement Data` in the Project window to change how the character moves. Changes apply while the game is running.

- **Locomotion**: top speed & how quickly the character speeds up on the ground & in the air
- **Jump**: jump height, time to reach the top, fall speed & the number of jumps allowed in a row
- **Jump Buffer & Coyote Time**: small grace windows that still accept a jump pressed slightly before landing or slightly after running off a ledge
- **Character Balancing**: how high the character floats & how firmly it springs back upright

The camera's distance, orbit speed, look-up & look-down limits and smoothing are set on the `Freelook Camera` itself.
