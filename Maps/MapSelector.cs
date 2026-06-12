using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapSelector : MonoBehaviour
{
    [SerializeField] private Image mapPreviewImage; // UI Image untuk preview map
    [SerializeField] private Text mapNameText; // UI Text untuk nama map
    [SerializeField] private Button nextButton; // Tombol arrow kanan
    [SerializeField] private Button backButton; // Tombol arrow kiri
    [SerializeField] private Button playButton; // Tombol play

    [System.Serializable]
    public class MapData
    {
        public string mapName; // Nama scene map
        public string displayName; // Nama map untuk ditampilkan di UI
        public Sprite previewSprite; // Sprite preview untuk map
    }

    [SerializeField] private MapData[] maps; // Array data map
    private int currentMapIndex = 0; // Index map yang sedang dipilih

    void Start()
    {
        // Tambahkan listener untuk tombol
        nextButton.onClick.AddListener(NextMap);
        backButton.onClick.AddListener(BackMap);
        playButton.onClick.AddListener(PlayMap);

        // Update UI untuk map pertama
        UpdateMapPreview();
    }

    void NextMap()
    {
        currentMapIndex = (currentMapIndex + 1) % maps.Length;
        UpdateMapPreview();
    }

    void BackMap()
    {
        currentMapIndex = (currentMapIndex - 1 + maps.Length) % maps.Length;
        UpdateMapPreview();
    }

    void UpdateMapPreview()
    {
        // Update gambar preview dan nama map
        mapPreviewImage.sprite = maps[currentMapIndex].previewSprite;
        mapNameText.text = maps[currentMapIndex].displayName;

        // Update status tombol
        backButton.interactable = maps.Length > 1;
        nextButton.interactable = maps.Length > 1;
    }

    void PlayMap()
    {
        // Memuat scene map yang dipilih
        SceneManager.LoadScene(maps[currentMapIndex].mapName);
    }
}