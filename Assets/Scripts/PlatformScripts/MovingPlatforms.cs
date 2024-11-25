using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatforms : MonoBehaviour, IPlatforms
{
    [Header("Direction")]

    [SerializeField, Tooltip("If set true it'll go up and down, if false it'll move side to side. Also don't forget to add the parent player script only for up and down")]
    bool MoveY;

    [SerializeField, Tooltip("If up and down if false, this'll will be for if the platform will move straight and back or left and right")]
    bool MoveX;

    [SerializeField, Tooltip("If up and down if false, this'll will be for if the platform will move straight and back or left and right")]
    bool MoveZ;

    [SerializeField, Tooltip("If you want it to go up and down and what ever you put for left and right")]
    bool Multiple;


    [SerializeField, Tooltip("The amount of time the platforms stops for when the platform hits the start of end point")]
    float MaxStopTimer;

    [Header("Distance")]

    [SerializeField, Range(-40f, 40f), Tooltip("The amount the platform will move up and down")]
    float DistanceToMoveY;

    [SerializeField, Range(-40f, 40f), Tooltip("The amount the platform will move left nad right or striaght and back")]
    float DistanceToMoveX;

    [SerializeField, Range(-40f, 40f), Tooltip("The amount the platform will move left nad right or striaght and back")]
    float DistanceToMoveZ;

    [Space(10)]

    [SerializeField, Range(1f, 10f), Tooltip("The speed at which the platform moves")]
    float speed = 2;

    [Tooltip("If you want it to activate when step on")]
    public bool StepOnToActivate;

    [SerializeField, Tooltip("How long it takes to start the platform")]
    float startTimer = 0f;

    [Tooltip("If this is true the moving platforms active and moving")]
    public bool active = true;

    private float EndPointY, EndPointX, EndPointZ;

    private Vector3 StartPoint;

    private bool GoBackY, GoBackX, GoBackZ, GoBackToStart;

    [HideInInspector]
    public bool isSteppedOn;

    Vector3 lastPosition, lastMove;
    float stopTimer;
    float timer;
    bool once;
    List<Rigidbody> rigidbodies = new List<Rigidbody>();
    Vector3 lastPos;
    Transform _transform;
    public void Activate()
    {
        active = true;
    }
    public void DeActivate()
    {
        active = false;
    }
    #region MonoBehaviour functions
    void Start()
    {
        _transform = transform;
        StartPoint = _transform.position;
        lastPos = _transform.position;
        timer = startTimer;
        if (MoveY)
            EndPointY = StartPoint.y + DistanceToMoveY;

        if (MoveZ)
            EndPointZ = StartPoint.z + DistanceToMoveZ;

        if (MoveX)
            EndPointX = StartPoint.x + DistanceToMoveX;
    }

    void Update()
    {
        if (active)
        {
            if(startTimer > 0f)
            {
                startTimer -= Time.fixedDeltaTime;
                return;
            }
            if (!StepOnToActivate && !GoBackToStart)
            {
                Move();
            }
            if (StepOnToActivate && !GoBackToStart)
            {
                if (isSteppedOn)
                {
                    Move();
                }
                else
                {
                    resetPosition();
                }
            }
            if (GoBackToStart)
            {
                resetPosition();
            }

            if (rigidbodies.Count > 0)
            {
                for (int i = 0; i < rigidbodies.Count; i++)
                {
                    if (rigidbodies[i] != null)
                    {
                        Rigidbody rb = rigidbodies[i];
                        Vector3 vel = new Vector3((_transform.position.x - lastPos.x) + ((rb.velocity.x * Time.deltaTime)),
                                                  (_transform.position.y - lastPos.y) + ((rb.velocity.y * Time.deltaTime) / 2),
                                                  (_transform.position.z - lastPos.z) + ((rb.velocity.z * Time.deltaTime)));
                        rb.transform.Translate(vel, transform);
                    }
                }
            }
            lastPos = _transform.position;
        }
    }
    #endregion
    #region Movement functions
    private void Move()
    {
        if (MoveY)
        {
            UpAndDown();
            if (Multiple)
            {
                if (MoveZ)
                {
                    FrontToBack();
                }
                if (MoveX)
                {
                    LeftToRight();
                }
            }

        }
        else
        {
            if (MoveZ)
            {
                FrontToBack();
            }
            if (MoveX)
            {
                LeftToRight();
            }
        }
        stopTimer -= Time.deltaTime;
    }
    void LeftToRight()
    {
        if (EndPointX > StartPoint.x)
        {
            if (GoBackX)
            {

                if (_transform.position.x <= StartPoint.x)
                {
                    GoBackX = false;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                        _transform.Translate(new Vector3(-1 * Time.deltaTime * speed, 0, 0), Space.World);
                }
            }
            else
            {

                if (_transform.position.x >= EndPointX)
                {
                    GoBackX = true;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                        _transform.Translate(new Vector3(1 * Time.deltaTime * speed, 0, 0), Space.World);
                }
            }
        }
        else
        {
            if (GoBackX)
            {

                if (_transform.position.x >= StartPoint.x)
                {
                    GoBackX = false;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                        _transform.Translate(new Vector3(1 * Time.deltaTime * speed, 0, 0), Space.World);
                }
            }
            else
            {

                if (_transform.position.x <= EndPointX)
                {
                    GoBackX = true;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                        _transform.Translate(new Vector3(-1 * Time.deltaTime * speed, 0, 0), Space.World);
                }
            }
        }
    }
    void FrontToBack()
    {
        if (EndPointZ > StartPoint.z)
        {
            if (GoBackZ)
            {

                if (_transform.position.z <= StartPoint.z)
                {
                    GoBackZ = false;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                        _transform.Translate(new Vector3(0, 0, -1 * Time.deltaTime * speed), Space.World);
                }
            }
            else
            {
                if (_transform.position.z >= EndPointZ)
                {
                    GoBackZ = true;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, 0, 1 * Time.deltaTime * speed), Space.World);
                        once = false;
                    }
                }
            }
        }
        else
        {
            if (GoBackZ)
            {

                if (_transform.position.z >= StartPoint.z)
                {
                    GoBackZ = false;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, 0, 1 * Time.deltaTime * speed), Space.World);
                        once = false;
                    }
                }
            }
            else
            {
                if (_transform.position.z <= EndPointZ)
                {
                    GoBackZ = true;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, 0, -1 * Time.deltaTime * speed), Space.World);
                        once = false;
                    }
                }
            }
        }
    }
    void UpAndDown()
    {
        if (EndPointY > StartPoint.y)
        {
            if (GoBackY)
            {
                if (_transform.position.y <= StartPoint.y)
                {
                    GoBackY = false;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, -1 * Time.deltaTime * speed, 0), Space.World);
                        once = false;
                    }
                }
            }
            else
            {
                if (_transform.position.y >= EndPointY)
                {
                    GoBackY = true;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, 1 * Time.deltaTime * speed, 0), Space.World);
                        once = false;
                    }
                }
            }
        }
        else
        {
            if (GoBackY)
            {
                if (_transform.position.y >= StartPoint.y)
                {
                    GoBackY = false;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, 1 * Time.deltaTime * speed, 0), Space.World);
                        once = false;
                    }
                }
            }
            else
            {
                if (_transform.position.y <= EndPointY)
                {
                    GoBackY = true;
                    if (!once && stopTimer <= 0)
                    {
                        once = true;
                        stopTimer = MaxStopTimer;
                    }
                }
                else
                {
                    if (stopTimer <= 0)
                    {
                        _transform.Translate(new Vector3(0, -1 * Time.deltaTime * speed, 0), Space.World);
                        once = false;
                    }
                }
            }
        }
    }
    void resetPosition()
    {
        _transform.position = Vector3.MoveTowards(_transform.position, StartPoint, speed * Time.deltaTime);
        if (Vector3.Distance(_transform.position, StartPoint) < .1)
        {
            GoBackToStart = false;
        }
    }
    #endregion
    #region collision functions
    private void OnCollisionEnter(Collision col)
    {
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null && !rigidbodies.Contains(rb))
        {
            Add(rb);
        }
    }
    private void OnCollisionStay(Collision col)
    {
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Add(rb);
        }
    }
    private void OnCollisionExit(Collision col)
    {
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Remove(rb);
        }
    }
    void Add(Rigidbody rb)
    {
        if (!rigidbodies.Contains(rb))
            rigidbodies.Add(rb);
    }
    void Remove(Rigidbody rb)
    {
        if (rigidbodies.Contains(rb))
            rigidbodies.Remove(rb);
    }
    #endregion
}
