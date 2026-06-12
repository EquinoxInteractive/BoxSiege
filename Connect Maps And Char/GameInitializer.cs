// GameInitializer.cs — Updated for 2 / 3 / 4 Players + Win Character UI
// Menginisialisasi karakter dan senjata semua pemain di awal scene.
// P3 dan P4 di-skip jika numberOfPlayers < 3 / < 4.
//
// TAMBAHAN: Win Character UI
//   Setiap player punya satu Image (UnityEngine.UI.Image) di dalam Win UI
//   mereka masing-masing. Saat Start(), sprite Image tersebut akan otomatis
//   diisi dengan aliveSprite dari karakter yang dipilih pemain saat selection.
//   Assign field "Player X Win Character Image" di Inspector, lalu atur
//   posisi/ukuran Image tersebut di Canvas sesuai keinginan Anda.

using UnityEngine;
using UnityEngine.UI;

public class GameInitializer : MonoBehaviour
{
    [Header("Character Sprite Renderers")]
    [SerializeField] private SpriteRenderer player1SpriteRenderer;
    [SerializeField] private SpriteRenderer player2SpriteRenderer;
    [SerializeField] private SpriteRenderer player3SpriteRenderer;
    [SerializeField] private SpriteRenderer player4SpriteRenderer;

    [Header("Player GameObjects (yang punya PlayerShooting)")]
    [SerializeField] private GameObject player1Object;
    [SerializeField] private GameObject player2Object;
    [SerializeField] private GameObject player3Object;
    [SerializeField] private GameObject player4Object;

    [Header("Weapon Sprite Renderers")]
    [SerializeField] private SpriteRenderer player1WeaponSpriteRenderer;
    [SerializeField] private SpriteRenderer player2WeaponSpriteRenderer;
    [SerializeField] private SpriteRenderer player3WeaponSpriteRenderer;
    [SerializeField] private SpriteRenderer player4WeaponSpriteRenderer;

    [Header("Ammo UI")]
    [SerializeField] private AmmoUI player1AmmoUI;
    [SerializeField] private AmmoUI player2AmmoUI;
    [SerializeField] private AmmoUI player3AmmoUI;
    [SerializeField] private AmmoUI player4AmmoUI;

    [Header("Win Character Image (UI Image di dalam Win Panel masing-masing player)")]
    [Tooltip("Image di dalam Player 1 Win UI — sprite akan otomatis diisi sesuai karakter yang dipilih P1")]
    [SerializeField] private Image player1WinCharacterImage;
    [Tooltip("Image di dalam Player 2 Win UI — sprite akan otomatis diisi sesuai karakter yang dipilih P2")]
    [SerializeField] private Image player2WinCharacterImage;
    [Tooltip("Image di dalam Player 3 Win UI — sprite akan otomatis diisi sesuai karakter yang dipilih P3")]
    [SerializeField] private Image player3WinCharacterImage;
    [Tooltip("Image di dalam Player 4 Win UI — sprite akan otomatis diisi sesuai karakter yang dipilih P4")]
    [SerializeField] private Image player4WinCharacterImage;

    [Header("Win Character Name (TMP Text di dalam Win Panel masing-masing player)")]
    [Tooltip("TMP Text di dalam Player 1 Win UI — teks akan otomatis diisi sesuai nama karakter yang dipilih P1")]
    [SerializeField] private Text player1WinCharacterName;
    [Tooltip("TMP Text di dalam Player 2 Win UI — teks akan otomatis diisi sesuai nama karakter yang dipilih P2")]
    [SerializeField] private Text player2WinCharacterName;
    [Tooltip("TMP Text di dalam Player 3 Win UI — teks akan otomatis diisi sesuai nama karakter yang dipilih P3")]
    [SerializeField] private Text player3WinCharacterName;
    [Tooltip("TMP Text di dalam Player 4 Win UI — teks akan otomatis diisi sesuai nama karakter yang dipilih P4")]
    [SerializeField] private Text player4WinCharacterName;

