using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRing : MonoBehaviour
{

    private Rigidbody playerrb;

    public float launchStrength;
    // Start is called before the first frame update
    void Start()
    {
        playerrb = GameObject.Find("Player").GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //get the rings local forward direction, set that to the launch direction
            Vector3 launchDirection = transform.forward;
            StartCoroutine(ApplyForceOverTime(launchDirection));

        }
    }

    IEnumerator ApplyForceOverTime(Vector3 direction)
    {
       // Debug.DrawRay(transform.position, direction * 100, Color.red, 8f);
        float duration = 0.8f;
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            //calculates the decreasing force over time
            float forceMultiplier = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            //sets launch this frame to launch strength * decay/force decrease
            float forceThisFrame = launchStrength * forceMultiplier;

            //adds force to launch player in the forward direction * the force this frame
             playerrb.AddForce(direction.x, direction.y , direction.z * forceThisFrame, ForceMode.Impulse);
            Vector3 launchVelocity = direction * forceThisFrame;
            //set the players velocity to be in the direction and force per frame strength
            
            playerrb.velocity = launchVelocity;
            elapsedTime += Time.deltaTime;

            yield return null;
            
        }
       
    }
}
