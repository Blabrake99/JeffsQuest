using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Camera : MonoBehaviour
{
    PlayerAction actions;
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector2 _framing = new Vector2(0, 0);

    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _defaultDistance = 5f;
    [SerializeField] private float _minDistance = 0f;
    [SerializeField] private float _maxDistance = 10f;

    [SerializeField] private bool _invertX = false;
    [SerializeField] private bool _invertY = false;
    [SerializeField] private float _RotationSharpness = 25f;
    [SerializeField] private float _defaultVerticalAngle = 20f;
    [SerializeField, Range(-90, 90)] private float _minVerticalAngle = -90;
    [SerializeField, Range(-90, 90)] private float _maxVerticalAngle = 90;

    [SerializeField] private float _checkRadius = .2f;
    [SerializeField] private LayerMask _obstructionLayers;
    private List<Collider> _ignoreColliders = new List<Collider>();
    private Vector3 _planarDirection; // cameras forward on the x,z plane
    private Quaternion _targetRotation;
    private float _targetDistance;
    private Vector3 _targetPosition;
    private float _targetVerticleAngle;

    private Vector3 _newPosition;
    private Quaternion _newRotation;
    private void OnValidate()
    {
        _defaultDistance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);
        _defaultVerticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
    }
    private void Start()
    {
        _ignoreColliders.AddRange(GetComponentsInChildren<Collider>());
        _planarDirection = _target.forward;
        _targetDistance = _defaultDistance;
        _targetVerticleAngle = _defaultVerticalAngle;
        _targetRotation = Quaternion.LookRotation(_planarDirection) * Quaternion.Euler(_targetVerticleAngle, 0, 0);
        _targetPosition = _target.position - (_targetRotation * Vector3.forward) * _targetDistance;
        GameManager.lockCursor();
        actions = new PlayerAction();
        actions.Player.Enable();
    }
    void Update()
    {
        Vector2 movement = actions.Player.Camera.ReadValue<Vector2>();
        float zoom = actions.Player.CameraScroll.ReadValue<float>() * _zoomSpeed;
        if (Cursor.lockState != CursorLockMode.Locked)
            return;
        float mouseX = movement.x;
        float mouseY = movement.y;
        if (_invertX) { mouseX *= -1f; }
        if (_invertY) { mouseY *= -1f; }

        Vector3 _focusPosition = _target.position + new Vector3(_framing.x, _framing.y, 0);

        _planarDirection = Quaternion.Euler(0, mouseX, 0) * _planarDirection;
        _targetDistance = Mathf.Clamp(_targetDistance + zoom, _minDistance, _maxDistance);
        _targetVerticleAngle = Mathf.Clamp(_targetVerticleAngle + mouseY, _minVerticalAngle,_maxVerticalAngle);

        float _smallestDistance = _targetDistance;
        RaycastHit[] _hits = Physics.SphereCastAll(_focusPosition, _checkRadius, _targetRotation * -Vector3.forward, _targetDistance, _obstructionLayers);
        if (_hits.Length != 0)
            foreach (RaycastHit hit in _hits)
                if (!_ignoreColliders.Contains(hit.collider))
                    if (hit.distance < _smallestDistance)
                        _smallestDistance = hit.distance;


        _targetRotation = Quaternion.LookRotation(_planarDirection) * Quaternion.Euler(_targetVerticleAngle,0,0);
        _targetPosition = _focusPosition - (_targetRotation * Vector3.forward) * _smallestDistance;

        _newRotation = Quaternion.Slerp(_camera.transform.rotation, _targetRotation, Time.deltaTime * _RotationSharpness);
        _newPosition = Vector3.Lerp(_camera.transform.position,_targetPosition,Time.deltaTime * _RotationSharpness);
        _camera.transform.rotation = _newRotation;
        _camera.transform.position = _newPosition;
    }
}
