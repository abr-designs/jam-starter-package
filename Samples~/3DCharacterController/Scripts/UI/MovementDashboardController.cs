// Created by Claude (claude-sonnet-4-6)
// Date: 2026-05-16

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Samples.CharacterController3D.Scripts.UI
{
    /// <summary>
    /// Runtime overlay for tuning CharacterMovement3DDataScriptableObject values during Play mode.
    /// Requires a UIDocument component on the same GameObject with MovementDashboard.uxml assigned.
    /// Active in DEBUG builds only; disabled automatically in release builds.
    /// </summary>
    /// <remarks>Created by Claude (claude-sonnet-4-6) — 2026-05-16</remarks>
    public class MovementDashboardController : MonoBehaviour
    {
#if DEBUG
        private struct Snapshot
        {
            public float maxSpeed, GroundAcceleration, AirAcceleration, maxAccelerationForce;
            public Vector3 forceScale;
            public float gravityScaleDrop;
            public float JumpHeight, TimeTillJumpApex, JumpHeightCompensationFactor;
            public float MaxVerticalVelocity, GravityOnReleaseMultiplier, MaxFallSpeed;
            public int NumberOfJumpsAllowed;
            public float TimeForUpwardsCancel, ApexThreshold, ApexHangTime;
            public float JumpBufferTime, JumpCoyoteTime;
            public float rideHeight, springStrength, springDamper, uprightStrength, uprightDamper;
        }

        private CharacterMovement3DDataScriptableObject m_data;
        private Snapshot m_snapshot;
        private VisualElement m_panel;
        private readonly List<System.Action> m_refreshActions = new();

        private void Start()
        {
            var controller = FindFirstObjectByType<CharacterController3D>();
            if (controller == null)
            {
                Debug.LogError("[MovementDashboard] No CharacterController3D found in scene.");
                enabled = false;
                return;
            }

            m_data = controller.MovementData;
            if (m_data == null)
            {
                Debug.LogError("[MovementDashboard] CharacterController3D has no MovementData assigned.");
                enabled = false;
                return;
            }

            if (!TryGetComponent<UIDocument>(out var doc))
            {
                Debug.LogError("[MovementDashboard] No UIDocument component found.");
                enabled = false;
                return;
            }

            m_snapshot = CaptureSnapshot();

            var root = doc.rootVisualElement;

            // Block WASD/arrow keys from navigating UIToolkit elements
            root.RegisterCallback<NavigationMoveEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
            // Remove all elements from Tab cycling order
            root.Query<VisualElement>().ForEach(el => el.tabIndex = -1);

            m_panel = root.Q<VisualElement>("dashboard-panel");
            m_panel.style.display = DisplayStyle.None;

            root.Q<Button>("toggle-button").clicked += TogglePanel;
            root.Q<Button>("reset-button").clicked += OnReset;

            BindLocomotion(root);
            BindJumpConfig(root);
            BindJumpTiming(root);
            BindBalancer(root);
        }

        private void TogglePanel()
        {
            var isVisible = m_panel.style.display == DisplayStyle.Flex;
            m_panel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnReset()
        {
            ApplySnapshot(m_snapshot);
            foreach (var refresh in m_refreshActions)
                refresh.Invoke();
        }

        private Snapshot CaptureSnapshot() => new Snapshot
        {
            maxSpeed = m_data.maxSpeed,
            GroundAcceleration = m_data.GroundAcceleration,
            AirAcceleration = m_data.AirAcceleration,
            maxAccelerationForce = m_data.maxAccelerationForce,
            forceScale = m_data.forceScale,
            gravityScaleDrop = m_data.gravityScaleDrop,
            JumpHeight = m_data.JumpHeight,
            TimeTillJumpApex = m_data.TimeTillJumpApex,
            JumpHeightCompensationFactor = m_data.JumpHeightCompensationFactor,
            MaxVerticalVelocity = m_data.MaxVerticalVelocity,
            GravityOnReleaseMultiplier = m_data.GravityOnReleaseMultiplier,
            MaxFallSpeed = m_data.MaxFallSpeed,
            NumberOfJumpsAllowed = m_data.NumberOfJumpsAllowed,
            TimeForUpwardsCancel = m_data.TimeForUpwardsCancel,
            ApexThreshold = m_data.ApexThreshold,
            ApexHangTime = m_data.ApexHangTime,
            JumpBufferTime = m_data.JumpBufferTime,
            JumpCoyoteTime = m_data.JumpCoyoteTime,
            rideHeight = m_data.rideHeight,
            springStrength = m_data.springStrength,
            springDamper = m_data.springDamper,
            uprightStrength = m_data.uprightStrength,
            uprightDamper = m_data.uprightDamper,
        };

        private void ApplySnapshot(Snapshot s)
        {
            m_data.maxSpeed = s.maxSpeed;
            m_data.GroundAcceleration = s.GroundAcceleration;
            m_data.AirAcceleration = s.AirAcceleration;
            m_data.maxAccelerationForce = s.maxAccelerationForce;
            m_data.forceScale = s.forceScale;
            m_data.gravityScaleDrop = s.gravityScaleDrop;
            m_data.JumpHeight = s.JumpHeight;
            m_data.TimeTillJumpApex = s.TimeTillJumpApex;
            m_data.JumpHeightCompensationFactor = s.JumpHeightCompensationFactor;
            m_data.MaxVerticalVelocity = s.MaxVerticalVelocity;
            m_data.GravityOnReleaseMultiplier = s.GravityOnReleaseMultiplier;
            m_data.MaxFallSpeed = s.MaxFallSpeed;
            m_data.NumberOfJumpsAllowed = s.NumberOfJumpsAllowed;
            m_data.TimeForUpwardsCancel = s.TimeForUpwardsCancel;
            m_data.ApexThreshold = s.ApexThreshold;
            m_data.ApexHangTime = s.ApexHangTime;
            m_data.JumpBufferTime = s.JumpBufferTime;
            m_data.JumpCoyoteTime = s.JumpCoyoteTime;
            m_data.rideHeight = s.rideHeight;
            m_data.springStrength = s.springStrength;
            m_data.springDamper = s.springDamper;
            m_data.uprightStrength = s.uprightStrength;
            m_data.uprightDamper = s.uprightDamper;
            m_data.RecalculateValues();
            Save();
        }

        private void BindLocomotion(VisualElement root)
        {
            BindFloat(root, "field-maxSpeed",
                () => m_data.maxSpeed, v => m_data.maxSpeed = v);
            BindFloat(root, "field-groundAcceleration",
                () => m_data.GroundAcceleration, v => m_data.GroundAcceleration = v);
            BindFloat(root, "field-airAcceleration",
                () => m_data.AirAcceleration, v => m_data.AirAcceleration = v);
            BindFloat(root, "field-maxAccelerationForce",
                () => m_data.maxAccelerationForce, v => m_data.maxAccelerationForce = v);
            BindVec3(root, "field-forceScaleX", "field-forceScaleY", "field-forceScaleZ",
                () => m_data.forceScale, v => m_data.forceScale = v);
            BindFloat(root, "field-gravityScaleDrop",
                () => m_data.gravityScaleDrop, v => m_data.gravityScaleDrop = v);
        }

        private void BindJumpConfig(VisualElement root)
        {
            BindFloat(root, "field-jumpHeight",
                () => m_data.JumpHeight, v => m_data.JumpHeight = v, recalculate: true);
            BindFloat(root, "field-timeTillJumpApex",
                () => m_data.TimeTillJumpApex, v => m_data.TimeTillJumpApex = v, recalculate: true);
            BindFloat(root, "field-jumpHeightCompFactor",
                () => m_data.JumpHeightCompensationFactor, v => m_data.JumpHeightCompensationFactor = v, recalculate: true);
            BindFloat(root, "field-maxVerticalVelocity",
                () => m_data.MaxVerticalVelocity, v => m_data.MaxVerticalVelocity = v);
            BindFloat(root, "field-gravityOnReleaseMultiplier",
                () => m_data.GravityOnReleaseMultiplier, v => m_data.GravityOnReleaseMultiplier = v);
            BindFloat(root, "field-maxFallSpeed",
                () => m_data.MaxFallSpeed, v => m_data.MaxFallSpeed = v);
            BindInt(root, "field-numberOfJumpsAllowed",
                () => m_data.NumberOfJumpsAllowed, v => m_data.NumberOfJumpsAllowed = v);
        }

        private void BindJumpTiming(VisualElement root)
        {
            BindFloat(root, "field-timeForUpwardsCancel",
                () => m_data.TimeForUpwardsCancel, v => m_data.TimeForUpwardsCancel = v);
            BindFloat(root, "field-apexThreshold",
                () => m_data.ApexThreshold, v => m_data.ApexThreshold = v);
            BindFloat(root, "field-apexHangTime",
                () => m_data.ApexHangTime, v => m_data.ApexHangTime = v);
            BindFloat(root, "field-jumpBufferTime",
                () => m_data.JumpBufferTime, v => m_data.JumpBufferTime = v);
            BindFloat(root, "field-jumpCoyoteTime",
                () => m_data.JumpCoyoteTime, v => m_data.JumpCoyoteTime = v);
        }

        private void BindBalancer(VisualElement root)
        {
            BindFloat(root, "field-rideHeight",
                () => m_data.rideHeight, v => m_data.rideHeight = v);
            BindFloat(root, "field-springStrength",
                () => m_data.springStrength, v => m_data.springStrength = v);
            BindFloat(root, "field-springDamper",
                () => m_data.springDamper, v => m_data.springDamper = v);
            BindFloat(root, "field-uprightStrength",
                () => m_data.uprightStrength, v => m_data.uprightStrength = v);
            BindFloat(root, "field-uprightDamper",
                () => m_data.uprightDamper, v => m_data.uprightDamper = v);
        }

        private void BindFloat(VisualElement root, string name,
            System.Func<float> get, System.Action<float> set, bool recalculate = false)
        {
            var field = root.Q<FloatField>(name);
            if (field == null) return;
            field.SetValueWithoutNotify(get());
            field.RegisterCallback<KeyDownEvent>(FilterFloat, TrickleDown.TrickleDown);
            field.RegisterValueChangedCallback(evt =>
            {
                set(evt.newValue);
                if (recalculate) m_data.RecalculateValues();
                Save();
            });
            m_refreshActions.Add(() => field.SetValueWithoutNotify(get()));
        }

        private void BindInt(VisualElement root, string name,
            System.Func<int> get, System.Action<int> set)
        {
            var field = root.Q<IntegerField>(name);
            if (field == null) return;
            field.SetValueWithoutNotify(get());
            field.RegisterCallback<KeyDownEvent>(FilterInt, TrickleDown.TrickleDown);
            field.RegisterValueChangedCallback(evt =>
            {
                set(evt.newValue);
                Save();
            });
            m_refreshActions.Add(() => field.SetValueWithoutNotify(get()));
        }

        private void BindVec3(VisualElement root,
            string nameX, string nameY, string nameZ,
            System.Func<Vector3> get, System.Action<Vector3> set)
        {
            var fx = root.Q<FloatField>(nameX);
            var fy = root.Q<FloatField>(nameY);
            var fz = root.Q<FloatField>(nameZ);
            if (fx == null || fy == null || fz == null) return;

            var v = get();
            fx.SetValueWithoutNotify(v.x);
            fy.SetValueWithoutNotify(v.y);
            fz.SetValueWithoutNotify(v.z);

            foreach (var f in new[] { fx, fy, fz })
                f.RegisterCallback<KeyDownEvent>(FilterFloat, TrickleDown.TrickleDown);

            fx.RegisterValueChangedCallback(evt => { var c = get(); set(new Vector3(evt.newValue, c.y, c.z)); Save(); });
            fy.RegisterValueChangedCallback(evt => { var c = get(); set(new Vector3(c.x, evt.newValue, c.z)); Save(); });
            fz.RegisterValueChangedCallback(evt => { var c = get(); set(new Vector3(c.x, c.y, evt.newValue)); Save(); });

            m_refreshActions.Add(() => { var r = get(); fx.SetValueWithoutNotify(r.x); fy.SetValueWithoutNotify(r.y); fz.SetValueWithoutNotify(r.z); });
        }

        private static void FilterFloat(KeyDownEvent evt)
        {
            var c = evt.character;
            if (c == '\0' || char.IsControl(c)) return;     // Special/function keys — allow
            if (evt.ctrlKey || evt.commandKey) return;       // Ctrl+A/C/V/X — allow
            if (char.IsDigit(c) || c == '-' || c == '.') return;
            evt.PreventDefault();
        }

        private static void FilterInt(KeyDownEvent evt)
        {
            var c = evt.character;
            if (c == '\0' || char.IsControl(c)) return;
            if (evt.ctrlKey || evt.commandKey) return;
            if (char.IsDigit(c) || c == '-') return;
            evt.PreventDefault();
        }

        private void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_data);
            AssetDatabase.SaveAssetIfDirty(m_data);
#endif
        }
#else
        private void Start()
        {
            if (TryGetComponent<UIDocument>(out var doc))
                doc.enabled = false;
        }
#endif
    }
}
