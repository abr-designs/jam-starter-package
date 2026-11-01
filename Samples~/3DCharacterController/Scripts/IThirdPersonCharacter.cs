// Created by Claude (claude-opus-5-5)
// Date: 2026-11-01

using UnityEngine;

namespace Samples.CharacterController3D.Scripts
{
    /// <summary>
    /// Shared surface of every third person character controller, read by the animation controller &
    /// the controller selector so neither depends on a concrete controller type.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-5-5) — 2026-11-01</remarks>
    public interface IThirdPersonCharacter
    {
        Vector3 Velocity { get; }
        float MaxSpeed { get; }
        bool IsGrounded { get; }

        Transform transform { get; }

        void SetCameraTransform(Transform cameraTransform);
    }
}
