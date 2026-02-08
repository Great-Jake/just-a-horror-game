using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiding spot that allows player to hide from the enemy
/// Attach to closet or bed objects
/// </summary>
public class HidingSpot : MonoBehaviour, IInteractable
{
    [Header("Hiding Settings")]
    [SerializeField] private Transform hidePosition;
    [SerializeField] private string hidingPrompt = "Press E to Hide";
    [SerializeField] private string exitPrompt = "Press E to Exit";
    [SerializeField] private float hideDetectionReduction = 0.9f; // 90% harder to detect
    
    [Header("Audio")]
    [SerializeField] private AudioSource hideAudio;
    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip exitSound;
    [SerializeField] private AudioClip breathingSound;
    
    [Header("Visual")]
    [SerializeField] private GameObject hideViewObject; // Optional: camera view while hiding
    [SerializeField] private Image darkOverlay;
    
    private bool playerHiding = false;
    private PlayerController player;
    private Camera playerCamera;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float hideTimer = 0f;
    
    void Start()
    {
        if (hidePosition == null)
        {
            // Create default hide position
            GameObject hidePosObj = new GameObject("HidePosition");
            hidePosObj.transform.parent = transform;
            hidePosObj.transform.localPosition = Vector3.zero;
            hidePosition = hidePosObj.transform;
        }
        
        if (hideViewObject != null)
        {
            hideViewObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (playerHiding)
        {
            hideTimer += Time.deltaTime;
            
            // Allow player to exit
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitHiding();
            }
            
            // Slight breathing effect
            if (hideViewObject != null && hideViewObject.activeSelf)
            {
                float breathe = Mathf.Sin(hideTimer * 2f) * 0.02f;
                hideViewObject.transform.localPosition = new Vector3(0, breathe, 0);
            }
        }
    }
    
    public void Interact(PlayerController interactingPlayer)
    {
        if (!playerHiding)
        {
            EnterHiding(interactingPlayer);
        }
        else if (interactingPlayer == player)
        {
            ExitHiding();
        }
    }
    
    void EnterHiding(PlayerController interactingPlayer)
    {
        player = interactingPlayer;
        playerCamera = Camera.main;
        playerHiding = true;
        hideTimer = 0f;
        
        // Store original camera transform
        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;
        
        // Disable player movement
        player.SetCanMove(false);
        player.IsHiding = true;
        
        // Move camera to hide position
        if (hideViewObject != null)
        {
            hideViewObject.SetActive(true);
            playerCamera.transform.position = hideViewObject.transform.position;
            playerCamera.transform.rotation = hideViewObject.transform.rotation;
        }
        else
        {
            playerCamera.transform.position = hidePosition.position;
            playerCamera.transform.LookAt(hidePosition.position + hidePosition.forward);
        }
        
        // Darken screen
        if (darkOverlay != null)
        {
            StartCoroutine(FadeOverlay(true));
        }
        
        // Play sound
        if (hideAudio != null && enterSound != null)
        {
            hideAudio.PlayOneShot(enterSound);
        }
        
        // Start breathing sound
        if (breathingSound != null && hideAudio != null)
        {
            hideAudio.clip = breathingSound;
            hideAudio.loop = true;
            hideAudio.Play();
        }
    }
    
    void ExitHiding()
    {
        if (player == null) return;
        
        playerHiding = false;
        
        // Restore player movement
        player.SetCanMove(true);
        player.IsHiding = false;
        
        // Restore camera
        if (hideViewObject != null)
        {
            hideViewObject.SetActive(false);
        }
        
        // Lighten screen
        if (darkOverlay != null)
        {
            StartCoroutine(FadeOverlay(false));
        }
        
        // Play sound
        if (hideAudio != null)
        {
            hideAudio.Stop();
            if (exitSound != null)
            {
                hideAudio.PlayOneShot(exitSound);
            }
        }
        
        player = null;
    }
    
    System.Collections.IEnumerator FadeOverlay(bool fadeIn)
    {
        if (darkOverlay == null) yield break;
        
        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = darkOverlay.color;
        Color targetColor = fadeIn ? new Color(0, 0, 0, 0.8f) : new Color(0, 0, 0, 0);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            darkOverlay.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }
        
        darkOverlay.color = targetColor;
    }
    
    public bool IsPlayerHiding()
    {
        return playerHiding;
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !playerHiding)
        {
            // Show interaction prompt
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.text = hidingPrompt;
                gm.interactionText.gameObject.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide interaction prompt
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.interactionText != null)
            {
                gm.interactionText.gameObject.SetActive(false);
            }
        }
    }
}
