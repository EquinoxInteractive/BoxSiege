using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button applyButton;

    private List<ResolutionOption> resolutionOptions = new List<ResolutionOption>();
    private List<string> dropdownOptions = new List<string>();
    private int pendingOptionIndex;
    private bool pendingFullscreen;
    private bool isInitialized;

    // Struktur untuk menyimpan kombinasi resolusi dan refresh rate
    private struct ResolutionOption
    {
        public int width;
        public int height;
        public RefreshRate refreshRate;
    }

    private void Start()
    {
        // Validasi referensi UI
        if (!ValidateUIReferences()) return;

        InitializeResolutions();
        LoadSettings();
        SetupUI();
        isInitialized = true;

        // Menambahkan listener untuk tombol Apply
        applyButton.onClick.AddListener(ApplySettings);
    }

    private bool ValidateUIReferences()
    {
        bool isValid = true;
        if (resolutionDropdown == null)
        {
            Debug.LogError("Resolution Dropdown is not assigned in the Inspector!", this);
            isValid = false;
        }
        if (fullscreenToggle == null)
        {
            Debug.LogError("Fullscreen Toggle is not assigned in the Inspector!", this);
            isValid = false;
        }
        if (applyButton == null)
        {
            Debug.LogError("Apply Button is not assigned in the Inspector!", this);
            isValid = false;
        }
        return isValid;
    }

    // Method untuk mengecek apakah resolusi memiliki aspek rasio 16:9
    private bool Is16by9AspectRatio(int width, int height)
    {
        // Menghitung GCD untuk menyederhanakan rasio
        int gcd = CalculateGCD(width, height);
        int simplifiedWidth = width / gcd;
        int simplifiedHeight = height / gcd;
        
        // Cek apakah rasio adalah 16:9
        return (simplifiedWidth == 16 && simplifiedHeight == 9);
    }

    // Method untuk menghitung Greatest Common Divisor (GCD)
    private int CalculateGCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private void InitializeResolutions()
    {
        // Mendapatkan daftar resolusi yang didukung
        Resolution[] resolutions = Screen.resolutions;
        resolutionOptions.Clear();
        dropdownOptions.Clear();

        // Filter hanya resolusi dengan aspek rasio 16:9
        var filtered169Resolutions = resolutions
            .Where(r => Is16by9AspectRatio(r.width, r.height))
            .ToArray();

        // Mengelompokkan resolusi berdasarkan ukuran dan mengumpulkan refresh rate
        var groupedResolutions = filtered169Resolutions
            .GroupBy(r => new { r.width, r.height })
            .Select(g => new
            {
                Width = g.Key.width,
                Height = g.Key.height,
                RefreshRates = g.Select(r => r.refreshRateRatio).Distinct().OrderBy(r => r.numerator / (float)r.denominator).ToList()
            })
            .OrderByDescending(g => g.Width)
            .ThenByDescending(g => g.Height)
            .ToList();

        // Membuat opsi untuk dropdown
        int currentIndex = 0;
        bool currentResolutionFound = false;

        foreach (var group in groupedResolutions)
        {
            foreach (RefreshRate refreshRate in group.RefreshRates)
            {
                resolutionOptions.Add(new ResolutionOption
                {
                    width = group.Width,
                    height = group.Height,
                    refreshRate = refreshRate
                });
                // Konversi refresh rate ke Hz untuk tampilan
                float hz = refreshRate.numerator / (float)refreshRate.denominator;
                dropdownOptions.Add($"{group.Width}x{group.Height} @ {hz:F0}Hz");

                // Menemukan resolusi dan refresh rate saat ini
                if (group.Width == Screen.currentResolution.width &&
                    group.Height == Screen.currentResolution.height &&
                    refreshRate.Equals(Screen.currentResolution.refreshRateRatio))
                {
                    pendingOptionIndex = currentIndex;
                    currentResolutionFound = true;
                }
                currentIndex++;
            }
        }

        // Jika tidak ada resolusi 16:9 yang ditemukan atau resolusi saat ini bukan 16:9
        if (resolutionOptions.Count == 0)
        {
            Debug.LogWarning("No 16:9 resolutions found! Adding common 16:9 resolutions as fallback.");
            AddFallback16by9Resolutions();
        }
        else if (!currentResolutionFound)
        {
            // Jika resolusi saat ini bukan 16:9, pilih resolusi 16:9 terdekat
            Debug.LogWarning($"Current resolution {Screen.currentResolution.width}x{Screen.currentResolution.height} is not 16:9. Selecting closest 16:9 resolution.");
            pendingOptionIndex = FindClosest16by9Resolution(Screen.currentResolution.width, Screen.currentResolution.height);
        }
    }

    // Method untuk menambahkan resolusi 16:9 umum sebagai fallback
    private void AddFallback16by9Resolutions()
    {
        // Resolusi 16:9 yang umum digunakan
        int[,] common16by9 = new int[,] {
            {1920, 1080}, // Full HD
            {1366, 768},  // HD
            {1280, 720},  // HD Ready
            {2560, 1440}, // QHD
            {3840, 2160}  // 4K
        };

        RefreshRate defaultRefreshRate = new RefreshRate { numerator = 60, denominator = 1 };

        for (int i = 0; i < common16by9.GetLength(0); i++)
        {
            int width = common16by9[i, 0];
            int height = common16by9[i, 1];

            resolutionOptions.Add(new ResolutionOption
            {
                width = width,
                height = height,
                refreshRate = defaultRefreshRate
            });
            dropdownOptions.Add($"{width}x{height} @ 60Hz");
        }

        pendingOptionIndex = 0; // Pilih resolusi pertama sebagai default
    }

    // Method untuk mencari resolusi 16:9 terdekat
    private int FindClosest16by9Resolution(int targetWidth, int targetHeight)
    {
        if (resolutionOptions.Count == 0) return 0;

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            var option = resolutionOptions[i];
            // Menghitung jarak menggunakan Euclidean distance
            float distance = Mathf.Sqrt(Mathf.Pow(option.width - targetWidth, 2) + Mathf.Pow(option.height - targetHeight, 2));
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void SetupUI()
    {
        // Mengatur dropdown resolusi
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(dropdownOptions);
        resolutionDropdown.value = pendingOptionIndex;
        resolutionDropdown.RefreshShownValue();

        // Mengatur toggle fullscreen
        pendingFullscreen = Screen.fullScreen;
        fullscreenToggle.isOn = pendingFullscreen;

        // Mengatur tombol apply
        applyButton.interactable = true;
        
        // Log informasi resolusi yang tersedia
        Debug.Log($"Available 16:9 resolutions: {resolutionOptions.Count} options loaded.");
        if (resolutionOptions.Count > 0)
        {
            var currentOption = resolutionOptions[pendingOptionIndex];
            float hz = currentOption.refreshRate.numerator / (float)currentOption.refreshRate.denominator;
            Debug.Log($"Selected resolution: {currentOption.width}x{currentOption.height} @ {hz:F0}Hz");
        }
    }

    private void LoadSettings()
    {
        // Memuat pengaturan yang tersimpan
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex");
            // Memastikan index valid
            if (savedIndex >= 0 && savedIndex < resolutionOptions.Count)
            {
                pendingOptionIndex = savedIndex;
                ResolutionOption savedOption = resolutionOptions[savedIndex];
                Screen.SetResolution(savedOption.width, savedOption.height, pendingFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, savedOption.refreshRate);
            }
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            pendingFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            Screen.fullScreen = pendingFullscreen;
        }
    }

    public void SetPendingResolution(int optionIndex)
    {
        pendingOptionIndex = optionIndex;
        ResolutionOption option = resolutionOptions[optionIndex];
        float hz = option.refreshRate.numerator / (float)option.refreshRate.denominator;
        Debug.Log($"Pending resolution set to: {option.width}x{option.height} @ {hz:F0}Hz (16:9 aspect ratio)");
    }

    public void SetPendingFullscreen(bool isFullscreen)
    {
        pendingFullscreen = isFullscreen;
        Debug.Log($"Pending fullscreen set to: {isFullscreen}");
    }

    public void ApplySettings()
    {
        if (!isInitialized) return;

        // Menerapkan pengaturan resolusi dan fullscreen
        ResolutionOption option = resolutionOptions[pendingOptionIndex];
        Screen.SetResolution(option.width, option.height, pendingFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, option.refreshRate);

        // Menyimpan pengaturan
        PlayerPrefs.SetInt("ResolutionIndex", pendingOptionIndex);
        PlayerPrefs.SetInt("Fullscreen", pendingFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        float hz = option.refreshRate.numerator / (float)option.refreshRate.denominator;
        Debug.Log($"Settings applied: {option.width}x{option.height} @ {hz:F0}Hz (16:9), Fullscreen: {pendingFullscreen}");
    }
}