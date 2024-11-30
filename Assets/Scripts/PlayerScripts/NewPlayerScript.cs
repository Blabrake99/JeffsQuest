using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.IMGUI.Controls.CapsuleBoundsHandle;

public class NewPlayerScript : MonoBehaviour
{
    [Header("Speed variables")]
    [SerializeField] float maxWalkSpeed = 5f;
    [SerializeField] float maxRunSpeed = 10f;
    [SerializeField] float maxCrouchSpeed = 3f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField, Range(0f, 10f), Tooltip("How high the player jumps")] float jumpHeight = 1.6f;
    [SerializeField] float longJumpHeight = 3, longJumpDistance = 3, longJumpKnockBackDistance = 7;
    [SerializeField, Range(0f, 100f), Tooltip("Acceleration for their respective names")] float maxAcceleration = 27f;
    [SerializeField, Range(0f, 100f), Tooltip("Acceleration for their respective names")]
    float animatorWalkAcceleration = .2f, animatorWalkdeceleration = .5f, crouchDeceleration = .5f, walkDeceleration = 20f, crouchTurnAcceleration = 3f, maxRunSpeedAcceleration = 4f;
    [SerializeField, Range(1f, 3), Tooltip("This affect the wall jump distance (It's multiplicative)")] float jumpSpeedMultiplyer = 1.1f;
    [SerializeField, Range(1, 3), Tooltip("deceleration for the player after long jumping the lower the number the longer he walks for")] float longJumpWalkDeceleration = 1.7f;
    [Header("For Rotation")]
    [SerializeField, Tooltip("The speed at which he rotates when he moves")] float turnSpeed = 5;
    [SerializeField, Range(.1f, 2f), Tooltip("How long you have to hold the jump button to do a full jump")] float fullJumpTime = .3f;
    [Header("Timers")]
    [SerializeField, Range(.1f, 5f)] float longJumpStunTimer = .5f, collectibleTimer = .5f;
    [SerializeField, Tooltip("The Transform of the camera")] Transform playerInputSpace = default;
    [SerializeField, Tooltip("The layermasks for their respective names")] LayerMask probeMask = -1, noneJumpableMask;
    protected Vector3 _playerInput;
    //protected Rigidbody _body;
    protected CharacterController _characterController;
    protected float curSpeed;
    protected int jumpPhase;
    public int _jumpPhase;
    protected bool jumping, isInteracting, isCrouching, isCrouchDeceleration, longJumping, LongJumpStunned, gotCollectible, jumpButtonUp;
    protected PlayerAction actions;
    protected Animator anim;
    protected GameObject lastWallHit;
    protected float animatorWalkSpeed, _jumpHoldTimer, _longJumpStunTimer;
    protected Vector3 _upAxis, _rightAxis, _forwardAxis;
    protected float _desiredJump, _desiredRunning;
    protected Vector3 _velocity;
    [HideInInspector] public bool ONGround;
    public Vector2 lastPlayerInput;
    void Awake()
    {
        //_body = GetComponent<Rigidbody>();
        _characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }
    protected void Start()
    {
        playerInputSpace = FindObjectOfType<CameraScript>().transform;
        actions = new PlayerAction();
        actions.Player.Enable();
        actions.Player.Interact.performed += OnInteract;
    }

    private void Update()
    {
        ONGround = _characterController.isGrounded;
        _playerInput = actions.Player.Move.ReadValue<Vector2>();
        _playerInput = Vector3.ClampMagnitude(_playerInput, 1f);
        _desiredRunning = actions.Player.Run.ReadValue<float>();
        _desiredJump = actions.Player.Jump.ReadValue<float>();
        //transform.eulerAngles = Vector3.up * Mathf.Atan2(_playerInput.x, _playerInput.y) * Mathf.Rad2Deg;
        if (ONGround && _velocity.y < 0)
        {
            anim.SetBool("Jump", false);
            _velocity.y = 0;
            jumping = false;
        }
        if (actions.Player.Crouch.ReadValue<float>() > 0 && ONGround)
        {
            anim.SetBool("Crouch", true);
            isCrouching = true;
        }
        if (actions.Player.Crouch.ReadValue<float>() <= 0)
        {
            isCrouching = false;
            anim.SetBool("Crouch", false);
            longJumping = false;
        }
        if (_desiredJump != 0 && _characterController.isGrounded)
        {
            lastPlayerInput = _playerInput;
            FixWalkingAnim(false);
        }
        Vector3 move = Vector3.zero;
        if (ONGround)
        {
            move = new Vector3(_playerInput.x, 0, _playerInput.y);
        }
        else
        {
            move = new Vector3(lastPlayerInput.x, 0, lastPlayerInput.y);
            _characterController.Move(transform.forward * Time.deltaTime * (curSpeed/1.5f));
            anim.SetFloat("Velocity", 0);
        }
        _characterController.Move(move * Time.deltaTime * curSpeed);
        //if player is using gamepad
        if (Gamepad.all.Count > 0)
        {
            if ((_playerInput.x >= .7f || _playerInput.y >= .7f ||
                _playerInput.x <= -.7f || _playerInput.y <= -.7f))
            {
                curSpeed = (isCrouching) ? Mathf.Lerp(curSpeed, maxCrouchSpeed, crouchDeceleration * Time.deltaTime) : Mathf.Lerp(curSpeed, maxRunSpeed, maxRunSpeedAcceleration * Time.deltaTime);
            }
            if (isCrouching && Mathf.Round(curSpeed) <= maxCrouchSpeed + 1 && ONGround)
            {
                longJumping = false;
                isCrouchDeceleration = false;
                FixWalkingAnim(false);
            }
            if (!isCrouching && ONGround)
            {
                longJumping = false;
                isCrouchDeceleration = false;
                FixWalkingAnim(true);
            }
        }
        else //player not using gamepad
        {
            if (!isCrouching)
            {
                if (move.x != 0 || move.z != 0)
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
                else if (!ONGround)
                {
                    anim.SetFloat("Velocity", 0);
                    anim.speed = 1;
                }
            }
            else
            {
                if (isCrouching && Mathf.Round(curSpeed) <= maxCrouchSpeed + 1 && ONGround)
                {
                    longJumping = false;
                    isCrouchDeceleration = false;
                    FixWalkingAnim(false);
                }
                if (!isCrouching && ONGround)
                {
                    longJumping = false;
                    isCrouchDeceleration = false;
                    FixWalkingAnim(true);
                }
                if (isCrouching && Mathf.Round(curSpeed) > maxCrouchSpeed + 1)
                {
                    isCrouchDeceleration = true;
                    //this is where we long jump
                    if (_desiredJump > 0 && ONGround && !longJumping && curSpeed < maxRunSpeed - 2)
                    {
                        longJumping = true;
                        isCrouchDeceleration = true;
                        anim.SetBool("Sliding", true);
                        Jump();
                        jumping = true;
                    }
                }
            }
            if (_playerInput != Vector3.zero)
            {
                if (!isCrouching)
                {
                    curSpeed = (_desiredRunning > 0) ? maxRunSpeed : maxWalkSpeed;
                }
                else
                {
                    curSpeed = (isCrouching) ? Mathf.Lerp(curSpeed, maxCrouchSpeed, crouchDeceleration * Time.deltaTime) : maxRunSpeed;
                }
            }

        }
        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }
        if (curSpeed > 0 && move == Vector3.zero)
        {
            _characterController.Move(transform.forward * Time.deltaTime * curSpeed);
            curSpeed -= walkDeceleration * Time.deltaTime;
        }
        if (ONGround && _desiredJump > 0 && !isCrouchDeceleration && !jumpButtonUp)
        {
            anim.SetBool("Jump", true);
            jumping = true;
            _jumpHoldTimer = fullJumpTime;
            //_velocity += transform.up * jumpHeight * Time.fixedDeltaTime;
            Jump();
        }
        if (ONGround && _desiredJump < 0.5f)
        {
            jumpButtonUp = false;
        }
        if (_desiredJump > 0 && jumping && !longJumping)
        {
            if (_jumpHoldTimer > 0)
            {
                //_velocity += transform.up * jumpHeight * Time.fixedDeltaTime;
                Jump();
                _jumpHoldTimer -= Time.deltaTime;
            }
            else
            {
                jumping = false;
                jumpButtonUp = true;
            }
        }
        if (_desiredJump < 0.05)
        {
            jumping = false;
                        anim.SetFloat("Velocity", 0);
        }
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
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
                anim.speed = (isCrouching) ? Mathf.Abs(_playerInput.x * 1.5f) : Mathf.Abs(_playerInput.x);
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
    void Jump()
    {
        if (!longJumping)
        {
            _velocity += transform.up * jumpHeight * Time.fixedDeltaTime;
        }
        else
        {
            //print(true);
            //_velocity += transform.up * longJumpHeight * Time.fixedDeltaTime;
            //_velocity += transform.forward * longJumpDistance * Time.fixedDeltaTime;
        }
    }
    public void GotCollectible()
    {
        gotCollectible = true;
        _playerInput = Vector3.zero;
        transform.LookAt(new Vector3(playerInputSpace.position.x, 0, playerInputSpace.position.z));
        playerInputSpace.GetComponent<CameraScript>().distance = 3;
        anim.SetBool("GotCollectible", true);
        StartCoroutine(CollectibleAnim());
    }
    IEnumerator CollectibleAnim()
    {
        yield return new WaitForSeconds(collectibleTimer);
        playerInputSpace.GetComponent<CameraScript>().distance = playerInputSpace.GetComponent<CameraScript>().startDistance;
        anim.SetBool("GotCollectible", false);
        gotCollectible = false;
    }
    #region Interact code
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
    #endregion
}
