using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float proneSpeed = 1.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float airControl = 0.3f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -15f;

    [Header("Stance Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1.2f;
    [SerializeField] private float proneHeight = 0.6f;
    [SerializeField] private float stanceTransitionSpeed = 8f;
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, -0.4f, 0);
    [SerializeField] private Vector3 proneCenter = new Vector3(0, -0.7f, 0);

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask = -1;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMovement;
    private bool isGrounded;
    
    // Input Actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction proneAction;

    // Input State
    private Vector2 moveInput;
    private bool isSprinting;
    
    public enum MovementStance
    {
        Standing,
        Crouching,
        Prone
    }
    
    private MovementStance currentStance = MovementStance.Standing;
    private MovementStance targetStance = MovementStance.Standing;
    
    private float currentHeight;
    private Vector3 currentCenter;

    public MovementStance CurrentStance => currentStance;
    public bool IsGrounded => isGrounded;
    public Vector3 Velocity => velocity;
    public float CurrentSpeed => currentMovement.magnitude;
    public bool IsSprinting => isSprinting;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentHeight = controller.height;
        currentCenter = controller.center;

        // Get the Player action map
        var playerActionMap = inputActions.FindActionMap("Player");
        
        // Get individual actions
        moveAction = playerActionMap.FindAction("Move");
        jumpAction = playerActionMap.FindAction("Jump");
        sprintAction = playerActionMap.FindAction("Sprint");
        crouchAction = playerActionMap.FindAction("Crouch");
        proneAction = playerActionMap.FindAction("Prone");

        // Subscribe to button press events
        crouchAction.performed += OnCrouchPerformed;
        proneAction.performed += OnPronePerformed;
        jumpAction.performed += OnJumpPerformed;
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
        // Unsubscribe from events
        crouchAction.performed -= OnCrouchPerformed;
        proneAction.performed -= OnPronePerformed;
        jumpAction.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        HandleMovementInput();
        ApplyGravity();
        UpdateStance();
        
        controller.Move(velocity * Time.deltaTime);
    }

    private void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        isSprinting = sprintAction.IsPressed();
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (targetStance == MovementStance.Crouching)
        {
            targetStance = MovementStance.Standing;
        }
        else if (targetStance != MovementStance.Prone)
        {
            targetStance = MovementStance.Crouching;
        }

        // Can't stand up if something is above
        if (targetStance == MovementStance.Standing && !CanStandUp())
        {
            targetStance = currentStance;
        }
    }

    private void OnPronePerformed(InputAction.CallbackContext context)
    {
        targetStance = targetStance == MovementStance.Prone ? MovementStance.Standing : MovementStance.Prone;

        // Can't stand up if something is above
        if (targetStance == MovementStance.Standing && !CanStandUp())
        {
            targetStance = currentStance;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && currentStance == MovementStance.Standing)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void CheckGround()
    {
        Vector3 spherePosition = transform.position - new Vector3(0, controller.height / 2f - controller.radius + 0.1f, 0);
        isGrounded = Physics.CheckSphere(spherePosition, controller.radius, groundMask, QueryTriggerInteraction.Ignore);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private bool CanStandUp()
    {
        float checkHeight = standingHeight - currentHeight;
        Vector3 start = transform.position + controller.center;
        Vector3 end = start + Vector3.up * checkHeight;
        
        return !Physics.CheckCapsule(start, end, controller.radius * 0.9f, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void HandleMovementInput()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 moveDirection = transform.TransformDirection(inputDirection);
        
        float targetSpeed = GetTargetSpeed();
        Vector3 targetMovement = moveDirection * targetSpeed;
        
        float accelRate = inputDirection.magnitude > 0 ? acceleration : deceleration;
        if (!isGrounded)
        {
            accelRate *= airControl;
        }
        
        currentMovement = Vector3.Lerp(currentMovement, targetMovement, accelRate * Time.deltaTime);
        
        velocity.x = currentMovement.x;
        velocity.z = currentMovement.z;
    }

    private float GetTargetSpeed()
    {
        switch (currentStance)
        {
            case MovementStance.Prone:
                return proneSpeed;
            case MovementStance.Crouching:
                return crouchSpeed;
            case MovementStance.Standing:
                if (isSprinting)
                {
                    return sprintSpeed;
                }
                return walkSpeed;
            default:
                return walkSpeed;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    private void UpdateStance()
    {
        float targetHeight = GetStanceHeight(targetStance);
        Vector3 targetCenter = GetStanceCenter(targetStance);
        
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, stanceTransitionSpeed * Time.deltaTime);
        currentCenter = Vector3.Lerp(currentCenter, targetCenter, stanceTransitionSpeed * Time.deltaTime);
        
        controller.height = currentHeight;
        controller.center = currentCenter;
        
        if (Mathf.Abs(currentHeight - targetHeight) < 0.01f)
        {
            currentStance = targetStance;
        }
    }

    private float GetStanceHeight(MovementStance stance)
    {
        switch (stance)
        {
            case MovementStance.Prone:
                return proneHeight;
            case MovementStance.Crouching:
                return crouchingHeight;
            case MovementStance.Standing:
            default:
                return standingHeight;
        }
    }

    private Vector3 GetStanceCenter(MovementStance stance)
    {
        switch (stance)
        {
            case MovementStance.Prone:
                return proneCenter;
            case MovementStance.Crouching:
                return crouchingCenter;
            case MovementStance.Standing:
            default:
                return standingCenter;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;
        
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 spherePosition = transform.position - new Vector3(0, controller.height / 2f - controller.radius + 0.1f, 0);
        Gizmos.DrawWireSphere(spherePosition, controller.radius);
    }
}
