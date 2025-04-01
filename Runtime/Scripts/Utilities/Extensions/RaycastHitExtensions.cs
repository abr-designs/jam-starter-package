using UnityEngine;

namespace Utilities
{
    public static class RaycastHitExtensions
    {
        public static RaycastHit GetNearestHit(this RaycastHit[] hits, int hitCount)
        {
            if (hitCount == 0 || hits == null || hits.Length == 0)
                return default;

            if (hitCount == 1)
                return hits[0];

            var outIndex = -1;
            var shortestDistance = float.MaxValue;
            for (var i = 0; i < hitCount; i++)
            {
                if(hits[i].distance >= shortestDistance)
                    continue;

                shortestDistance = hits[i].distance;
                outIndex = i;
            }

            return outIndex < 0 ? default : hits[outIndex];
        }
        public static RaycastHit GetFurthestHit(this RaycastHit[] hits, int hitCount)
        {
            if (hitCount == 0 || hits == null || hits.Length == 0)
                return default;

            if (hitCount == 1)
                return hits[0];

            var outIndex = -1;
            var largestDistance = float.MinValue;
            for (var i = 0; i < hits.Length; i++)
            {
                if(hits[i].distance <= largestDistance)
                    continue;

                largestDistance = hits[i].distance;
                outIndex = i;
            }

            return outIndex < 0 ? default : hits[outIndex];
        }
    }
}