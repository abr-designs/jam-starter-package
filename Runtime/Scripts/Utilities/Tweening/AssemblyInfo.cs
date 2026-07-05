// Created by Claude (claude-opus-4-8)
// Date: 2026-06-14

using System.Runtime.CompilerServices;

// One-way visibility so the gated async tween assembly can reach internal helpers
// (TweenController.HasActiveTween, TRANSFORM, TweenMath). Core keeps no compile-time
// knowledge of UniTask.
[assembly: InternalsVisibleTo("Jam-starter.Runtime.tweening.unitask")]
