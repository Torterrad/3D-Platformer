using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class GrindRail : MonoBehaviour
{
    public SplineContainer railSpline;
    public float grindSpeed = 5f;

    public bool isGrinding = false;
    private float splinePosition = 0f;
    private Rigidbody playerRb;
    private PlayerController playerScript;

    public float colliderLength = 1.0f;
    public float colliderWidth = 0.2f;
    public float colliderHeight = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GameObject.Find("Player").GetComponent<Rigidbody>();
        playerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        GenerateColliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrinding)
        {
            Grind();
            playerScript.isGrounded = true;
            playerScript.doubleJump = true;
        }

        if (Input.GetButtonDown("Jump"))
        {
            StopGrinding();
        }
    }

    public void ChildTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGrinding)
        {
            Debug.Log("Player entered the grind rail area");
            StartGrinding();
        }
    }

    void StartGrinding()
    {//might have to change this to use players gravity mod variable
        isGrinding = true;
        playerRb.useGravity = false;
        //Finds the cloeset part of the spline to the player
        splinePosition = FindClosestPointOnSpline(railSpline.Spline, playerRb.position);
        Debug.Log("Grind Started");
    }

    void Grind()
    {
        float splineLength = railSpline.CalculateLength();
      //  Debug.Log("Spline Length: " + splineLength);

        float normalizedSpeed = grindSpeed / splineLength;
       // Debug.Log("Normalized Speed: " + normalizedSpeed);

        //normalize the speed so goes at the same speed regardless of length
        splinePosition += normalizedSpeed * Time.deltaTime;
        //clamp the variable to stop it going above 1
        splinePosition = Mathf.Clamp(splinePosition, 0f, 1f);
        // Debugging the spline position before checking the condition
        Debug.Log("Spline Position: " + splinePosition);

        if (splinePosition >= 1f)
        {
            StopGrinding();
            return;
        }

        //get position and direction on spline, normalize not work?
        Vector3 splinePoint = railSpline.EvaluatePosition(splinePosition);
        Vector3 splineDirection = railSpline.EvaluateTangent(splinePosition);

        splineDirection.Normalize();

        //Smooth variables to interpolate
        Vector3 currentPosition = playerRb.position;
        Quaternion currentRotation = playerRb.rotation;

        //Interpolate the position and rotation to smooth
        Vector3 smoothPosition = Vector3.Lerp(currentPosition, splinePoint, Time.deltaTime * grindSpeed);
        Quaternion smoothRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(splineDirection), Time.deltaTime * grindSpeed);

        playerRb.MovePosition(smoothPosition);
        playerRb.MoveRotation(smoothRotation);
    }

    void StopGrinding()
    {
        isGrinding = false;
        playerRb.useGravity = true;
        //detach from rail, allow player to move normally again
        Debug.Log("Stop grind");
    }

    public void ChildTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopGrinding();
        }
    }

    void GenerateColliders()
    {
        float splineLength = railSpline.CalculateLength();

        float colliderFrequency = (colliderLength / splineLength) * 3;

        for (float t = 0; t < 1.0f; t += colliderFrequency)
        {
            Vector3 pointOnSpline = railSpline.EvaluatePosition(t);
            Vector3 tangentOnSpline = railSpline.EvaluateTangent(t);
            tangentOnSpline.Normalize();

            GameObject colliderObject = new GameObject("RailCollider");
            colliderObject.transform.position = pointOnSpline;
            colliderObject.transform.rotation = Quaternion.LookRotation(tangentOnSpline);

            BoxCollider boxCollider = colliderObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(colliderLength, colliderHeight, colliderWidth);

            // Ensure the collider is set as a trigger
            boxCollider.isTrigger = true;

            // Make the collider a child of the rail for organization
            colliderObject.transform.SetParent(transform);

            // Add the TriggerForwarder script
            colliderObject.AddComponent<RailTriggerForwarder>();
        }

    }

    float FindClosestPointOnSpline(Spline spline, Vector3 position)
    {
        float closestT = 0f;
        float closestDistance = float.MaxValue;

        //Iterates over small steps to find the closest point
        for (float t = 0f; t <= 1f; t += 0.01f)
        {
            Vector3 pointOnSpline = SplineUtility.EvaluatePosition(spline, t);
            float distance = Vector3.Distance(position, pointOnSpline);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestT = t;
            }
        }
        return closestT;
    }
}