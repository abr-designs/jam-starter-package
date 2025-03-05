using Trajectory;
using UnityEngine;

[RequireComponent(typeof(MouseTrajectoryExample), typeof(TrajectoryLine))]
public class ProjectileSpawnerExample : MonoBehaviour
{
    private class Projectile
    {
        public readonly GameObject GameObject;
        public readonly Rigidbody Rigidbody;
        
        public float Lifetime;
        public Projectile(GameObject gameObject)
        {
            GameObject = gameObject;
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            
            Lifetime = 0f;
        }
    }
    
    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private int maxProjectiles = 10;

    [SerializeField]
    private float projectileLife = 5f;

    private Projectile[] _projectilePool;

    private TrajectoryLine _trajectoryLine;


    //Unity Functions
    //============================================================================================================//

    private void Start()
    {
        _trajectoryLine = GetComponent<TrajectoryLine>();
        
        // Create object pool
        _projectilePool = new Projectile[maxProjectiles];
        for (int i = 0; i < _projectilePool.Length; i++)
        {
            var projectileGameObject = Instantiate(projectilePrefab, transform);
            projectileGameObject.SetActive(false);
            _projectilePool[i] = new Projectile(projectileGameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int index = GetFreeProjectileIndex();
            if(index >= 0) 
            {
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
            var projectile = _projectilePool[i];
            if (!projectile.GameObject.activeSelf)
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
            if (p.Lifetime <= 0f && p.GameObject.activeSelf)
            {
                p.GameObject.SetActive(false);
            } else if(p.GameObject.activeSelf) {
                p.Lifetime -= Time.deltaTime;
                _projectilePool[i] = p;
            }
        }
    }

    private void FireProjectile(int index)
    {
        var projectile = _projectilePool[index];
        projectile.GameObject.SetActive(true);
        projectile.GameObject.transform.position = transform.position;
        
        projectile.Rigidbody.linearVelocity = _trajectoryLine.LaunchVelocity;
        projectile.Lifetime = projectileLife;
        _projectilePool[index] = projectile;
        
        Debug.Log(_trajectoryLine.LaunchVelocity);
    }

}
