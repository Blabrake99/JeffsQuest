using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public abstract class Player : MonoBehaviour, IDamageble
{
    [SerializeField] protected int health;
    [SerializeField] protected int damage;
    [SerializeField] protected float damageCooldown;
    [SerializeField] protected float jumpHeight, turnSpeed = 2f, wallJumpForce = 5, wallJumpHeight = 7;
    [SerializeField, Range(1, 15f)] protected float maxWalkSpeed, maxRunSpeed;
    [SerializeField, Range(.1f,5f)] protected float walkAccelerationSpeed, runAccelerationSpeed, decelerationSpeed;
    [SerializeField] protected int jumpAmount;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int maxJumpAngle = 45;
    protected int startHealth;
    protected float damagedTimer, justjumpedTimer, distToGround;
    protected bool isInteracting, isNearWall;
    protected PlayerAction actions;
    protected int currentAmountOfJumps;
    protected Collider coll;
    protected Vector3 lastRelativeMovement;
    [SerializeField] protected float speed;
    [SerializeField] protected LayerMask mask;
    private Vector3 wallJumpDir;
    public int Health { get { return health; } set { health = value; } }
    public Camera CurrentCamera;
    public Vector3 RespawnPoint;
    //protected HealthBar bar;
    List<Collider> groundTouchPoints = new List<Collider>();
    protected bool isRunning, inWater;
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();

        startHealth = health;
        RespawnPoint = transform.position;

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
        if (actions.Player.Run.ReadValue<float>() > 0)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
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

        if (inputVector != Vector2.zero)
        {
            speed = (isRunning) ? Mathf.Lerp(speed, maxRunSpeed, Time.deltaTime * runAccelerationSpeed) : Mathf.Lerp(speed, maxWalkSpeed, Time.deltaTime * walkAccelerationSpeed);
            rb.velocity = new Vector3(cameraRelativeMovement.x * speed, rb.velocity.y, cameraRelativeMovement.z * speed);
        }
        else
        {
            speed = Mathf.Lerp(speed, 0, Time.deltaTime * decelerationSpeed);
            rb.velocity = new Vector3(lastRelativeMovement.x * speed, rb.velocity.y, lastRelativeMovement.z * speed);
        }

        if (groundTouchPoints.Count > 0)
        {
            currentAmountOfJumps = 1;
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
            if (currentAmountOfJumps < jumpAmount)
            {
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
    bool CompareLayerIndex(Transform t, LayerMask layer)
    {
        return Mathf.Pow(2, t.gameObject.layer) == layer;
    }
    #region Collision
    protected void OnTriggerStay(Collider col)
    {
        if (col.gameObject.layer == 4)
            inWater = true;
    }
    protected void OnCollisionStay(Collision col)
    {
        List<ContactPoint> points = new List<ContactPoint>();
        int numberOfContacts = col.GetContacts(points);
        for (int i = 0; i < numberOfContacts; i++)
        {
            Collider collider = col.collider;
            if (RoundedNormalVectorAngle(points[i].normal, 3) <= 45 && CompareLayerIndex(col.transform, mask) && !groundTouchPoints.Contains(collider))
            {
                groundTouchPoints.Add(collider);
            }
            else if(!IsStillTouchingGround(points,numberOfContacts) && groundTouchPoints.Contains(collider))
                {
                groundTouchPoints.Remove(collider);
            }
        }
    }
    protected void OnCollisionExit(Collision col)
    {
        Collider collider = col.collider;
        if (groundTouchPoints.Contains(collider))
            groundTouchPoints.Remove(collider);
    }
    bool IsStillTouchingGround(List<ContactPoint> points, int numberOfContacts)
    {
        for(int i = 0; i < numberOfContacts; i++)
        {
            if(RoundedNormalVectorAngle(points[i].normal,3) <= maxJumpAngle)
            {
                return true;
            }
        }
        return false;
    }
    float RoundedNormalVectorAngle(Vector3 normal, uint decimalAccuracy)
    {
        int accuracy = (int)Mathf.Pow(10, decimalAccuracy);
        return Mathf.RoundToInt(Vector3.Angle(normal, Vector3.up) * accuracy) / accuracy;
    }
    protected void OnTriggerExit(Collider col)
    {
        //if (col.gameObject.layer == 4)
        //    inWater = false;
    }
    #endregion
}