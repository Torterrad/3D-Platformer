using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody projectileRb;
    public float projectilespeed;
    // Start is called before the first frame update
    void Start()
    {
        projectileRb = GetComponent<Rigidbody>();
        StartCoroutine(Despawn());
    }

    // Update is called once per frame
    void Update()
    {
        projectileRb.AddForce(Vector3.right * projectilespeed);
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds (3);
        Destroy(gameObject);
    }

    
}
