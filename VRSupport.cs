using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// VR support for the horror game
/// Attach to player camera or VR rig
/// Supports Oculus, Vive, and other OpenXR-compatible headsets
/// </summary>
public class VRSupport : MonoBehaviour
{
    [Header("VR Settings")]
    [SerializeField] private bool enableVR = false;
    [SerializeField] private float vrMovementSpeed = 3f;
    [SerializeField] private float vrRotationSpeed = 90f;
    
    [Header("VR Camera")]
    [SerializeField] private Camera vrCamera;
    [SerializeField] private Transform cameraRig;
    
    [Header("VR Controllers")]
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform flashlightHand;
    
    [Header("VR Interaction")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactionMask;
    
    [Header("Comfort Settings")]
    [SerializeField] private bool enableTeleportation = true;
    [SerializeField] private bool enableSnapTurning = true;
    [SerializeField] private float snapTurnAngle = 45f;
    [SerializeField] private GameObject teleportIndicator;
    
    // Private variables
    private PlayerController playerController;
    private bool vrInitialized = false;
    private Vector2 leftStickInput;
    private Vector2 rightStickInput;
    private bool lastSnapTurnInput = false;
    
    // Input device references
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    private InputDevice headDevice;
    
    void Start()
    {
        if (enableVR)
        {
            InitializeVR();
        }
        
        playerController = GetComponentInParent<PlayerController>();
        
        if (teleportIndicator != null)
        {
            teleportIndicator.SetActive(false);
        }
    }
    
    void InitializeVR()
    {
        // Check if VR is available
        if (!XRSettings.enabled)
        {
            Debug.LogWarning("VR is not enabled in player settings. Disabling VR mode.");
            enableVR = false;
            return;
        }
        
        // Get input devices
        var leftHandDevices = new System.Collections.Generic.List<InputDevice>();
        var rightHandDevices = new System.Collections.Generic.List<InputDevice>();
        var headDevices = new System.Collections.Generic.List<InputDevice>();
        
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
        
        if (leftHandDevices.Count > 0)
            leftHandDevice = leftHandDevices[0];
        
        if (rightHandDevices.Count > 0)
            rightHandDevice = rightHandDevices[0];
        
        if (headDevices.Count > 0)
            headDevice = headDevices[0];
        
        vrInitialized = true;
        Debug.Log("VR Initialized");
    }
    
    void Update()
    {
        if (!enableVR || !vrInitialized) return;
        
        // Update controller inputs
        UpdateControllerInputs();
        
        // Handle VR-specific interactions
        HandleVRFlashlight();
        HandleVRInteraction();
        
        // Handle teleportation if enabled
        if (enableTeleportation)
        {
            HandleTeleportation();
        }
        
        // Handle snap turning if enabled
        if (enableSnapTurning)
        {
            HandleSnapTurning();
        }
    }
    
    void UpdateControllerInputs()
    {
        // Get left stick input (movement)
        if (leftHandDevice.isValid)
        {
            leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftStickInput);
        }
        
        // Get right stick input (turning/teleport)
        if (rightHandDevice.isValid)
        {
            rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightStickInput);
        }
    }
    
    void HandleVRFlashlight()
    {
        if (flashlightHand == null) return;
        
        // Check for trigger press to toggle flashlight
        bool triggerPressed = false;
        
        if (rightHandDevice.isValid)
        {
            rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
        }
        
        if (triggerPressed && playerController != null)
        {
            // Simulate flashlight toggle
            // Note: This would need to be integrated with the PlayerController's flashlight system
        }
    }
    
    void HandleVRInteraction()
    {
        // Raycast from the right controller for interactions
        if (rightController == null) return;
        
        bool gripPressed = false;
        if (rightHandDevice.isValid)
        {
            rightHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);
        }
        
        if (gripPressed)
        {
            RaycastHit hit;
            if (Physics.Raycast(rightController.position, rightController.forward, out hit, interactionDistance, interactionMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && playerController != null)
                {
                    interactable.Interact(playerController);
                }
            }
        }
    }
    
    void HandleTeleportation()
    {
        // Hold right stick up to show teleport indicator
        if (rightStickInput.y > 0.5f)
        {
            if (teleportIndicator != null)
            {
                // Raycast to find valid teleport location
                RaycastHit hit;
                if (Physics.Raycast(vrCamera.transform.position, vrCamera.transform.forward, out hit, 10f))
                {
                    teleportIndicator.SetActive(true);
                    teleportIndicator.transform.position = hit.point;
                }
            }
        }
        else if (teleportIndicator != null && teleportIndicator.activeSelf)
        {
            // Release to teleport
            if (cameraRig != null)
            {
                cameraRig.position = teleportIndicator.transform.position;
            }
            
            teleportIndicator.SetActive(false);
        }
    }
    
    void HandleSnapTurning()
    {
        bool snapTurnInput = Mathf.Abs(rightStickInput.x) > 0.5f;
        
        if (snapTurnInput && !lastSnapTurnInput)
        {
            // Perform snap turn
            float turnDirection = Mathf.Sign(rightStickInput.x);
            
            if (cameraRig != null)
            {
                cameraRig.Rotate(Vector3.up, turnDirection * snapTurnAngle);
            }
        }
        
        lastSnapTurnInput = snapTurnInput;
    }
    
    /// <summary>
    /// Get the current head position in world space
    /// </summary>
    public Vector3 GetHeadPosition()
    {
        if (vrCamera != null)
        {
            return vrCamera.transform.position;
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get the current head rotation in world space
    /// </summary>
    public Quaternion GetHeadRotation()
    {
        if (vrCamera != null)
        {
            return vrCamera.transform.rotation;
        }
        
        return Quaternion.identity;
    }
    
    /// <summary>
    /// Trigger haptic feedback on a controller
    /// </summary>
    public void TriggerHaptic(bool leftHand, float amplitude = 0.5f, float duration = 0.1f)
    {
        InputDevice device = leftHand ? leftHandDevice : rightHandDevice;
        
        if (device.isValid)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0, amplitude, duration);
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!enableVR || rightController == null) return;
        
        // Draw interaction raycast
        Gizmos.color = Color.green;
        Gizmos.DrawLine(rightController.position, rightController.position + rightController.forward * interactionDistance);
    }
}

