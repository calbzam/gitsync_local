using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private InputControls input;
    private Rigidbody2D rb;

    private Vector3 respawnPoint = Vector3.zero;

    private bool onGround = false;
    public Transform groundCheckTransform;

    [Header("Speed Variables")]
    public float maxWalkSpeed = 5f;
    public float acceleration = 5f;
    public float deceleration = 10f;

    public float jumpForce = 5f;

    public float upGravityModifier = 0.5f;
    public float downGravityModifier = 2f;
    //public Vector2 GroundCheckSize = new Vector2(0.2f, 0.2f);
    public float GroundCheckSize = 0.2f;
    public LayerMask[] walkableGrounds;

    private Vector2 inputDir;
    private Vector2 frameVelocity;

    private bool JumpHolding, JumpTriggered, JumpTriggeredPrev;

    private void Awake()
    {
        input = new InputControls();
        JumpHolding = JumpTriggered = JumpTriggeredPrev = false;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    // how to automatically unsubscribe from all events on OnDisable unity ?
    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // if (inside deadZone) respawn player

        HandleInput();
    }

    private void FixedUpdate()
    {
        //CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    [ContextMenu("Respawn Player")]
    public void RespawnPlayer()
    {
        //playerHealth.ResetHealth();

        transform.position = respawnPoint;
    }

    private void HandleInput()
    {
        inputDir = input.Player.Movement.ReadValue<Vector2>();

        JumpHolding = input.Player.Jump.IsPressed();
        JumpTriggered = !JumpTriggeredPrev && JumpHolding;
        JumpTriggeredPrev = JumpHolding;
    }

    private void HandleJump()
    {
        if (/*onGround && */JumpTriggered)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void HandleDirection()
    {
        if (inputDir.x == 0)
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        else
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, inputDir.x * maxWalkSpeed, acceleration * Time.fixedDeltaTime);

        //MyMath.lerp(rb.velocity.x, inputDir.x * maxWalkSpeed, acceleration * Time.deltaTime);

        //Debug.Log(inputDir);
    }

    private void HandleGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = upGravityModifier;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(frameVelocity.x, rb.velocity.y);
    }

    //private void OnTriggerEnter(Collider collider)
    //{
    //    //Debug.Log(collider.name);
    //    foreach (LayerMask layer in walkableGrounds)
    //        if (MyMath.IsInLayerMask(collider.gameObject.layer, layer))
    //        {
    //            onGround = true;
    //            //Grounded?.Invoke();
    //            break;
    //        }
    //}

    //private void OnTriggerExit(Collider collider)
    //{
    //    //Debug.Log(collider.name);
    //    //Debug.Log(collider.gameObject.layer);

    //    Vector3 groundChkPos = groundCheckTransform.position;
    //    if (onGround)
    //    {
    //        bool noGround = true;
    //        foreach (LayerMask layer in walkableGrounds)
    //            //if (Physics2D.CircleCast(groundChkPos,GroundCheckSize,Vector2.down,))
    //            if (Physics.CheckCapsule(groundChkPos, groundChkPos + Vector3.down * minJumpHeight, drawRadius, layer)) { noGround = false; break; }
    //        if (noGround)
    //        {
    //            onGround = false;
    //            //Jumped?.Invoke();
    //        }
    //    }
    //}
}
