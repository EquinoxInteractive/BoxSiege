using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TimeSelectionUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button playButton; // Tombol untuk memulai pemilihan map
    [SerializeField] private TextMeshProUGUI timeDisplayText;

    private float selectedTime = 30f; // Mulai dari 30 detik
    private const float TIME_STEP = 30f; // Increment/decrement 30 detik
    private const float MIN_TIME = 30f; // Minimum 30 detik
    private const float MAX_TIME = 300f; // Maximum 5 menit (300 detik)

    private void Start()
    {
        // Add listeners to buttons
        if (nextButton != null)
            nextButton.onClick.AddListener(IncreaseTime);

        if (backButton != null)
            backButton.onClick.AddListener(DecreaseTime);

        if (playButton != null)
            playButton.onClick.AddListener(Play);

        // Initialize time display
        UpdateTimeDisplay();
    }

    private void IncreaseTime()
    {
        if (selectedTime < MAX_TIME)
        {
            selectedTime += TIME_STEP;
            UpdateTimeDisplay();
        }
    }

    private void DecreaseTime()
    {
        if (selectedTime > MIN_TIME)
        {
            selectedTime -= TIME_STEP;
            UpdateTimeDisplay();
        }
    }

    private void Play()
    {
        // Simpan waktu yang dipilih ke PlayerPrefs
        PlayerPrefs.SetFloat("SelectedGameTime", selectedTime);
        PlayerPrefs.Save();
    }

    private void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(selectedTime / 60);
        int seconds = Mathf.FloorToInt(selectedTime % 60);
        timeDisplayText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (nextButton != null)
            nextButton.onClick.RemoveAllListeners();

        if (backButton != null)
            backButton.onClick.RemoveAllListeners();

        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
    }
}