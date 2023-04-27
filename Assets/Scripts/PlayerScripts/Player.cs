using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public abstract class Player : MonoBehaviour, IDamageble
{
    [SerializeField] protected int health;
    [SerializeField] protected int damage;
    [SerializeField] protected float damageCooldown;
    [SerializeField] protected float walkSpeed, runSpeed, jumpHeight, turnSpeed = 2f, wallJumpForce = 5, wallJumpHeight = 7;
    [SerializeField] protected int jumpAmount;
    [SerializeField] protected Rigidbody rb;
    protected int startHealth;
    protected float damagedTimer, justjumpedTimer, distToGround;
    protected bool isInteracting, isNearWall;
    protected PlayerAction actions;
    protected int currentAmountOfJumps;
    protected Collider coll;
    [SerializeField]
    protected LayerMask mask;
    private Vector3 wallJumpDir;
    public int Health { get { return health; } set { health = value; } }
    public Vector3 RespawnPoint;
    //protected HealthBar bar;
    protected bool grounded => IsGrounded();
    protected bool isRunning, inWater;
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        distToGround = GetComponent<Collider>().bounds.extents.y;
        startHealth = health;
        RespawnPoint = transform.position;

        coll = GetComponent<Collider>();
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
        if (actions.Player.Run.ReadValue<float>() > 0)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
        if (grounded)
        {
            //resets jump counters when on ground
            if (currentAmountOfJumps != jumpAmount)
                currentAmountOfJumps = jumpAmount;
        }
        if (damagedTimer > 0)
            damagedTimer -= Time.deltaTime;
        if (justjumpedTimer > 0)
            justjumpedTimer -= Time.deltaTime;
    }
    protected void FixedUpdate()
    {
        Vector2 inputVector = actions.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(inputVector.x, rb.velocity.y, inputVector.y);
        float magnitude = moveDir.magnitude;
        magnitude = Mathf.Clamp01(magnitude);
        float speed = (isRunning) ? runSpeed : walkSpeed;
        if (grounded)
        {
            rb.velocity = new Vector3(moveDir.x * magnitude * speed, rb.velocity.y, moveDir.z * magnitude * speed);
        }
        else
            rb.velocity = new Vector3(moveDir.x * magnitude * (speed / 2), rb.velocity.y, moveDir.z * magnitude * (speed/2));
        if (moveDir != Vector3.zero)
        {
            Quaternion toRotate = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotate, turnSpeed * Time.deltaTime);
        }
        if (!grounded)
        {
            RaycastHit hit;
            Vector3 rayCastDir = new Vector3(inputVector.x, 0, inputVector.y);
            if (Physics.Raycast(transform.position, transform.TransformDirection(rayCastDir), out hit, 1, mask))
            {
                isNearWall = true;
                wallJumpDir = rayCastDir;
            }
            else
            {
                isNearWall = false;
                wallJumpDir = Vector3.zero;
            }
        }
        else
        {
            isNearWall = false;
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && !isInteracting)
        {
            interact();
        }
    }
    protected void OnTriggerStay(Collider col)
    {
        if (col.gameObject.layer == 4)
            inWater = true;
    }
    protected void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == 4)
            inWater = false;
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
            if (!isNearWall)
            {
                if (currentAmountOfJumps > 0)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                    justjumpedTimer = .1f;
                    currentAmountOfJumps--;
                }
            }
            else
            {
                if (wallJumpDir != Vector3.zero)
                {
                    rb.AddForce(new Vector3(-wallJumpDir.x * wallJumpForce, wallJumpHeight, -wallJumpDir.z * wallJumpForce), ForceMode.Impulse);
                }
            }
        }
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

        }
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = true;
            print("running");
        }
        else if (context.canceled)
        {
            isRunning = false;
            print("stop");
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
    protected bool IsGrounded()
    {
        if (justjumpedTimer > 0)
            return false;

        // Object size in x
        float sizeX = coll.bounds.size.x;
        float sizeZ = coll.bounds.size.z;
        float sizeY = coll.bounds.size.y;
        // Position of the 4 bottom corners of the game object
        // We add 0.01 in Y so that there is some distance between the point and the floor
        Vector3 corner1 = transform.position + new Vector3(sizeX / 2, -sizeY / 2 + 0.01f, sizeZ / 2);
        Vector3 corner2 = transform.position + new Vector3(-sizeX / 2, -sizeY / 2 + 0.01f, sizeZ / 2);
        Vector3 corner3 = transform.position + new Vector3(sizeX / 2, -sizeY / 2 + 0.01f, -sizeZ / 2);
        Vector3 corner4 = transform.position + new Vector3(-sizeX / 2, -sizeY / 2 + 0.01f, -sizeZ / 2);
        // Send a short ray down the cube on all 4 corners to detect ground
        bool grounded1 = Physics.Raycast(corner1, new Vector3(0, -1, 0), 0.01f);
        bool grounded2 = Physics.Raycast(corner2, new Vector3(0, -1, 0), 0.01f);
        bool grounded3 = Physics.Raycast(corner3, new Vector3(0, -1, 0), 0.01f);
        bool grounded4 = Physics.Raycast(corner4, new Vector3(0, -1, 0), 0.01f);
        // If any corner is grounded, the object is grounded
        return (grounded1 || grounded2 || grounded3 || grounded4);
    }
}