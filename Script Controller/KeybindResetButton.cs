// KeybindResetButton.cs — Updated for 4 Players
// Taruh script ini pada Button "Reset P1", "Reset P2", "Reset P3", "Reset P4"
// di scene Settings/MainMenu.
// Saat tombol ditekan, semua binding override untuk player yang dipilih dihapus
// (kembali ke default yang ada di .inputactions), lalu PlayerPrefs-nya juga dibersihkan.
//
// Action Map per file .inputactions:
//   PlayerController.inputactions     → "Player"
//   PlayerControllerP2.inputactions   → "Player2"
//   PlayerControllerP3.inputactions   → "Player3"
//   PlayerControllerP4.inputactions   → "Player4"

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;

public class KeybindResetButton : MonoBehaviour
{
    public enum PlayerTarget { Player1, Player2, Player3, Player4 }

    [Header("Target Player")]
    [Tooltip("Pilih Player1/2/3/4 sesuai tombol reset ini")]
    public PlayerTarget playerTarget = PlayerTarget.Player1;

    [Header("Input Action Asset")]
    [Tooltip("Drag file .inputactions sesuai player:\n" +
             "P1 → PlayerController.inputactions\n" +
             "P2 → PlayerControllerP2.inputactions\n" +
             "P3 → PlayerControllerP3.inputactions\n" +
             "P4 → PlayerControllerP4.inputactions")]
    public InputActionAsset inputActionAsset;

    [Header("RebindActionUI dalam scene (untuk refresh tampilan)")]
    [Tooltip("Drag SEMUA RebindActionUI yang terkait dengan player ini")]
    public RebindActionUI[] rebindUIs;

    // Nama action map sesuai file .inputactions masing-masing player
    private string ActionMapName
    {
        get
        {
            switch (playerTarget)
            {
                case PlayerTarget.Player1: return "Player";
                case PlayerTarget.Player2: return "Player2";
                case PlayerTarget.Player3: return "Player3";
                case PlayerTarget.Player4: return "Player4";
                default:                   return "Player";
            }
        }
    }

    // Key PlayerPrefs yang dipakai oleh RebindActionUI.cs
    // Format: "InputBindings_<assetName>_<actionMapName>"
    private string PlayerPrefsKey =>
        inputActionAsset != null
            ? $"InputBindings_{inputActionAsset.name}_{ActionMapName}"
            : string.Empty;

    /// <summary>
    /// Panggil method ini dari onClick Button di Inspector.
    /// </summary>
    public void ResetToDefault()
    {
        if (inputActionAsset == null)
        {
            Debug.LogWarning("[KeybindResetButton] inputActionAsset belum diassign!");
            return;
        }

        // 1. Cari action map yang sesuai
        InputActionMap actionMap = inputActionAsset.FindActionMap(ActionMapName);
        if (actionMap == null)
        {
            Debug.LogWarning($"[KeybindResetButton] Action map '{ActionMapName}' tidak ditemukan di asset '{inputActionAsset.name}'.");
            return;
        }

        // 2. Hapus semua binding override → kembali ke default .inputactions
        actionMap.RemoveAllBindingOverrides();

        // 3. Hapus data tersimpan di PlayerPrefs
        string key = PlayerPrefsKey;
        if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        // 4. Refresh semua RebindActionUI agar teks binding di layar ikut update
        if (rebindUIs != null)
        {
            foreach (var ui in rebindUIs)
                if (ui != null) ui.UpdateBindingDisplay();
        }

        Debug.Log($"[KeybindResetButton] {playerTarget} keybind berhasil direset ke default.");
    }
}