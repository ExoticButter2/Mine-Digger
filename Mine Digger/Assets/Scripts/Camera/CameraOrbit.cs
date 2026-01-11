using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    public Transform targetTransform;
    private Transform _cameraTransform;

    public InputActionReference zoomInputAction;
    public InputActionReference moveInputAction;
    public InputActionReference rotateInputAction;
    public InputActionReference mouseMoveInputAction;

    private float _cameraZoomFromTarget;

    public float rotationLerpSpeed = 10f;
    public float positionLerpSpeed = 10f;

    public float zoomSensitivity = 0.5f;
    public float mouseSensitivity = 1f;

    private bool _rotatingCamera;

    private float _yaw = 0f;
    private float _pitch = 0f;

    private Vector3 _moveVector;
    private Vector3 _movementInputVector;

    [SerializeField]
    private float maxZoom = 10f;

    [SerializeField]
    private float _targetMoveSpeed = 5f;

    [SerializeField]
    private float climbableHeight;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_movementInputVector != Vector3.zero)
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f); // cap at 50ms (20 FPS)
            _moveVector = GetFlatRightVector() * _movementInputVector.x + GetFlatForwardVector() * _movementInputVector.z;
            Debug.Log($"Move vector: {_moveVector}");
            MoveTarget(_moveVector * _targetMoveSpeed * dt);//move target based on input
        }
    }

    private void LateUpdate()
    {
        UpdateCameraTransformToTarget();
    }

    private void UpdateCameraTransformToTarget()
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("Target transform not found");
            return;
        }

        if (_cameraTransform == null)
        {
            Debug.LogWarning("Camera transform not found");
            return;
        }

        Quaternion targetRotation = targetTransform.rotation; //* Quaternion.Euler(0f, 180f, 0f);
        Vector3 targetPosition = targetTransform.position - targetTransform.forward * _cameraZoomFromTarget;
        Vector3 targetToCameraDir = (_cameraTransform.position - targetTransform.position).normalized;

        _cameraTransform.rotation = Quaternion.Lerp(_cameraTransform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);//apply rotation

        if (Physics.Raycast(targetTransform.position, targetToCameraDir, out RaycastHit hit, Vector3.Distance(targetTransform.position, _cameraTransform.position)))//if clipping through wall
        {
            float safeDistance = hit.distance - 0.1f;
            safeDistance = Mathf.Max(safeDistance, 0.5f); //minimum distance
            targetPosition = hit.point - (targetToCameraDir * safeDistance);
            _cameraZoomFromTarget = Vector3.Distance(targetTransform.position, targetTransform.position) - 0.1f;
            _cameraTransform.position = targetPosition;//apply position without smoothing
            return;
        }

        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
    }

    private void RotateTarget(Vector2 rotation)
    {
        _yaw += rotation.x;
        _pitch -= rotation.y;
        _pitch = Mathf.Clamp(_pitch, -85f, 85f);
        Quaternion deltaRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        targetTransform.rotation = deltaRotation;
    }

    private void MoveTarget(Vector3 positionVectorToAdd)
    {
        Vector3 targetPosition = targetTransform.position + positionVectorToAdd;

        targetTransform.position = Vector3.Lerp(targetTransform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
    }

    #region ActionMeth

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        float zoomDirection = ctx.ReadValue<float>();
        _cameraZoomFromTarget -= zoomDirection * zoomSensitivity;

        _cameraZoomFromTarget = Mathf.Clamp(_cameraZoomFromTarget, 0f, maxZoom);
        Debug.Log($"Zoom: {_cameraZoomFromTarget}");
    }

    private Vector3 GetFlatForwardVector()
    {
        Vector3 flatForward = _cameraTransform.forward;
        flatForward.y = 0;
        flatForward.Normalize();
        return flatForward;
    }

    private Vector3 GetFlatRightVector()
    {
        Vector3 flatRight = _cameraTransform.right;
        flatRight.y = 0;
        flatRight.Normalize();
        return flatRight;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 movementInput = ctx.ReadValue<Vector2>();
        
        Vector3 movementVector = new Vector3(movementInput.x, 0f, movementInput.y);
        _movementInputVector = movementVector;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _movementInputVector = Vector3.zero;
    }

    private void OnRotate(InputAction.CallbackContext ctx)
    {
        _rotatingCamera = true;
    }

    private void OnStopRotate(InputAction.CallbackContext ctx)
    {
        _rotatingCamera = false;
    }

    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        if (!_rotatingCamera)
        {
            return;
        }

        Vector2 mouseInput = ctx.ReadValue<Vector2>();
        RotateTarget(mouseInput * mouseSensitivity);
    }

    #endregion

    private void OnEnable()
    {
        zoomInputAction.action.performed += OnZoom;
        moveInputAction.action.performed += OnMove;
        moveInputAction.action.canceled += OnMoveCanceled;
        rotateInputAction.action.started += OnRotate;
        rotateInputAction.action.canceled += OnStopRotate;
        mouseMoveInputAction.action.performed += OnMouseMove;
    }

    private void OnDisable()
    {
        zoomInputAction.action.performed -= OnZoom;
        moveInputAction.action.performed -= OnMove;
        moveInputAction.action.canceled -= OnMoveCanceled;
        rotateInputAction.action.started -= OnRotate;
        rotateInputAction.action.canceled -= OnStopRotate;
        mouseMoveInputAction.action.performed -= OnMouseMove;
    }
}