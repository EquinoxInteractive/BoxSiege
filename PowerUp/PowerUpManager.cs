using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Power Up Prefabs")]
    [SerializeField] private GameObject healthPowerUpPrefab;
    [SerializeField] private GameObject shieldPowerUpPrefab;
    [SerializeField] private GameObject jumpBoostPowerUpPrefab;
    [SerializeField] private GameObject speedBoostPowerUpPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2[] spawnPoints;              // Titik-titik spawn di map
    [SerializeField] private float minSpawnInterval = 3f;        // Interval minimum spawn
    [SerializeField] private float maxSpawnInterval = 5f;        // Interval maksimum spawn
    [SerializeField] private float powerUpDuration = 5f;         // Durasi power-up sebelum hilang
    [SerializeField] private float initialMinSpawnInterval = 3f; // Interval minimum untuk spawn pertama
    [SerializeField] private float initialMaxSpawnInterval = 5f; // Interval maksimum untuk spawn pertama

    private List<GameObject> powerUpPrefabs; // Daftar prefab power-up
    private AudioManager audioManager;
    private bool isFirstSpawn   = true;  // Menandakan apakah ini spawn pertama
    private bool isSuddenDeath  = false; // ← NEW: blokir spawn saat Sudden Death

    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        // Inisialisasi daftar prefab
        powerUpPrefabs = new List<GameObject>
        {
            healthPowerUpPrefab,
            shieldPowerUpPrefab,
            jumpBoostPowerUpPrefab,
            speedBoostPowerUpPrefab
        };
    }

    private void Start()
    {
        // Mulai coroutine untuk spawn power-up
        StartCoroutine(SpawnPowerUpRoutine());
    }

    // ─── SetSuddenDeathMode ───────────────────────────────────────────────────
    // Dipanggil GameManager: true = hentikan spawn, false = lanjutkan spawn.
    public void SetSuddenDeathMode(bool active)
    {
        isSuddenDeath = active;
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        while (true)
        {
            // Gunakan interval cepat untuk spawn pertama, lalu interval normal
            float spawnInterval;
            if (isFirstSpawn)
            {
                spawnInterval = Random.Range(initialMinSpawnInterval, initialMaxSpawnInterval);
                isFirstSpawn = false; // Set ke false setelah spawn pertama
                Debug.Log($"Spawn power-up pertama dalam {spawnInterval} detik.");
            }
            else
            {
                spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            }

            // Tunggu interval
            yield return new WaitForSeconds(spawnInterval);

            // Jangan spawn saat Sudden Death
            if (isSuddenDeath)
            {
                Debug.Log("[PowerUpManager] Sudden Death aktif — spawn power-up diblokir.");
                continue;
            }

            // Pilih prefab power-up acak
            GameObject powerUpToSpawn = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Count)];

            // Pilih titik spawn acak
            Vector2 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Spawn power-up
            GameObject spawnedPowerUp = Instantiate(powerUpToSpawn, spawnPoint, Quaternion.identity);

            // Destroy power-up setelah durasi tertentu jika tidak diambil
            Destroy(spawnedPowerUp, powerUpDuration);
        }
    }
}