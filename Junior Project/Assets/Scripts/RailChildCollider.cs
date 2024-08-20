using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailChildCollider : MonoBehaviour
{
    private GrindRail parentScript;

    void Start()
    {
        // Get the RailGrind script from the parent object
        parentScript = GetComponentInParent<GrindRail>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& !parentScript.isGrinding)
        {
            parentScript.OnChildTriggerEnter(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
       // if (other.CompareTag("Player"))
        {
     //       parentScript.OnChildTriggerExit(other);
        }
    }
}
