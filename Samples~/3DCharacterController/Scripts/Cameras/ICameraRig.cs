// Created by Claude (claude-opus-5-5)
// Date: 2026-11-01

using UnityEngine;

namespace Samples.Cameras
{
    /// <summary>
    /// Shared surface of every camera that follows a target, used by the controller selector.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-5-5) — 2026-11-01</remarks>
    public interface ICameraRig
    {
        Transform transform { get; }

        void SetTarget(Transform newTarget);
    }
}
