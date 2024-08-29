using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody projectileRb;
    public float projectilespeed;
    public float despawnTime;
    // Start is called before the first frame update
    void Start()
    {
        projectileRb = GetComponent<Rigidbody>();
        StartCoroutine(Despawn());
    }

    // Update is called once per frame
    void Update()
    {
        projectileRb.AddRelativeForce(Vector3.right * projectilespeed);
       // projectileRb.transform.TransformDirection(Vector3.back * projectilespeed);
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds (despawnTime);
        Destroy(gameObject);
    }

    
}
