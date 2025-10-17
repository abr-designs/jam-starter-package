using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Utilities;

namespace Tests.Utilities.Extensions
{
    public class RayCastHitExtensionTests
    {
        private const int CUBE_COUNT = 10;
        private const float CUBE_SPACING = 1.2f;
        
        private Transform[] cubes;
        private Collider[] cubeColliders;
        private Rigidbody[] cubeRigidbodies;

        private RaycastHit[] m_raycastHits;
        
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetup()
        {
            m_raycastHits = new RaycastHit[CUBE_COUNT];
            cubes = new Transform[CUBE_COUNT];
            cubeColliders = new Collider[CUBE_COUNT];
            cubeRigidbodies = new Rigidbody[CUBE_COUNT];
            
            for (int i = 0; i < CUBE_COUNT; i++)
            {
                var temp = ObjectFactory.CreatePrimitive(PrimitiveType.Cube);

                temp.name = $"{nameof(RayCastHitExtensionTests)}_{nameof(PrimitiveType.Cube)}_[{i}]";
                var collider = temp.GetComponent<BoxCollider>();
                
                var rb = temp.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                
                temp.transform.position = Vector3.forward * (i + 1) * CUBE_SPACING;

                cubes[i] = temp.transform;
                cubeColliders[i] = collider;
                cubeRigidbodies[i] = rb;
            }
            
            yield return null;
        }

        [TestCaseSource(nameof(NearestTestCases))]
        public void GetNearestHitTest(Vector3 position, Vector3 direction, int expectedColliderIndex)
        {
            var raycastAll = Physics.RaycastAll(position, direction);
            var count = raycastAll.Length;
            
            var furthestHit = raycastAll.GetNearestHit(count);
            
            if (expectedColliderIndex < 0)
            {
                Assert.True(furthestHit.Equals(default(RaycastHit)));
                return;
            }
            
            Assert.AreEqual(cubeColliders[expectedColliderIndex], furthestHit.collider);
        }
        
        [TestCaseSource(nameof(FurthestTestCases))]
        public void GetFurthestHitTest(Vector3 position, Vector3 direction, int expectedColliderIndex)
        {
            var raycastAll = Physics.RaycastAll(position, direction);
            var count = raycastAll.Length;
            
            var furthestHit = raycastAll.GetFurthestHit(count);
            
            if (expectedColliderIndex < 0)
            {
                Assert.True(furthestHit.Equals(default(RaycastHit)));
                return;
            }
            
            Assert.AreEqual(cubeColliders[expectedColliderIndex], furthestHit.collider);
        }
        
        [TestCaseSource(nameof(NearestTestCases))]
        public void GetNearestHitNonAllocTest(Vector3 position, Vector3 direction, int expectedColliderIndex)
        {
            var count = Physics.RaycastNonAlloc(position, direction, m_raycastHits);
            var furthestHit = m_raycastHits.GetNearestHit(count);
            
            if (expectedColliderIndex < 0)
            {
                Assert.True(furthestHit.Equals(default(RaycastHit)));
                return;
            }
            
            Assert.AreEqual(cubeColliders[expectedColliderIndex], furthestHit.collider);
        }
        
        [TestCaseSource(nameof(FurthestTestCases))]
        public void GetFurthestHitNonAllocTest(Vector3 position, Vector3 direction, int expectedColliderIndex)
        {
            var count = Physics.RaycastNonAlloc(position, direction, m_raycastHits);
            var furthestHit = m_raycastHits.GetFurthestHit(count);

            if (expectedColliderIndex < 0)
            {
                Assert.True(furthestHit.Equals(default(RaycastHit)));
                return;
            }
            
            Assert.AreEqual(cubeColliders[expectedColliderIndex], furthestHit.collider);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            for (int i = CUBE_COUNT - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(cubes[i]);
            }

            cubes = null;
            cubeColliders = null;
            cubeRigidbodies = null;
        }
        
        public static IEnumerable<TestCaseData> NearestTestCases()
        {
            yield return new TestCaseData(Vector3.zero, Vector3.forward, 0)
                .SetName("Cast forward from origin, expect first");
            yield return new TestCaseData(Vector3.forward * (CUBE_COUNT + 1) * CUBE_SPACING, Vector3.back, CUBE_COUNT - 1)
                .SetName("Cast back from end, expect last");
            yield return new TestCaseData(Vector3.zero, Vector3.back, -1)
                .SetName("Cast back from origin, expect null");
        }
        public static IEnumerable<TestCaseData> FurthestTestCases()
        {
            yield return new TestCaseData(Vector3.zero, Vector3.forward, CUBE_COUNT - 1)
                .SetName("Cast forward from origin, expect last");
            yield return new TestCaseData(Vector3.forward * (CUBE_COUNT + 1) * CUBE_SPACING, Vector3.back, 0)
                .SetName("Cast back from end, expect first");
            yield return new TestCaseData(Vector3.zero, Vector3.back, -1)
                .SetName("Cast back from origin, expect null");
        }
    }
}