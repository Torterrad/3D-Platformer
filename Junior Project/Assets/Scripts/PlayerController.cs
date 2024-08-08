using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float acceleration;
    public float deceleration;
    public float jumpForce;
    public float gravityModifier;

    public int health = 3;

    private float coyoteTime = 0.05f;
    private float coyoteTimeCounter;

    public bool isGrounded = true;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Rigidbody playerRb;

    public Vector3 currentVelocity;

    public TextMeshProUGUI healthText;

   

    // Start is called before the first frame update
    void Start()
    {
        //sets gravity modifier for entire scene
        playerRb = GetComponent<Rigidbody>();
        Physics.gravity *= gravityModifier;
        healthText.text = "Health: " + health;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        PlayerDeath();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //checks if the player is on the ground
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            health--;
            healthText.text = "Health: " + health;
        }
    }

    void MovePlayer()
    {
        //get input axis from input manager
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        //calculates target velocity based on player input and speed
        Vector3 targetVelocity = new Vector3(horizontalInput, 0f, verticalInput).normalized * speed;

        // Interpolate the current velocity towards the target velocity
        if (targetVelocity.magnitude > 0.1f)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        //updates the players position/applies the velocity moves player
        playerRb.velocity = new Vector3(currentVelocity.x, currentVelocity.y, currentVelocity.z);


        //calculate movement direction
        Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        //rotate player to face movement direction
        if(movementDirection!= Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        //if grounded, reset the coyote timer
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else//else countdown the timer, do count when off the ground/in air
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        //if space pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }


        //If the jump counter is zero and coyote time counter is zero jump when space is presed
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            StartJump();
        }
    }

    void StartJump()
    {
        StartCoroutine(Jump());
       //playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }

    public float duration = 0.1f;

    IEnumerator Jump()
    {
        
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            //calculates the decreasing force over time
            float forceMultiplier = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            //sets launch this frame to launch strength * decay/force decrease
            float forceThisFrame = jumpForce * forceMultiplier;

            //adds force to launch player in the forward direction * the force this frame
            // playerrb.AddForce(direction.x, direction.y , direction.z * forceThisFrame, ForceMode.Impulse);
            Vector3 launchVelocity = Vector3.up * forceThisFrame;
            //set the players velocity to be in the direction and force per frame strength

            playerRb.velocity += launchVelocity;
            elapsedTime += Time.deltaTime;

            yield return null;

        }
    }

    void PlayerDeath()
    {
        if(health < 1)
        {
            Debug.Log("Dead");
        }
    }


}
