using System;
using UnityEngine;
using Utilities;
using Utilities.Physics;

namespace Trajectory
{
    [RequireComponent(typeof(TrajectoryLine))]
    [RequireComponent(typeof(MouseCaster))]
    public class MouseTrajectory : MonoBehaviour
    {
        private TrajectoryLine _trajectoryLine;
        private MouseCaster _mouseCaster;

        [SerializeField]
        private GameObject _reticle;

        public float LaunchSpeed = 5.0f;

        //Unity Functions
        //============================================================================================================//

        void Update()
        {
            if (_trajectoryLine == null)
                _trajectoryLine = GetComponent<TrajectoryLine>();
            if (_mouseCaster == null)
                _mouseCaster = GetComponent<MouseCaster>();

            UpdateTrajectory();

        }


        //MouseTrajectory Functions
        //============================================================================================================//

        private void UpdateTrajectory()
        {
            if (_mouseCaster.HitObject == null)
            {
                _trajectoryLine.gameObject.SetActive(true);
                return;
            }

            _trajectoryLine.gameObject.SetActive(true);

            Vector3 target = _mouseCaster.HitPoint;
            Vector3 dir = target - transform.position;
            Vector3 groundVector = Vector3.ProjectOnPlane(dir, Vector3.up);

            float range = groundVector.magnitude;
            float yOffset = dir.y;

            float angle1, angle2;
            float angle = Mathf.PI / 4; // 45 degrees will be the default angle
            if (ProjectileMath.LaunchAngle(LaunchSpeed, range, yOffset, Mathf.Abs(Physics2D.gravity.y), out angle1, out angle2))
            {
                // Pick smaller angle
                angle = Mathf.Min(angle1, angle2);
            }
            Vector3 launchVelocity = (groundVector.normalized * (Mathf.Cos(angle) * LaunchSpeed)) + (Vector3.up * (Mathf.Sin(angle) * LaunchSpeed));
            _trajectoryLine.LaunchVelocity = launchVelocity;

            if (_reticle != null)
            {
                _reticle.transform.position = _mouseCaster.HitPoint;
            }

        }

    }
}
