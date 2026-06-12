// GameManager.cs — Updated: Sudden Death Round support + Bug fixes
// Aturan:
//   - Menang 3 round = menang game.
//   - 2P → max round 5 | 3P → max round 7 | 4P → max round 9
//   - Sudden Death dipicu saat waktu habis dan ada >=2 pemain yang tied di
//     TEPAT 2 kemenangan (== SUDDEN_DEATH_WINS) dengan HP tertinggi yang sama.
//     Pemain dengan win count berbeda dari nilai tied tersebut TIDAK ikut.
//   - Pada Sudden Death:
//       * Hanya peserta Sudden Death (tied 2 win) yang bisa bergerak (knife only).
//       * Pemain lain → sprite mati, tidak bisa bergerak, HP = 0.
//       * HP peserta di-reset ke 3. PowerUp tidak spawn. Timer infinite.
//       * Teks round = "Sudden Death".
//       * Pemenang Sudden Death = pemenang game.
// File ini menggantikan: Pause Script/GameManger.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ─── Win / Game Over UI ───────────────────────────────────────────────────
    [Header("Win UI")]
    public GameObject player1WinsUI;
    public GameObject player2WinsUI;
    public GameObject player3WinsUI;
    public GameObject player4WinsUI;
    public GameObject drawUI;

    // ─── Round Text & Fight Text ──────────────────────────────────────────────
    [Header("Round / Fight UI")]
    public GameObject      roundTextUI;
    public TextMeshProUGUI roundText;
    public GameObject      fightTextUI;

    // ─── Sudden Death UI ──────────────────────────────────────────────────────
    [Header("Sudden Death UI")]
    [Tooltip("Assign UI GameObject bertuliskan 'Sudden Death' (opsional, bisa null)")]
    public GameObject suddenDeathTextUI;

    // ─── Health References ────────────────────────────────────────────────────
    [Header("Player Health")]
    public Health   player1Health;
    public HealthP2 player2Health;
    public HealthP3 player3Health;
    public HealthP4 player4Health;

    // ─── Player Root GameObjects ──────────────────────────────────────────────
    [Header("Player Root GameObjects")]
    public GameObject player3Root;
    public GameObject player4Root;

    // ─── Shooting References ──────────────────────────────────────────────────
    [Header("Player Shooting")]
    public PlayerShooting player1Shooting;
    public PlayerShooting player2Shooting;
    public PlayerShooting player3Shooting;
    public PlayerShooting player4Shooting;

    // ─── Healthbar UI ─────────────────────────────────────────────────────────
    [Header("Healthbar UI (P3 & P4)")]
    public GameObject healthbarP3UI;
    public GameObject healthbarP4UI;

    // ─── Ammo UI ──────────────────────────────────────────────────────────────
    [Header("Ammo UI (P3 & P4)")]
    public GameObject ammoUIP3;
    public GameObject ammoUIP4;

    // ─── PowerUp Timer UI ─────────────────────────────────────────────────────
    [Header("PowerUp Timer UI (P3 & P4)")]
    public GameObject powerUpTimerUIP3;
    public GameObject powerUpTimerUIP4;

    // ─── Weapon Icon UI ───────────────────────────────────────────────────────
    [Header("Weapon Icon UI (P3 & P4)")]
    public GameObject weaponUIP3;
    public GameObject weaponUIP4;

    // ─── Round Box UI ─────────────────────────────────────────────────────────
    [Header("Round Box UI — P1")]
    public Image[]    player1RoundBoxes;
    public GameObject player1RoundBorder;

    [Header("Round Box UI — P2")]
    public Image[]    player2RoundBoxes;
    public GameObject player2RoundBorder;

    [Header("Round Box UI — P3 (hide jika 2P)")]
    public Image[]    player3RoundBoxes;
    public GameObject player3RoundBorder;

    [Header("Round Box UI — P4 (hide jika 2P/3P)")]
    public Image[]    player4RoundBoxes;
    public GameObject player4RoundBorder;

    [Header("Round Box Colors")]
    public Color roundBoxWonColor   = Color.yellow;
    public Color roundBoxEmptyColor = Color.gray;

    // ─── Round Audio ──────────────────────────────────────────────────────────
    [Header("Round Audio")]
    public AudioClip round1Sound;
    public AudioClip round2Sound;
    public AudioClip round3Sound;
    public AudioClip round4Sound;
    public AudioClip round5Sound;
    public AudioClip round6Sound;
    public AudioClip round7Sound;
    public AudioClip round8Sound;
    public AudioClip finalRoundSound;
    public AudioClip suddenDeathSound;

    // ─── State ────────────────────────────────────────────────────────────────
    private int[] roundsWon    = new int[5]; // index 1-4
    private int   currentRound = 1;
    public  int   CurrentRound => currentRound;
    private int   maxRounds;

    private const int ROUNDS_TO_WIN     = 3; // butuh 3 kemenangan untuk menang game
    private const int SUDDEN_DEATH_WINS = 2; // tepat 2 kemenangan → kandidat Sudden Death

    private bool gameEnded     = false;
    private bool roundEnded    = false;
    private bool isSuddenDeath = false;

    // Daftar index pemain yang masuk Sudden Death (1-4)
    private List<int> suddenDeathPlayers = new List<int>();

    private Vector3[]    startPositions = new Vector3[5];
    private AudioManager audioManager;
    private int          activePlayers;
    private PowerUpManager powerUpManager;

    // ─── Start ────────────────────────────────────────────────────────────────
    void Start()
    {
        activePlayers = GameData.Instance != null ? GameData.Instance.numberOfPlayers : 2;

        // 2P=5, 3P=7, 4P=9
        maxRounds = (activePlayers == 2) ? 5 : (activePlayers == 3) ? 7 : 9;

        SetupPlayerVisibility();

        HideUI(drawUI, player1WinsUI, player2WinsUI, player3WinsUI, player4WinsUI,
               roundTextUI, fightTextUI);
        if (suddenDeathTextUI != null) suddenDeathTextUI.SetActive(false);

        SaveStartPositions();

        audioManager   = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();
        powerUpManager = FindObjectOfType<PowerUpManager>();

        InitRoundBoxes();
        StartCoroutine(StartRound());
    }

    private void HideUI(params GameObject[] objs)
    {
        foreach (var o in objs) if (o != null) o.SetActive(false);
    }

    private void SaveStartPositions()
    {
        if (player1Health != null) startPositions[1] = player1Health.transform.position;
        if (player2Health != null) startPositions[2] = player2Health.transform.position;
        if (activePlayers >= 3 && player3Health != null) startPositions[3] = player3Health.transform.position;
        if (activePlayers >= 4 && player4Health != null) startPositions[4] = player4Health.transform.position;
    }

    // ─── Player Visibility ────────────────────────────────────────────────────
    private void SetupPlayerVisibility()
    {
        bool hasP3 = activePlayers >= 3;
        bool hasP4 = activePlayers >= 4;

        SetActive(player3Root,        hasP3);
        SetActive(player4Root,        hasP4);
        SetActive(healthbarP3UI,      hasP3);
        SetActive(healthbarP4UI,      hasP4);
        SetActive(ammoUIP3,           hasP3);
        SetActive(ammoUIP4,           hasP4);
        SetActive(powerUpTimerUIP3,   hasP3);
        SetActive(powerUpTimerUIP4,   hasP4);
        SetActive(weaponUIP3,         hasP3);
        SetActive(weaponUIP4,         hasP4);
        SetActive(player3RoundBorder, hasP3);
        SetActive(player4RoundBorder, hasP4);
        SetImagesActive(player3RoundBoxes, hasP3);
        SetImagesActive(player4RoundBoxes, hasP4);

        if (!hasP3) { SetActive(player3Health?.gameObject, false); SetActive(player3Shooting?.gameObject, false); }
        if (!hasP4) { SetActive(player4Health?.gameObject, false); SetActive(player4Shooting?.gameObject, false); }
    }

    private void SetActive(GameObject go, bool active) { if (go != null) go.SetActive(active); }
    private void SetImagesActive(Image[] arr, bool active)
    {
        if (arr == null) return;
        foreach (var img in arr) if (img != null) img.gameObject.SetActive(active);
    }

    // ─── LateUpdate ──────────────────────────────────────────────────────────
    void LateUpdate()
    {
        // Saat Sudden Death: roundEnded dari siklus sebelumnya tidak relevan,
        // cukup cek gameEnded saja agar CheckRoundEnd tetap jalan.
        if (isSuddenDeath)
        {
            if (!gameEnded) CheckRoundEnd();
        }
        else
        {
            if (!roundEnded && !gameEnded) CheckRoundEnd();
        }
    }

    // ─── ReportPlayerDeath ────────────────────────────────────────────────────
    // Dipanggil oleh Health scripts saat lives habis.
    public void ReportPlayerDeath(int playerNumber)
    {
        if (gameEnded) return;

        if (isSuddenDeath)
        {
            // Abaikan non-peserta (mereka sudah di-eliminate via SetHealthToZero)
            if (!suddenDeathPlayers.Contains(playerNumber))
            {
                Debug.Log($"[GM] P{playerNumber} bukan peserta SD, kematian diabaikan.");
                return;
            }

            Debug.Log($"[GM] SD: P{playerNumber} mati. Cek survivor...");
            int aliveSD = CountSuddenDeathSurvivors();
            Debug.Log($"[GM] SD: sisa {aliveSD} peserta hidup.");
            if (aliveSD <= 1)
            {
                int winner = FindLastSuddenDeathSurvivor(playerNumber);
                Debug.Log($"[GM] SD: pemenang = P{winner}");
                EndSuddenDeathRound(winner);
            }
        }
        else
        {
            if (roundEnded) return;
            if (CountAlivePlayers() <= 1)
                EndRound(FindLastAlivePlayer(playerNumber));
        }
    }

    // ─── Count / Find helpers ─────────────────────────────────────────────────

    // Hitung pemain aktif yang masih hidup (normal round)
    private int CountAlivePlayers()
    {
        int alive = 0;
        if (player1Health != null && !player1Health.IsOutOfLives()) alive++;
        if (player2Health != null && !player2Health.IsOutOfLives()) alive++;
        if (activePlayers >= 3 && player3Health != null && !player3Health.IsOutOfLives()) alive++;
        if (activePlayers >= 4 && player4Health != null && !player4Health.IsOutOfLives()) alive++;
        return alive;
    }

    // Cari satu-satunya pemain yang masih hidup, selain yang baru mati
    private int FindLastAlivePlayer(int justDied)
    {
        if (justDied != 1 && player1Health != null && !player1Health.IsOutOfLives()) return 1;
        if (justDied != 2 && player2Health != null && !player2Health.IsOutOfLives()) return 2;
        if (activePlayers >= 3 && justDied != 3 && player3Health != null && !player3Health.IsOutOfLives()) return 3;
        if (activePlayers >= 4 && justDied != 4 && player4Health != null && !player4Health.IsOutOfLives()) return 4;
        return 0;
    }

    // Hitung peserta Sudden Death yang masih hidup
    private int CountSuddenDeathSurvivors()
    {
        int alive = 0;
        foreach (int p in suddenDeathPlayers)
        {
            if (p == 1 && player1Health != null && !player1Health.IsOutOfLives()) alive++;
            else if (p == 2 && player2Health != null && !player2Health.IsOutOfLives()) alive++;
            else if (p == 3 && player3Health != null && !player3Health.IsOutOfLives()) alive++;
            else if (p == 4 && player4Health != null && !player4Health.IsOutOfLives()) alive++;
        }
        return alive;
    }

    // Cari peserta Sudden Death terakhir yang masih hidup
    private int FindLastSuddenDeathSurvivor(int justDied)
    {
        foreach (int p in suddenDeathPlayers)
        {
            if (p == justDied) continue;
            if (p == 1 && player1Health != null && !player1Health.IsOutOfLives()) return 1;
            if (p == 2 && player2Health != null && !player2Health.IsOutOfLives()) return 2;
            if (p == 3 && player3Health != null && !player3Health.IsOutOfLives()) return 3;
            if (p == 4 && player4Health != null && !player4Health.IsOutOfLives()) return 4;
        }
        return 0;
    }

    // ─── CheckRoundEnd (LateUpdate) ───────────────────────────────────────────
    private void CheckRoundEnd()
    {
        if (gameEnded) return;

        if (isSuddenDeath)
        {
            int aliveSD = CountSuddenDeathSurvivors();
            if (aliveSD == 1)
            {
                int winner = 0;
                foreach (int p in suddenDeathPlayers)
                {
                    bool alive = false;
                    if (p == 1 && player1Health != null && !player1Health.IsOutOfLives()) alive = true;
                    else if (p == 2 && player2Health != null && !player2Health.IsOutOfLives()) alive = true;
                    else if (p == 3 && player3Health != null && !player3Health.IsOutOfLives()) alive = true;
                    else if (p == 4 && player4Health != null && !player4Health.IsOutOfLives()) alive = true;
                    if (alive) { winner = p; break; }
                }
                EndSuddenDeathRound(winner);
            }
            else if (aliveSD == 0)
            {
                EndSuddenDeathRound(0);
            }
            return;
        }

        if (roundEnded) return;

        // Normal round: cek berdasarkan HP > 0
        int aliveCount = 0, lastAlive = 0;
        CheckAlive(1, player1Health?.currentHealth ?? -1, ref aliveCount, ref lastAlive);
        CheckAlive(2, player2Health?.currentHealth ?? -1, ref aliveCount, ref lastAlive);
        if (activePlayers >= 3) CheckAlive(3, player3Health?.currentHealth ?? -1, ref aliveCount, ref lastAlive);
        if (activePlayers >= 4) CheckAlive(4, player4Health?.currentHealth ?? -1, ref aliveCount, ref lastAlive);

        if      (aliveCount == 1) EndRound(lastAlive);
        else if (aliveCount == 0) EndRound(0);
    }

    private void CheckAlive(int idx, float hp, ref int cnt, ref int last)
    { if (hp > 0) { cnt++; last = idx; } }

    // ─── EndRound (1 pemenang, dipanggil saat ada yang mati) ─────────────────
    public void EndRound(int winningPlayer)
    {
        if (roundEnded || gameEnded) return;
        roundEnded = true;

        if (winningPlayer >= 1 && winningPlayer <= 4)
        {
            roundsWon[winningPlayer]++;
            Debug.Log($"[GM] P{winningPlayer} menang round {currentRound}. Total: {roundsWon[winningPlayer]}");
        }
        else
        {
            Debug.Log($"[GM] Round {currentRound} seri.");
        }

        UpdateRoundBoxes();
        FindObjectOfType<Timer>()?.StopTimer();
        DisableAllPlayers(winningPlayer);

        // Cek apakah ada yang menang 3 kali
        bool hasWinner = false;
        for (int i = 1; i <= activePlayers; i++)
            if (roundsWon[i] >= ROUNDS_TO_WIN) { hasWinner = true; break; }

        if (hasWinner || currentRound >= maxRounds)
            GameEnd(DetermineGameWinner());
        else
            StartCoroutine(PrepareNextRound());
    }

    // ─── EndRoundTimeout (dipanggil Timer saat waktu habis) ──────────────────
    public void EndRoundTimeout(int[] winners)
    {
        if (roundEnded || gameEnded) return;
        roundEnded = true;

        FindObjectOfType<Timer>()?.StopTimer();

        // ── Catat win count SEBELUM di-update ────────────────────────────────
        int[] winsBefore = new int[5];
        for (int i = 1; i <= 4; i++) winsBefore[i] = roundsWon[i];

        // ── Update win count ──────────────────────────────────────────────────
        if (winners != null)
        {
            foreach (int w in winners)
                if (w >= 1 && w <= 4)
                {
                    roundsWon[w]++;
                    Debug.Log($"[GM] P{w} menang round {currentRound} (timeout). Total: {roundsWon[w]}");
                }
        }

        UpdateRoundBoxes();
        DisablePlayersForTimeout(winners);

        // ── Deteksi Sudden Death ──────────────────────────────────────────────
        // Logika: Sudden Death terjadi jika ada >= 2 pemenang timeout yang
        // SEBELUM round ini sudah tepat di SUDDEN_DEATH_WINS-1 (1 win),
        // sehingga mereka semua naik ke SUDDEN_DEATH_WINS (2) bersamaan,
        // DAN tidak ada yang sudah >= ROUNDS_TO_WIN sebelum round ini.
        //
        // Juga: Sudden Death terjadi jika sudah ada >= 2 pemain di
        // SUDDEN_DEATH_WINS (2) sebelum round ini (tied dari sebelumnya)
        // dan setelah round ini mereka masih sama-sama tertinggi.
        //
        // Cara paling robust: cek berdasarkan winsBefore + siapa yang menang.
        List<int> sdCandidates = GetSuddenDeathCandidates(winners, winsBefore);

        if (sdCandidates.Count >= 2)
        {
            suddenDeathPlayers = sdCandidates;
            Debug.Log($"[GM] Sudden Death! Peserta: [{string.Join(",", suddenDeathPlayers)}]");
            StartCoroutine(PrepareSuddenDeath());
            return;
        }

        // ── Tidak ada Sudden Death — cek akhir game normal ────────────────────
        bool hasWinner = false;
        for (int i = 1; i <= activePlayers; i++)
            if (roundsWon[i] >= ROUNDS_TO_WIN) { hasWinner = true; break; }

        if (hasWinner || currentRound >= maxRounds)
            GameEnd(DetermineGameWinner());
        else
            StartCoroutine(PrepareNextRound());
    }

    // ─── GetSuddenDeathCandidates ─────────────────────────────────────────────
    // Sudden Death dipicu jika waktu habis dan ada >= 2 pemain yang:
    //   - SUDAH punya >= SUDDEN_DEATH_WINS (2) kemenangan SEBELUM round ini
    //     (bukan baru dapat 2 dari round ini)
    //   - Tidak ada yang sudah >= ROUNDS_TO_WIN sebelum round ini
    //   - Setelah timeout, pemain-pemain tersebut sama-sama tertinggi (HP tied)
    //     sehingga keduanya dapat +1 dan tidak ada pemenang tunggal
    //
    // Contoh BENAR — Sudden Death:
    //   winsBefore: P1=2, P2=2 → timeout HP sama → P1=3, P2=3 → SD
    //
    // Contoh SALAH — tidak perlu SD:
    //   winsBefore: P1=1, P2=1 → timeout HP sama → P1=2, P2=2
    //   → mereka BARU dapat 2, belum "sudah menang 2" → lanjut round normal
    //
    // Contoh normal win:
    //   winsBefore: P1=2, P2=1 → timeout P1 HP lebih tinggi → P1=3 sendirian → P1 menang
    private List<int> GetSuddenDeathCandidates(int[] winners, int[] winsBefore)
    {
        // Guard: jika sebelum round ini ada yang sudah >= ROUNDS_TO_WIN,
        // game harusnya sudah berakhir.
        for (int i = 1; i <= activePlayers; i++)
            if (winsBefore[i] >= ROUNDS_TO_WIN) return new List<int>();

        // Kumpulkan pemain yang SUDAH punya >= SUDDEN_DEATH_WINS sebelum round ini
        // DAN menjadi pemenang timeout (dapat +1 sehingga sekarang >= ROUNDS_TO_WIN)
        // DAN ada >= 2 dari mereka (tied, tidak ada pemenang tunggal)
        List<int> candidates = new List<int>();
        for (int i = 1; i <= activePlayers; i++)
        {
            // Pemain harus sudah punya SUDDEN_DEATH_WINS sebelum round ini
            if (winsBefore[i] >= SUDDEN_DEATH_WINS && IsWinner(i, winners))
                candidates.Add(i);
        }

        // Harus ada >= 2 yang tied (jika hanya 1 → dia menang sendirian)
        return (candidates.Count >= 2) ? candidates : new List<int>();
    }

    // ─── PrepareSuddenDeath ───────────────────────────────────────────────────
    private IEnumerator PrepareSuddenDeath()
    {
        if (audioManager != null) audioManager.PlaySFX(audioManager.gameOver);
        yield return new WaitForSeconds(2f);

        isSuddenDeath = true;
        roundEnded    = false;
        currentRound++;

        // Hentikan PowerUp spawn
        if (powerUpManager != null) powerUpManager.SetSuddenDeathMode(true);

        // Destroy semua PowerUp yang ada
        foreach (var pu in FindObjectsOfType<PowerUp>())
            Destroy(pu.gameObject);

        // Setup health & posisi semua pemain
        ResetPlayersForSuddenDeath();

        yield return StartCoroutine(StartSuddenDeathRound());
    }

    // ─── ResetPlayersForSuddenDeath ───────────────────────────────────────────
    private void ResetPlayersForSuddenDeath()
    {
        for (int i = 1; i <= activePlayers; i++)
        {
            bool isParticipant = suddenDeathPlayers.Contains(i);

            switch (i)
            {
                case 1:
                    if (player1Health != null)
                    {
                        player1Health.transform.position = startPositions[1];
                        if (isParticipant)
                        {
                            player1Health.ResetHealthSuddenDeath();
                            player1Health.EnablePlayer();
                            player1Health.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps();
                        }
                        else
                        {
                            player1Health.SetHealthToZero();
                            player1Health.DisablePlayer(true);
                        }
                    }
                    if (player1Shooting != null)
                    {
                        if (isParticipant) player1Shooting.ForceKnifeOnly();
                        else               player1Shooting.enabled = false;
                    }
                    break;

                case 2:
                    if (player2Health != null)
                    {
                        player2Health.transform.position = startPositions[2];
                        if (isParticipant)
                        {
                            player2Health.ResetHealthSuddenDeath();
                            player2Health.EnablePlayer();
                            player2Health.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps();
                        }
                        else
                        {
                            player2Health.SetHealthToZero();
                            player2Health.DisablePlayer(true);
                        }
                    }
                    if (player2Shooting != null)
                    {
                        if (isParticipant) player2Shooting.ForceKnifeOnly();
                        else               player2Shooting.enabled = false;
                    }
                    break;

                case 3:
                    if (player3Health != null)
                    {
                        player3Health.transform.position = startPositions[3];
                        if (isParticipant)
                        {
                            player3Health.ResetHealthSuddenDeath();
                            player3Health.EnablePlayer();
                            player3Health.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps();
                        }
                        else
                        {
                            player3Health.SetHealthToZero();
                            player3Health.DisablePlayer(true);
                        }
                    }
                    if (player3Shooting != null)
                    {
                        if (isParticipant) player3Shooting.ForceKnifeOnly();
                        else               player3Shooting.enabled = false;
                    }
                    break;

                case 4:
                    if (player4Health != null)
                    {
                        player4Health.transform.position = startPositions[4];
                        if (isParticipant)
                        {
                            player4Health.ResetHealthSuddenDeath();
                            player4Health.EnablePlayer();
                            player4Health.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps();
                        }
                        else
                        {
                            player4Health.SetHealthToZero();
                            player4Health.DisablePlayer(true);
                        }
                    }
                    if (player4Shooting != null)
                    {
                        if (isParticipant) player4Shooting.ForceKnifeOnly();
                        else               player4Shooting.enabled = false;
                    }
                    break;
            }
        }
    }

    // ─── StartSuddenDeathRound ────────────────────────────────────────────────
    private IEnumerator StartSuddenDeathRound()
    {
        // Freeze peserta selama countdown
        foreach (int p in suddenDeathPlayers)
        {
            if (p == 1 && player1Health != null) player1Health.DisablePlayer(false);
            else if (p == 2 && player2Health != null) player2Health.DisablePlayer(false);
            else if (p == 3 && player3Health != null) player3Health.DisablePlayer(false);
            else if (p == 4 && player4Health != null) player4Health.DisablePlayer(false);
        }

        FindObjectOfType<Timer>()?.SetSuddenDeathMode(true);

        // Teks "Sudden Death"
        if (roundTextUI != null && roundText != null)
        {
            roundText.text = "Sudden Death";
            roundTextUI.SetActive(true);
        }
        if (suddenDeathTextUI != null) suddenDeathTextUI.SetActive(true);

        // Sound
        if (audioManager != null)
        {
            AudioClip clip = suddenDeathSound != null ? suddenDeathSound : finalRoundSound;
            if (clip != null) audioManager.PlaySFX(clip);
        }

        yield return new WaitForSeconds(2.5f);

        if (roundTextUI != null)       roundTextUI.SetActive(false);
        if (suddenDeathTextUI != null) suddenDeathTextUI.SetActive(false);

        // Fight!
        if (fightTextUI != null) fightTextUI.SetActive(true);
        if (audioManager != null && audioManager.fight != null)
            audioManager.PlaySFX(audioManager.fight);

        yield return new WaitForSeconds(1f);
        if (fightTextUI != null) fightTextUI.SetActive(false);

        // Enable peserta Sudden Death
        foreach (int p in suddenDeathPlayers)
        {
            if (p == 1 && player1Health != null) player1Health.EnablePlayer();
            else if (p == 2 && player2Health != null) player2Health.EnablePlayer();
            else if (p == 3 && player3Health != null) player3Health.EnablePlayer();
            else if (p == 4 && player4Health != null) player4Health.EnablePlayer();
        }

        // Re-apply knife-only setelah EnablePlayer
        foreach (int p in suddenDeathPlayers)
        {
            if (p == 1 && player1Shooting != null) player1Shooting.ForceKnifeOnly();
            else if (p == 2 && player2Shooting != null) player2Shooting.ForceKnifeOnly();
            else if (p == 3 && player3Shooting != null) player3Shooting.ForceKnifeOnly();
            else if (p == 4 && player4Shooting != null) player4Shooting.ForceKnifeOnly();
        }

        // Terapkan Speed + Jump boost permanen untuk semua peserta Sudden Death
        foreach (int p in suddenDeathPlayers)
        {
            PlayerPowerUpEffects fx = null;
            if (p == 1 && player1Health != null) fx = player1Health.GetComponent<PlayerPowerUpEffects>();
            else if (p == 2 && player2Health != null) fx = player2Health.GetComponent<PlayerPowerUpEffects>();
            else if (p == 3 && player3Health != null) fx = player3Health.GetComponent<PlayerPowerUpEffects>();
            else if (p == 4 && player4Health != null) fx = player4Health.GetComponent<PlayerPowerUpEffects>();
            if (fx != null) fx.ApplySuddenDeathBoosts();
        }
    }

    // ─── EndSuddenDeathRound ──────────────────────────────────────────────────
    public void EndSuddenDeathRound(int winner)
    {
        // Gunakan gameEnded saja sebagai guard — roundEnded tidak relevan di SD
        // karena siklus round normal sudah selesai sebelum SD dimulai.
        if (gameEnded) return;
        gameEnded = true; // set langsung agar tidak dipanggil dua kali

        isSuddenDeath = false;
        roundEnded    = true;

        FindObjectOfType<Timer>()?.StopTimer();
        FindObjectOfType<Timer>()?.SetSuddenDeathMode(false);
        if (powerUpManager != null) powerUpManager.SetSuddenDeathMode(false);

        // Hapus SD boost dari semua peserta
        foreach (int p in suddenDeathPlayers)
        {
            PlayerPowerUpEffects fx = null;
            if (p == 1 && player1Health != null) fx = player1Health.GetComponent<PlayerPowerUpEffects>();
            else if (p == 2 && player2Health != null) fx = player2Health.GetComponent<PlayerPowerUpEffects>();
            else if (p == 3 && player3Health != null) fx = player3Health.GetComponent<PlayerPowerUpEffects>();
            else if (p == 4 && player4Health != null) fx = player4Health.GetComponent<PlayerPowerUpEffects>();
            if (fx != null) fx.RemoveSuddenDeathBoosts();
        }

        Debug.Log(winner > 0
            ? $"[GM] P{winner} MENANG Sudden Death!"
            : "[GM] Sudden Death selesai tanpa pemenang jelas, fallback ke round wins.");

        // Disable semua pemain (pemenang tidak ditampilkan death sprite)
        if (player1Health != null) player1Health.DisablePlayer(winner != 1);
        if (player2Health != null) player2Health.DisablePlayer(winner != 2);
        if (activePlayers >= 3 && player3Health != null) player3Health.DisablePlayer(winner != 3);
        if (activePlayers >= 4 && player4Health != null) player4Health.DisablePlayer(winner != 4);

        // Tampilkan Win UI langsung tanpa memanggil GameEnd (untuk menghindari
        // double-call DisableAllPlayers dan potensi race condition)
        int finalWinner = winner > 0 ? winner : DetermineGameWinner();

        if      (finalWinner == 1 && player1WinsUI != null) player1WinsUI.SetActive(true);
        else if (finalWinner == 2 && player2WinsUI != null) player2WinsUI.SetActive(true);
        else if (finalWinner == 3 && player3WinsUI != null) player3WinsUI.SetActive(true);
        else if (finalWinner == 4 && player4WinsUI != null) player4WinsUI.SetActive(true);
        else if (drawUI != null)                             drawUI.SetActive(true);

        if (audioManager != null)
        {
            audioManager.StopMusic();
            audioManager.PlaySFX(audioManager.winGame);
        }

        Debug.Log(finalWinner > 0
            ? $"[GM] P{finalWinner} MENANG game (via Sudden Death)!"
            : "[GM] Game DRAW (via Sudden Death)!");
    }

    // ─── DisablePlayersForTimeout ─────────────────────────────────────────────
    private void DisablePlayersForTimeout(int[] winners)
    {
        if (player1Health != null) player1Health.DisablePlayer(!IsWinner(1, winners));
        if (player2Health != null) player2Health.DisablePlayer(!IsWinner(2, winners));
        if (activePlayers >= 3 && player3Health != null) player3Health.DisablePlayer(!IsWinner(3, winners));
        if (activePlayers >= 4 && player4Health != null) player4Health.DisablePlayer(!IsWinner(4, winners));
    }

    private bool IsWinner(int idx, int[] winners)
    {
        if (winners == null) return false;
        foreach (int w in winners) if (w == idx) return true;
        return false;
    }

    // ─── DetermineGameWinner ──────────────────────────────────────────────────
    private int DetermineGameWinner()
    {
        int maxWins = 0, winner = 0;
        bool tie = false;
        for (int i = 1; i <= activePlayers; i++)
        {
            if (roundsWon[i] > maxWins)
            {
                maxWins = roundsWon[i];
                winner  = i;
                tie     = false;
            }
            else if (roundsWon[i] == maxWins && maxWins > 0)
            {
                tie = true;
            }
        }
        return tie ? 0 : winner;
    }

    // ─── GameEnd ──────────────────────────────────────────────────────────────
    private void GameEnd(int winner)
    {
        gameEnded = true;
        DisableAllPlayers(winner);

        if      (winner == 1 && player1WinsUI != null) player1WinsUI.SetActive(true);
        else if (winner == 2 && player2WinsUI != null) player2WinsUI.SetActive(true);
        else if (winner == 3 && player3WinsUI != null) player3WinsUI.SetActive(true);
        else if (winner == 4 && player4WinsUI != null) player4WinsUI.SetActive(true);
        else if (drawUI != null)                        drawUI.SetActive(true);

        if (audioManager != null)
        {
            audioManager.StopMusic();
            audioManager.PlaySFX(audioManager.winGame);
        }

        Debug.Log(winner > 0 ? $"[GM] P{winner} MENANG game!" : "[GM] Game DRAW!");
    }

    private void DisableAllPlayers(int winner)
    {
        if (player1Health != null) player1Health.DisablePlayer(winner != 1);
        if (player2Health != null) player2Health.DisablePlayer(winner != 2);
        if (activePlayers >= 3 && player3Health != null) player3Health.DisablePlayer(winner != 3);
        if (activePlayers >= 4 && player4Health != null) player4Health.DisablePlayer(winner != 4);
    }

    // ─── PrepareNextRound ─────────────────────────────────────────────────────
    private IEnumerator PrepareNextRound()
    {
        if (audioManager != null) audioManager.PlaySFX(audioManager.gameOver);
        yield return new WaitForSeconds(2f);
        ResetPlayers();
        currentRound++;
        roundEnded = false;
        yield return StartCoroutine(StartRound());
    }

    // ─── StartRound ──────────────────────────────────────────────────────────
    private IEnumerator StartRound()
    {
        // Freeze semua selama countdown
        if (player1Health != null) player1Health.DisablePlayer(false);
        if (player2Health != null) player2Health.DisablePlayer(false);
        if (activePlayers >= 3 && player3Health != null) player3Health.DisablePlayer(false);
        if (activePlayers >= 4 && player4Health != null) player4Health.DisablePlayer(false);

        FindObjectOfType<Timer>()?.ResetTimer();

        bool isFinalRound = (currentRound == maxRounds);
        if (roundTextUI != null && roundText != null)
        {
            roundText.text = isFinalRound ? "Final Round" : $"Round {currentRound}";
            roundTextUI.SetActive(true);
        }

        if (audioManager != null)
        {
            AudioClip clip = isFinalRound ? finalRoundSound : GetRoundSound(currentRound);
            if (clip != null) audioManager.PlaySFX(clip);
        }

        yield return new WaitForSeconds(2.5f);
        if (roundTextUI != null) roundTextUI.SetActive(false);

        if (fightTextUI != null) fightTextUI.SetActive(true);
        if (audioManager != null && audioManager.fight != null)
            audioManager.PlaySFX(audioManager.fight);

        yield return new WaitForSeconds(1f);
        if (fightTextUI != null) fightTextUI.SetActive(false);

        if (player1Health != null) player1Health.EnablePlayer();
        if (player2Health != null) player2Health.EnablePlayer();
        if (activePlayers >= 3 && player3Health != null) player3Health.EnablePlayer();
        if (activePlayers >= 4 && player4Health != null) player4Health.EnablePlayer();

        FindObjectOfType<Timer>()?.StartTimer();
    }

    private AudioClip GetRoundSound(int round)
    {
        switch (round)
        {
            case 1: return round1Sound;
            case 2: return round2Sound;
            case 3: return round3Sound;
            case 4: return round4Sound;
            case 5: return round5Sound;
            case 6: return round6Sound;
            case 7: return round7Sound;
            case 8: return round8Sound;
            default: return null;
        }
    }

    // ─── ResetPlayers ─────────────────────────────────────────────────────────
    private void ResetPlayers()
    {
        ResetP1(player1Health, player1Shooting, startPositions[1]);
        ResetP2(player2Health, player2Shooting, startPositions[2]);
        if (activePlayers >= 3) ResetP3(player3Health, player3Shooting, startPositions[3]);
        if (activePlayers >= 4) ResetP4(player4Health, player4Shooting, startPositions[4]);
    }

    private void ResetP1(Health h, PlayerShooting s, Vector3 pos)
    {
        if (h != null) { h.transform.position = pos; h.ResetHealth(); h.EnablePlayer(); h.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps(); }
        if (s != null) s.ResetToPrimary();
    }
    private void ResetP2(HealthP2 h, PlayerShooting s, Vector3 pos)
    {
        if (h != null) { h.transform.position = pos; h.ResetHealth(); h.EnablePlayer(); h.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps(); }
        if (s != null) s.ResetToPrimary();
    }
    private void ResetP3(HealthP3 h, PlayerShooting s, Vector3 pos)
    {
        if (h != null) { h.transform.position = pos; h.ResetHealth(); h.EnablePlayer(); h.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps(); }
        if (s != null) s.ResetToPrimary();
    }
    private void ResetP4(HealthP4 h, PlayerShooting s, Vector3 pos)
    {
        if (h != null) { h.transform.position = pos; h.ResetHealth(); h.EnablePlayer(); h.GetComponent<PlayerPowerUpEffects>()?.ResetPowerUps(); }
        if (s != null) s.ResetToPrimary();
    }

    // ─── Round Box UI ─────────────────────────────────────────────────────────
    private void InitRoundBoxes()
    {
        SetBoxColors(player1RoundBoxes, 0);
        SetBoxColors(player2RoundBoxes, 0);
        SetBoxColors(player3RoundBoxes, 0);
        SetBoxColors(player4RoundBoxes, 0);
    }

    private void UpdateRoundBoxes()
    {
        SetBoxColors(player1RoundBoxes, roundsWon[1]);
        SetBoxColors(player2RoundBoxes, roundsWon[2]);
        SetBoxColors(player3RoundBoxes, roundsWon[3]);
        SetBoxColors(player4RoundBoxes, roundsWon[4]);
    }

    private void SetBoxColors(Image[] boxes, int won)
    {
        if (boxes == null) return;
        for (int i = 0; i < boxes.Length; i++)
            if (boxes[i] != null)
                boxes[i].color = i < won ? roundBoxWonColor : roundBoxEmptyColor;
    }

    // ─── Public getter ────────────────────────────────────────────────────────
    public bool IsSuddenDeath => isSuddenDeath;

    // ─── Restart ──────────────────────────────────────────────────────────────
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}