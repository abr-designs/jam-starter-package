using Trajectory;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MouseTrajectoryExample), typeof(TrajectoryLine))]
public class ProjectileSpawnerExample : MonoBehaviour
{
    [SerializeField]
    private GameObject _projectilePrefab;

    [SerializeField]
    private int maxProjectiles = 10;

    [SerializeField]
    private float projectileLife = 5f;

    private struct Projectile
    {
        public GameObject obj;
        public float lifetime;
        public Projectile(GameObject o)
        {
            this.obj = o;
            this.lifetime = 0f;
        }
    }

    private Projectile[] _projectilePool;

    private TrajectoryLine _trajectoryLine;


    //Unity Functions
    //============================================================================================================//

    private void Start()
    {
        // Create object pool
        _projectilePool = new Projectile[maxProjectiles];
        for (int i = 0; i < _projectilePool.Length; i++)
        {
            var gObj = Instantiate(_projectilePrefab, transform);
            gObj.SetActive(false);
            _projectilePool[i] = new Projectile(gObj);
        }

        _trajectoryLine = GetComponent<TrajectoryLine>();

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            int index = GetFreeProjectileIndex();
            Debug.Log($"Mouse click, {index}");
            if(index >= 0) {
                FireProjectile(index);
            }

            
        }

        UpdateProjectiles();


    }

    //ProjectileSpawner Functions
    //============================================================================================================//    

    // Get an available projectile from the pool
    // Returns null if they are all in use
    private int GetFreeProjectileIndex()
    { 
        for (int i = 0; i < _projectilePool.Length; i++)
        {
            var p = _projectilePool[i];
            if (!p.obj.activeSelf)
                return i;
        }
        return -1;
    }

    // Check the projectile pool for any expired projectiles
    // This could also be used to move them if they are kinematic etc.
    private void UpdateProjectiles()
    {
        for (int i = 0; i < _projectilePool.Length; i++)
        {
            var p = _projectilePool[i];
            if(i==0)
                Debug.Log(p.lifetime);
            if (p.lifetime <= 0f && p.obj.activeSelf)
            {
                p.obj.SetActive(false);
            } else if(p.obj.activeSelf) {
                p.lifetime -= Time.deltaTime;
                _projectilePool[i] = p;
            }
        }
    }

    private void FireProjectile(int index)
    {
        var p = _projectilePool[index];
        p.obj.SetActive(true);
        p.obj.transform.position = transform.position;
        var rb = p.obj.GetComponent<Rigidbody>();
        rb.linearVelocity = _trajectoryLine.LaunchVelocity;
        p.lifetime = projectileLife;
        _projectilePool[index] = p;

    }

}
