// Main Menu.cs — Updated for 2P / 3P / 4P Selection
// Tambahan: SetPlayerCount() untuk menyimpan jumlah pemain ke GameData
//           sebelum masuk ke scene CharacterSelection yang sesuai.
// Tambahan: semua pindah scene sekarang lewat SceneTransitionManager supaya
//           ada animasi fade halus, bukan pindah scene mendadak.
// File ini menggantikan: Main Menu/Main Menu.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Helper kecil: kalau SceneTransitionManager sudah di-setup (lihat
    // SceneTransitionManager.cs), pakai itu supaya ada fade. Kalau belum
    // sempat di-setup, tetap fallback ke load biasa supaya TIDAK ERROR.
    private void GoToScene(string sceneName)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadSceneAsync(sceneName);
    }

    private void GoToScene(int sceneIndex)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }

    public void Menu()
    {
        GoToScene(0);
    }

    // ─── Map Scenes (IN GAME) ─────────────────────────────────────────────
    public void TheEarth()
    {
        GoToScene("TheEarth");
    }

    public void TheHell()
    {
        GoToScene("TheHell");
    }

    public void TheDessert()
    {
        GoToScene("TheDessert");
    }

    public void TheSnow()
    {
        GoToScene("TheSnow");
    }

    public void TheJungle()
    {
        GoToScene("TheJungle");
    }

    public void TheSafari()
    {
        GoToScene("TheSafari");
    }

    public void TheTample()
    {
        GoToScene("TheTample");
    }

    // ─── Character Selection Scenes ───────────────────────────────────────
    // Masing-masing tombol di Main Menu (Play → 2P / 3P / 4P) memanggil
    // salah satu method ini. GameData.numberOfPlayers diset di sini
    // sehingga sudah tersedia saat CharacterSelection di-load.

    public void SelectionP2()
    {
        if (GameData.Instance != null) GameData.Instance.numberOfPlayers = 2;
        GoToScene("2PCharacterSelection");
    }

    public void SelectionP3()
    {
        if (GameData.Instance != null) GameData.Instance.numberOfPlayers = 3;
        GoToScene("3PCharacterSelection");
    }

    public void SelectionP4()
    {
        if (GameData.Instance != null) GameData.Instance.numberOfPlayers = 4;
        GoToScene("4PCharacterSelection");
    }

    // ─── Quit ─────────────────────────────────────────────────────────────
    public void QuitGame()
    {
        Application.Quit();
    }
}