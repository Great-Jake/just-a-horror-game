using UnityEngine;
using System.Collections;

/// <summary>
/// Controls environmental effects like flickering lights, fog, and ambient sounds
/// Attach to empty GameObject in scene
/// </summary>
public class EnvironmentController : MonoBehaviour
{
    [Header("Lighting")]
    [SerializeField] private Light[] flickeringLights;
    [SerializeField] private float flickerMinInterval = 2f;
    [SerializeField] private float flickerMaxInterval = 8f;
    [SerializeField] private float flickerDuration = 0.5f;
    [SerializeField] private bool enableRandomFlickers = true;
    
    [Header("Fog Settings")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color fogColor = new Color(0.1f, 0.1f, 0.15f);
    [SerializeField] private float fogDensity = 0.05f;
    [SerializeField] private FogMode fogMode = FogMode.Exponential;
    
    [Header("Ambient Audio")]
    [SerializeField] private AudioSource[] ambientSources;
    [SerializeField] private AudioClip[] ambientClips;
    [SerializeField] private float ambientMinInterval = 10f;
    [SerializeField] private float ambientMaxInterval = 30f;
    [SerializeField] private float ambientVolume = 0.3f;
    
    [Header("Random Events")]
    [SerializeField] private GameObject[] paranormalObjects; // Objects that randomly appear/activate
    [SerializeField] private float paranormalEventInterval = 45f;
    
    private float nextFlickerTime;
    private float nextAmbientTime;
    private float nextParanormalTime;
    
    void Start()
    {
        // Setup fog
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
        }
        
        // Initialize timers
        nextFlickerTime = Time.time + Random.Range(flickerMinInterval, flickerMaxInterval);
        nextAmbientTime = Time.time + Random.Range(ambientMinInterval, ambientMaxInterval);
        nextParanormalTime = Time.time + paranormalEventInterval;
        
        // Start continuous ambient sound
        if (ambientSources != null && ambientSources.Length > 0)
        {
            foreach (AudioSource source in ambientSources)
            {
                if (source != null && ambientClips.Length > 0)
                {
                    source.clip = ambientClips[Random.Range(0, ambientClips.Length)];
                    source.loop = true;
                    source.volume = ambientVolume;
                    source.Play();
                }
            }
        }
        
        // Hide paranormal objects initially
        if (paranormalObjects != null)
        {
            foreach (GameObject obj in paranormalObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
    
    void Update()
    {
        // Handle random light flickers
        if (enableRandomFlickers && Time.time >= nextFlickerTime)
        {
            TriggerRandomFlicker();
            nextFlickerTime = Time.time + Random.Range(flickerMinInterval, flickerMaxInterval);
        }
        
        // Handle random ambient sounds
        if (Time.time >= nextAmbientTime)
        {
            PlayRandomAmbientSound();
            nextAmbientTime = Time.time + Random.Range(ambientMinInterval, ambientMaxInterval);
        }
        
        // Handle paranormal events
        if (Time.time >= nextParanormalTime)
        {
            TriggerParanormalEvent();
            nextParanormalTime = Time.time + paranormalEventInterval;
        }
    }
    
    void TriggerRandomFlicker()
    {
        if (flickeringLights == null || flickeringLights.Length == 0) return;
        
        // Pick random light(s) to flicker
        int numLightsToFlicker = Random.Range(1, Mathf.Min(3, flickeringLights.Length + 1));
        
        for (int i = 0; i < numLightsToFlicker; i++)
        {
            Light randomLight = flickeringLights[Random.Range(0, flickeringLights.Length)];
            if (randomLight != null)
            {
                StartCoroutine(FlickerLight(randomLight));
            }
        }
    }
    
    IEnumerator FlickerLight(Light light)
    {
        if (light == null) yield break;
        
        float originalIntensity = light.intensity;
        bool wasEnabled = light.enabled;
        
        float elapsed = 0f;
        while (elapsed < flickerDuration)
        {
            // Random flicker pattern
            light.enabled = Random.value > 0.5f;
            light.intensity = Random.Range(0f, originalIntensity);
            
            yield return new WaitForSeconds(0.05f);
            elapsed += 0.05f;
        }
        
        // Restore original state
        light.enabled = wasEnabled;
        light.intensity = originalIntensity;
    }
    
    void PlayRandomAmbientSound()
    {
        if (ambientClips == null || ambientClips.Length == 0) return;
        
        AudioClip clip = ambientClips[Random.Range(0, ambientClips.Length)];
        
        // Play at random position around player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Vector3 soundPosition = player.Position + Random.insideUnitSphere * 15f;
            soundPosition.y = player.Position.y;
            AudioSource.PlayClipAtPoint(clip, soundPosition, ambientVolume);
        }
    }
    
    void TriggerParanormalEvent()
    {
        if (paranormalObjects == null || paranormalObjects.Length == 0) return;
        
        // Pick random paranormal object
        GameObject obj = paranormalObjects[Random.Range(0, paranormalObjects.Length)];
        if (obj != null)
        {
            StartCoroutine(ParanormalEventSequence(obj));
        }
        
        // Affect player sanity
        SanitySystem sanity = FindObjectOfType<SanitySystem>();
        if (sanity != null)
        {
            sanity.TriggerParanormalEvent();
        }
    }
    
    IEnumerator ParanormalEventSequence(GameObject obj)
    {
        // Activate object
        obj.SetActive(true);
        
        // Let it exist for a moment
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        
        // Deactivate
        obj.SetActive(false);
    }
    
    /// <summary>
    /// Manually trigger a light flicker
    /// </summary>
    public void FlickerSpecificLight(Light light, float duration = 1f)
    {
        if (light != null)
        {
            StartCoroutine(FlickerLightDuration(light, duration));
        }
    }
    
    IEnumerator FlickerLightDuration(Light light, float duration)
    {
        float originalIntensity = light.intensity;
        bool wasEnabled = light.enabled;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            light.enabled = Random.value > 0.3f;
            light.intensity = Random.Range(0f, originalIntensity * 1.2f);
            
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        light.enabled = wasEnabled;
        light.intensity = originalIntensity;
    }
    
    /// <summary>
    /// Gradually dim all lights over time
    /// </summary>
    public void DimAllLights(float targetIntensity, float duration)
    {
        StartCoroutine(DimLightsCoroutine(targetIntensity, duration));
    }
    
    IEnumerator DimLightsCoroutine(float targetIntensity, float duration)
    {
        Light[] allLights = FindObjectsOfType<Light>();
        float[] originalIntensities = new float[allLights.Length];
        
        for (int i = 0; i < allLights.Length; i++)
        {
            originalIntensities[i] = allLights[i].intensity;
        }
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            for (int i = 0; i < allLights.Length; i++)
            {
                if (allLights[i] != null)
                {
                    allLights[i].intensity = Mathf.Lerp(originalIntensities[i], targetIntensity, t);
                }
            }
            
            yield return null;
        }
    }
}

/// <summary>
/// Individual flickering light behavior
/// Attach to individual light objects for continuous flickering
/// </summary>
public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private Light lightComponent;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private bool randomFlicker = true;
    
