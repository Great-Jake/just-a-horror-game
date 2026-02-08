using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface for all interactable objects
/// </summary>
public interface IInteractable
{
    void Interact(PlayerController player);
}

/// <summary>
/// Collectible key item
/// </summary>
public class KeyItem : MonoBehaviour, IInteractable
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "key_01";
    [SerializeField] private string keyName = "Basement Key";
    [SerializeField] private string pickupMessage = "Found Basement Key";
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 50f;
    
    void Update()
    {
        // Rotate key for visual effect
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    
    public void Interact(PlayerController player)
    {
        // Add key to player inventory
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddKey(keyID);
            gm.ShowMessage(pickupMessage);
        }
        
        // Play sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Destroy key
        Destroy(gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.text = $"Press E to pick up {keyName}";
                gm.interactionText.gameObject.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.gameObject.SetActive(false);
            }
        }
    }
}

/// <summary>
/// Door that requires a key to unlock
/// </summary>
public class LockedDoor : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyID = "key_01";
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openRotation = new Vector3(0, 90, 0);
    [SerializeField] private float openSpeed = 2f;
    
    [Header("Messages")]
    [SerializeField] private string lockedMessage = "Door is locked. Need a key.";
    [SerializeField] private string unlockMessage = "Door unlocked!";
    
    [Header("Audio")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip openSound;
    
    private bool isLocked = true;
    private bool isOpening = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    
    void Start()
    {
        if (doorTransform == null)
        {
            doorTransform = transform;
        }
        
        closedRotation = doorTransform.localRotation;
        targetRotation = closedRotation;
    }
    
    void Update()
    {
        if (isOpening)
        {
            doorTransform.localRotation = Quaternion.Lerp(
                doorTransform.localRotation, 
                targetRotation, 
                Time.deltaTime * openSpeed
            );
        }
    }
    
    public void Interact(PlayerController player)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        
        if (isLocked)
        {
            if (gm != null && gm.HasKey(requiredKeyID))
            {
                // Unlock door
                isLocked = false;
                gm.ShowMessage(unlockMessage);
                
                if (unlockSound != null)
                {
                    AudioSource.PlayClipAtPoint(unlockSound, transform.position);
                }
                
                OpenDoor();
            }
            else
            {
                // Door is locked
                gm?.ShowMessage(lockedMessage);
                
                if (lockedSound != null)
                {
                    AudioSource.PlayClipAtPoint(lockedSound, transform.position);
                }
            }
        }
        else
        {
            // Door is unlocked, just open it
            OpenDoor();
        }
    }
    
    void OpenDoor()
    {
        isOpening = true;
        targetRotation = closedRotation * Quaternion.Euler(openRotation);
        
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.text = "Press E to open door";
                gm.interactionText.gameObject.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.gameObject.SetActive(false);
            }
        }
    }
}

/// <summary>
/// Generator that can be activated to restore power
/// </summary>
public class Generator : MonoBehaviour, IInteractable
{
    [Header("Generator Settings")]
    [SerializeField] private string generatorID = "gen_01";
    [SerializeField] private Light[] lightsToActivate;
    [SerializeField] private ParticleSystem sparkEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioSource generatorAudio;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip runningSound;
    
    [Header("Visual")]
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Renderer generatorRenderer;
    
    private bool isActivated = false;
    
    public void Interact(PlayerController player)
    {
        if (!isActivated)
        {
            ActivateGenerator();
        }
    }
    
    void ActivateGenerator()
    {
        isActivated = true;
        
        // Turn on lights
        foreach (Light light in lightsToActivate)
        {
            if (light != null)
            {
                light.enabled = true;
                StartCoroutine(FlickerLight(light));
            }
        }
        
        // Visual feedback
        if (generatorRenderer != null && onMaterial != null)
        {
            generatorRenderer.material = onMaterial;
        }
        
        if (sparkEffect != null)
        {
            sparkEffect.Play();
        }
        
        // Audio feedback
        if (generatorAudio != null)
        {
            if (activationSound != null)
            {
                generatorAudio.PlayOneShot(activationSound);
            }
            
            if (runningSound != null)
            {
                generatorAudio.clip = runningSound;
                generatorAudio.loop = true;
                generatorAudio.Play();
            }
        }
        
        // Notify game manager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.OnGeneratorActivated(generatorID);
            gm.ShowMessage("Generator activated! Power restored.");
        }
    }
    
    System.Collections.IEnumerator FlickerLight(Light light)
    {
        float originalIntensity = light.intensity;
        
        for (int i = 0; i < 5; i++)
        {
            light.intensity = Random.Range(0.3f, originalIntensity);
            yield return new WaitForSeconds(0.1f);
        }
        
        light.intensity = originalIntensity;
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.text = "Press E to activate generator";
                gm.interactionText.gameObject.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.gameObject.SetActive(false);
            }
        }
    }
}

/// <summary>
/// Patient file document that provides story and evidence
/// </summary>
public class PatientFile : MonoBehaviour, IInteractable
{
    [Header("Document Settings")]
    [SerializeField] private string documentID = "file_01";
    [SerializeField] private string documentTitle = "Patient File #247";
    [SerializeField][TextArea(5, 10)] private string documentContent = "Patient shows signs of severe paranoia...";
    [SerializeField] private bool isEvidence = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    
    public void Interact(PlayerController player)
    {
        // Show document UI
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ShowDocument(documentTitle, documentContent);
            
            if (isEvidence)
            {
                gm.AddEvidence(documentID);
            }
        }
        
        // Play sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Destroy or hide document
        Destroy(gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.text = $"Press E to read {documentTitle}";
                gm.interactionText.gameObject.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.gameObject.SetActive(false);
            }
        }
    }
}

/// <summary>
/// Battery pickup to recharge flashlight
/// </summary>
public class BatteryPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private float batteryAmount = 50f;
    [SerializeField] private AudioClip pickupSound;
    
    void Update()
    {
        transform.Rotate(Vector3.up, 50f * Time.deltaTime);
    }
    
    public void Interact(PlayerController player)
    {
        player.AddBattery(batteryAmount);
        
        GameManager gm = FindObjectOfType<GameManager>();
        gm?.ShowMessage($"Battery recharged +{batteryAmount}%");
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.text = "Press E to pick up battery";
                gm.interactionText.gameObject.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.gameObject.SetActive(false);
            }
        }
    }
}
