using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointPlatforms : MonoBehaviour, IPlatforms
{
    [SerializeField, Tooltip("The waypoints that the platform fallows")]
    GameObject[] WayPoints;

    [SerializeField, Tooltip("The different ways the platform loops")]
    WayToLoop wayToLoop;

    [SerializeField, Tooltip("The Speed of the platform"), Range(1,15)]
    float speed = 2;
    [Tooltip("If this is true the moving platforms active and moving")]
    public bool active = true;

    [SerializeField, Tooltip("If you want it to activate when step on")]
    bool StepOnToActivate;

    [SerializeField, Tooltip("How long it takes to start the platform")]
    float startTimer = 0f;

    GameObject[] WaypointsToGoTo;
    GameObject NextPlatform;
    int ArrayIndex = 0;
    [HideInInspector]
    public bool isSteppedOn;
    bool AtDestination = true;
    float timer;
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
    void Start()
    {
        _transform = transform;
        GoToFirstWayPoint();
        timer = startTimer;
    }
    void Update()
    {
        if (active)
        {
            if (startTimer > 0f)
            {
                startTimer -= Time.fixedDeltaTime;
                return;
            }
            Move();

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
    private void Move()
    {
        if (!AtDestination)
        {
            if (Vector3.Distance(transform.position, NextPlatform.transform.position) > .1f)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, NextPlatform.transform.position, step);
            }
            else
            {
                AtDestination = true;
            }
        }
        else
        {
            if (ArrayIndex != WaypointsToGoTo.Length)
            {
                NextPlatform = WaypointsToGoTo[ArrayIndex];
            }
            else
            {
                if (wayToLoop == WayToLoop.GoBackward)
                {
                    GoBackward();
                }
                if (wayToLoop == WayToLoop.GoToFirstWaypoint)
                {
                    GoToFirstWayPoint();
                }
                NextPlatform = WaypointsToGoTo[0];
                ArrayIndex = 0;
            }
            ArrayIndex++;
            AtDestination = false;
        }
    }
    void GoBackward()
    {
        System.Array.Reverse(WaypointsToGoTo);
    }
    void GoToFirstWayPoint()
    {
        WaypointsToGoTo = WayPoints;
    }
    enum WayToLoop
    {
        GoBackward,
        GoToFirstWaypoint
    }
    #region collision functions
    private void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.tag == "Player" && StepOnToActivate
            && col.transform.position.y > transform.position.y)
        {
            active = true;
        }
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Add(rb);
        }
    }
    private void OnCollisionStay(Collision col)
    {
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null && !rigidbodies.Contains(rb))
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
