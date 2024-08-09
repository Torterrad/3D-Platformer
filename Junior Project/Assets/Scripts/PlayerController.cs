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
    public int coinCount;

    private float coyoteTime = 0.05f;
    private float coyoteTimeCounter;

    public bool isGrounded = true;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Rigidbody playerRb;

    public Vector3 currentVelocity;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI coinText;


    // Start is called before the first frame update
    void Start()
    {
        //sets gravity modifier for entire scene
        playerRb = GetComponent<Rigidbody>();
    //    Physics.gravity *= gravityModifier;
        healthText.text = "Health: " + health;
        coinText.text = "Coin: " + coinCount;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            coinCount++;
            coinText.text = "Coins: " + coinCount;
            Destroy(other.gameObject);
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

        if (!isGrounded)
        {
            playerRb.AddForce(Vector3.down * gravityModifier, ForceMode.Acceleration);
        }


        //If the jump counter is zero and coyote time counter is zero jump when space is presed
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            StartJump();
        }
        if (Input.GetKeyUp(KeyCode.Space) && !isGrounded && playerRb.velocity.y > 0)
        {
            float smoothedYVelocity = Mathf.Lerp(playerRb.velocity.y * 0.6f, playerRb.velocity.y * 0.9f, Time.deltaTime * 10f);
            //code here for stopping the player rising, the short jump
            currentVelocity = new Vector3(currentVelocity.x, smoothedYVelocity, currentVelocity.z);
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

    }

    void StartJump()
    {
        //jump code goes here
        float smoothedJumpForce = Mathf.Lerp(jumpForce, jumpForce * 0.2f, Time.deltaTime * 10f);
        currentVelocity = new Vector3(playerRb.velocity.x, smoothedJumpForce, playerRb.velocity.z);
        

        isGrounded = false;

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }

    public float duration = 0.1f;

    void PlayerDeath()
    {
        if(health < 1)
        {
            Debug.Log("Dead");
        }
    }


}