    private float originalIntensity;
    private float nextFlickerTime;
    
    void Start()
    {
        if (lightComponent == null)
        {
            lightComponent = GetComponent<Light>();
        }
        
        if (lightComponent != null)
        {
            originalIntensity = lightComponent.intensity;
        }
    }
    
    void Update()
    {
        if (lightComponent == null) return;
        
        if (randomFlicker)
        {
            if (Time.time >= nextFlickerTime)
            {
                lightComponent.intensity = Random.Range(minIntensity, maxIntensity);
                nextFlickerTime = Time.time + flickerSpeed;
            }
        }
        else
        {
            // Smooth flickering
            lightComponent.intensity = Mathf.Lerp(
                minIntensity, 
                maxIntensity, 
                (Mathf.Sin(Time.time * (1f / flickerSpeed)) + 1f) / 2f
            );
        }
    }
}

/// <summary>
/// Ambient sound zone that plays sounds when player enters
/// </summary>
public class AmbientSoundZone : MonoBehaviour
{
    [SerializeField] private AudioClip[] zoneSounds;
    [SerializeField] private float minInterval = 5f;
    [SerializeField] private float maxInterval = 15f;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool playOnEnter = true;
    
    private bool playerInZone = false;
    private float nextSoundTime;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            
            if (playOnEnter && zoneSounds.Length > 0)
            {
                PlayRandomSound();
            }
            
            nextSoundTime = Time.time + Random.Range(minInterval, maxInterval);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }
    
    void Update()
    {
        if (playerInZone && Time.time >= nextSoundTime)
        {
            PlayRandomSound();
            nextSoundTime = Time.time + Random.Range(minInterval, maxInterval);
        }
    }
    
    void PlayRandomSound()
    {
        if (zoneSounds == null || zoneSounds.Length == 0) return;
        
        AudioClip clip = zoneSounds[Random.Range(0, zoneSounds.Length)];
        AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }
}
