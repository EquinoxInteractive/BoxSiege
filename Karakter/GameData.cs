// GameData.cs — Updated for 4 Players
// Menyimpan data karakter, senjata, gamepad assignment, dan jumlah pemain.
// File ini menggantikan: Karakter/GameData.cs

using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    // ─── Jumlah Pemain ──────────────────────────────────────────────────────
    // Diset dari MainMenu saat memilih 2P / 3P / 4P
    public int numberOfPlayers = 2;

    // ─── Data Karakter ──────────────────────────────────────────────────────
    public CharacterData player1Character;
    public CharacterData player2Character;
    public CharacterData player3Character;
    public CharacterData player4Character;

    // ─── Data Senjata ───────────────────────────────────────────────────────
    public WeaponData player1Weapon;
    public WeaponData player2Weapon;
    public WeaponData player3Weapon;
    public WeaponData player4Weapon;

    // ─── Gamepad Assignment ─────────────────────────────────────────────────
    public string player1GamepadLayout = "";
    public int    player1GamepadIndex  = -1;

    public string player2GamepadLayout = "";
    public int    player2GamepadIndex  = -1;

    public string player3GamepadLayout = "";
    public int    player3GamepadIndex  = -1;

    public string player4GamepadLayout = "";
    public int    player4GamepadIndex  = -1;

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
        }
    }

    // ─── Helper: cari Gamepad dari layout + index ───────────────────────────

    public UnityEngine.InputSystem.Gamepad GetPlayer1Gamepad() => FindGamepad(player1GamepadLayout, player1GamepadIndex);
    public UnityEngine.InputSystem.Gamepad GetPlayer2Gamepad() => FindGamepad(player2GamepadLayout, player2GamepadIndex);
    public UnityEngine.InputSystem.Gamepad GetPlayer3Gamepad() => FindGamepad(player3GamepadLayout, player3GamepadIndex);
    public UnityEngine.InputSystem.Gamepad GetPlayer4Gamepad() => FindGamepad(player4GamepadLayout, player4GamepadIndex);

    private static UnityEngine.InputSystem.Gamepad FindGamepad(string layout, int index)
    {
        if (string.IsNullOrEmpty(layout) || index < 0) return null;

        int count = 0;
        foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
        {
            if (device == null) continue;
            if (device.layout == layout && device is UnityEngine.InputSystem.Gamepad gp)
            {
                if (count == index) return gp;
                count++;
            }
        }
        return null;
    }

    // ─── Helper: ambil CharacterData by player index ────────────────────────
    public CharacterData GetCharacter(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1: return player1Character;
            case 2: return player2Character;
            case 3: return player3Character;
            case 4: return player4Character;
            default: return null;
        }
    }

    // ─── Helper: ambil WeaponData by player index ───────────────────────────
    public WeaponData GetWeapon(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1: return player1Weapon;
            case 2: return player2Weapon;
            case 3: return player3Weapon;
            case 4: return player4Weapon;
            default: return null;
        }
    }
}