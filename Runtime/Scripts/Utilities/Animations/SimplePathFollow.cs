using UnityEngine;

namespace Utilities.Animations
{
    public class SimplePathFollow : SimplePath
    {
        //Fields
        //================================================================================================================//

        #region Fields

        [SerializeField]
        //This could be in reverse, so no Min() required
        private float speed;

        [SerializeField]
        //This could be in reverse, so no Min() required
        protected bool faceDirection;

        [SerializeField, Min(0f)]
        protected float rotationSpeed; // [Claude 2026-05-18]

        [SerializeField, Range(0f, 1f)]
        private float startingPosition;

        [SerializeField]
        protected Transform targetMoveTransform;

        private float m_distanceTravelled;
        private bool m_pingPongForward = true;

        #endregion // Fields

        //Unity Functions
        //================================================================================================================//

        #region Unity Functions

        protected override void Start()
        {
            base.Start();

            m_distanceTravelled = startingPosition * m_totalLength;
            m_pingPongForward = speed >= 0f;
        }

        private void Update()
        {
            if (speed == 0f)
                return;
            if (targetMoveTransform == null)
                return;
            if (m_totalLength <= 0f)
                return;

            var delta = speed * Time.deltaTime;

            if (looping)
            {
                m_distanceTravelled = (m_distanceTravelled + delta) % m_totalLength;

                if (m_distanceTravelled < 0f)
                    m_distanceTravelled += m_totalLength;
            }
            else
            {
                m_distanceTravelled += m_pingPongForward ? delta : -delta;

                if (m_distanceTravelled >= m_totalLength)
                {
                    m_distanceTravelled = m_totalLength;
                    m_pingPongForward = !m_pingPongForward;
                }
                else if (m_distanceTravelled <= 0f)
                {
                    m_distanceTravelled = 0f;
                    m_pingPongForward = !m_pingPongForward;
                }
            }

            var position = Evaluate(m_distanceTravelled / m_totalLength, out var tangent);
            ApplyPathTransform(position, speed < 0f ? -tangent : tangent);
        }

        #endregion // Unity Functions

        //Protected Methods
        //================================================================================================================//

        #region Protected Methods

        /// <summary>
        /// Applies <paramref name="position"/> and optionally rotates <paramref name="targetMoveTransform"/> toward <paramref name="tangent"/>.
        /// Override to customize how the follower responds to path evaluation results.
        /// </summary>
        /// <remarks>Created by Claude (claude-sonnet-4-6) — 2026-05-18</remarks>
        protected virtual void ApplyPathTransform(Vector3 position, Vector3 tangent)
        {
            targetMoveTransform.position = position;

            if (!faceDirection || tangent == Vector3.zero)
                return;

            var targetRotation = Quaternion.LookRotation(tangent);

            targetMoveTransform.rotation = rotationSpeed <= 0f
                ? targetRotation
                : Quaternion.RotateTowards(targetMoveTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        #endregion // Protected Methods

        //================================================================================================================//
    }
}
