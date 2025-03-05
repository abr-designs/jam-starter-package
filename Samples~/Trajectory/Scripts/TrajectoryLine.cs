using System;
using NaughtyAttributes;
using UnityEngine;
using Utilities.Physics;

namespace Trajectory
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryLine : MonoBehaviour
    {

        [ReadOnly]
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
        private Vector3[] _linePoints = new Vector3[20];


        //Unity Functions
        //============================================================================================================//

        private void Update()
        {
            CacheValues();

            if (_needsUpdate)
            {
                RecalculateLinePoints();
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

            // Resize points array if resolution was changed
            if(LineResolution != _cachedLineResolution || _linePoints.Length == 0) {
                _linePoints = new Vector3[20 * LineResolution];
            }

            _cachedLaunchVelocity = LaunchVelocity;
            _cachedLineResolution = LineResolution;
            _cachedLinePreviewTime = LinePreviewTime;
        }

        private void RecalculateLinePoints()
        {
            if (_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();

            ProjectileMath.ProjectileArcPointsNonAlloc3D(LaunchVelocity, Physics.gravity, LinePreviewTime, ref _linePoints);

            _lineRenderer.positionCount = _linePoints.Length;
            _lineRenderer.SetPositions(_linePoints);
        }

        //Editor Functions
        //============================================================================================================//
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Update the line points when values are changed in the editor
            RecalculateLinePoints();
        }
#endif

    }

}
