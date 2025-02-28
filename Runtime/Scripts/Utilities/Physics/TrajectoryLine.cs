using System;
using UnityEngine;

namespace Utilities.Physics
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryLine : MonoBehaviour
    {

        public Vector2 LaunchVelocity;
        private Vector2 _cachedLaunchVelocity;

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


        void Start()
        {
        }

        void Update()
        {
            CacheValues();

            if (_needsUpdate)
            {
                RecalcLinePoints();
            }
        }


        private void OnValidate()
        {
            RecalcLinePoints();
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

            float launchSpeed = LaunchVelocity.magnitude;

            Vector2[] points;
            points = ProjectileMath.ProjectileArcPoints(LaunchVelocity, Physics2D.gravity.y, LinePreviewTime, 20 * LineResolution);

            Vector3[] pts = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                pts[i] = new Vector3(points[i].x, points[i].y, 0);
            }
            _lineRenderer.positionCount = pts.Length;
            _lineRenderer.SetPositions(pts);

        }


    }
}
