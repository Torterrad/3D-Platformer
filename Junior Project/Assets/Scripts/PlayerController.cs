using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float acceleration;
    public float deceleration;
    public float jumpForce;
    public float gravityModifier;

    private Rigidbody playerRb;

    private Vector3 currentVelocity;

    public bool isGrounded = true;

    // Start is called before the first frame update
    void Start()
    {
        //sets gravity modifier for entire scene
        playerRb = GetComponent<Rigidbody>();
        Physics.gravity *= gravityModifier;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
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
            Debug.Log("Ow");
        }
    }

    void MovePlayer()
    {
        //get input axis from input manager
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        //calculates target velocity based on player input and speed
        Vector3 targetVelocity = new Vector3(horizontalInput, 0f, verticalInput) * speed;

        // Interpolate the current velocity towards the target velocity
        if (targetVelocity.magnitude > 0.1f)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        playerRb.velocity = new Vector3(currentVelocity.x, playerRb.velocity.y, currentVelocity.z);

        //calculate movement direction
        Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        //rotate player to face movement direction
        if(movementDirection!= Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }


        //is press space and on the ground, add foce to jump, set them to not be on ground
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            Jump();
        }
    }

    void Jump()
    {
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }


}
