// Timer.cs — Updated: Sudden Death infinite timer UI (assignable GameObject)
// Perubahan:
//   - Tambah [SerializeField] GameObject infiniteTimerUI → assign sendiri di Inspector
//   - Saat Sudden Death: timerText disembunyikan, infiniteTimerUI ditampilkan
//   - Saat kembali normal / reset: infiniteTimerUI disembunyikan, timerText kembali tampil
// File ini menggantikan: Timer Script/Timer.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject timeUpUI;

    [Header("Sudden Death UI")]
    [Tooltip("Assign GameObject UI infinite timer di sini (Image/sprite ∞). Akan tampil saat Sudden Death, timerText akan disembunyikan.")]
    [SerializeField] GameObject infiniteTimerUI;

    [Header("Player 1 References")]
    [SerializeField] PlayerMovement player1Movement;
    [SerializeField] PlayeJump2D player1Jump;
    [SerializeField] PlayerShooting player1Shooting;
    [SerializeField] PlayerInput player1Input;
    [SerializeField] Health player1Health;

    [Header("Player 2 References")]
    [SerializeField] PlayerMovement player2Movement;
    [SerializeField] PlayeJump2D player2Jump;
    [SerializeField] PlayerShooting player2Shooting;
    [SerializeField] PlayerInput player2Input;
    [SerializeField] HealthP2 player2Health;

    [Header("Player 3 References")]
    [SerializeField] PlayerMovement player3Movement;
    [SerializeField] PlayeJump2D player3Jump;
    [SerializeField] PlayerShooting player3Shooting;
    [SerializeField] PlayerInput player3Input;
    [SerializeField] HealthP3 player3Health;

    [Header("Player 4 References")]
    [SerializeField] PlayerMovement player4Movement;
    [SerializeField] PlayeJump2D player4Jump;
    [SerializeField] PlayerShooting player4Shooting;
    [SerializeField] PlayerInput player4Input;
    [SerializeField] HealthP4 player4Health;

    [Header("Game Manager Reference")]
    [SerializeField] GameManager gameManager;

    private float remainingTime;
    private bool isTimerStarted  = false;
    private bool isTimeUp        = false;
    private bool isGameOver      = false;
    private bool isSuddenDeath   = false;
    private AudioManager audioManager;
    private int activePlayers = 2;

    private void Start()
    {
        if (timeUpUI != null)       timeUpUI.SetActive(false);
        if (infiniteTimerUI != null) infiniteTimerUI.SetActive(false);

        remainingTime  = PlayerPrefs.GetFloat("SelectedGameTime", 30f);
        isTimerStarted = false;

        audioManager = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();
        if (audioManager == null)
            Debug.LogWarning("AudioManager not found.");

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
                Debug.LogWarning("GameManager not found.");
        }

        activePlayers = GameData.Instance != null ? GameData.Instance.numberOfPlayers : 2;

        FindPlayerComponents();
    }

    void FindPlayerComponents()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        if (player1 != null)
        {
            if (player1Movement == null) player1Movement = player1.GetComponent<PlayerMovement>();
            if (player1Jump == null)     player1Jump     = player1.GetComponent<PlayeJump2D>();
            if (player1Shooting == null) player1Shooting = player1.GetComponent<PlayerShooting>();
            if (player1Input == null)    player1Input    = player1.GetComponent<PlayerInput>();
            if (player1Health == null)   player1Health   = player1.GetComponent<Health>();
        }

        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null)
        {
            if (player2Movement == null) player2Movement = player2.GetComponent<PlayerMovement>();
            if (player2Jump == null)     player2Jump     = player2.GetComponent<PlayeJump2D>();
            if (player2Shooting == null) player2Shooting = player2.GetComponent<PlayerShooting>();
            if (player2Input == null)    player2Input    = player2.GetComponent<PlayerInput>();
            if (player2Health == null)   player2Health   = player2.GetComponent<HealthP2>();
        }

        if (activePlayers >= 3 && player3Health != null)
        {
            if (player3Movement == null) player3Movement = player3Health.GetComponent<PlayerMovement>();
            if (player3Jump == null)     player3Jump     = player3Health.GetComponent<PlayeJump2D>();
            if (player3Shooting == null) player3Shooting = player3Health.GetComponent<PlayerShooting>();
            if (player3Input == null)    player3Input    = player3Health.GetComponent<PlayerInput>();
        }

        if (activePlayers >= 4 && player4Health != null)
        {
            if (player4Movement == null) player4Movement = player4Health.GetComponent<PlayerMovement>();
            if (player4Jump == null)     player4Jump     = player4Health.GetComponent<PlayeJump2D>();
            if (player4Shooting == null) player4Shooting = player4Health.GetComponent<PlayerShooting>();
            if (player4Input == null)    player4Input    = player4Health.GetComponent<PlayerInput>();
        }
    }

    void Update()
    {
        if (!isTimerStarted) return;

        // Saat Sudden Death: tidak countdown, infiniteTimerUI sudah aktif
        if (isSuddenDeath)
        {
            CheckGameState();
            return;
        }

        CheckGameState();

        if (isTimeUp || isGameOver) return;

        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else if (remainingTime <= 0)
        {
            remainingTime = 0;
            if (timerText != null) timerText.color = Color.red;
            if (!isTimeUp) TimesUp();
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        if (timerText != null) timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void CheckGameState()
    {
        if (isGameOver) return;

        if (gameManager != null)
        {
            bool anyWinUIActive =
                (gameManager.player1WinsUI != null && gameManager.player1WinsUI.activeSelf) ||
                (gameManager.player2WinsUI != null && gameManager.player2WinsUI.activeSelf) ||
                (gameManager.player3WinsUI != null && gameManager.player3WinsUI.activeSelf) ||
                (gameManager.player4WinsUI != null && gameManager.player4WinsUI.activeSelf) ||
                (gameManager.drawUI        != null && gameManager.drawUI.activeSelf);

            if (anyWinUIActive)
            {
                isGameOver     = true;
                isTimerStarted = false;
                if (timerText != null) timerText.color = Color.red;
            }
        }
    }

    void TimesUp()
    {
        isTimeUp = true;
        if (timeUpUI != null) timeUpUI.SetActive(true);
        DetermineRoundWinnerOnTimeout();
        Debug.Log("Time's Up! Round Over!");
    }

    void DetermineRoundWinnerOnTimeout()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager reference missing in Timer.");
            DisableAllPlayerInputs();
            return;
        }

        float hp1 = (player1Health != null) ? player1Health.currentHealth : -1f;
        float hp2 = (player2Health != null) ? player2Health.currentHealth : -1f;
        float hp3 = (activePlayers >= 3 && player3Health != null) ? player3Health.currentHealth : -1f;
        float hp4 = (activePlayers >= 4 && player4Health != null) ? player4Health.currentHealth : -1f;

        float maxHP = hp1;
        if (hp2 > maxHP) maxHP = hp2;
        if (activePlayers >= 3 && hp3 > maxHP) maxHP = hp3;
        if (activePlayers >= 4 && hp4 > maxHP) maxHP = hp4;

        List<int> winners = new List<int>();
        if (hp1 >= maxHP && hp1 >= 0) winners.Add(1);
        if (hp2 >= maxHP && hp2 >= 0) winners.Add(2);
        if (activePlayers >= 3 && hp3 >= maxHP && hp3 >= 0) winners.Add(3);
        if (activePlayers >= 4 && hp4 >= maxHP && hp4 >= 0) winners.Add(4);

        Debug.Log($"[Timer] Timeout — HP: P1={hp1} P2={hp2}" +
                  (activePlayers >= 3 ? $" P3={hp3}" : "") +
                  (activePlayers >= 4 ? $" P4={hp4}" : "") +
                  $" → MaxHP={maxHP}, Winners=[{string.Join(",", winners)}]");

        DisableAllPlayerInputs();
        gameManager.EndRoundTimeout(winners.ToArray());

        if (audioManager != null) audioManager.PlaySFX(audioManager.gameOver);
        if (timeUpUI != null) timeUpUI.SetActive(false);
    }

    void DisableAllPlayerInputs()
    {
        DisablePlayerComponents(player1Movement, player1Jump, player1Shooting, player1Input);
        DisablePlayerComponents(player2Movement, player2Jump, player2Shooting, player2Input);
        if (activePlayers >= 3)
            DisablePlayerComponents(player3Movement, player3Jump, player3Shooting, player3Input);
        if (activePlayers >= 4)
            DisablePlayerComponents(player4Movement, player4Jump, player4Shooting, player4Input);
    }

    void DisablePlayerComponents(PlayerMovement movement, PlayeJump2D jump, PlayerShooting shooting, PlayerInput input)
    {
        if (movement != null) movement.enabled = false;
        if (jump != null)     jump.enabled     = false;
        if (shooting != null) shooting.enabled = false;
        if (input != null)    input.enabled    = false;
    }

    public void StartTimer()
    {
        isTimerStarted = true;
        isTimeUp       = false;
        remainingTime  = PlayerPrefs.GetFloat("SelectedGameTime", 30f);
        if (timerText != null) timerText.color = Color.white;
    }

    public void StopTimer()
    {
        isTimerStarted = false;
        isTimeUp       = false;
    }

    public void ResetTimer()
    {
        remainingTime = PlayerPrefs.GetFloat("SelectedGameTime", 30f);
        isTimeUp      = false;
        isGameOver    = false;
        isSuddenDeath = false;

        if (timerText != null)
        {
            timerText.color = Color.white;
            timerText.gameObject.SetActive(true);
        }
        if (infiniteTimerUI != null) infiniteTimerUI.SetActive(false);
        if (timeUpUI != null)        timeUpUI.SetActive(false);
    }

    // ─── Sudden Death Mode ────────────────────────────────────────────────────
    public void SetSuddenDeathMode(bool active)
    {
        isSuddenDeath = active;

        if (active)
        {
            isTimerStarted = true;
            isTimeUp       = false;
            isGameOver     = false;

            // Sembunyikan timerText, tampilkan infiniteTimerUI
            if (timerText != null)        timerText.gameObject.SetActive(false);
            if (infiniteTimerUI != null)  infiniteTimerUI.SetActive(true);
        }
        else
        {
            // Kembalikan timerText, sembunyikan infiniteTimerUI
            if (timerText != null)        timerText.gameObject.SetActive(true);
            if (infiniteTimerUI != null)  infiniteTimerUI.SetActive(false);
        }
    }
}