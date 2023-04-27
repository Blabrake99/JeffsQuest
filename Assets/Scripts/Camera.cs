using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Camera : MonoBehaviour
{
    [SerializeField] float cameraSpeed = 6;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] Transform cam;
    float turnSmoothVel;
    PlayerAction actions;
    private void Start()
    {
        actions = new PlayerAction();
        actions.Player.Enable();
    }
    void Update()
    {
        Vector2 movement = actions.Player.Camera.ReadValue<Vector2>();
        Vector3 dir = new Vector3(movement.x, 0f, movement.y).normalized;
        if(dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel,turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f,angle,0f);

            Vector3 movedir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
    }
}
