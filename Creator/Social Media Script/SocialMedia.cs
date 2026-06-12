using UnityEngine;
using UnityEngine.UI;

public class SocialMedia : MonoBehaviour
{
    [Header("Website URL")]
    public string websiteURL = "https://example.com";
    
    private void Start()
    {
        // Jika GameObject memiliki Button component (untuk UI)
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OpenWebsite);
        }
    }
    
    // Method untuk handle click pada gambar (untuk GameObject dengan Collider)
    private void OnMouseDown()
    {
        OpenWebsite();
    }
    
    // Method untuk membuka website
    public void OpenWebsite()
    {
        if (string.IsNullOrEmpty(websiteURL))
        {
            Debug.LogError("Website URL kosong!");
            return;
        }
        
        // Pastikan URL memiliki https://
        string url = websiteURL;
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }
        
        // Buka website
        Application.OpenURL(url);
        Debug.Log("Membuka website: " + url);
    }
}