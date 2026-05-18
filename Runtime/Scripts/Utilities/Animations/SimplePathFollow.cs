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
        private bool faceDirection;

        [SerializeField, Range(0f, 1f)]
        private float startingPosition;

        [SerializeField]
        private Transform targetMoveTransform;

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
            m_pingPongForward = speed > 0f;
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

            targetMoveTransform.position = Evaluate(m_distanceTravelled / m_totalLength, out var tangent);

            if (faceDirection)
                targetMoveTransform.forward = speed < 0f ? -tangent : tangent;
        }

        #endregion // Unity Functions

        //================================================================================================================//
    }
}
