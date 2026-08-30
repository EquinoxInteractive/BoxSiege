using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// LoadingScreenManager.cs
// Script SINGLETON untuk layar loading custom saat pindah scene:
// tombol/trigger dipanggil -> Loading Screen (background sudah kamu buat sendiri) FADE IN
// -> scene baru dimuat di background secara ASYNC, sambil:
//      - progress bar KOSONG (track) selalu terlihat sebagai dasar
//      - progress bar TERISI (fill) mengejar progress asli secara HALUS di atasnya
//      - teks persentase berubah dari 0% -> 100%
//      - icon lingkaran (spinner) berputar terus selama proses loading
// -> setelah selesai -> Loading Screen FADE OUT -> scene baru terlihat.
//
// SIFAT: sama seperti SceneTransitionManager, cukup di-assign SATU KALI di scene
// paling awal (Main Menu), otomatis ikut (persist) ke semua scene lewat
// DontDestroyOnLoad. Efek ini HANYA berlaku untuk pemanggilan yang sengaja lewat
// LoadingScreenManager.Instance.LoadScene(...). Scene yang masih pakai
// SceneManager.LoadScene(...) biasa TIDAK akan kena efek ini.
//
// ============================== KENAPA SEBELUMNYA "TERACAK"/MENGECIL ==============================
// Penyebabnya BUKAN dari logic script ini, tapi dari komponen "Canvas Scaler" di
// GameObject "LoadingScreenCanvas" yang settingnya beda sendiri dibanding Canvas UI
// utama kamu (yang lain semua pakai "Scale With Screen Size" + Reference Resolution
// 800x600). Karena "LoadingScreenCanvas" kepasang "Constant Pixel Size", ukuran &
// posisi semua child (Background, ProgressBar, Teks, Spinner) yang sudah kamu tata
// rapi di Editor jadi TIDAK ikut menyesuaikan resolusi layar sebenarnya saat Play
// Mode / Build -> makanya terlihat mengecil dan geser dari posisi yang kamu atur.
//
// Sekarang script ini OTOMATIS menyamakan setting Canvas Scaler tersebut setiap kali
// game jalan (lihat ApplyCanvasScalerFix() di Awake), jadi kamu TIDAK PERLU ubah
// apa pun manual lagi, dan tata letak yang sudah kamu susun akan TETAP seperti itu.
// Script ini sama sekali tidak mengubah posisi/ukuran (RectTransform) elemen apa pun
// secara manual -- hanya menjalankan fungsi fill bar, teks persen, dan spinner saja.
// =====================================================================================================
//
// ============================== CARA ASSIGN DI UNITY ==============================
// 1. Di scene Main Menu, buat Canvas baru -> beri nama "LoadingScreenCanvas".
//    Set Sort Order-nya lebih tinggi dari canvas lain (misal 1000) supaya selalu
//    tampil paling depan (kalau kamu juga pakai SceneTransitionManager yang
//    Sort Order-nya 999, ini otomatis tampil di atasnya).
//
// 2. Add Component "Canvas Group" langsung di GameObject "LoadingScreenCanvas"
//    (root). Ini dipakai untuk fade in/out SELURUH loading screen sekaligus.
//    -> Drag CanvasGroup ini ke field "Fade Canvas Group" di script ini.
//
// 3. Buat child Image bernama "Background", pasang sprite background loading
//    yang SUDAH KAMU PUNYA SENDIRI, anchor stretch-stretch (full screen).
//    (Script ini tidak mengatur tampilan background sama sekali, tinggal taruh)
//
// 4. Untuk BAR LOADING, sekarang buat 2 child Image bertumpuk (urutan di Hierarchy
//    penting, yang di atas render belakang -> taruh "ProgressBarBackground" DI ATAS
//    "ProgressBarFill" di Hierarchy supaya track kosong ada di belakang dan fill
//    di depannya):
//      a. Child Image "ProgressBarBackground" -> Source Image = sprite bar KOSONG
//         milikmu. Image Type = Simple saja (tidak perlu Filled, karena bar kosong
//         ini memang selalu tampil penuh sebagai track/bingkai).
//         -> Drag Image "ProgressBarBackground" ini ke field baru "Progress Bar
//            Background" di script ini.
//      b. Child Image "ProgressBarFill" -> Source Image = sprite bar TERISI milikmu.
//           - Image Type = FILLED
//           - Fill Method = Horizontal
//           - Fill Origin = Left
//         -> Drag Image "ProgressBarFill" ini ke field "Progress Bar Fill".
//         PENTING: posisi & ukuran RectTransform "ProgressBarFill" harus PERSIS
//         menumpuk di atas "ProgressBarBackground" (copy Anchor/Pos/Size yang sama),
//         supaya waktu terisi terlihat pas mengikuti bentuk track kosongnya.
//
// 5. Buat child Text (TextMeshPro - Text) bernama "PercentageText".
//    -> Drag ke field "Progress Text".
//
// 6. Buat child Image bernama "LoadingSpinner" -> Source Image = sprite
//    "Spinner_Circle" (disertakan di paket ini).
//    -> Drag GameObject/RectTransform-nya ke field "Spinner Icon".
//    (Icon ini akan diputar terus otomatis oleh script selama loading berjalan)
//
// 7. Posisi, ukuran, dan tata letak SEMUA elemen di atas (background, bar kosong,
//    bar isi, teks persentase, spinner) BEBAS kamu atur sendiri di Scene view /
//    Inspector, script ini tidak memaksa posisi tertentu dan TIDAK akan mengubahnya
//    lagi saat game berjalan -- cukup tata SATU KALI, akan tetap seperti itu.
//
// 8. Terakhir, add Component "LoadingScreenManager" (script ini) di GameObject
//    "LoadingScreenCanvas", lalu pastikan semua field referensi di atas sudah
//    ke-drag semua (termasuk field baru "Progress Bar Background").
//
// 9. "LoadingScreenCanvas" HANYA perlu ada di scene Main Menu saja (jangan
//    digandakan manual di scene lain, karena sudah otomatis persist/DontDestroyOnLoad).
//
// ============================== CARA PAKAI DARI SCRIPT LAIN ==============================
//   LoadingScreenManager.Instance.LoadScene("TheEarth");
//   LoadingScreenManager.Instance.LoadScene(3); // pakai build index
// ==========================================================================================
[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("CanvasGroup di root Canvas ini, dipakai untuk fade in/out seluruh loading screen")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("Image untuk bar loading yang masih KOSONG (track/bingkai). Image Type = Simple, selalu tampil penuh di belakang bar isi.")]
    [SerializeField] private Image progressBarBackground;

    [Tooltip("Image dengan Image Type = Filled (Fill Method = Horizontal), untuk bar loading yang terisi")]
    [SerializeField] private Image progressBarFill;

    [Tooltip("Teks TextMeshPro untuk menampilkan persentase, contoh: 0% - 100%")]
    [SerializeField] private TMP_Text progressText;

    [Tooltip("RectTransform icon lingkaran yang akan berputar terus selama loading berjalan")]
    [SerializeField] private RectTransform spinnerIcon;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [Tooltip("Aktifkan supaya fade & progress tetap jalan walau Time.timeScale = 0 (misal loading dipicu dari Pause Menu)")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Progress Settings")]
    [Tooltip("Kecepatan bar 'mengejar' nilai progress asli, biar bar terisi halus (tidak lompat-lompat) dari kosong ke penuh mengikuti persentase")]
    [SerializeField] private float progressFillSpeed = 2.5f;
    [Tooltip("Minimal loading screen ditampilkan berapa detik, walau scene sudah selesai dimuat lebih cepat (biar tidak terasa 'kedip')")]
    [SerializeField] private float minimumDisplayDuration = 1f;
    [Tooltip("Jeda di angka 100% sebelum fade out, biar transisinya terasa selesai dengan mantap")]
    [SerializeField] private float holdAtCompleteDuration = 0.25f;

    [Header("Spinner Settings")]
    [Tooltip("Kecepatan putar icon spinner dalam derajat per detik. Nilai negatif = putar berlawanan arah jarum jam")]
    [SerializeField] private float spinnerRotationSpeed = -220f;

    [Header("Canvas Scaler Auto-Fix")]
    [Tooltip("Kalau aktif, script otomatis menyamakan Canvas Scaler loading screen ini dengan Canvas UI utama (Scale With Screen Size), supaya tata letak yang sudah kamu atur TIDAK berubah/mengecil lagi saat dijalankan di resolusi layar yang berbeda.")]
    [SerializeField] private bool autoFixCanvasScaler = true;
    [Tooltip("Reference Resolution yang dipakai Canvas UI utama di project ini (samakan dengan Canvas lain, defaultnya 800x600)")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(800f, 600f);
    [Tooltip("0 = cocokkan lebar layar (Match Width), 1 = cocokkan tinggi layar (Match Height). Samakan dengan Canvas utama.")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0f;

    private bool isLoading;
    private bool isSpinning;

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

        if (fadeCanvasGroup == null) fadeCanvasGroup = GetComponent<CanvasGroup>();

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }

        // Perbaikan utama: samakan Canvas Scaler supaya tata letak yang sudah kamu
        // rapikan di Editor TIDAK berubah ukuran/posisi lagi saat game berjalan.
        ApplyCanvasScalerFix();

        SetProgressVisual(0f);
    }

    /// <summary>
    /// Memaksa Canvas Scaler di Canvas loading screen ini memakai mode "Scale With
    /// Screen Size" dengan Reference Resolution yang sama seperti Canvas UI utama.
    /// Ini yang memperbaiki bug tata letak "teracak"/mengecil, karena sebelumnya
    /// Canvas ini memakai mode "Constant Pixel Size" yang berbeda sendiri.
    /// </summary>
    private void ApplyCanvasScalerFix()
    {
        if (!autoFixCanvasScaler) return;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
    }

    private void Update()
    {
        // Spinner diputar di Update supaya animasinya tetap mulus setiap frame,
        // tidak tersendat mengikuti langkah coroutine loading.
        if (isSpinning && spinnerIcon != null)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            spinnerIcon.Rotate(0f, 0f, spinnerRotationSpeed * dt);
        }
    }

    /// <summary>Pindah scene berdasarkan nama, dengan loading screen (fade + progress bar + persen + spinner).</summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading) return; // cegah dipanggil berkali-kali saat sedang loading
        StartCoroutine(LoadRoutine(sceneName, -1, false));
    }

    /// <summary>Pindah scene berdasarkan build index, dengan loading screen (fade + progress bar + persen + spinner).</summary>
    public void LoadScene(int sceneIndex)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(null, sceneIndex, true));
    }

    private IEnumerator LoadRoutine(string sceneName, int sceneIndex, bool useIndex)
    {
        isLoading = true;
        SetProgressVisual(0f);

        // ── Tahap 1: FADE IN — loading screen (background milikmu) perlahan muncul ──
        if (fadeCanvasGroup != null)
        {
            LeanTween.cancel(fadeCanvasGroup.gameObject);
            fadeCanvasGroup.blocksRaycasts = true; // cegah klik tombol lain saat loading jalan
            fadeCanvasGroup.interactable = true;

            bool fadeInDone = false;
            LeanTween.alphaCanvas(fadeCanvasGroup, 1f, fadeInDuration)
                .setEase(LeanTweenType.easeInOutSine)
                .setIgnoreTimeScale(useUnscaledTime)
                .setOnComplete(() => fadeInDone = true);

            while (!fadeInDone) yield return null;
        }

        isSpinning = true;

        // ── Tahap 2: load scene baru secara ASYNC, bar & persen mengikuti progress asli ──
        AsyncOperation op = useIndex
            ? SceneManager.LoadSceneAsync(sceneIndex)
            : SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // ditahan dulu supaya bar sempat sampai 100% dengan mulus

        float displayedProgress = 0f;
        float elapsed = 0f;

        while (!op.isDone)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += dt;

            // op.progress mentok di 0.9 selama scene belum diaktifkan, jadi dipetakan ke rentang 0-1
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, progressFillSpeed * dt);
            SetProgressVisual(displayedProgress);

            bool sceneReady = op.progress >= 0.9f;
            bool minTimePassed = elapsed >= minimumDisplayDuration;

            if (sceneReady && displayedProgress >= 0.999f && minTimePassed)
            {
                SetProgressVisual(1f);

                if (holdAtCompleteDuration > 0f)
                {
                    if (useUnscaledTime) yield return new WaitForSecondsRealtime(holdAtCompleteDuration);
                    else yield return new WaitForSeconds(holdAtCompleteDuration);
                }

                op.allowSceneActivation = true;
            }

            yield return null;
        }

        isSpinning = false;

        // ── Tahap 3: FADE OUT — loading screen perlahan hilang, scene baru terlihat ──
        if (fadeCanvasGroup != null)
        {
            LeanTween.cancel(fadeCanvasGroup.gameObject);

            bool fadeOutDone = false;
            LeanTween.alphaCanvas(fadeCanvasGroup, 0f, fadeOutDuration)
                .setEase(LeanTweenType.easeInOutSine)
                .setIgnoreTimeScale(useUnscaledTime)
                .setOnComplete(() => fadeOutDone = true);

            while (!fadeOutDone) yield return null;

            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }

        isLoading = false;
    }

    private void SetProgressVisual(float t01)
    {
        t01 = Mathf.Clamp01(t01);

        // Bar kosong (track) selalu tampil penuh sebagai dasar/bingkai.
        if (progressBarBackground != null && !progressBarBackground.gameObject.activeSelf)
            progressBarBackground.gameObject.SetActive(true);

        // Bar isi mengejar persentase secara halus (sudah diatur lewat progressFillSpeed
        // di LoadRoutine menggunakan Mathf.MoveTowards), di sini tinggal menerapkan nilainya.
        if (progressBarFill != null)
            progressBarFill.fillAmount = t01;

        if (progressText != null)
            progressText.text = Mathf.RoundToInt(t01 * 100f) + "%";
    }
}