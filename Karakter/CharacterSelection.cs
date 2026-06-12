// CharacterSelection.cs — Updated for 2P / 3P / 4P
// Script ini digunakan di 3 scene:
//   2PCharacterSelection  → numberOfPlayersInScene = 2
//   3PCharacterSelection  → numberOfPlayersInScene = 3
//   4PCharacterSelection  → numberOfPlayersInScene = 4
//
// Di Inspector, set "Number Of Players In Scene" sesuai scene masing-masing.
// File ini menggantikan: Karakter/CharacterSelection.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [Header("Jumlah Pemain di Scene Ini")]
    [Tooltip("Isi 2, 3, atau 4 sesuai scene ini (2PCharSel=2, 3PCharSel=3, 4PCharSel=4)")]
    [SerializeField] private int numberOfPlayersInScene = 2;

    // ─── Character Data Arrays ──────────────────────────────────────────────
    [Header("Character Data")]
    [SerializeField] private CharacterData[] player1Characters;
    [SerializeField] private CharacterData[] player2Characters;
    [SerializeField] private CharacterData[] player3Characters; // Untuk 3P & 4P
    [SerializeField] private CharacterData[] player4Characters; // Untuk 4P saja

    // ─── Weapon Data Arrays ─────────────────────────────────────────────────
    [Header("Weapon Data")]
    [SerializeField] private WeaponData[] player1Weapons;
    [SerializeField] private WeaponData[] player2Weapons;
    [SerializeField] private WeaponData[] player3Weapons; // Untuk 3P & 4P
    [SerializeField] private WeaponData[] player4Weapons; // Untuk 4P saja

    // ─── Character UI ────────────────────────────────────────────────────────
    [Header("Character UI — P1")]
    [SerializeField] private SpriteRenderer player1SpriteRenderer;
    [SerializeField] private Text player1NameText;
    [SerializeField] private Button player1NextButton;
    [SerializeField] private Button player1BackButton;


    [Header("Character UI — P2")]
    [SerializeField] private SpriteRenderer player2SpriteRenderer;
    [SerializeField] private Text player2NameText;
    [SerializeField] private Button player2NextButton;
    [SerializeField] private Button player2BackButton;


    [Header("Character UI — P3 (3P & 4P scene)")]
    [SerializeField] private SpriteRenderer player3SpriteRenderer;
    [SerializeField] private Text player3NameText;
    [SerializeField] private Button player3NextButton;
    [SerializeField] private Button player3BackButton;

    [Header("Character UI — P4 (4P scene saja)")]
    [SerializeField] private SpriteRenderer player4SpriteRenderer;
    [SerializeField] private Text player4NameText;
    [SerializeField] private Button player4NextButton;
    [SerializeField] private Button player4BackButton;

    // ─── Weapon UI ───────────────────────────────────────────────────────────
    [Header("Weapon UI — P1")]
    [SerializeField] private SpriteRenderer player1WeaponImage;
    [SerializeField] private Text player1WeaponNameText;
    [SerializeField] private Button player1WeaponNextButton;
    [SerializeField] private Button player1WeaponBackButton;

    [Header("Weapon UI — P2")]
    [SerializeField] private SpriteRenderer player2WeaponImage;
    [SerializeField] private Text player2WeaponNameText;
    [SerializeField] private Button player2WeaponNextButton;
    [SerializeField] private Button player2WeaponBackButton;


    [Header("Weapon UI — P3")]
    [SerializeField] private SpriteRenderer player3WeaponImage;
    [SerializeField] private Text player3WeaponNameText;
    [SerializeField] private Button player3WeaponNextButton;
    [SerializeField] private Button player3WeaponBackButton;

    [Header("Weapon UI — P4")]
    [SerializeField] private SpriteRenderer player4WeaponImage;
    [SerializeField] private Text player4WeaponNameText;
    [SerializeField] private Button player4WeaponNextButton;
    [SerializeField] private Button player4WeaponBackButton;

    // ─── Panel GameObjects (opsional, untuk hide/show) ───────────────────────
    [Header("Panel P3 & P4 (opsional — bisa di-hide lewat sini)")]
    [SerializeField] private GameObject panelP3; // Panel/GameObject seluruh UI P3
    [SerializeField] private GameObject panelP4; // Panel/GameObject seluruh UI P4

    // ─── Play Button ─────────────────────────────────────────────────────────
    [Header("Play Button")]
    [SerializeField] private Button playButton;

    // ─── Indices ─────────────────────────────────────────────────────────────
    private int p1CharIdx = 0, p2CharIdx = 0, p3CharIdx = 0, p4CharIdx = 0;
    private int p1WpnIdx  = 0, p2WpnIdx  = 0, p3WpnIdx  = 0, p4WpnIdx  = 0;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Simpan jumlah pemain ke GameData
        if (GameData.Instance != null)
            GameData.Instance.numberOfPlayers = numberOfPlayersInScene;

        // Show/hide panel P3 dan P4
        if (panelP3 != null) panelP3.SetActive(numberOfPlayersInScene >= 3);
        if (panelP4 != null) panelP4.SetActive(numberOfPlayersInScene >= 4);

        // Validasi
        if (player1Characters.Length == 0 || player2Characters.Length == 0)
        { Debug.LogError("P1/P2 character list kosong!"); return; }
        if (player1Weapons.Length == 0 || player2Weapons.Length == 0)
        { Debug.LogError("P1/P2 weapon list kosong!"); return; }

        if (numberOfPlayersInScene >= 3)
        {
            if (player3Characters.Length == 0) { Debug.LogError("P3 character list kosong!"); return; }
            if (player3Weapons.Length == 0)    { Debug.LogError("P3 weapon list kosong!"); return; }
        }
        if (numberOfPlayersInScene >= 4)
        {
            if (player4Characters.Length == 0) { Debug.LogError("P4 character list kosong!"); return; }
            if (player4Weapons.Length == 0)    { Debug.LogError("P4 weapon list kosong!"); return; }
        }

        // Init tampilan
        UpdateCharacter(1); UpdateWeapon(1);
        UpdateCharacter(2); UpdateWeapon(2);
        if (numberOfPlayersInScene >= 3) { UpdateCharacter(3); UpdateWeapon(3); }
        if (numberOfPlayersInScene >= 4) { UpdateCharacter(4); UpdateWeapon(4); }

        // Listener P1
        player1NextButton?.onClick.AddListener(() => Navigate(ref p1CharIdx, player1Characters.Length, 1, UpdateCharacter1));
        player1BackButton?.onClick.AddListener(() => Navigate(ref p1CharIdx, player1Characters.Length, -1, UpdateCharacter1));
        player1WeaponNextButton?.onClick.AddListener(() => Navigate(ref p1WpnIdx, player1Weapons.Length, 1, UpdateWeapon1));
        player1WeaponBackButton?.onClick.AddListener(() => Navigate(ref p1WpnIdx, player1Weapons.Length, -1, UpdateWeapon1));

        // Listener P2
        player2NextButton?.onClick.AddListener(() => Navigate(ref p2CharIdx, player2Characters.Length, 1, UpdateCharacter2));
        player2BackButton?.onClick.AddListener(() => Navigate(ref p2CharIdx, player2Characters.Length, -1, UpdateCharacter2));
        player2WeaponNextButton?.onClick.AddListener(() => Navigate(ref p2WpnIdx, player2Weapons.Length, 1, UpdateWeapon2));
        player2WeaponBackButton?.onClick.AddListener(() => Navigate(ref p2WpnIdx, player2Weapons.Length, -1, UpdateWeapon2));

        // Listener P3
        if (numberOfPlayersInScene >= 3)
        {
            player3NextButton?.onClick.AddListener(() => Navigate(ref p3CharIdx, player3Characters.Length, 1, UpdateCharacter3));
            player3BackButton?.onClick.AddListener(() => Navigate(ref p3CharIdx, player3Characters.Length, -1, UpdateCharacter3));
            player3WeaponNextButton?.onClick.AddListener(() => Navigate(ref p3WpnIdx, player3Weapons.Length, 1, UpdateWeapon3));
            player3WeaponBackButton?.onClick.AddListener(() => Navigate(ref p3WpnIdx, player3Weapons.Length, -1, UpdateWeapon3));
        }

        // Listener P4
        if (numberOfPlayersInScene >= 4)
        {
            player4NextButton?.onClick.AddListener(() => Navigate(ref p4CharIdx, player4Characters.Length, 1, UpdateCharacter4));
            player4BackButton?.onClick.AddListener(() => Navigate(ref p4CharIdx, player4Characters.Length, -1, UpdateCharacter4));
            player4WeaponNextButton?.onClick.AddListener(() => Navigate(ref p4WpnIdx, player4Weapons.Length, 1, UpdateWeapon4));
            player4WeaponBackButton?.onClick.AddListener(() => Navigate(ref p4WpnIdx, player4Weapons.Length, -1, UpdateWeapon4));
        }

        playButton?.onClick.AddListener(Play);
    }

    // ─── Navigation Helper ───────────────────────────────────────────────────
    private void Navigate(ref int index, int length, int dir, System.Action updateFn)
    {
        index = (index + dir + length) % length;
        updateFn();
    }

    // ─── Update Character Wrappers ───────────────────────────────────────────
    private void UpdateCharacter1() => UpdateCharacter(1);
    private void UpdateCharacter2() => UpdateCharacter(2);
    private void UpdateCharacter3() => UpdateCharacter(3);
    private void UpdateCharacter4() => UpdateCharacter(4);

    private void UpdateCharacter(int p)
    {
        CharacterData[] arr; int idx; SpriteRenderer sr; Text nm;
        switch (p)
        {
            case 1: arr = player1Characters; idx = p1CharIdx; sr = player1SpriteRenderer; nm = player1NameText; break;
            case 2: arr = player2Characters; idx = p2CharIdx; sr = player2SpriteRenderer; nm = player2NameText; break;
            case 3: arr = player3Characters; idx = p3CharIdx; sr = player3SpriteRenderer; nm = player3NameText; break;
            case 4: arr = player4Characters; idx = p4CharIdx; sr = player4SpriteRenderer; nm = player4NameText; break;
            default: return;
        }
        if (arr == null || arr.Length == 0) return;
        var data = arr[idx];
        if (sr != null && data != null) sr.sprite = data.aliveSprite;
        if (nm != null) nm.text = string.IsNullOrEmpty(data?.characterName) ? "Unknown" : data.characterName;
    }

    // ─── Update Weapon Wrappers ──────────────────────────────────────────────
    private void UpdateWeapon1() => UpdateWeapon(1);
    private void UpdateWeapon2() => UpdateWeapon(2);
    private void UpdateWeapon3() => UpdateWeapon(3);
    private void UpdateWeapon4() => UpdateWeapon(4);

    private void UpdateWeapon(int p)
    {
        WeaponData[] arr; int idx; SpriteRenderer sr; Text nm;
        switch (p)
        {
            case 1: arr = player1Weapons; idx = p1WpnIdx; sr = player1WeaponImage; nm = player1WeaponNameText; break;
            case 2: arr = player2Weapons; idx = p2WpnIdx; sr = player2WeaponImage; nm = player2WeaponNameText; break;
            case 3: arr = player3Weapons; idx = p3WpnIdx; sr = player3WeaponImage; nm = player3WeaponNameText; break;
            case 4: arr = player4Weapons; idx = p4WpnIdx; sr = player4WeaponImage; nm = player4WeaponNameText; break;
            default: return;
        }
        if (arr == null || arr.Length == 0) return;
        var data = arr[idx];
        if (data == null) return;
        if (nm != null) nm.text = string.IsNullOrEmpty(data.weaponName) ? "Unknown Weapon" : data.weaponName;
        if (sr != null && data.weaponSprite != null)
        {
            sr.sprite = data.weaponSprite;
            sr.gameObject.SetActive(true);
        }
    }

    // ─── Play ─────────────────────────────────────────────────────────────────
    private void Play()
    {
        if (GameData.Instance == null) { Debug.LogError("GameData.Instance tidak ditemukan!"); return; }

        GameData.Instance.numberOfPlayers = numberOfPlayersInScene;

        GameData.Instance.player1Character = player1Characters[p1CharIdx];
        GameData.Instance.player2Character = player2Characters[p2CharIdx];
        GameData.Instance.player1Weapon    = player1Weapons[p1WpnIdx];
        GameData.Instance.player2Weapon    = player2Weapons[p2WpnIdx];

        if (numberOfPlayersInScene >= 3)
        {
            GameData.Instance.player3Character = player3Characters[p3CharIdx];
            GameData.Instance.player3Weapon    = player3Weapons[p3WpnIdx];
        }
        if (numberOfPlayersInScene >= 4)
        {
            GameData.Instance.player4Character = player4Characters[p4CharIdx];
            GameData.Instance.player4Weapon    = player4Weapons[p4WpnIdx];
        }

        Debug.Log($"[CharSel] {numberOfPlayersInScene}P: " +
                  $"P1={player1Characters[p1CharIdx].characterName}/{player1Weapons[p1WpnIdx].weaponName} | " +
                  $"P2={player2Characters[p2CharIdx].characterName}/{player2Weapons[p2WpnIdx].weaponName}" +
                  (numberOfPlayersInScene >= 3 ? $" | P3={player3Characters[p3CharIdx].characterName}/{player3Weapons[p3WpnIdx].weaponName}" : "") +
                  (numberOfPlayersInScene >= 4 ? $" | P4={player4Characters[p4CharIdx].characterName}/{player4Weapons[p4WpnIdx].weaponName}" : ""));

        // Tombol Play di scene ini menuju ke MapSelector scene
        // Arahkan ke scene MapSelector sesuai kebutuhan project Anda
        // Contoh: SceneManager.LoadScene("MapSelection");
        // Atau bisa dihubungkan ke MapSelector yang ada di scene ini.
    }
}