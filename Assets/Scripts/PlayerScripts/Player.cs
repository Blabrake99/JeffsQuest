using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public abstract class Player : MonoBehaviour, IDamageble
{
    [SerializeField, Tooltip("Players health")] protected int health = 6;
    [SerializeField, Tooltip("Players IFrames")] protected float damageCooldown;
    [SerializeField, Range(0f, 100f), Tooltip("Max speed after Acceleration")] float maxWalkSpeed = 7f, maxRunSpeed = 10f, maxClimbSpeed = 2f, maxSwimSpeed = 5f, maxCrouchSpeed = 3f;
    [SerializeField, Range(0f, 100f), Tooltip("Acceleration for their respective names")]
    float maxAcceleration = 10f, maxAirAcceleration = 1f, maxSwimAcceleration = 5f, maxClimbAcceleration = 20f
        , animatorWalkAcceleration = .2f, animatorWalkdeceleration = .5f;
    [SerializeField, Range(0f, 10f), Tooltip("How high the player jumps")] float jumpHeight = 2f, wallJumpHeight = 6;
    [SerializeField, Range(0, 5), Tooltip("How many jumps the players allowed to do")] int maxAirJumps = 1;
    [SerializeField, Range(0f, 90f), Tooltip("This is the max ground angle to tell if the players Grounded or not")]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)] float maxSnapSpeed = 100f;
    [SerializeField, Min(0f), Tooltip("This is a raycast distance for below the player")] float probeDistance = 1f;
    [SerializeField, Tooltip("The layermasks for their respective names")] LayerMask probeMask = -1, stairsMask = -1, waterMask = 0, climbMask = -1, noneJumpableMask;
    [SerializeField, Tooltip("The Transform of the camera")] Transform playerInputSpace = default;
    [SerializeField, Tooltip("THe offset of water o nthe player. the higher the number the lower jeff will be in the water" +
        "befor he's at the top")]
    float submergenceOffset = 0.5f;
    [SerializeField, Range(90f, 170f)] private float maxClimbingAngle = 140f;
    [SerializeField, Min(0.1f), Tooltip("This is for detecting when the player is fully submerged")] float submergenceRange = 1f;
    [SerializeField, Range(0f, 10f), Tooltip("Makes movement more sluggesh underwater")] float waterDrag = 1f;
    [SerializeField, Min(0f), Tooltip("the lower the bouyancy the faster it sinks underwater")] float buoyancy = 1f;
    [SerializeField, Range(0.01f, 1f), Tooltip("This defines the minimum submergence required for swimming")] float swimThreshold = 0.5f;
    [SerializeField, Tooltip("The speed at which he rotates when he moves")] float turnSpeed = 5;
    [Header("For spine rotation")]
    [SerializeField] Transform boneToRotate;
    [SerializeField] float leftRotation, rightRotation;
    [HideInInspector] public Vector3 RespawnPoint;
    protected float curSpeed;
    protected int jumpPhase;
    protected Rigidbody _body, _connectedBody, _previousConnectedBody;
    protected Vector3 _playerInput;
    protected Vector3 _velocity, _connectionVelocity;
    protected Vector3 _connectionWorldPosition, _connectionLocalPosition;
    protected Vector3 _contactNormal, _steepNormal, _climbNormal, _lastClimbNormal;
    protected Vector3 _upAxis, _rightAxis, _forwardAxis;
    protected GameObject lastWallHit;
    protected bool _desiresClimbing;
    protected float _desiredJump, _desiredRunning;
    protected int _groundContactCount, _steepContactCount, _climbContactCount;
    protected bool ONGround => _groundContactCount > 0;
    protected bool ONSteep => _steepContactCount > 0;
    protected bool Climbing => _climbContactCount > 0 && _stepsSinceLastJump > 2;
    protected int _jumpPhase;
    protected float _minGroundDotProduct, _minStairsDotProduct, _minClimbDotProduct, animatorWalkSpeed;
    protected int _stepsSinceLastGrounded, _stepsSinceLastJump;
    protected bool InWater => _submergence > 0f;
    protected float _submergence, damagedTimer;
    protected bool Swimming => _submergence >= swimThreshold;
    protected bool jumping, isInteracting, isCrouching;
    protected Animator anim;
    protected int startHealth;
    protected PlayerAction actions;
    public int Health { get { return health; } set { health = value; } }
    private void OnValidate()
    {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        _minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        _minClimbDotProduct = Mathf.Cos(maxClimbingAngle * Mathf.Deg2Rad);
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
    protected void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
        anim = GetComponent<Animator>();
        OnValidate();
    }
    protected void Start()
    {
        startHealth = health;
        RespawnPoint = transform.position;
        OnValidate();
        actions = new PlayerAction();
        actions.Player.Enable();
    }
    protected void Update()
    {
        _playerInput = actions.Player.Move.ReadValue<Vector2>();
        _playerInput.z = actions.Player.UpDown.ReadValue<float>();
        _playerInput = Vector3.ClampMagnitude(_playerInput, 1f);
        _desiredJump = actions.Player.Jump.ReadValue<float>();
        _desiredRunning = actions.Player.Run.ReadValue<float>();
        if (playerInputSpace)
        {
            _rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, _upAxis);
            _forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, _upAxis);
        }
        else
        {
            _rightAxis = ProjectDirectionOnPlane(Vector3.right, _upAxis);
            _forwardAxis = ProjectDirectionOnPlane(Vector3.forward, _upAxis);
        }
        if (ONGround && _body.velocity.y < 0 || Swimming)
        {
            anim.SetBool("Jump", false);
        }
        if (actions.Player.Crouch.ReadValue<float>() > 0 && ONGround)
        {
            anim.SetBool("Crouch", true);
            isCrouching = true;
        }
        if (actions.Player.Crouch.ReadValue<float>() <= 0 || !ONGround)
        {
            isCrouching = false;
            anim.SetBool("Crouch", false);
        }
        if (Swimming)
        {
            _desiresClimbing = false;
        }
        if (damagedTimer > 0)
            damagedTimer -= Time.deltaTime;
    }

    protected void FixedUpdate()
    {
        var gravity = CustomGravity.GetGravity(_body.position, out _upAxis);
        UpdateState();
        if (InWater)
        {
            _velocity *= 1f - waterDrag * _submergence * Time.deltaTime;
        }
        if (Gamepad.all.Count > 0)
        {
            if ((_playerInput.x >= .7f || _playerInput.y >= .7f ||
                _playerInput.x <= -.7f || _playerInput.y <= -.7f) && !isCrouching)
            {
                curSpeed = maxRunSpeed;
                FixWalkingAnim(true);

                anim.speed = 1;
            }
            if (_playerInput.x < .7f && _playerInput.y < .7f &&
                _playerInput.x > -.7f && _playerInput.y > -.7f)
            {
                curSpeed = (isCrouching) ? maxCrouchSpeed : maxWalkSpeed;
                FixWalkingAnim(false);
            }
        }
        else
        {
            curSpeed = (_desiredRunning > 0) ? curSpeed = maxRunSpeed : curSpeed = maxWalkSpeed;
            if (_velocity.x != 0 || _velocity.z != 0)
            {
                if (_desiredRunning == 0)
                {
                    FixWalkingAnim(false);
                }
                else
                {
                    FixWalkingAnim(true);
                }
            }
        }
        AdjustVelocity();

        if (_desiredJump > 0.05 && !jumping)
        {
            anim.SetBool("Jump", true);
            Jump(gravity);
            jumping = true;
        }
        if (_desiredJump < .05)
        {

            jumping = false;
        }

        if (Climbing)
        {
            _velocity -= _contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
        }
        else if (InWater)
        {
            _velocity += gravity * ((1f - buoyancy * _submergence) * Time.deltaTime);
        }
        else if (ONGround && _velocity.sqrMagnitude < 0.01f)
        {
            _velocity +=
                _contactNormal *
                (Vector3.Dot(gravity, _contactNormal) * Time.deltaTime);
        }
        else if (_desiresClimbing && ONGround)
        {
            _velocity += (gravity - _contactNormal * (maxClimbAcceleration * 0.9f)) * Time.deltaTime;
        }
        else
        {
            _velocity += gravity * Time.deltaTime;
        }
        if (_velocity.x <= .05 && _velocity.z <= .05 && _velocity.z >= -.05 && _velocity.x >= -.05 || !ONGround)
        {
            anim.SetFloat("Velocity", 0);
            anim.speed = 1;
        }

        _body.velocity = _velocity;
        if (_playerInput.x > 0 || _playerInput.y > 0 || _playerInput.x < 0 || _playerInput.y < 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(_body.velocity.x, 0, _body.velocity.z)), Time.deltaTime * turnSpeed);
        }

        ClearState();
    }
    void FixWalkingAnim(bool isRunning)
    {
        if (isRunning)
        {
            anim.SetFloat("Velocity", 1f);
            if (animatorWalkSpeed < 1f)
                animatorWalkSpeed += animatorWalkAcceleration * Time.deltaTime;

            anim.speed = 1;

        }
        else
        {
            anim.SetFloat("Velocity", .5f);
            if (Mathf.Abs(_playerInput.x) > Mathf.Abs(_playerInput.y))
            {
                anim.speed = (isCrouching) ? Mathf.Abs(_playerInput.x * 1.5f) :Mathf.Abs(_playerInput.x);
            }
            else if (Mathf.Abs(_playerInput.x) < Mathf.Abs(_playerInput.y))
            {
                anim.speed = (isCrouching) ? Mathf.Abs(_playerInput.y * 1.5f) : Mathf.Abs(_playerInput.y);
            }
            if (animatorWalkSpeed < .5f)
                animatorWalkSpeed += animatorWalkAcceleration * Time.deltaTime;
            if (animatorWalkSpeed > .5f)
                animatorWalkSpeed = .5f;
        }
    }
    private void ClearState()
    {
        _groundContactCount = _steepContactCount = _climbContactCount = 0;
        _contactNormal = _steepNormal = _climbNormal = Vector3.zero;
        _connectionVelocity = Vector3.zero;
        _previousConnectedBody = _connectedBody;
        _connectedBody = null;
        _submergence = 0f;
    }
    private void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection = Vector3.up;
        var jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        if (ONGround)
        {
            jumpDirection = _contactNormal;
        }
        else if (ONSteep)
        {
            if (lastWallHit == null)
            {
                jumpDirection = _steepNormal;
            }
            else
            {
                if (lastWallHit.layer != 8)
                {
                    _velocity.y = 0;
                    jumpDirection = _steepNormal;
                    jumpSpeed = 2 * wallJumpHeight;
                }
                else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
                {

                    if (_jumpPhase == 0)
                    {
                        _jumpPhase = 1;
                    }
                    jumpDirection = Vector3.up;
                }
                else
                    return;
            }
            _jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
        {
            if (_jumpPhase == 0)
            {
                _jumpPhase = 1;
            }

            jumpDirection = _contactNormal;

        }
        else
        {
            return;
        }

        _stepsSinceLastJump = 0;
        _jumpPhase += 1;

        if (InWater)
        {
            jumpSpeed *= Mathf.Max(0f, 1f - _submergence / swimThreshold);
        }
        jumpDirection = (jumpDirection + _upAxis).normalized;
        var alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - _velocity.y, 0f);
        }

        _velocity += jumpDirection * jumpSpeed;
    }
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
    private void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;
        if (Climbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            xAxis = Vector3.Cross(_contactNormal, _upAxis);
            zAxis = _upAxis;
        }
        else if (InWater)
        {
            var swimFactor = Mathf.Min(1f, _submergence / swimThreshold);
            acceleration = Mathf.LerpUnclamped(ONGround ? maxAcceleration : maxAirAcceleration, maxSwimAcceleration, swimFactor);
            speed = Mathf.LerpUnclamped(curSpeed, maxSwimSpeed, swimFactor);
            xAxis = _rightAxis;
            zAxis = _forwardAxis;
        }
        else
        {
            acceleration = ONGround ? maxAcceleration : maxAirAcceleration;
            speed = ONGround && _desiresClimbing ? maxClimbSpeed : curSpeed;
            xAxis = _rightAxis;
            zAxis = _forwardAxis;
        }

        xAxis = ProjectDirectionOnPlane(xAxis, _contactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, _contactNormal);

        var relativeVelocity = _velocity - _connectionVelocity;
        var currentX = Vector3.Dot(relativeVelocity, xAxis);
        var currentZ = Vector3.Dot(relativeVelocity, zAxis);

        var maxSpeedChange = acceleration * Time.deltaTime;

        var newX = Mathf.MoveTowards(currentX, _playerInput.x * speed, maxSpeedChange);
        var newZ = Mathf.MoveTowards(currentZ, _playerInput.y * speed, maxSpeedChange);

        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        if (Swimming)
        {
            var currentY = Vector3.Dot(relativeVelocity, _upAxis);
            var newY = Mathf.MoveTowards(currentY, _playerInput.z * speed, maxSpeedChange);
            _velocity += _upAxis * (newY - currentY);
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
    private void UpdateState()
    {
        _stepsSinceLastGrounded += 1;
        _stepsSinceLastJump += 1;
        _velocity = _body.velocity;
        if (CheckClimbing() || CheckSwimming() || ONGround || SnapToGround() || CheckSteepContacts())
        {
            _stepsSinceLastGrounded = 0;

            if (_stepsSinceLastJump > 1)
            {
                _jumpPhase = 0;
            }

            if (_groundContactCount > 1)
            {
                _contactNormal.Normalize();
            }
        }
        else
        {
            _contactNormal = _upAxis;
        }

        if (_connectedBody)
        {
            if (_connectedBody.isKinematic || _connectedBody.mass >= _body.mass)
            {
                UpdateConnectionState();
            }
        }
    }
    private bool CheckClimbing()
    {
        if (Climbing)
        {
            if (_climbContactCount > 1)
            {
                _climbNormal.Normalize();
                var upDot = Vector3.Dot(_upAxis, _climbNormal);
                if (upDot >= _minGroundDotProduct)
                {
                    _climbNormal = _lastClimbNormal;
                }
            }
            _groundContactCount = 1;
            _contactNormal = _climbNormal;
            return true;
        }

        return false;
    }

    private bool CheckSwimming()
    {
        if (Swimming)
        {
            _groundContactCount = 0;
            _contactNormal = _upAxis;
            return true;
        }
        return false;
    }
    private void UpdateConnectionState()
    {
        if (_connectedBody == _previousConnectedBody)
        {
            var connectionMovement = _connectedBody.transform.TransformPoint(_connectionLocalPosition) -
                                     _connectionWorldPosition;
            _connectionVelocity = connectionMovement / Time.deltaTime;
        }

        _connectionWorldPosition = _body.position;
        _connectionLocalPosition = _connectedBody.transform.InverseTransformPoint(_connectionWorldPosition);
    }
    private bool SnapToGround()
    {
        if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2)
        {
            return false;
        }

        var speed = _velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(_body.position, -_upAxis, out var hit, probeDistance, probeMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        var upDot = Vector3.Dot(_upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        _groundContactCount = 1;
        _contactNormal = hit.normal;
        var dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f)
        {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        }

        _connectedBody = hit.rigidbody;
        return true;
    }
    private float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
    }
    protected void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    protected void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }
    protected void OnTriggerEnter(Collider col)
    {
        if ((waterMask & (1 << col.gameObject.layer)) != 0)
        {
            EvaluateSubmergence(col);
        }
    }

    protected void OnTriggerStay(Collider col)
    {
        if ((waterMask & (1 << col.gameObject.layer)) != 0)
        {
            EvaluateSubmergence(col);
        }
    }
    private void EvaluateSubmergence(Collider collision)
    {
        if (Physics.Raycast(_body.position + _upAxis * submergenceOffset, -_upAxis, out RaycastHit hit,
            submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide))
        {
            _submergence = 1f - hit.distance / submergenceRange;
        }
        else
        {
            _submergence = 1f;
        }
        if (Swimming)
        {
            _connectedBody = collision.attachedRigidbody;
        }
    }
    private void EvaluateCollision(Collision collision)
    {
        if (Swimming)
        {
            return;
        }
        var layer = collision.gameObject.layer;
        var minDot = GetMinDot(layer);
        for (var i = 0; i < collision.contactCount; i++)
        {
            var normal = collision.GetContact(i).normal;
            var upDot = Vector3.Dot(_upAxis, normal);

            if (upDot >= minDot)
            {
                lastWallHit = null;
                _groundContactCount += 1;
                _contactNormal += normal;
                _connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    lastWallHit = collision.gameObject;
                    _steepContactCount += 1;
                    _steepNormal += normal;
                    if (_groundContactCount == 0)
                    {
                        _connectedBody = collision.rigidbody;
                    }
                }

                if (_desiresClimbing && upDot >= _minClimbDotProduct && (climbMask & (1 << layer)) != 0)
                {
                    _climbContactCount += 1;
                    _climbNormal += normal;
                    _lastClimbNormal = normal;
                    _connectedBody = collision.rigidbody;
                }
            }
        }
    }
    private bool CheckSteepContacts()
    {
        if (_steepContactCount > 1)
        {
            _steepNormal.Normalize();
            var upDot = Vector3.Dot(_upAxis, _steepNormal);
            if (upDot >= _minGroundDotProduct)
            {
                _steepContactCount = 0;
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }

        return false;
    }

}
