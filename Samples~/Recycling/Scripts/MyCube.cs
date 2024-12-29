using System;
using UnityEngine;
using Utilities.Recycling;


public class MyCube : MonoBehaviour, IRecyclable
{
    [SerializeField]
    private float killHeight;
    
    
    public bool IsRecycled { get; set; }
    public void OnRecycled()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void Update()
    {
        if (transform.position.y > killHeight)
            return;
        
        Recycler.Recycle(this);
    }
}
