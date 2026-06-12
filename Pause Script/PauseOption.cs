using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button continueButton2;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private int mainMenuSceneIndex = 0;
    [SerializeField] private int[] gameSceneIndices;

    [Header("Player References")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    // References to player movement and shooting scripts
    private PlayerMovement player1Movement;
    private PlayerMovement player2Movement;
    private PlayerShooting player1Shooting;
    private PlayerShooting player2Shooting;
    private PlayeJump2D player1Jump;
    private PlayeJump2D player2Jump;

    private bool isPaused = false;

    // Static variables to persist between scenes
    private static bool isReturningFromMenu = false;
    private static int lastGameSceneIndex = -1;

    private void Awake()
    {
        // When scene is loaded, check if returning from menu
        if (isReturningFromMenu)
        {
            // Reset the flag
            isReturningFromMenu = false;
            // Ensure the game is not paused when returning
            Time.timeScale = 1f;
        }

        // Store current scene index for possible later use
        lastGameSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void Start()
    {
        // Make sure pause menu is not active when game starts
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Add listener for continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ResumeGame);
        }

        // Add listener for continue button 2
        if (continueButton2 != null)
        {
            continueButton2.onClick.AddListener(ResumeGame);
        }

        // Add listener for pause button if available
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }

        // Add listener for main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        // Get references to player components
        if (player1 != null)
        {
            player1Movement = player1.GetComponent<PlayerMovement>();
            player1Shooting = player1.GetComponent<PlayerShooting>();
            player1Jump = player1.GetComponent<PlayeJump2D>();
        }

        if (player2 != null)
        {
            player2Movement = player2.GetComponent<PlayerMovement>();
            player2Shooting = player2.GetComponent<PlayerShooting>();
            player2Jump = player2.GetComponent<PlayeJump2D>();
        }

        // Ensure time scale is set to 1 at the start of any scene
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Check if ESC key is pressed
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Toggle pause status of the game
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pause the game and show pause menu
    /// </summary>
    public void PauseGame()
    {
        // Set time scale to 0 to pause the game
        Time.timeScale = 0f;

        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Disable player movement and shooting
        DisablePlayerControls();

        isPaused = true;
    }

    /// <summary>
    /// Resume the game and hide pause menu
    /// </summary>
    public void ResumeGame()
    {
        // Set time scale back to 1 to resume the game
        Time.timeScale = 1f;

        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Enable player movement and shooting
        EnablePlayerControls();

        isPaused = false;
    }

    /// <summary>
    /// Return to the main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Set the flag indicating we're returning from menu
        isReturningFromMenu = true;

        // Resume time scale before loading new scene
        Time.timeScale = 1f;

        // Reset pause state
        isPaused = false;

        // Load the main menu scene using index
        try
        {
            SceneManager.LoadScene(mainMenuSceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load main menu scene: " + e.Message);
            // As a fallback, just resume the game if we can't load the main menu
            ResumeGame();
        }
    }

    /// <summary>
    /// Disable player controls when game is paused
    /// </summary>
    private void DisablePlayerControls()
    {
        // Disable player 1 controls
        if (player1Movement != null)
        {
            player1Movement.enabled = false;
        }

        if (player1Shooting != null)
        {
            player1Shooting.enabled = false;
        }
        if (player1Jump != null)
        {
            player1Jump.enabled = false;
        }

        // Disable player 2 controls
        if (player2Movement != null)
        {
            player2Movement.enabled = false;
        }

        if (player2Shooting != null)
        {
            player2Shooting.enabled = false;
        }

        if (player2Jump != null)
        {
            player2Jump.enabled = false;
        }
    }

    /// <summary>
    /// Enable player controls when game is resumed
    /// </summary>
    private void EnablePlayerControls()
    {
        // Enable player 1 controls
        if (player1Movement != null)
        {
            player1Movement.enabled = true;
        }

        if (player1Shooting != null)
        {
            player1Shooting.enabled = true;
        }
        if (player1Jump != null)
        {
            player1Jump.enabled = true;
        }

        // Enable player 2 controls
        if (player2Movement != null)
        {
            player2Movement.enabled = true;
        }

        if (player2Shooting != null)
        {
            player2Shooting.enabled = true;
        }

        if (player2Jump != null)
        {
            player2Jump.enabled = true;
        }
    }

    /// <summary>
    /// Method to call when loading a specific game scene from the main menu
    /// </summary>
    /// <param name="sceneIndex">The index of the game scene to load</param>
    public static void LoadGameScene(int sceneIndex)
    {
        // Set flag that we're returning to a game
        isReturningFromMenu = true;

        // Force time scale to 1
        Time.timeScale = 1f;

        // Load the requested scene
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Return to the last played game scene from main menu
    /// </summary>
    public static void ReturnToLastGameScene()
    {
        if (lastGameSceneIndex > 0)
        {
            LoadGameScene(lastGameSceneIndex);
        }
        else
        {
            // If no previous scene was recorded, use the first game scene
            Debug.LogWarning("No previous game scene recorded, loading default");
            SceneManager.LoadScene(1); // Assuming the first game scene is at index 1
        }
    }

    /// <summary>
    /// Add this method to a button in your main menu to load Map 1
    /// </summary>
    public void LoadMap1()
    {
        if (gameSceneIndices != null && gameSceneIndices.Length > 0)
        {
            LoadGameScene(gameSceneIndices[0]);
        }
        else
        {
            Debug.LogError("Map 1 scene index not configured in gameSceneIndices array");
        }
    }

    /// <summary>
    /// Add this method to a button in your main menu to load Map 2
    /// </summary>
    public void LoadMap2()
    {
        if (gameSceneIndices != null && gameSceneIndices.Length > 1)
        {
            LoadGameScene(gameSceneIndices[1]);
        }
        else
        {
            Debug.LogError("Map 2 scene index not configured in gameSceneIndices array");
        }
    }

    /// <summary>
    /// For use in OnApplicationQuit or other places where you want to reset the statics
    /// </summary>
    private void OnDestroy()
    {
        // Make sure time scale is reset when the manager is destroyed
        Time.timeScale = 1f;
    }

    private void OnApplicationQuit()
    {
        // Reset static variables when application quits
        isReturningFromMenu = false;
        lastGameSceneIndex = -1;
        Time.timeScale = 1f;
    }
}