using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailTriggerForwarder : MonoBehaviour
{
    private GrindRail parentScript;

    void Start()
    {
        // Find the GrindRail script on the parent object
        parentScript = GetComponentInParent<GrindRail>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Forward the OnTriggerEnter event to the parent GrindRail script
        parentScript.ChildTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        // Forward the OnTriggerExit event to the parent GrindRail script
      //  parentScript.ChildTriggerExit(other);
    }
}
