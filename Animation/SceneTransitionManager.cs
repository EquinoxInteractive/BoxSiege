using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// SceneTransitionManager.cs
// Script SINGLETON untuk animasi fade saat pindah scene.
//
// ============================== KENAPA SEBELUMNYA TIDAK KONSISTEN ==============================
// Sebelumnya, HANYA scene yang dipindah lewat SceneTransitionManager.Instance.LoadScene(...)
// (yaitu dari Main Menu) yang benar-benar dapat fade OUT (ke hitam) -> load -> fade IN
// (dari hitam) secara utuh dan mulus.
//
// Script-script lain (MapSelector, PauseOption, GameManger/Restart, ExitScript) masih
// memanggil SceneManager.LoadScene(...) / LoadSceneAsync(...) BIASA secara langsung, bukan
// lewat manager ini. Sebelumnya ada "jaring pengaman" (listener SceneManager.sceneLoaded)
// yang mencoba menutupi ini secara otomatis, tapi hasilnya TIDAK konsisten -- kadang cuma
// kelihatan fade-out/cut ke hitam tanpa fade-in yang mulus, karena tidak ada fade-out
// terkontrol sebelum scene lama diganti (perpindahannya instan/di luar kendali script ini).
//
// PERBAIKANNYA: semua tombol/script yang memindah scene (Main Menu, MapSelector, PauseOption,
// GameManger, ExitScript) SEKARANG dipastikan memanggil manager ini (lewat method static
// SceneTransitionManager.GoToScene(...) di bawah), supaya SETIAP perpindahan scene, di
// MANAPUN itu dipanggil, selalu melewati urutan animasi yang SAMA PERSIS:
//     1) FADE OUT (layar jadi hitam)  ->  2) load scene baru  ->  3) FADE IN (hitam hilang)
// Hasilnya: efeknya sama dan konsisten di semua scene, termasuk Main Menu <-> scene lain,
// dan scene lain <-> scene lain lainnya.
//
// Listener SceneManager.sceneLoaded di bawah (auto-reveal) TETAP dipertahankan sebagai
// JARING PENGAMAN TAMBAHAN saja -- untuk jaga-jaga kalau suatu saat ada script BARU yang lupa
// dipanggil lewat GoToScene(...)/LoadScene(...), scene tsb tetap tidak akan muncul mentah
// tanpa fade-in. Tapi jalur utama yang dipakai semua script di project ini sekarang SELALU
// lewat manager ini, jadi jaring pengaman ini seharusnya jarang/tidak pernah kepakai lagi.
//
// SIFAT: hanya perlu di-assign SATU KALI, di scene paling awal (Main Menu),
// otomatis ikut (persist) ke semua scene lewat DontDestroyOnLoad.
//
// CARA ASSIGN DI UNITY: (sama seperti sebelumnya, tidak berubah)
// 1. Di scene Main Menu, buat Canvas baru -> beri nama "SceneTransitionCanvas".
//    Set Sort Order Canvas-nya = 999 (biar selalu paling depan).
// 2. Buat child Image bernama "FadeImage", warna hitam, anchor stretch-stretch
//    (menutupi seluruh layar).
// 3. Add Component "Canvas Group" di "FadeImage".
// 4. Add Component "SceneTransitionManager" (script ini) di "SceneTransitionCanvas".
// 5. Drag CanvasGroup dari "FadeImage" ke field "Fade Canvas Group".
// 6. "SceneTransitionCanvas" HANYA ada di scene Main Menu saja (jangan digandakan
//    manual di scene lain).
//
// CARA PAKAI DARI SCRIPT LAIN (disarankan, paling gampang & aman):
//   SceneTransitionManager.GoToScene("TheEarth");   // by name
//   SceneTransitionManager.GoToScene(2);            // by build index
// Method static ini otomatis pakai manager ini kalau sudah ada (dapat fade-out+fade-in
// penuh), dan otomatis fallback ke SceneManager.LoadScene biasa kalau manager belum
// sempat ada di scene (misal waktu testing langsung dari scene selain Main Menu) --
// supaya TIDAK ERROR.
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("CanvasGroup pada Image full-screen yang dipakai untuk efek fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [Tooltip("Jeda tambahan saat layar full hitam sebelum mulai fade in, biar transisinya tidak terasa terburu-buru")]
    [SerializeField] private float holdDuration = 0.1f;
    [Tooltip("Aktifkan supaya fade tetap jalan walau Time.timeScale = 0 (misal transisi dari Pause Menu)")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Global Auto-Reveal (jaring pengaman)")]
    [Tooltip("Kalau AKTIF: scene APAPUN yang selesai dimuat TANPA lewat GoToScene()/LoadScene() " +
             "milik manager ini (misal ada script baru yang lupa diupdate) tetap otomatis kena " +
             "efek tutup-hitam lalu fade-in, supaya tidak pernah muncul mentah tanpa animasi.")]
    [SerializeField] private bool autoRevealOnAnySceneLoad = true;

    private bool isTransitioning;
    private bool readyForAutoReveal; // supaya scene pertama saat game dibuka tidak ikut ke-fade

    private void Awake()
    {
        // Pola Singleton: kalau sudah ada instance lain, hancurkan duplikatnya.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    private void OnEnable()
    {
        // Berlangganan event GLOBAL Unity sebagai jaring pengaman (lihat komentar di atas).
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance != this) return; // jangan sampai unsubscribe milik instance yang aktif
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        // Start() baru jalan SETELAH event sceneLoaded utk scene pertama (Main Menu)
        // selesai ditembak, jadi flag ini baru aktif utk perpindahan scene berikutnya.
        // Efeknya: scene pertama saat game baru dibuka tetap tampil normal (tidak ada
        // fade dadakan), persis seperti perilaku sebelumnya.
        readyForAutoReveal = true;
    }

    /// <summary>
    /// Helper STATIC yang paling gampang dipakai dari script manapun: pindah scene
    /// berdasarkan nama, dengan fade-out -> load -> fade-in penuh kalau manager ini ada,
    /// atau fallback ke load biasa (tanpa fade) kalau manager belum sempat di-setup.
    /// </summary>
    public static void GoToScene(string sceneName)
    {
        if (Instance != null)
            Instance.LoadScene(sceneName);
        else
            SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>Sama seperti di atas, tapi berdasarkan build index.</summary>
    public static void GoToScene(int sceneIndex)
    {
        if (Instance != null)
            Instance.LoadScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>Pindah scene berdasarkan nama, dengan animasi fade out -> load -> fade in.</summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return; // cegah tombol dipencet berkali-kali saat sedang transisi
        StartCoroutine(TransitionRoutine(sceneName, -1, false));
    }

    /// <summary>Pindah scene berdasarkan build index, dengan animasi fade out -> load -> fade in.</summary>
    public void LoadScene(int sceneIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(null, sceneIndex, true));
    }

    private IEnumerator TransitionRoutine(string sceneName, int sceneIndex, bool useIndex)
    {
        isTransitioning = true;

        // ── Tahap 1: FADE OUT — layar game sekarang perlahan menjadi hitam ──
        if (fadeCanvasGroup != null)
        {
            LeanTween.cancel(fadeCanvasGroup.gameObject);
            fadeCanvasGroup.blocksRaycasts = true; // cegah klik tombol lain saat transisi jalan
            fadeCanvasGroup.interactable = true;

            bool fadeOutDone = false;
            LeanTween.alphaCanvas(fadeCanvasGroup, 1f, fadeOutDuration)
                .setEase(LeanTweenType.easeInOutSine)
                .setIgnoreTimeScale(useUnscaledTime)
                .setOnComplete(() => fadeOutDone = true);

            while (!fadeOutDone) yield return null;
        }

        // ── Tahap 2: load scene baru secara ASYNC di background (tidak nge-freeze) ──
        AsyncOperation op = useIndex
            ? SceneManager.LoadSceneAsync(sceneIndex)
            : SceneManager.LoadSceneAsync(sceneName);

        while (op != null && !op.isDone) yield return null;

        // ── Tahap 3: (jeda opsional lalu) FADE IN — hitam perlahan hilang, scene baru terlihat ──
        // Jeda "holdDuration" ditangani di dalam RevealRoutine() supaya tidak dihitung dobel.
        yield return RevealRoutine();

        isTransitioning = false;
    }

    /// <summary>
    /// JARING PENGAMAN: dipanggil otomatis kalau ada scene yang selesai dimuat TANPA lewat
    /// GoToScene()/LoadScene() milik manager ini (misal script lain yang belum sempat
    /// diupdate). Idealnya jarang/tidak pernah kepakai karena semua script utama di
    /// project ini sekarang sudah lewat manager ini.
    /// </summary>
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return; // hanya instance singleton aktif yang boleh proses
        if (!autoRevealOnAnySceneLoad) return; // fitur global ini bisa dimatikan lewat Inspector
        if (!readyForAutoReveal) return; // abaikan scene pertama saat game baru dibuka
        if (mode != LoadSceneMode.Single) return; // additive load (misal load UI tambahan) diabaikan
        if (isTransitioning) return; // sudah/lagi ditangani manual oleh TransitionRoutine, jangan dobel
        if (fadeCanvasGroup == null) return;

        StartCoroutine(AutoRevealAfterExternalLoadRoutine());
    }

    private IEnumerator AutoRevealAfterExternalLoadRoutine()
    {
        isTransitioning = true; // kunci supaya tidak bentrok kalau ada LoadScene() lain dipanggil bersamaan

        // Scene baru sudah keburu aktif SEBELUM script ini sempat tahu, jadi tidak sempat
        // ada animasi fade-out sebelumnya. Yang bisa kita jamin: layar langsung ditutup
        // hitam DI FRAME YANG SAMA (sebelum sempat dirender ke pemain), lalu baru diungkap
        // dengan animasi fade-in seperti transisi normal.
        LeanTween.cancel(fadeCanvasGroup.gameObject);
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;

        yield return RevealRoutine();

        isTransitioning = false;
    }

    /// <summary>Animasi fade-in (hitam -> transparan) yang dipakai bersama oleh alur manual maupun otomatis.</summary>
    private IEnumerator RevealRoutine()
    {
        if (fadeCanvasGroup == null) yield break;

        if (holdDuration > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(holdDuration);
            else yield return new WaitForSeconds(holdDuration);
        }

        LeanTween.cancel(fadeCanvasGroup.gameObject);
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;

        bool fadeInDone = false;
        LeanTween.alphaCanvas(fadeCanvasGroup, 0f, fadeInDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setIgnoreTimeScale(useUnscaledTime)
            .setOnComplete(() => fadeInDone = true);

        while (!fadeInDone) yield return null;

        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }
}