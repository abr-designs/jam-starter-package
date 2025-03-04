using System;
using UnityEngine;
using Utilities.Physics;

namespace Trajectory
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryLine : MonoBehaviour
    {

        public Vector3 LaunchVelocity;
        private Vector3 _cachedLaunchVelocity;

        [Range(1, 5)]
        public int LineResolution = 1;
        private int _cachedLineResolution;

        [Min(0f)]
        public float LinePreviewTime = 5f;
        private float _cachedLinePreviewTime;


        private bool _needsUpdate = true;

        private LineRenderer _lineRenderer;


        //Unity Functions
        //============================================================================================================//

        private void Update()
        {
            CacheValues();

            if (_needsUpdate)
            {
                RecalcLinePoints();
            }
        }

        //TrajectoryLine Functions
        //============================================================================================================//

        // Compare past values to new values and see if this frame needs an update
        private void CacheValues()
        {
            _needsUpdate = (
                LaunchVelocity != _cachedLaunchVelocity
                || LineResolution != _cachedLineResolution
                || LinePreviewTime != _cachedLinePreviewTime
            );

            _cachedLaunchVelocity = LaunchVelocity;
            _cachedLineResolution = LineResolution;
            _cachedLinePreviewTime = LinePreviewTime;
        }

        private void RecalcLinePoints()
        {
            if (_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();

            Vector3[] points;
            points = ProjectileMath.ProjectileArcPoints3D(LaunchVelocity, Physics2D.gravity, LinePreviewTime, 20 * LineResolution);

            _lineRenderer.positionCount = points.Length;
            _lineRenderer.SetPositions(points);

        }

        //Editor Functions
        //============================================================================================================//
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Update the line points when values are changed in the editor
            RecalcLinePoints();
        }
#endif

    }

}
