---
title: Text Animation
---
# Text Animation
Per-character animation for [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest) text. You wrap text in a `<link>` tag, and the link 
id selects a registered effect _(shake, wave, jitter, pulse, or custom)_. The animation is driven by a single [`TextAnimator`](../../Runtime/Scripts/Utilities/TextAnimation/TextAnimator.cs)
tick injected into Unity's PlayerLoop, so no extra components are added to your GameObject.

### Features
- Opt-in per `TMP_Text` via a single extension method, works with both UGUI (`TextMeshProUGUI`) & world-space (`TextMeshPro`) text
- Effects are discovered automatically using reflection
  - This allows you to create your own effects 
- Animations preview live in the Editor, in both Scene & Game view
- Re-parses automatically when the text changes, so swapping strings at runtime keeps animating

## Writing the text
> [!NOTE]
> _A `<link>` whose key matches no registered effect is left static & inert, so genuine hyperlinks are unaffected._
 
> [!WARNING]
> **_Links cannot overlap, so a character belongs to at most one effect._**

Wrap the characters you want to animate in a `<link="registeredkey">...</link>` tag, where `registeredkey` is the effect's id. Plain text
outside the tag stays the same. 

```
Hello <link="shake">world</link>! Watch this <link="wave">wave</link> go by.
```
![text-animation-example.gif](../Images/Utilities/text-animation-example.gif)

### Built-in effects

| Key | Effect |
|---|---|
| `shake` | Perlin-noise positional shake |
| `wave` | Travelling vertical sine wave |
| `jitter` | Random position & rotation jitter |
| `pulse` | Per-character size pulse (scale) |

![text-animation.gif](../Images/Utilities/text-animation.gif)

## Playing an animation

> [!NOTE]
> _In the Editor, any loaded TMP text containing a `<link>` tag previews automatically, so you do not need to call
> `PlayTextAnimation()` to see it animate in Scene or Game view. The call is what opts the text in at runtime._

Enabling animation on a GameObject means it uses an [`AnimatedTextMarker`](../../Runtime/Scripts/Utilities/TextAnimation/AnimatedTextMarker.cs) component. This is how the animation is 
registered & updated. 
There are three ways to add it:
1. Enabled the inspector "Animate Text" toggle
2. Calling the `PlayTextAnimation()` extension method
3. Adding the `AnimatedTextMarker` manually
   - Call the [`PlayTextAnimation()`](../../Runtime/Scripts/Utilities/TextAnimation/TMP_TextExtensions.cs) extension method on any `TMP_Text` to add the marker. Call `StopTextAnimation()` to remove it, restoring the original mesh & unregistering the text.

```csharp
using TMPro;
using UnityEngine;
using Utilities.TextAnimation;

public class TextAnimationExample : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_label;

    private void OnEnable()
    {
        m_label.text = "Hello <link=\"shake\">world</link>!";
        m_label.PlayTextAnimation();
    }

    private void OnDisable()
    {
        m_label.StopTextAnimation();
    }
}
```

### Inspector toggle

> [!NOTE]
> _`AnimatedTextMarker` is `[ExecuteAlways]`, so the toggle also enables the live edit-mode preview in Scene & Game view._

[`AnimatedTextHeaderToggle`](../../Runtime/Scripts/Utilities/TextAnimation/Editor/AnimatedTextHeaderToggle.cs) appends an "Animate Text" toggle to the GameObject inspector header 
of any GameObject carrying a `TMP_Text`. 
- [x] Enabled adds an `AnimatedTextMarker` component _(Will Animate)_
- [ ] Disabled removes the `AnimatedTextMarker` component _(Wont Animate)_

![text-animation-toggle.png](../Images/Utilities/text-animation-toggle.png)

### Pausing while not visible
> [!NOTE]
> _UGUI has no frustum culling, so scrolling a UGUI label fully off a screen edge does not pause it. Only the signals listed above do._

Animated labels skip their per-character work while not visible & resume seamlessly when shown again.

What "Not visible" entails:
- The text is inactive or disabled.
- World-space 3D TMP text that is frustum-culled off screen.
- UGUI TMP text whose `CanvasRenderer` is culled, whose inherited alpha is approximately 0.0, or whose `Canvas` is disabled.

## How effects are loaded
> [!IMPORTANT]
> Types are marked `[Preserve]` so IL2CPP stripping keeps effect classes that nothing references directly.

Effects are discovered by reflection at boot time using `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`. The [`TextEffectRegistry`](../../Runtime/Scripts/Utilities/TextAnimation/TextEffectRegistry.cs) scans every loaded 
assembly for non-abstract [`TextEffect`](../../Runtime/Scripts/Utilities/TextAnimation/TextEffect.cs) subclasses carrying a [`[TextEffect("key")]`](../../Runtime/Scripts/Utilities/TextAnimation/TextEffectAttribute.cs) attribute, instantiates
one shared instance per key.

## Adding a new effect
Inherit from `TextEffect` & tag it with the following attribute `[TextEffect("yourKey")]`, you can then implement the 
per-character math.

```csharp
using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    [Preserve]
    [TextEffect("rainbow")]
    public class RainbowEffect : TextEffect
    {
        protected float m_speed = 2f;
        protected float m_charPhase = 0.15f;

        // charIndex is the character's index within the span; time is the animation clock.
        public override void Apply(ref CharMod mod, int charIndex, float time)
        {
            var hue = Mathf.Repeat(time * m_speed * 0.1f + charIndex * m_charPhase, 1f);
            mod.Color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }
}
```

You write into the [`CharMod`](../../Runtime/Scripts/Utilities/TextAnimation/CharMod.cs) struct passed by `ref`. It carries
every channel an effect can drive, and starts at an identity value (zero offset, zero rotation, unit scale, white):

| Field | Type | Meaning |
|---|---|---|
| `Offset` | `Vector3` | Position offset, applied after rotation & scale |
| `RotationDeg` | `float` | Rotation about the character center, in degrees (Z axis) |
| `Scale` | `float` | Uniform scale about the character center, `1` is unchanged |
| `Color` | `Color32` | Multiplied against the character's original vertex color |

> [!TIP]
> _To make a tuned variant of an existing effect, subclass it with a different key & default field values, for
> example a `shake2` that overrides `m_amplitude`. The tuning fields are `protected` for exactly this._

The four built-in effects each show a different channel: `shake` & `wave` drive `Offset`, `jitter` adds `RotationDeg`, and
[`PulseEffect`](../../Runtime/Scripts/Utilities/TextAnimation/Effects/PulseEffect.cs) drives `Scale`. Read those for reference patterns.

## Limitations
- Inline per-tag parameters such as `<link="shake:amp=4">` are not currently supported. 
  - Current workaround is to use a subclass variant with your desired changes.
- TMP links cannot overlap, so a character can belong to only one effect at a time.
- Rotation is single-axis (Z) about the character center, which suits 2D / screen-space text.
