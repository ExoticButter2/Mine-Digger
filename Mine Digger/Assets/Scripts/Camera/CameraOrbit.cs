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

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
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

        Quaternion targetRotation = targetTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
        Vector3 targetPosition = targetTransform.position - targetTransform.forward * _cameraZoomFromTarget;

        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
        _cameraTransform.rotation = Quaternion.Lerp(_cameraTransform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    private void RotateTarget(Vector2 rotation)
    {
        _yaw += rotation.x;
        _pitch -= rotation.y;
        _pitch = Mathf.Clamp(_pitch, -85f, 85f);
        Quaternion deltaRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        targetTransform.rotation = deltaRotation;
        Debug.Log($"Rotated target by rotation: {rotation}");
    }

    #region ActionMeth

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        float zoomDirection = ctx.ReadValue<float>();
        _cameraZoomFromTarget += zoomDirection * zoomSensitivity;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 movementInput = ctx.ReadValue<Vector2>();
        Vector3 movementVector = new Vector3(movementInput.y, movementInput.x, 0f);

        targetTransform.position += movementVector;
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
        if (_rotatingCamera)
        {
            Vector2 mouseInput = ctx.ReadValue<Vector2>() * mouseSensitivity;
            Debug.Log($"Mouse input: {mouseInput}");
            RotateTarget(mouseInput);
        }
    }

    #endregion

    private void OnEnable()
    {
        zoomInputAction.action.performed += OnZoom;
        moveInputAction.action.performed += OnMove;
        rotateInputAction.action.started += OnRotate;
        rotateInputAction.action.canceled += OnStopRotate;
        mouseMoveInputAction.action.performed += OnMouseMove;
    }

    private void OnDisable()
    {
        zoomInputAction.action.performed -= OnZoom;
        moveInputAction.action.performed -= OnMove;
        rotateInputAction.action.started -= OnRotate;
        rotateInputAction.action.canceled -= OnStopRotate;
        mouseMoveInputAction.action.performed -= OnMouseMove;
    }
}
