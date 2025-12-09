using UnityEngine;

namespace Samples.Geodesics.Sphere
{
    public static class Input
    {
        public static void AxisInput(KeyCode positive, KeyCode negative, ref float value)
        {
            if (UnityEngine.Input.GetKey(positive) || UnityEngine.Input.GetKey(negative))
            {
                if(UnityEngine.Input.GetKeyDown(positive))
                    value = 1f;
                if (UnityEngine.Input.GetKeyDown(negative))
                    value = -1f;
            }
            else
            {
                value = 0f;
            }
        }
    }
}