/// <summary>
/// VR-specific UI handler
/// Attach to Canvas with WorldSpace render mode for VR
/// </summary>
public class VRUIHandler : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private float uiDistance = 2f;
    [SerializeField] private bool followPlayer = true;
    
    private Transform vrCamera;
    
    void Start()
    {
        vrCamera = Camera.main.transform;
        
        if (uiCanvas != null)
        {
            uiCanvas.renderMode = RenderMode.WorldSpace;
        }
    }
    
    void LateUpdate()
    {
        if (followPlayer && vrCamera != null && uiCanvas != null)
        {
            // Position UI in front of player
            Vector3 targetPosition = vrCamera.position + vrCamera.forward * uiDistance;
            uiCanvas.transform.position = Vector3.Lerp(uiCanvas.transform.position, targetPosition, Time.deltaTime * 5f);
            
            // Face the player
            uiCanvas.transform.LookAt(vrCamera);
            uiCanvas.transform.Rotate(0, 180, 0);
        }
    }
}

/// <summary>
/// VR comfort options
/// Attach to player to provide comfort settings
/// </summary>
public class VRComfortSettings : MonoBehaviour
{
    [Header("Vignette")]
    [SerializeField] private bool enableVignetteDuringMovement = true;
    [SerializeField] private float vignetteStrength = 0.5f;
    [SerializeField] private UnityEngine.UI.Image vignetteImage;
    
    [Header("Field of View")]
    [SerializeField] private bool reduceFOVDuringMovement = true;
    [SerializeField] private float normalFOV = 90f;
    [SerializeField] private float reducedFOV = 70f;
    
    private Camera vrCamera;
    private bool isMoving = false;
    
    void Start()
    {
        vrCamera = Camera.main;
        
        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(0, 0, 0, 0);
        }
    }
    
    void Update()
    {
        // Detect if player is moving
        PlayerController player = GetComponent<PlayerController>();
        if (player != null)
        {
            isMoving = player.IsMoving;
        }
        
        // Apply comfort settings
        if (enableVignetteDuringMovement && vignetteImage != null)
        {
            float targetAlpha = isMoving ? vignetteStrength : 0f;
            Color currentColor = vignetteImage.color;
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * 5f);
            vignetteImage.color = currentColor;
        }
        
        if (reduceFOVDuringMovement && vrCamera != null)
        {
            float targetFOV = isMoving ? reducedFOV : normalFOV;
            vrCamera.fieldOfView = Mathf.Lerp(vrCamera.fieldOfView, targetFOV, Time.deltaTime * 5f);
        }
    }
}
