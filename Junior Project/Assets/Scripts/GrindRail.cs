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

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GameObject.Find("Player").GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrinding)
        {
            Grind();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
        splinePosition+= grindSpeed * Time.deltaTime / railSpline.CalculateLength();

        if(splinePosition >= 1f)
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

}
