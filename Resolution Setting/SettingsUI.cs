using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GraphicsSettings graphicsSettings;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Validasi referensi
        if (!ValidateReferences()) return;

        // Mengatur listener untuk UI
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(delegate { graphicsSettings.SetPendingResolution(resolutionDropdown.value); });
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.AddListener(delegate { graphicsSettings.SetPendingFullscreen(fullscreenToggle.isOn); });
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        if (graphicsSettings == null)
        {
            Debug.LogError("GraphicsSettings is not assigned in the Inspector!", this);
            isValid = false;
        }
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
        return isValid;
    }
}