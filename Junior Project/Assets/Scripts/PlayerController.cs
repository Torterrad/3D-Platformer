using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float acceleration;
    public float deceleration;
    public float jumpForce;
    public float gravityModifier;
    public float hangTime;
    public float hangGravity;
    public float groundCheckDistance = 0.3f;


    public int health = 3;
    public int coinCount;

    private float coyoteTime = 0.05f;
    private float coyoteTimeCounter;

    public bool isGrounded = true;
    private bool isHanging = false;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Rigidbody playerRb;

    public Vector3 currentVelocity;
    public Transform groundCheckPoint;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI coinText;

    public LayerMask groundLayer;

    private float horizontalInput;
    private float verticalInput;
    public bool jumpPressed;
    public bool jumpReleased;
    public bool willJump;
    public bool stopJump;

    void Start()
    {
        //sets gravity modifier for entire scene
        playerRb = GetComponent<Rigidbody>();
    //    Physics.gravity *= gravityModifier;
        healthText.text = "Health: " + health;
        coinText.text = "Coin: " + coinCount;
    }

  
    //change this to fixed update cos physics, but jump doesn't work in fixed update rn idk why
    void Update()
    {
        //get input axis from input manager
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        jumpPressed = Input.GetButtonDown("Jump");
        jumpReleased = Input.GetButtonUp("Jump");

        if(jumpPressed)
        {
            Debug.Log("In Update");
            willJump = true;
        }
        if (jumpReleased)
        {
            Debug.Log("In Update");
            stopJump = true;
        }

        // Non-physics related operations
        PlayerDeath();
    }

    void FixedUpdate()
    {
        // Handle physics-related updates
        MovePlayer();
        Jump();
        GroundCheck();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //checks if the player is on the ground
       /* if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }*/

         if (collision.gameObject.CompareTag("Enemy"))
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
       
        //calculates target velocity based on player input and speed
        Vector3 targetVelocity = new Vector3(horizontalInput, 0f, verticalInput).normalized * speed;

        // Interpolate the current velocity towards the target velocity
        if (targetVelocity.magnitude > 0.1f)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        //updates the players position/applies the velocity moves player
        playerRb.velocity = new Vector3(currentVelocity.x, playerRb.velocity.y, currentVelocity.z);

        //calculate movement direction
        Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        //rotate player to face movement direction
        if(movementDirection!= Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void Jump()
    {
        //if grounded, reset the coyote timer
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else//else countdown the timer, do count when off the ground/in air
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        //if space pressed
        if (willJump)
        {
            Debug.Log("Jump");
            jumpBufferCounter = jumpBufferTime;
            willJump = false;
        }
        else
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }
        //if in the air and not hanging, apply gravity downwards
        if (!isGrounded && !isHanging)
        {
            playerRb.AddForce(Vector3.down * gravityModifier, ForceMode.Acceleration);
        }


        //If the jump counter is zero and coyote time counter is zero jump when space is presed
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            //mathf.lerp used to smooth the velocity change
            playerRb.velocity = new Vector3(playerRb.velocity.x, jumpForce, playerRb.velocity.z);

            //reset flags after jumping
            isGrounded = false;
            isHanging = false;
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }
        if (stopJump && !isGrounded && playerRb.velocity.y > 0)
        {
            float smoothedYVelocity = Mathf.Lerp(playerRb.velocity.y * 0.6f, playerRb.velocity.y * 0.9f, Time.fixedDeltaTime * 10f);
            playerRb.velocity = new Vector3(playerRb.velocity.x, smoothedYVelocity, playerRb.velocity.z);
            stopJump = false;
        }

        if (!isGrounded && !isHanging && currentVelocity.y > 0 && currentVelocity.y < 10  )
        {
            StartCoroutine(HangTime());
        }


    }

    void PlayerDeath()
    {
        if(health < 1)
        {
            Debug.Log("Dead");
        }
    }

    void GroundCheck()
    {
        
        // Cast a ray downwards from the groundCheckPoint
        RaycastHit hit;
        //debug ray to show the raycast in unity
        Debug.DrawRay(groundCheckPoint.position, Vector3.down, Color.green, groundCheckDistance);

        //if the ray touches object on ground layer
        if (Physics.Raycast(groundCheckPoint.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            //if the ray touchs object tagged as ground, player is grounded
            isGrounded = true;
        }
        else//else the ray doesn't touch ground
        {
            isGrounded = false;
        }

    }

    IEnumerator HangTime()
    {
        gravityModifier /= hangGravity;
        isHanging = true;
        yield return new WaitForSeconds(hangTime);
        gravityModifier *= hangGravity;
        isHanging = false;
    }

}
