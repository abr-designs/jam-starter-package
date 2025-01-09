using System;
using UnityEngine;

namespace Utilities
{
    public static class JMath
    {
        //FIXME Convert this to use the string builder so that it will reduce operational overhead
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) 
                throw new ArgumentException("insert value between 1 and 3999");
            
            if (number < 1) return string.Empty;            
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900); 
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);            
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            
            throw new ArgumentException("something bad happened");
        }
        
        /// <summary>
        /// Returns point on circle edge based on the degrees. 0deg is the top/North part of the circle
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2 GetAsPointOnCircle(float degrees, float radius = 1f)
        {
            if(radius == 0)
                return Vector2.zero;
            
            var radians = degrees * Mathf.Deg2Rad;

            var x = Mathf.Cos(radians);
            var y = Mathf.Sin(radians);
            
            return new Vector2(x, y) * radius; //Vector2 is fine, if you're in 2D
        }

        /// <summary>
        /// Force Vector2 into a non-negative state
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector2 Abs(this Vector2 vector2)
        {
            return new Vector2
            {
                x = Mathf.Abs(vector2.x),
                y = Mathf.Abs(vector2.y)
            };
        }
        
        /// <summary>
        /// Force Vector3 into a non-negative state
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector3 Abs(this Vector3 vector3)
        {
            return new Vector3
            {
                x = Mathf.Abs(vector3.x),
                y = Mathf.Abs(vector3.y),
                z = Mathf.Abs(vector3.z),
            };
        }
        
        /// <summary>
        /// Prevents an angle (deg) from moving beyond 360, wrapping around to 0
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float ClampAngle(float degrees)
        {
            degrees %= 360;

            if (degrees < 0)
            {
                return 360 + degrees;
            }

            return degrees;
        }
    }
}