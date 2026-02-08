using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// First-person player controller with walking, sprinting, crouching, and flashlight mechanics
/// Attach to player object with CharacterController component
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 2.0f;
    [SerializeField] private float gravity = -15.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    
    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float lookXLimit = 80.0f;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10.0f;
    
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlight;
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainRate = 5f; // Per second
    [SerializeField] private float batteryRechargeRate = 10f;
    [SerializeField] private AudioSource flashlightAudio;
    [SerializeField] private AudioClip flashlightToggleSound;
    
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    
    [Header("Noise Generation")]
    [SerializeField] private float walkNoiseLevel = 0.3f;
    [SerializeField] private float sprintNoiseLevel = 1.0f;
    [SerializeField] private float crouchNoiseLevel = 0.1f;
    
    // Private variables
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool isCrouching = false;
    private float currentHeight;
    private float currentBattery;
    private float currentStamina;
    private bool isFlashlightOn = false;
    private bool canMove = true;
    private bool isHiding = false;
    
    // References
    private SanitySystem sanitySystem;
    private GameManager gameManager;
    
    // Public accessors
    public bool IsFlashlightOn => isFlashlightOn;
    public float CurrentNoiseLevel { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching => isCrouching;
    public bool IsHiding { get => isHiding; set => isHiding = value; }
    public Vector3 Position => transform.position;
    public float CurrentBattery => currentBattery;
    public float CurrentStamina => currentStamina;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentHeight = standingHeight;
        currentBattery = maxBattery;
        currentStamina = maxStamina;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Get references
        sanitySystem = GetComponent<SanitySystem>();
        gameManager = FindObjectOfType<GameManager>();
        
        // Setup camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Setup flashlight
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }
    
    void Update()
    {
        if (!canMove || isHiding) return;
        
        // Handle movement
        HandleMovement();
        
        // Handle camera rotation
        HandleRotation();
        
        // Handle crouching
        HandleCrouching();
        
        // Handle flashlight
        HandleFlashlight();
        
        // Handle stamina
        HandleStamina();
        
        // Update noise level
        UpdateNoiseLevel();
        
        // Check for interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteraction();
        }
    }
    
    void HandleMovement()
    {
        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Determine speed
        float currentSpeed = walkSpeed;
        bool tryingSprint = Input.GetKey(KeyCode.LeftShift);
        
        if (tryingSprint && !isCrouching && currentStamina > 0)
        {
            currentSpeed = sprintSpeed;
            IsSprinting = true;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            IsSprinting = false;
        }
        else
        {
            IsSprinting = false;
        }
        
        // Calculate movement direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        float curSpeedX = currentSpeed * moveX;
        float curSpeedZ = currentSpeed * moveZ;
        
        // Apply gravity
        if (characterController.isGrounded)
        {
            moveDirection.y = -0.5f;
        }
        else
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        
        // Move the controller
        Vector3 move = (forward * curSpeedZ) + (right * curSpeedX);
        characterController.Move(move * Time.deltaTime + Vector3.up * moveDirection.y * Time.deltaTime);
        
        // Update IsMoving
        IsMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f);
    }
    
    void HandleRotation()
    {
        // Rotate camera
        rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        
        // Rotate player
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
    }
    
    void HandleCrouching()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }
        
        // Smoothly transition height
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        characterController.height = currentHeight;
        
        // Adjust camera position
        Vector3 cameraPos = cameraTransform.localPosition;
        cameraPos.y = currentHeight * 0.9f;
        cameraTransform.localPosition = cameraPos;
    }
    
    void HandleFlashlight()
    {
        // Toggle flashlight
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentBattery > 0 || isFlashlightOn)
            {
                isFlashlightOn = !isFlashlightOn;
                flashlight.enabled = isFlashlightOn;
                
                if (flashlightAudio != null && flashlightToggleSound != null)
                {
                    flashlightAudio.PlayOneShot(flashlightToggleSound);
                }
            }
        }
        
        // Drain or recharge battery
        if (isFlashlightOn && currentBattery > 0)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;
            currentBattery = Mathf.Max(0, currentBattery);
            
            // Flicker when low
            if (currentBattery < 20f)
            {
                flashlight.intensity = Random.Range(0.3f, 1.0f);
            }
            
            // Turn off when dead
            if (currentBattery <= 0)
            {
                isFlashlightOn = false;
                flashlight.enabled = false;
            }
        }
        else if (!isFlashlightOn && currentBattery < maxBattery)
        {
            currentBattery += batteryRechargeRate * Time.deltaTime;
            currentBattery = Mathf.Min(maxBattery, currentBattery);
        }
        
        // Update flashlight intensity
        if (isFlashlightOn && currentBattery >= 20f)
        {
            flashlight.intensity = Mathf.Lerp(flashlight.intensity, 1.0f, Time.deltaTime * 5f);
        }
    }
    
    void HandleStamina()
    {
        if (IsSprinting && IsMoving)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }
    
    void UpdateNoiseLevel()
    {
        if (!IsMoving)
        {
            CurrentNoiseLevel = 0f;
        }
        else if (IsSprinting)
        {
            CurrentNoiseLevel = sprintNoiseLevel;
        }
        else if (isCrouching)
        {
            CurrentNoiseLevel = crouchNoiseLevel;
        }
        else
        {
            CurrentNoiseLevel = walkNoiseLevel;
        }
    }
    
    void CheckInteraction()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 2.5f))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
    }
    
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
    
    public void AddBattery(float amount)
    {
        currentBattery = Mathf.Min(maxBattery, currentBattery + amount);
    }
}
