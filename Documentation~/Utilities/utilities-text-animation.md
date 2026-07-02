---
title: Text Animation
---
# Text Animation
Per-character animation for [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest) text. You wrap text in a custom `<anim>` tag with two
independent channels, `motion` and/or `color`. The animation is driven by a single [`TextAnimator`](../../Runtime/Scripts/Utilities/TextAnimation/TextAnimator.cs) component
which is automatically added when animation on the object is enabled.

### Features
- Per `TMP_Text` via a single extension method, works with both UGUI (`TextMeshProUGUI`) & world-space (`TextMeshPro`) text
- A `motion` channel (offset / scale / rotation) & a `color` channel (tint / alpha) that compose on the same characters
- Inline arguments allows you to adjust an effect per tag
  - e.g. `motion="wave(20, 2)"`
- Effects are discovered automatically using reflection, **allowing you to create your own effects**
- Animations will preview live in the Editor, in both Scene & Game view
- Re-parses automatically only when the text changes, so swapping strings at runtime keeps animating

## Writing the text
> [!NOTE]
> _An `<anim>` tag whose keys match no registered effect will not animate_

> [!WARNING]
> **_`<anim>` tags do not layer: a character belongs to one tag. When tags overlap, the innermost tag will be used_**

Wrap the characters you want to animate in an `<anim motion="key" color="key">...</anim>` tag. Both attributes are optional, 
so a tag can use **motion only or color only**, or both at the same time.

```
Hello <anim motion="shake">world</anim>!
Watch this <anim motion="wave" color="rainbow">wave</anim> go by.
```
![text-animation-example.gif](../Images/Utilities/text-animation-example.gif)

### Inline arguments

An effect key may carry positional arguments in parentheses to tune it for that one tag. Omitted arguments fall back to the effect's defaults.

```
<anim motion="wave(20, 2)">big slow wave</anim>
<anim color="fade(4)">breathing text</anim>
```

## Built-in Effets
The built-in effects each show a different channel: `shake` & `wave` drive `Offset`, `jitter` adds `RotationDeg`,
[`PulseMotionEffect`](../../Runtime/Scripts/Utilities/TextAnimation/Effects/PulseMotionEffect.cs) drives `Scale`, and the color effects drive `Color`. Read those for reference patterns.

### Built-in motion effects

| Key | Effect | Arguments |
|---|---|---|
| `shake` | Perlin-noise positional shake | `shake(amplitude, frequency)` |
| `wave` | Travelling vertical sine wave | `wave(amplitude, speed, charPhase)` |
| `jitter` | Random position & rotation jitter | `jitter(positionAmount, rotationAmount)` |
| `pulse` | Per-character size pulse (scale) | `pulse(amplitude, speed, charPhase)` |

![text-motion-animation-example.gif](../Images/Utilities/text-motion-animation-example.gif)

```text
<anim motion="shake">shake</anim>
<anim motion="wave">wave</anim>
<anim motion="jitter">jitter</anim>
<anim motion="pulse">pulse</anim>
```

### Built-in color effects

> [!NOTE]
> _Color effects multiply the character's original vertex color, so they are most vivid on white text & modulate tinted text._

| Key | Effect | Arguments |
|---|---|---|
| `rainbow` | Hue cycles across characters & over time | `rainbow(speed, charPhase)` |
| `gradient` | Static two-color blend across the span | none |
| `fade` | Alpha breathes over time | `fade(speed, minAlpha)` |
| `flash` | Blinks between two colors on a period | `flash(period)` |

![text-color-animation-example.gif](../Images/Utilities/text-color-animation-example.gif)

```text
<anim color="rainbow">rainbow</anim>
<anim color="gradient">gradient</anim>
<anim color="fade">fade</anim>
<anim color="flash">flash</anim>
```

## Playing an animation

> [!NOTE]
> _In the Editor, any loaded TMP text containing an `<anim>` tag previews automatically, so you do not need to call
> `PlayTextAnimation()` to see it animate in Scene or Game view. The call is what opts the text in at runtime._

Enabling animation on a GameObject means it uses an [`AnimatedTextMarker`](../../Runtime/Scripts/Utilities/TextAnimation/AnimatedTextMarker.cs) component. This is how the animation is
registered & updated.
**There are three ways to add it:**
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
        m_label.text = "Hello <anim motion=\"shake\">world</anim>!";
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
- [ ] Disabled removes the `AnimatedTextMarker` component _(**Will not** Animate)_

