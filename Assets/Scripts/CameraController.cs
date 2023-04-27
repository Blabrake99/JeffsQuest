using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    [Header("Camera Properties")]
    private float DistanceAway;                     //how far the camera is from the player.

    [SerializeField, Tooltip("min camera distance")] 
    float minDistance = 1;
    [SerializeField, Tooltip("max camera distance")] 
    float maxDistance = 2;

    [SerializeField, Tooltip("how high the camera is above the player")] 
    float DistanceUp = -2;
    [SerializeField, Tooltip("how smooth the camera moves into place")] 
    float smooth = 4.0f;
    [SerializeField, Tooltip("the angle at which you will rotate the camera (on an axis)")]
    float rotateAround = 70f;

    [Header("Player to follow")]
    public Transform target;

    [Header("Layer(s) to include")]
    [SerializeField, Tooltip("the layers that will be affected by collision")] 
    LayerMask CamOcclusion;

    RaycastHit hit;
    float cameraHeight = 55f;
    float cameraPan = 0f;
    float camRotateSpeed = 180f;
    Vector3 camPosition;
    Vector3 camMask;

    private float HorizontalAxis;
    private float VerticalAxis;
    PlayerAction actions;


    // Use this for initialization
    void Start()
    {
        //the statement below automatically positions the camera behind the target.
        rotateAround = target.eulerAngles.y - 45f;
        actions = new PlayerAction();
        actions.Player.Enable();

    }

    void LateUpdate()
    {
        Vector2 movement = actions.Player.Camera.ReadValue<Vector2>();
        HorizontalAxis = movement.x;
        VerticalAxis = movement.y;

        //Offset of the targets transform (Since the pivot point is usually at the feet).
        Vector3 targetOffset = new Vector3(target.position.x, (target.position.y + 2f), target.position.z);
        Quaternion rotation = Quaternion.Euler(cameraHeight, rotateAround, cameraPan);
        Vector3 vectorMask = Vector3.one;
        Vector3 rotateVector = rotation * vectorMask;
        //this determines where both the camera and it's mask will be.
        //the camMask is for forcing the camera to push away from walls.
        camPosition = targetOffset + Vector3.up * DistanceUp - rotateVector * DistanceAway;
        camMask = targetOffset + Vector3.up * DistanceUp - rotateVector * DistanceAway;

        occludeRay(ref targetOffset);
        smoothCamMethod();

        transform.LookAt(target);

        #region wrap the cam orbit rotation
        if (rotateAround > 360)
        {
            rotateAround = 0f;
        }
        else if (rotateAround < 0f)
        {
            rotateAround = (rotateAround + 360f);
        }
        #endregion

        rotateAround += HorizontalAxis * camRotateSpeed * Time.deltaTime;
        DistanceAway = Mathf.Clamp(DistanceAway += VerticalAxis, minDistance, maxDistance);

    }
    void smoothCamMethod()
    {

        smooth = 4f;
        transform.position = Vector3.Lerp(transform.position, camPosition, Time.deltaTime * smooth);
    }
    void occludeRay(ref Vector3 targetFollow)
    {
        #region prevent wall clipping
        //declare a new raycast hit.
        RaycastHit wallHit = new RaycastHit();
        //linecast from your player (targetFollow) to your cameras mask (camMask) to find collisions.
        if (Physics.Linecast(targetFollow, camMask, out wallHit, CamOcclusion))
        {
            //the smooth is increased so you detect geometry collisions faster.
            smooth = 10f;
            //the x and z coordinates are pushed away from the wall by hit.normal.
            //the y coordinate stays the same.
            camPosition = new Vector3(wallHit.point.x + wallHit.normal.x * 0.5f, camPosition.y, wallHit.point.z + wallHit.normal.z * 0.5f);
        }
        #endregion
    }

}