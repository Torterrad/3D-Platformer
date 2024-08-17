using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class GrindRail : MonoBehaviour
{
    public SplineContainer railSpline;
    public float grindSpeed = 5f;

    private bool isGrinding = false;
    private float splinePosition = 0f;
    private Rigidbody playerRb;

    public float colliderLength = 1.0f;
    public float colliderWidth = 0.2f;
    public float colliderHeight = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GameObject.Find("Player").GetComponent<Rigidbody>();
        GenerateColliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrinding)
        {
            Grind();
        }

        if(Input.GetButtonDown("Jump"))
        {
            StopGrinding();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("player col");
            StartGrinding();
        }
    }

    void StartGrinding()
    {//might have to change this to use players gravity mod variable
        //might have to change splineposition as this always starts grind at start of rail
        isGrinding = true;
        playerRb.useGravity = false;
        splinePosition = 0f;
    }

    void Grind()
    {//uses calculatelength as length doesn't work
        splinePosition += grindSpeed * Time.deltaTime / railSpline.CalculateLength();

        if (splinePosition >= 1f)
        {
            StopGrinding();
            return;
        }

        //get position and direction on spline, normalize not work?
        Vector3 splinePoint = railSpline.EvaluatePosition(splinePosition);
        Vector3 splineDirection = railSpline.EvaluateTangent(splinePosition);
        splineDirection.Normalize();

        //move and rotate the player along the spline
        playerRb.MovePosition(splinePoint);
        playerRb.MoveRotation(Quaternion.LookRotation(splineDirection));
    }

    void StopGrinding()
    {
        isGrinding = false;
        playerRb.useGravity = true;
        //detach from rail, allow player to move normally again
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopGrinding();
        }
    }

    void GenerateColliders()
    {
        float splineLength = railSpline.CalculateLength();

        for (float t = 0; t < 1.0f; t += colliderLength / splineLength)
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
        }

    }
}