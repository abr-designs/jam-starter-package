using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Utilities.Physics;

namespace Tests.Utilities.Physics
{
    public class CollisionChecksTests
    {
        [Test]
        [TestCase(-1,-1,1,1,1,1,1,1, ExpectedResult = false)]
        [TestCase(-1,-1,2,2,1,1,2,2, ExpectedResult = true)]
        [TestCase(0,0,1,1,0,0,2,2, ExpectedResult = true)]
        public bool Rect2Rect(float r1x, float r1y, float r1w, float r1h, float r2x, float r2y, float r2w, float r2h)
        {
            return CollisionChecks.Rect2Rect(r1x, r1y, r1w, r1h, r2x, r2y, r2w, r2h);
        }

        [Test]
        [TestCase(-1,0, 1,0, 0, 1, 0, -1, ExpectedResult = true)]
        [TestCase(-1,0, 1,0, 1, 1, -1, -1, ExpectedResult = true)]
        [TestCase(-1,0, 1,0, -1,1, 1,1, ExpectedResult = false)]
        // LINE/LINE
        public bool Line2Line(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            return CollisionChecks.Line2Line(x1, y1, x2, y2, x3, y3, x4, y4);
        }
        
        [Test]
        [TestCase(0,0,0,0,0.5f, ExpectedResult = true)]
        [TestCase(0,0,0,0,0, ExpectedResult = true)]
        [TestCase(0,0,2,0,0.5f, ExpectedResult = false)]
        [TestCase(0.5f,0,0,0,0.5f, ExpectedResult = true)]
        [TestCase(1,0,0.5f,0,0.5f, ExpectedResult = true)]
        [TestCase(2,0,0,0,0.5f, ExpectedResult = false)]
        public bool Point2Circle(float px, float py, float cx, float cy, float r)
        {
            return CollisionChecks.Point2Circle(px, py, cx, cy, r);
        }
        
        [Test]
        [TestCase(0,0,0.5f,0,0,0.5f, ExpectedResult = true)]
        [TestCase(1,0,0.5f,-1,0,0.5f, ExpectedResult = false)]
        [TestCase(0,0,1,1,0,0.5f, ExpectedResult = true)]
        public bool CircleToCircle(float c1x, float c1y, float c1r, float c2x, float c2y, float c2r)
        {
            return CollisionChecks.CircleToCircle(c1x, c1y, c1r, c2x, c2y, c2r);
        }

        [Test]
        [TestCase(-1,0,1,0,0,0,0.5f, ExpectedResult = true)]
        [TestCase(-1,0,1,0,0,2,0.5f, ExpectedResult = false)]
        [TestCase(-1,0,-1,1,0,0,0.5f, ExpectedResult = false)]
        public bool Line2Circle(float x1, float y1, float x2, float y2, float cx, float cy, float r)
        {
            return CollisionChecks.Line2Circle(x1,y1, x2, y2, cx, cy, r, out _, out _);
        }
        
        [Test]
        [TestCase(-1,0,1,0,0,0,ExpectedResult = true)]
        [TestCase(-1,-1,-1,1,0,0,ExpectedResult = false)]
        public bool Line2Point(float x1, float y1, float x2, float y2, float px, float py)
        {
            return CollisionChecks.Line2Point(x1, y1, x2, y2, px, py);
        }
        
        [Test]
        [TestCase(-1,0, 1, 0, 0,0,1,1, ExpectedResult = true)]
        [TestCase(-1,2, 1, 2, 0,0,1,1, ExpectedResult = false)]
        [TestCase(-1,-1, 1, 1, 0,0,1,1, ExpectedResult = true)]
        [TestCase(-1,0, 0, 0, 0,0,1,1, ExpectedResult = true)]
        public bool Line2Rect(float x1, float y1, float x2, float y2, float rx, float ry, float rw, float rh) 
        {
            return CollisionChecks.Line2Rect(x1, y1, x2, y2, rx, ry, rw, rh);
        }
        [Test]
        [TestCase(-2,0,1,0,0,1,1, ExpectedResult = false)]
        [TestCase(-1,0,1,0,0,1,1, ExpectedResult = true)]
        [TestCase(0,0,0.5f,0,0,1,1, ExpectedResult = true)]
        [TestCase(-2,0,1,0,0,1,10, ExpectedResult = false)]
        [TestCase(0,2,0.5f,0,0,10,1, ExpectedResult = false)]
        public bool Circle2Rect(float cx, float cy, float radius, float rx, float ry, float rw, float rh)
        {
            return CollisionChecks.Circle2Rect(cx, cy, radius, rx, ry, rw, rh);
        }
        
        // POLYGON/POINT
        public void Poly2Point(Vector3[] vertices, float px, float py)
        {
            //TODO
        }

        // POLYGON/RECTANGLE
        public void Poly2Rect(Vector3[] vertices, float rx, float ry, float rw, float rh)
        {
            //TODO
        }

        //============================================================================================================//


        //Based on: https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/closest_point_on_line.html
        [Test]
        [TestCaseSource(nameof(ClosestPointOnLineTestCaseData))]
        public Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            return CollisionChecks.ClosestPointOnLine(lineStart, lineEnd, point);
        }

        public static IEnumerable<TestCaseData> ClosestPointOnLineTestCaseData()
        {
            yield return new TestCaseData(Vector2.left, Vector2.right, Vector2.up)
            {
                ExpectedResult = Vector2.zero
            };
            yield return new TestCaseData(Vector2.left, Vector2.right, new Vector2(-1, 1))
            {
                ExpectedResult = Vector2.left
            };
            yield return new TestCaseData(Vector2.left, new Vector2(-1, -1), new Vector2(1, 0))
            {
                ExpectedResult = Vector2.left
            };
        }
    }
}