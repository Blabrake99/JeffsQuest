using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    PlayerAction actions;
    [SerializeField, Tooltip("The object the camera is focusing on")] private Transform focus;
    [SerializeField, Range(1f, 20f), Tooltip("how far the camera's away from the object")] float distance = 5f;
    [SerializeField, Tooltip("The zoom speed of the camera")] private float zoomSpeed = 10f;
    [SerializeField, Min(0f)] float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] float focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f), Tooltip("The speed at which the camera can rotate")] float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f), Tooltip("How far the camera can go up and down")] float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [SerializeField, Min(0f)] float alignDelay = 5f;
    [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
    [SerializeField, Tooltip("The layers that collide with the camera")] LayerMask obstructionMask = -1;
    [SerializeField, Tooltip("How the fast the camera accelerates or decelerates")] float cameraAcceleration = .5f, cameraDeceleration = .5f;
    public float speed;
    Camera regularCamera;
    Vector2 orbitAngles = new Vector2(45f, 0f);
    Vector3 focusPoint, previousFocusPoint;
    float lastManualRotationTime;
    Quaternion orbitRotation;
    Quaternion gravityAlignment = Quaternion.identity;
    Vector2 input;
    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }
    private void Start()
    {
        GameManager.lockCursor();
        actions = new PlayerAction();
        regularCamera = GetComponent<Camera>();
        actions.Player.Enable();
        focusPoint = focus.position;
        transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
    }
    void LateUpdate()
    {
        gravityAlignment = Quaternion.FromToRotation(gravityAlignment * Vector3.up,
            CustomGravity.GetUpAxis(focusPoint)) * gravityAlignment;
        UpdateFocusPoint();
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            orbitRotation = Quaternion.Euler(orbitAngles);
        }
        if(input.x > 0f || input.x < 0 ||
            input.y > 0f || input.y < 0)
        {
            speed = Mathf.Lerp(speed, rotationSpeed, Time.deltaTime * cameraAcceleration);
        }
        if(input.x == 0 && input.y == 0 && speed > 0.01)
        {
            speed = Mathf.Lerp(speed, 0, Time.deltaTime * cameraDeceleration);
        }
        Quaternion lookRotation = gravityAlignment * orbitRotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
            lookRotation, castDistance, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
    bool ManualRotation()
    {
        input = new Vector2(actions.Player.Camera.ReadValue<Vector2>().y,
            actions.Player.Camera.ReadValue<Vector2>().x);
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += speed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                regularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }
    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }
        Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);
        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f)
        {
            return false;
        }
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = speed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }
    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }
    void ConstrainAngles()
    {
        orbitAngles.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }
    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }
}
