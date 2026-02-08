using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Main game manager handling HUD, inventory, game state, and progression
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Text interactionText;
    [SerializeField] private Text messageText;
    [SerializeField] private Text batteryText;
    [SerializeField] private Text sanityText;
    [SerializeField] private Image batteryBar;
    [SerializeField] private Image sanityBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private Text documentTitleText;
    [SerializeField] private Text documentContentText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Text evidenceCountText;
    
    [Header("Game Settings")]
    [SerializeField] private int totalEvidenceRequired = 5;
    [SerializeField] private int generatorsRequired = 3;
    [SerializeField] private string goodEndingScene = "GoodEnding";
    [SerializeField] private string badEndingScene = "BadEnding";
    
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mainTheme;
    [SerializeField] private AudioClip chaseTheme;
    [SerializeField] private AudioClip gameOverMusic;
    
    // Private variables
    private HashSet<string> collectedKeys = new HashSet<string>();
    private HashSet<string> collectedEvidence = new HashSet<string>();
    private HashSet<string> activatedGenerators = new HashSet<string>();
    private PlayerController player;
    private SanitySystem sanitySystem;
    private EnemyAI enemy;
    private bool isPaused = false;
    private bool isGameOver = false;
    private float messageTimer = 0f;
    private string currentMessage = "";
    
    void Start()
    {
        // Get references
        player = FindObjectOfType<PlayerController>();
        sanitySystem = FindObjectOfType<SanitySystem>();
        enemy = FindObjectOfType<EnemyAI>();
        
        // Initialize UI
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
        
        if (documentPanel != null)
        {
            documentPanel.SetActive(false);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        // Start music
        if (musicSource != null && mainTheme != null)
        {
            musicSource.clip = mainTheme;
            musicSource.loop = true;
            musicSource.Play();
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Update HUD
        UpdateHUD();
        
        // Handle pause
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
        
        // Handle document closing
        if (documentPanel != null && documentPanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            CloseDocument();
        }
        
        // Update message timer
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && messageText != null)
            {
                messageText.text = "";
            }
        }
        
        // Update music based on enemy state
        UpdateMusic();
        
        // Check win condition
        CheckWinCondition();
    }
    
    void UpdateHUD()
    {
        if (player == null) return;
        
        // Battery
        if (batteryText != null)
        {
            batteryText.text = $"Battery: {Mathf.RoundToInt(player.CurrentBattery)}%";
        }
        
        if (batteryBar != null)
        {
            batteryBar.fillAmount = player.CurrentBattery / 100f;
            
            // Color code battery
            if (player.CurrentBattery < 20f)
            {
                batteryBar.color = Color.red;
            }
            else if (player.CurrentBattery < 50f)
            {
                batteryBar.color = Color.yellow;
            }
            else
            {
                batteryBar.color = Color.green;
            }
        }
        
        // Sanity
        if (sanitySystem != null)
        {
            if (sanityText != null)
            {
                sanityText.text = $"Sanity: {Mathf.RoundToInt(sanitySystem.CurrentSanity)}%";
            }
            
            if (sanityBar != null)
            {
                sanityBar.fillAmount = sanitySystem.SanityPercentage;
                
                // Color code sanity
                if (sanitySystem.SanityPercentage < 0.3f)
                {
                    sanityBar.color = Color.red;
                }
                else if (sanitySystem.SanityPercentage < 0.6f)
                {
                    sanityBar.color = Color.yellow;
                }
                else
                {
                    sanityBar.color = Color.cyan;
                }
            }
        }
        
        // Stamina
        if (staminaBar != null)
        {
            staminaBar.fillAmount = player.CurrentStamina / 100f;
        }
        
        // Evidence count
        if (evidenceCountText != null)
        {
            evidenceCountText.text = $"Evidence: {collectedEvidence.Count}/{totalEvidenceRequired}";
        }
    }
    
    void UpdateMusic()
    {
        if (enemy != null && musicSource != null)
        {
            if (enemy.IsHunting && musicSource.clip != chaseTheme)
            {
                musicSource.clip = chaseTheme;
                musicSource.Play();
            }
            else if (!enemy.IsHunting && musicSource.clip != mainTheme)
            {
                musicSource.clip = mainTheme;
                musicSource.Play();
            }
        }
    }
    
    public void AddKey(string keyID)
    {
        collectedKeys.Add(keyID);
    }
    
    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }
    
    public void AddEvidence(string evidenceID)
    {
        if (!collectedEvidence.Contains(evidenceID))
        {
            collectedEvidence.Add(evidenceID);
            ShowMessage($"Evidence collected ({collectedEvidence.Count}/{totalEvidenceRequired})");
        }
    }
    
    public void OnGeneratorActivated(string generatorID)
    {
        if (!activatedGenerators.Contains(generatorID))
        {
            activatedGenerators.Add(generatorID);
            ShowMessage($"Generator activated ({activatedGenerators.Count}/{generatorsRequired})");
        }
    }
    
    public void ShowMessage(string message, float duration = 3f)
    {
        currentMessage = message;
        messageTimer = duration;
        
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
    
    public void ShowDocument(string title, string content)
    {
        if (documentPanel == null) return;
        
        documentPanel.SetActive(true);
        
        if (documentTitleText != null)
        {
            documentTitleText.text = title;
        }
        
        if (documentContentText != null)
        {
            documentContentText.text = content;
        }
        
        // Pause game
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (player != null)
        {
            player.SetCanMove(false);
        }
    }
    
    public void CloseDocument()
    {
        if (documentPanel != null)
        {
            documentPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (player != null)
        {
            player.SetCanMove(true);
        }
    }
    
    void CheckWinCondition()
    {
        // Check if all generators are activated and enough evidence is collected
        if (activatedGenerators.Count >= generatorsRequired)
        {
            // Player has restored power - can now escape
            // This could trigger an exit door to unlock, etc.
            // For now, we'll check for evidence count for different endings
        }
    }
    
    public void GameOver(bool playerWon)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Stop game
        Time.timeScale = 0f;
        
        // Determine ending based on evidence
        if (playerWon)
        {
            if (collectedEvidence.Count >= totalEvidenceRequired)
            {
                if (gameOverText != null)
                {
                    gameOverText.text = "YOU ESCAPED!\n\nYou collected enough evidence to expose the truth about The Echoing Asylum.\n\nGOOD ENDING";
                }
            }
            else
            {
                if (gameOverText != null)
                {
                    gameOverText.text = "YOU ESCAPED!\n\nBut without enough evidence, the truth remains buried...\n\nNEUTRAL ENDING";
                }
            }
        }
        else
        {
            if (gameOverText != null)
            {
                gameOverText.text = "YOU DIED\n\nThe asylum has claimed another victim...\n\nPress R to Restart";
            }
        }
        
        // Change music
        if (musicSource != null && gameOverMusic != null)
        {
            musicSource.clip = gameOverMusic;
            musicSource.loop = false;
            musicSource.Play();
        }
    }
    
    void TogglePause()
    {
        isPaused = !isPaused;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        
        Time.timeScale = isPaused ? 0f : 1f;
        
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void ResumeGame()
    {
        TogglePause();
    }
}
