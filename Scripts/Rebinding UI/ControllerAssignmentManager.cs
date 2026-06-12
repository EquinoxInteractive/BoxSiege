// ControllerAssignmentManager.cs — v5 (P1, P2, P3, P4 Support)
// ============================================================
// ROOT CAUSE yang ditemukan:
//   InputUser.UnpairDevices() menyebabkan PlayerInput menerima event "DeviceLost"
//   → PlayerInput otomatis re-pair dengan device manapun yang tersedia
//   → Isolasi controller gagal.
//
// SOLUSI v4 — playerInput.actions.devices (level Action Asset):
//   Daripada berjuang dengan InputUser pairing, filter input langsung di level
//   action asset menggunakan playerInput.actions.devices. Ini bekerja karena:
//   - P1 menggunakan PlayerController.inputactions (asset berbeda dari P2/P3/P4)
//   - P2 menggunakan PlayerControllerP2.inputactions
//   - P3 menggunakan PlayerControllerP3.inputactions
//   - P4 menggunakan PlayerControllerP4.inputactions
//   - Setting .devices di masing-masing asset HANYA memfilter input untuk
//     asset tersebut, tanpa mempengaruhi InputUser auto-pairing sama sekali
//   - PlayerInput bebas auto-pair ke device apapun, tapi ACTION hanya fired
//     jika input berasal dari device yang ada dalam .devices filter.
//
// v5 FIX: Menambahkan support untuk P3 (PlayerControllerP3) dan P4 (PlayerControllerP4)
//         dengan pola yang sama persis seperti P1 dan P2.
//
// File: Scripts/Rebinding UI/ControllerAssignmentManager.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class ControllerAssignmentManager : MonoBehaviour
{
    public static ControllerAssignmentManager Instance { get; private set; }

    public const string P1_ASSET_NAME = "PlayerController";
    public const string P2_ASSET_NAME = "PlayerControllerP2";
    public const string P3_ASSET_NAME = "PlayerControllerP3";
    public const string P4_ASSET_NAME = "PlayerControllerP4";

    private const string PREF_P1 = "DeviceLock_PlayerController";
    private const string PREF_P2 = "DeviceLock_PlayerControllerP2";
    private const string PREF_P3 = "DeviceLock_PlayerControllerP3";
    private const string PREF_P4 = "DeviceLock_PlayerControllerP4";

    // ──────────────────────────────────────────────────────────────────────────
    //  Auto-create — tidak perlu tambah GameObject manual
    // ──────────────────────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[ControllerAssignmentManager]");
        go.AddComponent<ControllerAssignmentManager>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded   += OnSceneLoaded;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded   -= OnSceneLoaded;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  OnSceneLoaded
    // ──────────────────────────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Retry beberapa kali karena PlayerInput.Start() bisa lebih lambat
        StartCoroutine(ApplyWithRetry());
    }

    private IEnumerator ApplyWithRetry()
    {
        // Coba 3x dengan jeda, agar PlayerInput punya waktu inisialisasi
        for (int i = 0; i < 3; i++)
        {
            yield return null; // tunggu 1 frame
        }
        ApplyAllDeviceFilters();

        // Retry setelah 0.5 detik untuk berjaga-jaga
        yield return new WaitForSeconds(0.5f);
        ApplyAllDeviceFilters();
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  ApplyAllDeviceFilters — CORE FIX
    //  Menggunakan playerInput.actions.devices untuk filter input di level action.
    //  Tidak menyentuh InputUser — tidak ada DeviceLost triggered.
    // ──────────────────────────────────────────────────────────────────────────

    public void ApplyAllDeviceFilters()
    {
        bool appliedAny = false;

        // Cari semua PlayerInput yang ada di scene
        var allPlayerInputs = FindObjectsOfType<PlayerInput>(includeInactive: true);
        if (allPlayerInputs == null || allPlayerInputs.Length == 0)
        {
            Debug.Log("[CAM] Tidak ada PlayerInput di scene.");
            return;
        }

        foreach (var pi in allPlayerInputs)
        {
            if (pi == null || pi.actions == null) continue;

            string assetName = pi.actions.name;
            string prefKey   = GetPrefKey(assetName);
            if (string.IsNullOrEmpty(prefKey)) continue;

            Gamepad target = GetStoredGamepad(prefKey);
            if (target == null) continue;

            ApplyDeviceFilter(pi, target);
            appliedAny = true;
        }

        if (appliedAny)
            Debug.Log("[CAM] Device filters applied to all PlayerInput in scene.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  ApplyDeviceFilter — kunci PlayerInput ke gamepad tertentu
    //  via playerInput.actions.devices (bukan InputUser).
    //
    //  Cara kerja:
    //  playerInput.actions adalah InputActionAsset yang dipakai PlayerInput ini.
    //  Dengan set .devices, hanya input dari device dalam list yang akan
    //  memicu action. Device lain (meski secara InputUser ter-pair) akan diabaikan.
    //  Keyboard & Mouse selalu dimasukkan agar binding keyboard default tetap kerja.
    // ──────────────────────────────────────────────────────────────────────────

    private static void ApplyDeviceFilter(PlayerInput playerInput, Gamepad targetGamepad)
    {
        if (playerInput == null || playerInput.actions == null) return;
        if (targetGamepad == null) return;

        // Bangun daftar device yang boleh men-trigger action untuk player ini
        var deviceList = new List<InputDevice>();
        deviceList.Add(targetGamepad); // hanya gamepad yang di-assign

        // Keyboard & mouse selalu diijinkan (default binding keyboard harus tetap kerja)
        foreach (var d in InputSystem.devices)
        {
            if (d != null && (d is Keyboard || d is Mouse))
                deviceList.Add(d);
        }

        // Set filter — semua action dalam asset ini HANYA merespons device dalam list
        playerInput.actions.devices = new ReadOnlyArray<InputDevice>(deviceList.ToArray());

        Debug.Log($"[CAM] '{playerInput.gameObject.name}' (asset='{playerInput.actions.name}') " +
                  $"→ device filter set to '{targetGamepad.name}' + keyboard/mouse");
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  ClearDeviceFilter — hapus filter (terima input dari semua device)
    // ──────────────────────────────────────────────────────────────────────────

    private static void ClearDeviceFilter(PlayerInput playerInput)
    {
        if (playerInput == null || playerInput.actions == null) return;
        playerInput.actions.devices = null;
        Debug.Log($"[CAM] Device filter cleared for '{playerInput.gameObject.name}'");
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  AssignGamepadToPlayer — dipanggil dari RebindActionUI saat rebinding selesai
    // ──────────────────────────────────────────────────────────────────────────

    public void AssignGamepadToPlayer(string assetName, Gamepad gamepad)
    {
        if (gamepad == null)
        {
            ClearAssignment(assetName);
            return;
        }

        string prefKey = GetPrefKey(assetName);
        if (string.IsNullOrEmpty(prefKey))
        {
            Debug.LogWarning($"[CAM] Tidak mengenal asset '{assetName}'.");
            return;
        }

        int    idx  = GetGamepadIndex(gamepad);
        string data = $"{gamepad.layout}:{idx}";

        PlayerPrefs.SetString(prefKey, data);
        PlayerPrefs.Save();
        Debug.Log($"[CAM] Saved '{prefKey}' = '{data}'");

        // Update GameData jika tersedia
        UpdateGameData(assetName, gamepad.layout, idx);

        // Langsung apply filter ke PlayerInput yang ada di scene saat ini
        var allPlayerInputs = FindObjectsOfType<PlayerInput>(includeInactive: true);
        foreach (var pi in allPlayerInputs)
        {
            if (pi != null && pi.actions != null && pi.actions.name == assetName)
            {
                ApplyDeviceFilter(pi, gamepad);
                break;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  ClearAssignment
    // ──────────────────────────────────────────────────────────────────────────

    public void ClearAssignment(string assetName)
    {
        string prefKey = GetPrefKey(assetName);
        if (!string.IsNullOrEmpty(prefKey))
        {
            PlayerPrefs.DeleteKey(prefKey);
            PlayerPrefs.Save();
        }
        UpdateGameData(assetName, "", -1);

        // Hapus filter dari PlayerInput yang ada
        var allPlayerInputs = FindObjectsOfType<PlayerInput>(includeInactive: true);
        foreach (var pi in allPlayerInputs)
        {
            if (pi != null && pi.actions != null && pi.actions.name == assetName)
            {
                ClearDeviceFilter(pi);
                break;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  GetDeviceIndexSuffix — untuk display (0)/(1) di UI
    //  Membaca dari PlayerPrefs — tidak perlu GameData.
    // ──────────────────────────────────────────────────────────────────────────

    public static string GetDeviceIndexSuffix(string assetName)
    {
        string prefKey = GetPrefKey(assetName);
        if (string.IsNullOrEmpty(prefKey))            return string.Empty;
        if (!PlayerPrefs.HasKey(prefKey))             return string.Empty;

        string data = PlayerPrefs.GetString(prefKey);
        if (!ParsePrefData(data, out string layout, out int storedIndex))
            return string.Empty;

        // Hitung berapa gamepad dengan layout yang sama
        int sameLayoutCount = 0;
        foreach (var d in InputSystem.devices)
        {
            if (d != null && d.layout == layout && d is Gamepad)
                sameLayoutCount++;
        }

        // Tampilkan suffix hanya jika ada >1 controller dengan layout sama
        if (sameLayoutCount <= 1) return string.Empty;
        return $" ({storedIndex})";
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  OnDeviceChange — re-apply filter saat controller connect
    // ──────────────────────────────────────────────────────────────────────────

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if ((change == InputDeviceChange.Added ||
             change == InputDeviceChange.Reconnected) && device is Gamepad)
        {
            StartCoroutine(ApplyWithRetry());
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static string GetPrefKey(string assetName)
    {
        if (assetName == P1_ASSET_NAME) return PREF_P1;
        if (assetName == P2_ASSET_NAME) return PREF_P2;
        if (assetName == P3_ASSET_NAME) return PREF_P3;
        if (assetName == P4_ASSET_NAME) return PREF_P4;
        return string.Empty;
    }

    private static Gamepad GetStoredGamepad(string prefKey)
    {
        if (!PlayerPrefs.HasKey(prefKey)) return null;
        string data = PlayerPrefs.GetString(prefKey);
        if (!ParsePrefData(data, out string layout, out int idx)) return null;

        int count = 0;
        foreach (var d in InputSystem.devices)
        {
            if (d != null && d.layout == layout && d is Gamepad g)
            {
                if (count == idx) return g;
                count++;
            }
        }

        Debug.LogWarning($"[CAM] Gamepad '{data}' tidak ditemukan — controller mungkin belum terhubung.");
        return null;
    }

    private static bool ParsePrefData(string data, out string layout, out int idx)
    {
        layout = string.Empty;
        idx    = -1;
        if (string.IsNullOrEmpty(data)) return false;
        int lastColon = data.LastIndexOf(':');
        if (lastColon < 0) return false;
        layout = data.Substring(0, lastColon);
        return int.TryParse(data.Substring(lastColon + 1), out idx);
    }

    private static int GetGamepadIndex(Gamepad gamepad)
    {
        int resultIdx = 0;
        int count     = 0;
        foreach (var d in InputSystem.devices)
        {
            if (d != null && d.layout == gamepad.layout && d is Gamepad g)
            {
                if (g == gamepad) resultIdx = count;
                count++;
            }
        }
        return resultIdx;
    }

    private static void UpdateGameData(string assetName, string layout, int idx)
    {
        if (GameData.Instance == null) return;
        if (assetName == P1_ASSET_NAME)
        {
            GameData.Instance.player1GamepadLayout = layout;
            GameData.Instance.player1GamepadIndex  = idx;
        }
        else if (assetName == P2_ASSET_NAME)
        {
            GameData.Instance.player2GamepadLayout = layout;
            GameData.Instance.player2GamepadIndex  = idx;
        }
        else if (assetName == P3_ASSET_NAME)
        {
            GameData.Instance.player3GamepadLayout = layout;
            GameData.Instance.player3GamepadIndex  = idx;
        }
        else if (assetName == P4_ASSET_NAME)
        {
            GameData.Instance.player4GamepadLayout = layout;
            GameData.Instance.player4GamepadIndex  = idx;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  IsDeviceAllowedFor — digunakan oleh PlayerMovement, PlayeJump2D, PlayerShooting
    //  Mengembalikan true jika device boleh mengontrol player dengan asset ini.
    //  - Keyboard dan Mouse selalu diijinkan
    //  - Jika tidak ada assignment → terima semua
    //  - Jika ada assignment → hanya gamepad yang di-assign
    // ──────────────────────────────────────────────────────────────────────────

    public static bool IsDeviceAllowedFor(string assetName, InputDevice device)
    {
        if (device == null)               return true;
        if (device is Keyboard)           return true;
        if (device is Mouse)              return true;
        if (!(device is Gamepad gamepad)) return true; // non-gamepad → allow

        var assigned = GetAssignedGamepad(assetName);
        if (assigned == null) return true; // tidak ada assignment → terima semua

        return gamepad == assigned;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  GetAssignedGamepad — cari gamepad yang tersimpan untuk asset ini
    // ──────────────────────────────────────────────────────────────────────────

    public static Gamepad GetAssignedGamepad(string assetName)
    {
        string prefKey = GetPrefKey(assetName);
        if (string.IsNullOrEmpty(prefKey)) return null;
        return GetStoredGamepad(prefKey);
    }

}