using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FirstPersonController characterController;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float lookSmoothness = 10f;
    [SerializeField] private bool invertY = false;

    [Header("Look Limits")]
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;

    [Header("Camera Position by Stance")]
    [SerializeField] private float standingCameraHeight = 0.7f;
    [SerializeField] private float crouchingCameraHeight = 0.3f;
    [SerializeField] private float proneCameraHeight = -0.3f;
    [SerializeField] private float cameraHeightTransitionSpeed = 8f;

    [Header("Head Bob Settings")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobHorizontalAmplitude = 0.05f;
    [SerializeField] private float bobVerticalAmplitude = 0.05f;
    [SerializeField] private float bobSmoothing = 10f;
    [SerializeField] private float sprintBobFrequencyMultiplier = 1.5f;
    [SerializeField] private float sprintBobAmplitudeMultiplier = 1.2f;
    [SerializeField] private float crouchBobReduction = 0.5f;
    [SerializeField] private float proneBobReduction = 0.3f;

    [Header("Camera Tilt Settings")]
    [SerializeField] private bool enableCameraTilt = true;
    [SerializeField] private float tiltAngle = 2f;
    [SerializeField] private float tiltSpeed = 5f;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction lookAction;
    private InputAction toggleCursorAction;

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    private float targetXRotation = 0f;
    private float targetYRotation = 0f;

    private float currentCameraHeight;
    private float targetCameraHeight;

    private float bobTimer = 0f;
    private Vector3 bobOffset = Vector3.zero;
    private float currentTilt = 0f;

    private bool cursorLocked = true;

    private void Start()
    {
        if (cameraHolder == null)
        {
            cameraHolder = transform;
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (characterController == null)
        {
            characterController = GetComponentInParent<FirstPersonController>();
        }

        // Get input actions
        var playerActionMap = inputActions.FindActionMap("Player");
        var uiActionMap = inputActions.FindActionMap("UI");

        lookAction = playerActionMap.FindAction("Look");
        toggleCursorAction = uiActionMap.FindAction("ToggleCursor");

        // Subscribe to toggle cursor action
        toggleCursorAction.performed += OnToggleCursor;

        SetCursorState(true);

        currentCameraHeight = standingCameraHeight;
        targetCameraHeight = standingCameraHeight;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        toggleCursorAction.performed -= OnToggleCursor;
    }

    private void Update()
    {
        HandleLookInput();
        UpdateCameraHeight();
        UpdateHeadBob();
        UpdateCameraTilt();
        ApplyCameraTransform();
    }

    private void OnToggleCursor(InputAction.CallbackContext context)
    {
        SetCursorState(!cursorLocked);
    }

    private void SetCursorState(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleLookInput()
    {
        if (!cursorLocked) return;

        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // Apply sensitivity and framerate independence
        float mouseX = lookInput.x * mouseSensitivity * 0.02f;
        float mouseY = lookInput.y * mouseSensitivity * 0.02f;

        if (invertY)
        {
            mouseY = -mouseY;
        }

        targetYRotation += mouseX;
        targetXRotation -= mouseY;
        targetXRotation = Mathf.Clamp(targetXRotation, minVerticalAngle, maxVerticalAngle);

        currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, lookSmoothness * Time.deltaTime);
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, lookSmoothness * Time.deltaTime);
    }

    private void UpdateCameraHeight()
    {
        if (characterController == null) return;

        switch (characterController.CurrentStance)
        {
            case FirstPersonController.MovementStance.Standing:
                targetCameraHeight = standingCameraHeight;
                break;
            case FirstPersonController.MovementStance.Crouching:
                targetCameraHeight = crouchingCameraHeight;
                break;
            case FirstPersonController.MovementStance.Prone:
                targetCameraHeight = proneCameraHeight;
                break;
        }

        currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, cameraHeightTransitionSpeed * Time.deltaTime);
    }

    private void UpdateHeadBob()
    {
        if (!enableHeadBob || characterController == null || !characterController.IsGrounded)
        {
            bobTimer = 0f;
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, bobSmoothing * Time.deltaTime);
            return;
        }

        float speed = characterController.CurrentSpeed;
        
        if (speed > 0.1f)
        {
            float frequencyMultiplier = 1f;
            float amplitudeMultiplier = 1f;
            
            // Apply sprint multipliers
            if (characterController.IsSprinting && characterController.CurrentStance == FirstPersonController.MovementStance.Standing)
            {
                frequencyMultiplier = sprintBobFrequencyMultiplier;
                amplitudeMultiplier = sprintBobAmplitudeMultiplier;
            }

            // Apply stance-based reduction
            switch (characterController.CurrentStance)
            {
                case FirstPersonController.MovementStance.Crouching:
                    amplitudeMultiplier *= crouchBobReduction;
                    break;
                case FirstPersonController.MovementStance.Prone:
                    amplitudeMultiplier *= proneBobReduction;
                    break;
            }

            bobTimer += Time.deltaTime * bobFrequency * frequencyMultiplier;
            
            // Horizontal bob (side to side)
            float horizontalBob = Mathf.Sin(bobTimer) * bobHorizontalAmplitude * amplitudeMultiplier;
            
            // Vertical bob (up and down) - uses 2x frequency for more natural gait
            float verticalBob = Mathf.Sin(bobTimer * 2f) * bobVerticalAmplitude * amplitudeMultiplier;
            
            Vector3 targetBob = new Vector3(horizontalBob, verticalBob, 0f);
            bobOffset = Vector3.Lerp(bobOffset, targetBob, bobSmoothing * Time.deltaTime);
        }
        else
        {
            bobTimer = 0f;
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, bobSmoothing * Time.deltaTime);
        }
    }

    private void UpdateCameraTilt()
    {
        if (!enableCameraTilt || characterController == null) return;

        // Read the move input for strafe detection
        var playerActionMap = inputActions.FindActionMap("Player");
        var moveAction = playerActionMap.FindAction("Move");
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        float targetTilt = -moveInput.x * tiltAngle;
        
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
    }

    private void ApplyCameraTransform()
    {
        // Apply body rotation (horizontal)
        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        // Apply camera holder position with height offset
        Vector3 targetPosition = new Vector3(0f, currentCameraHeight, 0f) + bobOffset;
        cameraHolder.localPosition = targetPosition;

        // Apply camera rotation (vertical) and tilt
        cameraHolder.localRotation = Quaternion.Euler(currentXRotation, 0f, currentTilt);
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    public void SetInvertY(bool invert)
    {
        invertY = invert;
    }

    public float GetCurrentXRotation() => currentXRotation;
    public float GetCurrentYRotation() => currentYRotation;
}
