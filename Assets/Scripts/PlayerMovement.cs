using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float speed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    bool readyToJump;
    public float airMultiplier;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsBuilding;
    bool grounded;
    bool groundedBuilding;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;
    public MovementState state;

    public enum MovementState
    {
        //Creating different states of movement so it is easier to know what the player can and cannot do
        walking,
        sprinting,
        air,
        wallrunning,
        wallstick
    }

    // --- SpiderVR: entrada sem controle (correr balancando o celular / passos reais) ---
    [HideInInspector] public float VrForward;
    [HideInInspector] public bool VrRun;
    [HideInInspector] public float VrJump;      // 0 = nada, 0.4 pulinho, 0.7 normal, 1 alto
    [HideInInspector] public bool VrJumpForward; // pulo com impulso (veio correndo)
    [HideInInspector] public bool VrCrouch;

    public bool wallrunning;
    public bool wallstick;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void Update()
    {
        //Checking if the player is on the ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        //Checking is the player is ontop of a building
        groundedBuilding = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsBuilding);

        MyInput();
        SpeedControl();
        StateHandler();

        //Drag
        if (grounded || groundedBuilding)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        //Getting inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (VrForward != 0f) verticalInput = VrForward;

        if (VrJump > 0f && readyToJump && (grounded || groundedBuilding)) { readyToJump = false; JumpVr(VrJump, VrJumpForward); VrJump = 0f; Invoke(nameof(ResetJump), jumpCooldown); }
        //Jump mechanics
        if (Input.GetKey(jumpKey) && readyToJump && (grounded || groundedBuilding))
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        //Sprinting
        if ((grounded || groundedBuilding) && (Input.GetKey(sprintKey) || VrRun))
        {
            state = MovementState.sprinting;
            speed = sprintSpeed;
        }

        //Walking
        else if (grounded || groundedBuilding)
        {
            state = MovementState.walking;
            speed = walkSpeed;
        }

        //Airborne
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //Grounded
        if (grounded || groundedBuilding)
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);

        //Airborne
        else if (!(grounded || groundedBuilding))
            rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //Speed limit
        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        //Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void JumpVr(float power, bool forward)
    {
        Vector3 v = rb.velocity; v.y = 0f; if (!forward) { v.x = 0f; v.z = 0f; }
        rb.velocity = v;
        rb.AddForce(transform.up * jumpForce * Mathf.Lerp(0.5f, 1.5f, power), ForceMode.Impulse);
        if (forward) rb.AddForce(orientation.forward * jumpForce * 0.6f * power, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}