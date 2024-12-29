using UnityEngine;
using Utilities.Recycling;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    private MyCube prefab;

    [SerializeField, Min(0f)]
    private float spawnTime;

    private float _spawnTimer;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer < spawnTime)
            return;

        _spawnTimer = 0f;

        if (Recycler.TryGrab<MyCube>(null, Vector3.zero, Quaternion.identity, out var newCube) == false)
            newCube = Instantiate(prefab);
    }
}
