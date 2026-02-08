using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Sanity system that affects player perception based on darkness and paranormal events
/// Attach to player object
/// </summary>
public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity = 100f;
    [SerializeField] private float darknessDecayRate = 2f; // Per second in darkness
    [SerializeField] private float lightRecoveryRate = 5f; // Per second in light
    [SerializeField] private float paranormalDecayRate = 10f;
    
    [Header("Light Detection")]
    [SerializeField] private float lightCheckRadius = 5f;
    [SerializeField] private LayerMask lightSourceLayer;
    
    [Header("Visual Effects")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private Image vignetteOverlay;
    [SerializeField] private Image staticOverlay;
    [SerializeField] private CanvasGroup hallucinationGroup;
    [SerializeField] private Image[] hallucinationImages;
    
    [Header("Audio Effects")]
    [SerializeField] private AudioSource sanityAudio;
    [SerializeField] private AudioClip[] whisperClips;
    [SerializeField] private AudioClip heartbeatClip;
    [SerializeField] private AudioClip staticClip;
    
    [Header("Screen Effects Intensity")]
    [SerializeField] private float maxVignetteIntensity = 0.5f;
    [SerializeField] private float maxStaticIntensity = 0.3f;
    [SerializeField] private float maxChromaticAberration = 1f;
    [SerializeField] private float maxLensDistortion = -50f;
    
    // Private variables
    private PlayerController playerController;
    private bool isInDarkness = true;
    private float hallucinationTimer = 0f;
    private float whisperTimer = 0f;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Vignette vignette;
    private Bloom bloom;
    
    // Public accessors
    public float CurrentSanity => currentSanity;
    public float SanityPercentage => currentSanity / maxSanity;
    public bool IsInsane => currentSanity <= 0f;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        currentSanity = maxSanity;
        
        // Setup post-processing effects
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out lensDistortion);
            postProcessVolume.profile.TryGetSettings(out vignette);
            postProcessVolume.profile.TryGetSettings(out bloom);
        }
        
        // Initialize overlays
        if (vignetteOverlay != null)
        {
            vignetteOverlay.color = new Color(0, 0, 0, 0);
        }
        
        if (staticOverlay != null)
        {
            staticOverlay.color = new Color(1, 1, 1, 0);
        }
        
        if (hallucinationGroup != null)
        {
            hallucinationGroup.alpha = 0f;
        }
    }
    
    void Update()
    {
        // Check if player is in darkness
        CheckDarkness();
        
        // Update sanity based on conditions
        UpdateSanity();
        
        // Apply visual effects
        ApplyVisualEffects();
        
        // Handle audio effects
        HandleAudioEffects();
        
        // Handle hallucinations
        HandleHallucinations();
    }
    
    void CheckDarkness()
    {
        // Check for nearby light sources
        Collider[] lightSources = Physics.OverlapSphere(transform.position, lightCheckRadius, lightSourceLayer);
        
        // Also consider player's flashlight
        bool hasLight = playerController.IsFlashlightOn;
        
        foreach (Collider col in lightSources)
        {
            Light light = col.GetComponent<Light>();
            if (light != null && light.enabled && light.intensity > 0.3f)
            {
                hasLight = true;
                break;
            }
        }
        
        isInDarkness = !hasLight;
    }
    
    void UpdateSanity()
    {
        if (isInDarkness)
        {
            // Lose sanity in darkness
            currentSanity -= darknessDecayRate * Time.deltaTime;
        }
        else
        {
            // Recover sanity in light
            currentSanity += lightRecoveryRate * Time.deltaTime;
        }
        
        // Clamp sanity
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
        
        // Check for insanity
        if (currentSanity <= 0f)
        {
            OnBecomeInsane();
        }
    }
    
    void ApplyVisualEffects()
    {
        float insanityLevel = 1f - SanityPercentage;
        
        // Vignette overlay
        if (vignetteOverlay != null)
        {
            Color vignetteColor = vignetteOverlay.color;
            vignetteColor.a = maxVignetteIntensity * insanityLevel;
            vignetteOverlay.color = vignetteColor;
        }
        
        // Static overlay
        if (staticOverlay != null)
        {
            Color staticColor = staticOverlay.color;
            staticColor.a = maxStaticIntensity * insanityLevel * Random.Range(0.8f, 1.2f);
            staticOverlay.color = staticColor;
        }
        
        // Post-processing effects
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = maxChromaticAberration * insanityLevel;
        }
        
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = maxLensDistortion * insanityLevel;
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = 0.3f + (0.5f * insanityLevel);
        }
        
        if (bloom != null)
        {
            bloom.intensity.value = 1f + (5f * insanityLevel);
        }
    }
    
    void HandleAudioEffects()
    {
        float insanityLevel = 1f - SanityPercentage;
        
        // Play heartbeat when sanity is low
        if (insanityLevel > 0.5f && heartbeatClip != null)
        {
            if (sanityAudio != null && !sanityAudio.isPlaying)
            {
                sanityAudio.clip = heartbeatClip;
                sanityAudio.loop = true;
                sanityAudio.volume = (insanityLevel - 0.5f) * 2f;
                sanityAudio.Play();
            }
            else if (sanityAudio != null)
            {
                sanityAudio.volume = Mathf.Lerp(sanityAudio.volume, (insanityLevel - 0.5f) * 2f, Time.deltaTime);
            }
        }
        else if (sanityAudio != null && sanityAudio.clip == heartbeatClip)
        {
            sanityAudio.Stop();
        }
        
        // Random whispers
        if (insanityLevel > 0.3f)
        {
            whisperTimer += Time.deltaTime;
            float whisperInterval = Mathf.Lerp(20f, 5f, insanityLevel);
            
            if (whisperTimer > whisperInterval && whisperClips != null && whisperClips.Length > 0)
            {
                AudioClip whisper = whisperClips[Random.Range(0, whisperClips.Length)];
                AudioSource.PlayClipAtPoint(whisper, transform.position, 0.3f);
                whisperTimer = 0f;
            }
        }
    }
    
    void HandleHallucinations()
    {
        float insanityLevel = 1f - SanityPercentage;
        
        if (insanityLevel > 0.6f && hallucinationImages != null && hallucinationImages.Length > 0)
        {
            hallucinationTimer += Time.deltaTime;
            
            // Show random hallucination
            if (hallucinationTimer > Random.Range(10f, 20f))
            {
                StartCoroutine(ShowHallucination());
                hallucinationTimer = 0f;
            }
        }
    }
    
    System.Collections.IEnumerator ShowHallucination()
    {
        if (hallucinationGroup == null || hallucinationImages == null) yield break;
        
        // Pick random hallucination image
        Image selectedImage = hallucinationImages[Random.Range(0, hallucinationImages.Length)];
        
        foreach (Image img in hallucinationImages)
        {
            img.enabled = false;
        }
        
        selectedImage.enabled = true;
        
        // Fade in
        float fadeTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            hallucinationGroup.alpha = Mathf.Lerp(0f, 0.7f, elapsed / fadeTime);
            yield return null;
        }
        
        // Hold
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            hallucinationGroup.alpha = Mathf.Lerp(0.7f, 0f, elapsed / fadeTime);
            yield return null;
        }
        
        hallucinationGroup.alpha = 0f;
        selectedImage.enabled = false;
    }
    
    public void AddStress(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Max(0f, currentSanity);
    }
    
    public void TriggerParanormalEvent()
    {
        currentSanity -= paranormalDecayRate;
        currentSanity = Mathf.Max(0f, currentSanity);
        
        // Play special effect
        if (staticClip != null && sanityAudio != null)
        {
            AudioSource.PlayClipAtPoint(staticClip, transform.position);
        }
    }
    
    void OnBecomeInsane()
    {
        // Trigger special game over or event
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver(false);
        }
    }
}
