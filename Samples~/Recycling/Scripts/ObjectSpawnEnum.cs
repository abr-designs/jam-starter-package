using System.Collections.Generic;
using UnityEngine;
using Utilities.Recycling;

namespace Samples.Recycling
{
    public class ObjectSpawnEnum : MonoBehaviour
    {
        private enum TEST
        {
            NONE,
            OBJECT_1,
            OBJECT_2,
            OBJECT_3,
            OBJECT_4,
        }
        
        [SerializeField]
        private GameObject prefab;

        [SerializeField, Min(0f)]
        private float spawnTime;
        private float _spawnTimer;

        [SerializeField]
        private float killYPos = -10;

        private List<GameObject> _objects1;
        private List<GameObject> _objects2;
    
        // Start is called before the first frame update
        private void Start()
        {
            _objects1 = new List<GameObject>();
            _objects2 = new List<GameObject>();
        }

        // Update is called once per frame
        private void Update()
        {
            CheckObjectPositions();
            
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer < spawnTime)
                return;

            _spawnTimer = 0f;

            if (Recycler.TryGrabEnum(TEST.OBJECT_1, null, Vector3.zero, Quaternion.identity, out var newObject) == false)
                newObject = Instantiate(prefab);
            if (Recycler.TryGrabEnum(TEST.OBJECT_2, null, Vector3.zero, Quaternion.identity, out var newObject2) == false)
                newObject2 = Instantiate(prefab);
            
            _objects1.Add(newObject);
            _objects2.Add(newObject2);
        }

        private void CheckObjectPositions()
        {
            for (int i = _objects1.Count - 1; i >= 0; i--)
            {
                var temp = _objects1[i];

                if (temp.transform.position.y > killYPos)
                    continue;
                
                _objects1.RemoveAt(i);
                
                Recycler.RecycleEnum(TEST.OBJECT_1, temp);
            }
            
            for (int i = _objects2.Count - 1; i >= 0; i--)
            {
                var temp = _objects2[i];
                
                if (temp.transform.position.y > killYPos)
                    continue;
                
                _objects2.RemoveAt(i);
                
                Recycler.RecycleEnum(TEST.OBJECT_2, temp);
            }
        }
    }
}