![text-animation-toggle.png](../Images/Utilities/text-animation-toggle.png)

### Pausing while not visible
> [!NOTE]
> _UGUI has no frustum culling, so scrolling a UGUI label fully off a screen edge does not pause it. Only the signals listed above do._

Animated labels skip their per-character work while not visible & resume seamlessly when shown again.

What "Not visible" entails:
- The text is inactive or disabled.
- World-space 3D TMP text that is frustum-culled off screen.
- UGUI TMP text whose `CanvasRenderer` is culled, whose inherited alpha is approximately 0.0, or whose `Canvas` is disabled.

## How the tag is parsed
> [!IMPORTANT]
> TMP only understands its own rich-text tags, so [`AnimTagPreprocessor`](../../Runtime/Scripts/Utilities/TextAnimation/AnimTagPreprocessor.cs) (an `ITextPreprocessor`) strips every `<anim>` tag before TMP parses & records each run's range and keys. Every other tag is passed through untouched.

`AnimatedText` maps those recorded ranges back to visible characters using `TMP_CharacterInfo.index`, then resolves each channel through the registry.

## How effects are loaded
> [!IMPORTANT]
> Types are marked `[Preserve]` so IL2CPP stripping keeps effect classes that nothing references directly.

Effects are discovered by reflection at boot time using `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`. The [`TextEffectRegistry`](../../Runtime/Scripts/Utilities/TextAnimation/TextEffectRegistry.cs) scans every loaded
assembly for non-abstract [`MotionTextEffect`](../../Runtime/Scripts/Utilities/TextAnimation/MotionTextEffect.cs) & [`ColorTextEffect`](../../Runtime/Scripts/Utilities/TextAnimation/ColorTextEffect.cs) subclasses carrying a [`[TextEffect("key")]`](../../Runtime/Scripts/Utilities/TextAnimation/TextEffectAttribute.cs) attribute, instantiates
one shared instance per key. Keys resolve per channel, so a motion key & a color key may reuse the same string.

## Adding a new effect

> [!TIP]
> _To make a tuned variant of an existing effect, subclass it with a different key & default field values, for
> example a `shake2` that overrides `m_amplitude`. The tuning fields are `protected` for exactly this. For a one-off tweak, prefer inline arguments._

Inherit from `MotionTextEffect` (offset / scale / rotation) or `ColorTextEffect` (tint / alpha) & tag it with `[TextEffect("yourKey")]`, then implement the
per-character math. The base you extend is what routes the key into the motion or color channel.

Inline arguments arrive as raw tokens in [`EffectArgs`](../../Runtime/Scripts/Utilities/TextAnimation/EffectArgs.cs); read them with the typed getters (`GetFloat`, `GetInt`, `GetBool`, `GetString`, `GetColor`),
each taking a fallback for when the author omitted that argument. The getters do not allocate, so reading them each frame is free.

```csharp
using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    [Preserve]
    [TextEffect("swing")]
    public class SwingEffect : MotionTextEffect
    {
        protected float m_angle = 15f;
        protected float m_speed = 4f;

        // charIndex is the character's index within the span; spanLength is the span's character count;
        // time is the animation clock; args holds any inline values from the tag (swing(angle, speed)).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var angle = args.GetFloat(0, m_angle);
            var speed = args.GetFloat(1, m_speed);

            mod.RotationDeg = Mathf.Sin(time * speed + charIndex) * angle;
        }
    }
}
```

You write into the [`CharMod`](../../Runtime/Scripts/Utilities/TextAnimation/CharMod.cs) struct passed by `ref`. It carries
every channel an effect can drive, and starts at an identity value (zero offset, zero rotation, unit scale, white):

| Field | Type | Channel | Meaning |
|---|---|---|---|
| `Offset` | `Vector3` | motion | Position offset, applied after rotation & scale |
| `RotationDeg` | `float` | motion | Rotation about the character center, in degrees (Z axis) |
| `Scale` | `float` | motion | Uniform scale about the character center, `1` is unchanged |
| `Color` | `Color32` | color | Multiplied against the character's original vertex color |

## Limitations
- `<anim>` tags do not layer, so a character can belong to only one tag at a time. When tags overlap, the innermost tag wins.
- Rotation is single-axis (Z) about the character center, which suits 2D / screen-space text.
