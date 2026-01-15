using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class CameraOrbit : MonoBehaviour
{
    public Transform targetTransform;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private Transform _targetDownRaycasterTransform;

    public InputActionReference zoomInputAction;
    public InputActionReference moveInputAction;
    public InputActionReference rotateInputAction;
    public InputActionReference mouseMoveInputAction;
    public InputActionReference targetHeightInputAction;

    private float _cameraZoomFromTarget;
    private float _collisionZoom = 0f;
    private bool _cameraColliding = false;
    private Vector3 _camVelocity = Vector3.zero;

    public float rotationLerpSpeed = 10f;
    public float positionLerpSpeed = 10f;

    public float zoomSensitivity = 0.5f;
    public float mouseSensitivity = 1f;

    private bool _rotatingCamera;

    private float _yaw = 0f;
    private float _pitch = 0f;

    private Vector3 _moveVector;
    private Vector3 _movementInputVector;

    private float _targetHeight = 1f;
    private Vector3 _targetVelocity = Vector3.zero;
    [SerializeField]
    private float _maxTargetHeight = 10f;
    public float targetHeightChangeMultiplier;
    private bool _changingHeight;
    private float _heightChangeAmount;
    [SerializeField]
    private float _targetSlideCastRadius = 3f;
    [SerializeField]
    private float _targetSlideCastHeight = 3f;

    [SerializeField]
    private Vector3 _raycasterOffsetVector;

    [SerializeField]
    private float _maxZoom = 10f;

    [SerializeField]
    private float _targetMoveSpeed = 5f;

    [SerializeField]
    private float _targetSlideSensitivity = 0.2f;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.05f); // cap at 50ms (20 FPS)
        _moveVector = GetFlatRightVector() * _movementInputVector.x + GetFlatForwardVector() * _movementInputVector.z;
        Debug.Log($"Move vector: {_moveVector}");

        _targetDownRaycasterTransform.position = targetTransform.position + _raycasterOffsetVector;

        MoveTarget(_moveVector * _targetMoveSpeed * dt);//move target based on input

        if (_changingHeight)
        {
            AddHeightToTarget(_heightChangeAmount * Time.deltaTime);
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

        Vector3 targetPosition = Vector3.zero;

        Quaternion targetRotation = targetTransform.rotation;

        Vector3 targetToCameraDir = (_cameraTransform.position - targetTransform.position).normalized;

        _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);//apply rotation

        if (Physics.Raycast(targetTransform.position, targetToCameraDir, out RaycastHit hit, Vector3.Distance(targetTransform.position, _cameraTransform.position) + 0.3f))//if clipping through wall
        {
            _cameraColliding = true;
            Vector3 safeHitPoint = hit.point + _cameraTransform.forward * 0.1f;
            _collisionZoom = Vector3.Distance(safeHitPoint, targetTransform.position);//set temp collision zoom to distance from target

            Debug.Log($"Safe hit point for camera: {safeHitPoint}, Collision zoom: {_collisionZoom}");
            //targetPosition = hit.point - (targetToCameraDir * 1f);
            //_collisionZoom = Vector3.Distance(_cameraTransform.position, targetTransform.position);
            //_cameraTransform.position = targetPosition;//apply position without smoothing
            //return;
        }

        if (_cameraColliding)//check if camera colliding before reset
        {
            targetPosition = targetTransform.position - targetTransform.forward * _collisionZoom;//use collision zoom
        }
        else
        {
            targetPosition = targetTransform.position - targetTransform.forward * _cameraZoomFromTarget;//use regular camera zoom
        }

        _cameraColliding = false;

        _cameraTransform.position = Vector3.SmoothDamp(_cameraTransform.position, targetPosition, ref _camVelocity, positionLerpSpeed);
    }

    private void RotateTarget(Vector2 rotation)
    {
        _yaw += rotation.x;
        _pitch -= rotation.y;
        _pitch = Mathf.Clamp(_pitch, -85f, 85f);
        Quaternion deltaRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        targetTransform.rotation = deltaRotation;
    }

    private float _lastHeight = 0f;

    private void MoveTarget(Vector3 positionVectorToAdd)
    {
        float groundY = 0f;

        if (Physics.Raycast(targetTransform.position, Vector3.down, out RaycastHit groundHit))
        {
            groundY = groundHit.point.y;
        }

        Vector3 dir = _moveVector.normalized;
        Vector3 delta = Vector3.zero;

        Vector3 p1 = targetTransform.position + Vector3.up * (_targetSlideCastHeight * 0.5f - _targetSlideCastRadius);
        Vector3 p2 = targetTransform.position + Vector3.up * (_targetSlideCastRadius - _targetSlideCastHeight * 0.5f);

        if (Physics.CapsuleCast(p1, p2, _targetSlideCastRadius, dir, out RaycastHit secondHit, _moveVector.magnitude))
        {
            float dot = Vector3.Dot(dir.normalized, secondHit.normal);

            float angleSpeedMultiplier = Mathf.Clamp01(1f - dot);

            Vector3 angleSlowedMove = dir.normalized * (_moveVector.magnitude * angleSpeedMultiplier * _targetSlideSensitivity);
            Vector3 slideVector = Vector3.ProjectOnPlane(angleSlowedMove, secondHit.normal) * angleSpeedMultiplier;
            Vector3 newPositionVector = new Vector3(targetTransform.position.x + slideVector.x, groundY + _targetHeight, targetTransform.position.z + slideVector.z);

            targetTransform.position = newPositionVector;
            Debug.Log("Sliding gameobject across collider by second raycast");

            _lastHeight = targetTransform.position.y + _targetHeight;
            return;
        }

        if (Physics.Raycast(_targetDownRaycasterTransform.position, Vector3.down, out RaycastHit hit))
        {
            Vector3 planeMoveVector = Vector3.ProjectOnPlane(positionVectorToAdd, hit.normal);
            Vector3 targetPos = new Vector3(targetTransform.position.x, hit.point.y + _targetHeight, targetTransform.position.z) + positionVectorToAdd + planeMoveVector;
            targetTransform.position = Vector3.SmoothDamp(targetTransform.position, targetPos, ref _targetVelocity, positionLerpSpeed);
            return;
        }

        if (Physics.Raycast(targetTransform.position + dir * _moveVector.magnitude, Vector3.down, out RaycastHit thirdHit))
        {
            Vector3 planeMoveVector = Vector3.ProjectOnPlane(positionVectorToAdd, thirdHit.normal);
            Vector3 targetPos = new Vector3(targetTransform.position.x, thirdHit.point.y + _targetHeight, targetTransform.position.z) + positionVectorToAdd + planeMoveVector;
            targetTransform.position = Vector3.SmoothDamp(targetTransform.position, targetPos, ref _targetVelocity, positionLerpSpeed);
            Debug.Log("Moving gameobject by third raycast");
            return;
        }

        Debug.LogWarning("No target raycaster hit detected");
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

    #region ActionMeth

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        float zoomDirection = ctx.ReadValue<float>();
        _cameraZoomFromTarget -= zoomDirection * zoomSensitivity;

        _cameraZoomFromTarget = Mathf.Clamp(_cameraZoomFromTarget, 0.1f, _maxZoom);
        Debug.Log($"Zoom: {_cameraZoomFromTarget}");
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

    private void OnHeightChangeStart(InputAction.CallbackContext ctx)
    {
        _changingHeight = true;

        float input = ctx.ReadValue<float>();
        _heightChangeAmount = input * targetHeightChangeMultiplier;
        Debug.Log("Started height change");
    }

    private void OnHeightChangeEnd(InputAction.CallbackContext ctx)
    {
        _changingHeight = false;

        _heightChangeAmount = 0;
        Debug.Log("Stopped height change");
    }

    private void AddHeightToTarget(float amount)
    {
        if (Physics.Raycast(_cameraTransform.position, Vector3.up, amount))
        {
            return;
        }

        _targetHeight += amount;
        _targetHeight = Mathf.Clamp(_targetHeight, 0f, _maxTargetHeight);
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
        targetHeightInputAction.action.started += OnHeightChangeStart;
        targetHeightInputAction.action.canceled += OnHeightChangeEnd;
    }

    private void OnDisable()
    {
        zoomInputAction.action.performed -= OnZoom;
        moveInputAction.action.performed -= OnMove;
        moveInputAction.action.canceled -= OnMoveCanceled;
        rotateInputAction.action.started -= OnRotate;
        rotateInputAction.action.canceled -= OnStopRotate;
        mouseMoveInputAction.action.performed -= OnMouseMove;
        targetHeightInputAction.action.started -= OnHeightChangeStart;
        targetHeightInputAction.action.canceled -= OnHeightChangeEnd;
    }
}