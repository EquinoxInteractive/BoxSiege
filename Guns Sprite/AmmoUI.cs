using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("Ammo Text")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Unlimited UI")]
    [SerializeField] private GameObject unlimitedUI;

    private void Start()
    {
        if (unlimitedUI != null) unlimitedUI.SetActive(false);
        if (ammoText != null)    ammoText.gameObject.SetActive(true);
    }

    public void SetMaxAmmo(int max)
    {
        if (unlimitedUI != null) unlimitedUI.SetActive(false);
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = max.ToString();
        }
    }

    public void UpdateAmmo(int remaining)
    {
        if (unlimitedUI != null) unlimitedUI.SetActive(false);
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = Mathf.Max(0, remaining).ToString();
        }
    }

    public void ShowUnlimited()
    {
        Debug.Log($"[AmmoUI] {gameObject.name}: ShowUnlimited dipanggil.");

        if (ammoText != null) ammoText.gameObject.SetActive(false);

        if (unlimitedUI != null)
        {
            unlimitedUI.SetActive(true);
            Debug.Log($"[AmmoUI] {gameObject.name}: unlimitedUI di-SetActive(true).");
        }
        else
        {
            Debug.LogWarning($"[AmmoUI] {gameObject.name}: unlimitedUI BELUM DIASSIGN di Inspector!");
        }
    }

    /// <summary>
    /// Dipanggil saat player beralih ke melee mode.
    /// Tampilkan UI infinite (sama seperti secondary weapon).
    /// </summary>
    public void ShowMeleeMode()
    {
        Debug.Log($"[AmmoUI] {gameObject.name}: ShowMeleeMode dipanggil.");

        if (ammoText != null) ammoText.gameObject.SetActive(false);

        if (unlimitedUI != null)
            unlimitedUI.SetActive(true);
        else
            Debug.LogWarning($"[AmmoUI] {gameObject.name}: unlimitedUI BELUM DIASSIGN di Inspector!");
    }

    /// <summary>
    /// Dipanggil saat player kembali dari melee ke senjata utama.
    /// Kembalikan tampilan ammo yang tersisa.
    /// </summary>
    public void HideMeleeMode(int currentAmmo)
    {
        if (unlimitedUI != null) unlimitedUI.SetActive(false);
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = Mathf.Max(0, currentAmmo).ToString();
        }
    }

    public void ResetUI(int maxAmmo)
    {
        if (unlimitedUI != null) unlimitedUI.SetActive(false);
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = maxAmmo.ToString();
        }
    }
}