using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utilities.Debugging;

namespace Utilities.Physics
{
    public static class CollisionChecks
    {
                //============================================================================================================//
        public static bool Rect2Rect(float r1x, float r1y, float r1w, float r1h, float r2x, float r2y, float r2w, float r2h) 
        {
            // are the sides of one rectangle touching the other?
            if (r1x + r1w >= r2x &&    // r1 right edge past r2 left
                r1x <= r2x + r2w &&    // r1 left edge past r2 right
                r1y + r1h >= r2y &&    // r1 top edge past r2 bottom
                r1y <= r2y + r2h) {    // r1 bottom edge past r2 top
                return true;
            }
            return false;
        }

        public static bool Line2Line(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
        {
            return Line2Line(
                line1Start.x, line1Start.y, line1End.x, line1End.y, 
                line2Start.x, line2Start.y, line2End.x, line2End.y);
        }
        // LINE/LINE
        public static bool Line2Line(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) 
        {

            // calculate the distance to intersection point
            float uA = ((x4-x3)*(y1-y3) - (y4-y3)*(x1-x3)) / ((y4-y3)*(x2-x1) - (x4-x3)*(y2-y1));
            float uB = ((x2-x1)*(y1-y3) - (y2-y1)*(x1-x3)) / ((y4-y3)*(x2-x1) - (x4-x3)*(y2-y1));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1) {

                // optionally, draw a circle where the lines meet
                //float intersectionX = x1 + (uA * (x2-x1));
                //float intersectionY = y1 + (uA * (y2-y1));

                return true;
            }
            return false;
        }
        
        // POINT/CIRCLE
        //based on: https://www.jeffreythompson.org/collision-detection/point-circle.php
        public static bool Point2Circle(float px, float py, float cx, float cy, float r)
        {
            var rSqr = r * r;
            // get distance between the point and circle's center
            // using the Pythagorean Theorem
            float distX = px - cx;
            float distY = py - cy;
            float distanceSqr = (distX*distX) + (distY*distY);

            // if the distance is less than the circle's
            // radius the point is inside!
            return distanceSqr <= rSqr;
        }

        public static bool Point2Circle(in Vector2 point, in Vector2 circlePosition, float r)
        {
            return Point2Circle(point.x, point.y, circlePosition.x, circlePosition.y, r);
        }

        public static bool Line2Circle(Vector2 lineStart, Vector2 lineEnd, Vector2 circlePos, float r, out Vector2 closest)
        {
            var temp = Line2Circle(lineStart.x, lineStart.y, 
                lineEnd.x, lineEnd.y, 
                circlePos.x, circlePos.y, 
                r,
                out var x, out var y);

            closest = new Vector2(x, y);
            
            Debug.DrawLine(lineStart, lineEnd, Color.green);
            Debug.DrawLine(circlePos, closest, Color.red);
            
            if(temp)
                Draw.Circle(circlePos, Color.green, r * 0.5f, 24);

            
            return temp;
        }

        //Based on: https://www.jeffreythompson.org/collision-detection/line-circle.php
        public static bool Line2Circle(float x1, float y1, float x2, float y2, float cx, float cy, float r, out float closestX, out float closestY)
        {
            closestX = 0f;
            closestY = 0f;

            // get length of the line
            var len = Distance(x1,y1, x2,y2);

            // get dot product of the line and circle
            var dot = (((cx - x1) * (x2 - x1)) + ((cy - y1) * (y2 - y1))) / (float)Math.Pow(len, 2);

            // find the closest point on the line
            closestX = x1 + (dot * (x2 - x1));
            closestY = y1 + (dot * (y2 - y1));

            // is this point actually on the line segment?
            // if so keep going, but if not, return false
            bool onSegment = Line2Point(x1, y1, x2, y2, closestX, closestY);
            if (!onSegment) 
                return false;

            var distance = Distance(cx, cy, closestX, closestY);

            return distance <= r;
        }
        
        public static bool Line2Point(float x1, float y1, float x2, float y2, float px, float py) {

            // get distance from the point to the two ends of the line
            float d1 = Distance(px,py, x1,y1);
            float d2 = Distance(px,py, x2,y2);

            // get the length of the line
            float lineLen = Distance(x1,y1, x2,y2);

            // since floats are so minutely accurate, add
            // a little buffer zone that will give collision
            float buffer = 0.1f;    // higher # = less accurate

            // if the two distances are equal to the line's
            // length, the point is on the line!
            // note we use the buffer here to give a range,
            // rather than one #
            return d1 + d2 >= lineLen - buffer && d1 + d2 <= lineLen + buffer;
        }

        //FIXME I really want to make this not use Sqrt()!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Distance(float x1, float y1, float x2, float y2)
        {
            var distX = x1 - x2;
            var distY = y1 - y2;
            return (float)Math.Sqrt((distX * distX) + (distY * distY));
        }

        //============================================================================================================//


        //Based on: https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/closest_point_on_line.html
        public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            var line = lineEnd - lineStart;
            var t = Vector2.Dot(point - lineStart, line) / Vector2.Dot(line, line);

            t = Math.Clamp(t, 0f, 1f);

            return lineStart + (t * line);
        }
    }
}