    private void Start()
    {
        int activePlayers = GameData.Instance != null ? GameData.Instance.numberOfPlayers : 2;

        InitializeCharacters(activePlayers);
        InitializeWeapons(activePlayers);
        InitializeWinCharacterImages(activePlayers);
        InitializeWinCharacterNames(activePlayers);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Character Sprites (SpriteRenderer di scene)
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeCharacters(int activePlayers)
    {
        if (GameData.Instance == null) { Debug.LogError("GameData.Instance tidak ditemukan!"); return; }

        ApplyCharacterSprite(player1SpriteRenderer, GameData.Instance.player1Character, "Player 1");
        ApplyCharacterSprite(player2SpriteRenderer, GameData.Instance.player2Character, "Player 2");

        if (activePlayers >= 3)
            ApplyCharacterSprite(player3SpriteRenderer, GameData.Instance.player3Character, "Player 3");
        if (activePlayers >= 4)
            ApplyCharacterSprite(player4SpriteRenderer, GameData.Instance.player4Character, "Player 4");
    }

    private void ApplyCharacterSprite(SpriteRenderer sr, CharacterData data, string label)
    {
        if (sr != null && data != null) sr.sprite = data.aliveSprite;
        else Debug.LogWarning($"{label}: Character Data atau SpriteRenderer belum diassign.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Win Character Images (UI Image di dalam Win Panel)
    //  Setiap Image akan diisi dengan aliveSprite karakter yang dipilih pemain.
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeWinCharacterImages(int activePlayers)
    {
        if (GameData.Instance == null) return;

        ApplyWinCharacterImage(player1WinCharacterImage, GameData.Instance.player1Character, "Player 1");
        ApplyWinCharacterImage(player2WinCharacterImage, GameData.Instance.player2Character, "Player 2");

        if (activePlayers >= 3)
            ApplyWinCharacterImage(player3WinCharacterImage, GameData.Instance.player3Character, "Player 3");
        if (activePlayers >= 4)
            ApplyWinCharacterImage(player4WinCharacterImage, GameData.Instance.player4Character, "Player 4");
    }

    private void ApplyWinCharacterImage(Image img, CharacterData data, string label)
    {
        if (img == null)
        {
            // Tidak di-warn — boleh tidak diassign jika tidak mau pakai fitur ini
            return;
        }
        if (data == null || data.aliveSprite == null)
        {
            Debug.LogWarning($"{label}: Win Character Image diassign tapi CharacterData/aliveSprite kosong.");
            return;
        }

        img.sprite = data.aliveSprite;

        // Pastikan image terlihat (preserve aspect ratio)
        img.preserveAspect = true;

        Debug.Log($"{label}: Win Character Image → '{data.characterName}' ({data.aliveSprite.name})");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Win Character Names (Text di dalam Win Panel)
    //  Setiap Text akan diisi dengan characterName dari karakter yang dipilih.
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeWinCharacterNames(int activePlayers)
    {
        if (GameData.Instance == null) return;

        ApplyWinCharacterName(player1WinCharacterName, GameData.Instance.player1Character, "Player 1");
        ApplyWinCharacterName(player2WinCharacterName, GameData.Instance.player2Character, "Player 2");

        if (activePlayers >= 3)
            ApplyWinCharacterName(player3WinCharacterName, GameData.Instance.player3Character, "Player 3");
        if (activePlayers >= 4)
            ApplyWinCharacterName(player4WinCharacterName, GameData.Instance.player4Character, "Player 4");
    }

    private void ApplyWinCharacterName(Text txt, CharacterData data, string label)
    {
        if (txt == null) return; // Tidak di-warn — boleh tidak diassign

        if (data == null || string.IsNullOrEmpty(data.characterName))
        {
            Debug.LogWarning($"{label}: Win Character Name diassign tapi characterName di CharacterData kosong.");
            return;
        }

        txt.text = $"{data.characterName} WIN";
        Debug.Log($"{label}: Win Character Name → '{data.characterName}'");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Weapons
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeWeapons(int activePlayers)
    {
        if (GameData.Instance == null) return;

        ApplyWeapon(player1Object, player1WeaponSpriteRenderer, player1AmmoUI, GameData.Instance.player1Weapon, "Player 1");
        ApplyWeapon(player2Object, player2WeaponSpriteRenderer, player2AmmoUI, GameData.Instance.player2Weapon, "Player 2");

        if (activePlayers >= 3)
            ApplyWeapon(player3Object, player3WeaponSpriteRenderer, player3AmmoUI, GameData.Instance.player3Weapon, "Player 3");
        if (activePlayers >= 4)
            ApplyWeapon(player4Object, player4WeaponSpriteRenderer, player4AmmoUI, GameData.Instance.player4Weapon, "Player 4");
    }

    private void ApplyWeapon(GameObject playerObj, SpriteRenderer weaponSR,
                             AmmoUI ammoUI, WeaponData weaponData, string label)
    {
        if (weaponData == null) { Debug.LogWarning($"{label}: WeaponData null."); return; }

        if (weaponSR != null && weaponData.weaponSprite != null) weaponSR.sprite = weaponData.weaponSprite;
        else
        {
            if (weaponSR == null)                Debug.LogWarning($"{label}: WeaponSpriteRenderer belum diassign!");
            if (weaponData.weaponSprite == null) Debug.LogWarning($"{label}: weaponSprite di WeaponData kosong!");
        }

        if (playerObj == null) { Debug.LogWarning($"{label}: playerObject belum diassign!"); return; }

        PlayerShooting shooting = playerObj.GetComponent<PlayerShooting>();
        if (shooting == null) { Debug.LogWarning($"{label}: PlayerShooting tidak ditemukan di '{playerObj.name}'."); return; }

        shooting.weaponData           = weaponData;
        shooting.weaponSpriteRenderer = weaponSR;
        shooting.InitAmmo();

        if (ammoUI != null)
        {
            ammoUI.SetMaxAmmo(weaponData.maxAmmo);
            shooting.OnAmmoPrimaryChanged  += ammoUI.UpdateAmmo;
            shooting.OnSwitchedToSecondary += ammoUI.ShowUnlimited;
            shooting.OnResetAmmo          += ammoUI.ResetUI;

            int cachedAmmo = weaponData.maxAmmo;
            shooting.OnAmmoPrimaryChanged += (a) => { cachedAmmo = a; };
            shooting.OnSwitchedToMelee   += ammoUI.ShowMeleeMode;
            shooting.OnSwitchedFromMelee += () => ammoUI.HideMeleeMode(cachedAmmo);
        }
        else
        {
            Debug.LogWarning($"{label}: AmmoUI belum diassign — ammo tidak akan tampil di UI.");
        }

        Debug.Log($"{label}: '{weaponData.weaponName}' | ammo={weaponData.maxAmmo} " +
                  $"secondary={weaponData.hasSecondary} | melee range={weaponData.meleeRange}");
    }
}