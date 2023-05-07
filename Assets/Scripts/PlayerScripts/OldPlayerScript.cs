using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public abstract class OldPlayerScript : MonoBehaviour
{
    [Header("For Combat")]
    [SerializeField, Tooltip("Players health")] protected int health;
    [SerializeField, Tooltip("Players attack damage")] protected int damage;
    [SerializeField, Tooltip("Players IFrames")] protected float damageCooldown;

    [Header("For Movement")]
    [SerializeField, Tooltip("How high the player jumps")] protected float jumpHeight;
    [SerializeField] protected float turnSpeed = 2f, wallJumpForce = 5, wallJumpHeight = 7;
    [SerializeField, Range(1, 15f)] protected float maxWalkSpeed = 5, maxRunSpeed = 7, maxAirSpeed = 1f, maxCrouchSpeed = 3, longJumpXZForce
        , longJumpYForce;
    [SerializeField, Range(.1f, 5f)]
    protected float walkAccelerationSpeed, runAccelerationSpeed, decelerationSpeed, airAcceleration, airDecelerationSpeed
        , animatorWalkAcceleration = .2f, animatorWalkdeceleration = .5f, crouchacceration = .2f, crouchdecleration = .5f;
    [SerializeField, Tooltip("Players jump count")] protected int jumpAmount;
    protected int startHealth;
    protected float damagedTimer, justjumpedTimer, distToGround;
    protected bool isInteracting, isNearWall;
    protected PlayerAction actions;
    protected int currentAmountOfJumps;
    protected Collider coll;
    protected Vector3 lastRelativeMovement;
    protected float speed, animatorWalkSpeed;
    protected Rigidbody rb;
    protected Animator anim;
    [Header("Pat Don't touch")]
    [SerializeField, Tooltip("Ground check mask")] protected LayerMask mask;
    [SerializeField, Tooltip("The object at the bottom of the player")] protected Transform groundCheck;
    [SerializeField, Tooltip("The Main Camera")] Camera CurrentCamera;
    private Vector3 wallJumpDir;
    public int Health { get { return health; } set { health = value; } }
    //protected HealthBar bar;
    protected bool isRunning, inWater, isCrouching, isLongJumping;
    [HideInInspector] public Vector3 RespawnPoint;
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();

        startHealth = health;
        RespawnPoint = transform.position;
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider>();
        distToGround = coll.bounds.extents.y;
        actions = new PlayerAction();
        actions.Player.Enable();
        actions.Player.Jump.performed += OnJump;
        actions.Player.Interact.performed += OnInteract;
        actions.Player.Fire.performed += OnFire;
    }
    public void Damage(int amount)
    {
        if (Time.time > damagedTimer)
        {
            damagedTimer = Time.time + damageCooldown;
            Health -= amount;
            //bar.SetHealth(Health);
        }
        if (Health < 1)
        {
            Respawn();
        }
    }
    protected void Update()
    {
        if (actions.Player.Run.ReadValue<float>() > 0 && !isCrouching)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
        if (actions.Player.Crouch.ReadValue<float>() > 0 && IsGrounded())
        {
            if (speed <= maxWalkSpeed)
            {
                isLongJumping = false;
                anim.SetBool("Crouch", true);
                isCrouching = true;
            }
            else
            {
                print("Longjumping");
                isLongJumping = true;
            }
        }
        if(actions.Player.Crouch.ReadValue<float>() <= 0 || !IsGrounded())
        {
            isCrouching = false;
            anim.SetBool("Crouch", false);
        }
        if (damagedTimer > 0)
            damagedTimer -= Time.deltaTime;
        if (justjumpedTimer > 0)
            justjumpedTimer -= Time.deltaTime;
    }
    protected void FixedUpdate()
    {
        //Gets movement input
        Vector2 inputVector = actions.Player.Move.ReadValue<Vector2>();
        //getting camera direction
        Vector3 forward = CurrentCamera.transform.forward;
        Vector3 right = CurrentCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        //normalizing vectors 
        forward = forward.normalized;
        right = right.normalized;
        Vector3 forwardRelativeInput = inputVector.y * forward;
        Vector3 rightRelativeInput = inputVector.x * right;
        Vector3 cameraRelativeMovement = forwardRelativeInput + rightRelativeInput;
        if (cameraRelativeMovement != Vector3.zero)
            lastRelativeMovement = cameraRelativeMovement;


        if (IsGrounded())
        {
            anim.SetBool("Jump", false);
            if (inputVector != Vector2.zero)
            {
                if (!isCrouching)
                {
                    speed = (isRunning) ? Mathf.Lerp(speed, maxRunSpeed, Time.deltaTime * runAccelerationSpeed) : Mathf.Lerp(speed, maxWalkSpeed, Time.deltaTime * walkAccelerationSpeed);
                }
                else
                {
                    speed = Mathf.Lerp(speed, maxCrouchSpeed, Time.deltaTime * crouchacceration);
                }
                if (isRunning)
                {
                    if (animatorWalkSpeed < 1f)
                        animatorWalkSpeed += animatorWalkAcceleration * Time.deltaTime;

                    anim.speed = 1;
                }
                else
                {

                    if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
                    {
                        anim.speed = Mathf.Abs(inputVector.x);
                    }
                    else if (Mathf.Abs(inputVector.x) < Mathf.Abs(inputVector.y))
                    {
                        anim.speed = Mathf.Abs(inputVector.y);
                    }
                    if (animatorWalkSpeed < .5f)
                        animatorWalkSpeed += animatorWalkAcceleration * Time.deltaTime;
                    if (animatorWalkSpeed > .5f)
                        animatorWalkSpeed = .5f;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraRelativeMovement), Time.deltaTime * turnSpeed);
                rb.velocity = new Vector3(cameraRelativeMovement.x * speed, rb.velocity.y, cameraRelativeMovement.z * speed);
            }
            else
            {
                anim.speed = 1;
                if (rb.velocity.x > .01f && rb.velocity.z > .01f)
                {
                    if (!isLongJumping)
                    {
                        speed = (isCrouching) ? Mathf.Lerp(speed, 0, Time.deltaTime * crouchdecleration) : Mathf.Lerp(speed, 0, Time.deltaTime * decelerationSpeed);
                        rb.velocity = new Vector3(lastRelativeMovement.x * speed, rb.velocity.y, lastRelativeMovement.z * speed);
                    }
                    else
                    {
                        rb.AddForce(Vector3.up * longJumpYForce, ForceMode.Impulse);
                        rb.AddForce(Vector3.forward * longJumpXZForce, ForceMode.Impulse);
                        isLongJumping = false;
                    }
                }
                if (animatorWalkSpeed > 0)
                {
                    animatorWalkSpeed -= animatorWalkdeceleration * Time.deltaTime;
                }
                else
                {
                    animatorWalkSpeed = 0;
                }
            }

            anim.SetFloat("Velocity", animatorWalkSpeed);
            currentAmountOfJumps = 1;
        }
        else
        {
            anim.speed = 1;
            anim.SetFloat("Velocity", 0);
            if (inputVector != Vector2.zero)
            {
                speed = (speed < maxAirSpeed) ? Mathf.Lerp(speed, maxAirSpeed, Time.deltaTime * airAcceleration) : Mathf.Lerp(speed, maxAirSpeed, Time.deltaTime * airDecelerationSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lastRelativeMovement), Time.deltaTime * turnSpeed);
            }
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && !isInteracting)
        {
            interact();
        }
    }
    void interact()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 3, Vector3.forward, 4, 9);
        foreach (RaycastHit hit in hits)
        {
            Iinteractibles i = hit.collider.gameObject.GetComponent<Iinteractibles>();
            if (i != null)
            {
                i.Interact();
            }
            else
                return;
        }
        isInteracting = true;
        StartCoroutine(interacting());
    }
    IEnumerator interacting()
    {
        yield return new WaitForSeconds(.5f);
        isInteracting = false;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentAmountOfJumps <= jumpAmount)
            {
                anim.SetBool("Jump", true);
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                justjumpedTimer = .1f;
                currentAmountOfJumps++;
            }
            if (wallJumpDir != Vector3.zero)
            {
                rb.AddForce(new Vector3(-wallJumpDir.x * wallJumpForce, wallJumpHeight, -wallJumpDir.z * wallJumpForce), ForceMode.Impulse);
            }
        }
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

        }
    }
    public bool MaxHealth()
    {
        if (health == startHealth)
            return true;
        else
        {
            return false;
        }
    }
    public void GainHealth(int amount)
    {
        if (health + amount > startHealth)
            health = startHealth;
        else
        {
            health += startHealth;
        }
    }
    public void Respawn()
    {
        Health = startHealth;
        //bar.SetHealth(Health);
        transform.position = RespawnPoint;
    }
    bool CompareLayerIndex(Transform t, LayerMask layer)
    {
        return Mathf.Pow(2, t.gameObject.layer) == layer;
    }
    #region Collision
    protected void OnTriggerStay(Collider col)
    {
        //if (col.gameObject.layer == 4)
        //    inWater = true;
    }
    bool IsGrounded()
    {
        if (justjumpedTimer <= 0)
            return Physics.CheckSphere(groundCheck.position, .1f, mask);
        else
            return false;
    }

    protected void OnTriggerExit(Collider col)
    {
        //if (col.gameObject.layer == 4)
        //    inWater = false;
    }
    #endregion
}