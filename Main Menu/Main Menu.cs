// Main Menu.cs — Updated for 2P / 3P / 4P Selection
// Tambahan: SetPlayerCount() untuk menyimpan jumlah pemain ke GameData
//           sebelum masuk ke scene CharacterSelection yang sesuai.
// File ini menggantikan: Main Menu/Main Menu.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }

    // ─── Map Scenes (IN GAME) ─────────────────────────────────────────────
    public void TheEarth()
    {
        SceneManager.LoadSceneAsync("TheEarth");
    }

    public void TheHell()
    {
        SceneManager.LoadSceneAsync("TheHell");
    }

    public void TheDessert()
    {
        SceneManager.LoadSceneAsync("TheDessert");
    }

    public void TheSnow()
    {
        SceneManager.LoadSceneAsync("TheSnow");
    }

    public void TheJungle()
    {
        SceneManager.LoadSceneAsync("TheJungle");
    }

    public void TheSafari()
    {
        SceneManager.LoadSceneAsync("TheSafari");
    }

    public void TheTample()
    {
        SceneManager.LoadSceneAsync("TheTample");
    }

    // ─── Character Selection Scenes ───────────────────────────────────────
    // Masing-masing tombol di Main Menu (Play → 2P / 3P / 4P) memanggil
    // salah satu method ini. GameData.numberOfPlayers diset di sini
    // sehingga sudah tersedia saat CharacterSelection di-load.

    public void SelectionP2()
    {
        if (GameData.Instance != null) GameData.Instance.numberOfPlayers = 2;
        SceneManager.LoadSceneAsync("2PCharacterSelection");
    }

    public void SelectionP3()
    {
        if (GameData.Instance != null) GameData.Instance.numberOfPlayers = 3;
        SceneManager.LoadSceneAsync("3PCharacterSelection");
    }

    public void SelectionP4()
    {
        if (GameData.Instance != null) GameData.Instance.numberOfPlayers = 4;
        SceneManager.LoadSceneAsync("4PCharacterSelection");
    }

    // ─── Quit ─────────────────────────────────────────────────────────────
    public void QuitGame()
    {
        Application.Quit();
    }
}