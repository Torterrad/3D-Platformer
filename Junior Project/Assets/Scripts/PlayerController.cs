using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    public float speed;
    public float rotationSpeed;
    public float acceleration;
    public float deceleration;

    //jump and ground
    public float jumpForce;
    public float dashForce;
    public float gravityModifier;
    public float hangTime;
    public float hangGravity;
    public float groundCheckDistance = 0.3f;
    public float airDragStrength;
    public float dashDuration = 0.4f;
    public float groundStick = 0;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float horizontalInput;
    private float verticalInput;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    private float maxSlopeAngle = 50f;

    public int health = 3;
    public int coinCount;

    public bool isGrounded = true;
    public bool jumpPressed;
    public bool jumpReleased;
    public bool willJump;
    public bool doubleJump;
    public bool willDoubleJump;
    public bool stopJump;
    public bool leftClick;
    public bool willDash;
    public bool dashCoolDown = false;
    public bool controlable = true;
    private bool isHanging = false;

    public Vector3 currentVelocity;
    private Vector3 groundNormal;
    public LayerMask groundLayer;
    
    private Rigidbody playerRb;
    public Transform groundCheckPoint;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI coinText;

    

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        healthText.text = "Health: " + health;
        coinText.text = "Coin: " + coinCount;
    }

    // Called every frame, user inputs 
    void Update()
    {
        if (!controlable)
        {
            // get input axis from input manager
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            jumpPressed = Input.GetButtonDown("Jump");
            jumpReleased = Input.GetButtonUp("Jump");
            leftClick = Input.GetMouseButtonDown(0);

            if (jumpPressed)
            {
                willJump = true;

                if (!isGrounded && doubleJump && coyoteTimeCounter < 0f)
                {
                    willDoubleJump = true;
                }
            }
            if (jumpReleased)
            {
                stopJump = true;
            }

            if (leftClick && !dashCoolDown)
            {
                willDash = true;
            }
        }
       
        PlayerDeath();
    }

    void FixedUpdate()
    {
        // Handle physics-related updates
        MovePlayer();
        Jump();
        GroundCheck();
        StartCoroutine(Dash());
        

        if(!isGrounded)
        {
            // Get the horizontal velocity x and z axes
            Vector3 horizontalVelocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z);

            // Calculate the drag force for horizontal movement
            Vector3 airDrag = -horizontalVelocity * airDragStrength * horizontalVelocity.magnitude;

            // Apply the drag force to the player's rigidbody on x and z axes
            playerRb.AddForce(airDrag, ForceMode.Force);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
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
        if (other.gameObject.CompareTag("Boost"))
        {
            currentVelocity *= 1.5f;
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
        //calculates the direction to move the player on the slope
        Vector3 projectedVelocity = Vector3.ProjectOnPlane(currentVelocity, groundNormal);

        //updates the players position/applies the velocity moves player
        playerRb.velocity = new Vector3(projectedVelocity.x, playerRb.velocity.y, projectedVelocity.z);

        //calculate movement direction
        Vector3 movementDirection = Vector3.ProjectOnPlane(targetVelocity, groundNormal).normalized;

        //rotate player to face movement direction
        if (movementDirection!= Vector3.zero)
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
            //sicks player to ground to stop running off slope so player runs down them
            playerRb.AddForce(Vector3.down * groundStick, ForceMode.Acceleration);
        }
        else//else countdown the timer, do count when off the ground/in air
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        //if space pressed
        if (willJump)
        {
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
            float smoothedJumpForce = Mathf.Lerp(jumpForce, jumpForce * 0.2f, Time.fixedDeltaTime * 10f);
            playerRb.velocity = new Vector3(playerRb.velocity.x, smoothedJumpForce, playerRb.velocity.z);

            //reset flags after jumping
            isGrounded = false;
            isHanging = false;
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }
        if (stopJump && !isGrounded && playerRb.velocity.y > 0)
        {//if player lets go of jump while going up, reduce their veolicty to stall the jump
            float smoothedYVelocity = Mathf.Lerp(playerRb.velocity.y * 0.6f, playerRb.velocity.y * 0.9f, Time.fixedDeltaTime * 10f);
            playerRb.velocity = new Vector3(playerRb.velocity.x, smoothedYVelocity, playerRb.velocity.z);
            stopJump = false;
        }

        if(playerRb.velocity.y < 0)
        {
            //this here to prevent short hop bug of stop jump stay true after a full jump then landing 
            stopJump = false;
        }

        if (!isGrounded && !isHanging && playerRb.velocity.y > 0 && playerRb.velocity.y < 10  )
        {//if in the air and not already hanging at at peak of jump, hang
            StartCoroutine(HangTime());
        }
        //double jump if in air and will double jump is true
        if(!isGrounded && willDoubleJump)
        {
            //Double jump 
            float smoothedJumpForce = Mathf.Lerp(jumpForce, jumpForce * 0.2f, Time.fixedDeltaTime * 10f);
            playerRb.velocity = new Vector3(playerRb.velocity.x, smoothedJumpForce, playerRb.velocity.z);
            //reset double jump to stop infinate jumps
            doubleJump = false;
            willDoubleJump = false;
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
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            //if the ray touchs object tagged as ground, player is grounded
            //if the angle is not to steep, counts as ground
            if (angle <= maxSlopeAngle)
            {
                isGrounded = true;
                doubleJump = true;

                groundNormal = hit.normal;
            }
            else
            {
                isGrounded = false;
                groundNormal = Vector3.up;
            }


        }
        else//else the ray doesn't touch ground
        {
            isGrounded = false;
            groundNormal = Vector3.up;
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

    IEnumerator Dash()
    {
        if(willDash && !dashCoolDown)
        {//Propells the player forward
            dashCoolDown = true;

            float elapsedTime = 0f;
            //get the forward direction from player input
            Vector3 dashDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

            if (dashDirection == Vector3.zero)
            {
                //If there's no input, default to the player's facing direction
                dashDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            }

            //Ensure the dash direction is always normalized
            dashDirection = dashDirection.normalized;

            while (elapsedTime < dashDuration)
            {
                //calculates the decreasing force over time
                //sets launch this frame to launch strength * decay/force decrease
                float forceMultiplier = Mathf.Lerp(1f, 0f, elapsedTime / dashDuration);
                float forceThisFrame = dashForce * forceMultiplier;

                //adds force to launch player in the forward direction * the force this frame only on the x and z axis
                playerRb.AddForce(dashDirection * forceThisFrame, ForceMode.Impulse);
                elapsedTime += Time.deltaTime;

                yield return null;

            }
            willDash = false;
            yield return new WaitForSeconds(0.4f);
            dashCoolDown = false;
        }
        
    }

    public void StartDisableControl(float disableTime)
    {
        StartCoroutine(DisableControl(disableTime));
        Debug.Log("startdisable");
    }

    public IEnumerator DisableControl(float disableTime)
    {
        controlable = true;
        Debug.Log("Disabled");
        yield return new WaitForSeconds(disableTime);
        controlable = false;
        Debug.Log("Enabled");
    }
}
