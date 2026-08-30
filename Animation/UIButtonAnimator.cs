using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// UIButtonAnimator.cs
// Script UMUM (reusable) untuk kasih efek "juicy" ke tombol: membesar saat di-hover,
// mengecil sedikit saat ditekan, lalu balik lagi. Efek ini yang bikin UI terasa
// interaktif/mahal seperti game profesional, dibanding tombol diam saja.
//
// CARA ASSIGN DI UNITY:
// 1. Pilih GameObject Button yang mau dianimasikan (bisa tombol Play, Pause, Resume, dll).
// 2. Add Component -> UIButtonAnimator.
// 3. (Opsional) Di komponen Button bawaan Unity, ubah "Transition" jadi "None"
//    supaya tidak tabrakan dengan animasi warna/scale bawaan Unity.
// 4. Tidak perlu drag apapun ke field manapun — script otomatis ambil RectTransform
//    dan Image di GameObject yang sama. Field "Target Graphic" opsional kalau mau override.
// 5. Selesai. Tinggal Play, arahkan mouse ke tombolnya.
[RequireComponent(typeof(RectTransform))]
public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Punch")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.94f;
    [SerializeField] private float scaleDuration = 0.15f;

    [Header("Color (opsional)")]
    [SerializeField] private bool animateColor = true;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Graphic targetGraphic;

    private RectTransform rect;
    private Vector3 baseScale = Vector3.one;
    private Color baseColor = Color.white;
    private bool hasBaseColor = false;
    private bool isHovering = false;
    private bool isPressed = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;

        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }
        if (targetGraphic != null)
        {
            baseColor = targetGraphic.color;
            hasBaseColor = true;
        }
    }

    private void OnEnable()
    {
        // Reset ke kondisi normal setiap kali tombolnya diaktifkan lagi
        // (menghindari tombol "nyangkut" dalam kondisi membesar/kecil).
        if (rect != null)
        {
            LeanTween.cancel(gameObject);
            rect.localScale = baseScale;
        }
        if (targetGraphic != null && hasBaseColor)
        {
            targetGraphic.color = baseColor;
        }
        isHovering = false;
        isPressed = false;
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        isHovering = false;
        isPressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        AnimateTo(hoverScale);
        AnimateColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isPressed)
        {
            AnimateTo(1f);
            AnimateColor(baseColor);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        AnimateTo(pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        AnimateTo(isHovering ? hoverScale : 1f);
        if (!isHovering)
        {
            AnimateColor(baseColor);
        }
    }

    private void AnimateTo(float scaleMultiplier)
    {
        if (rect == null) return;
        LeanTween.cancel(gameObject);
        LeanTween.scale(rect, baseScale * scaleMultiplier, scaleDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true); // tombol menu tetap responsif walau game di-pause
    }

    private void AnimateColor(Color color)
    {
        if (!animateColor || targetGraphic == null) return;
        LeanTween.value(gameObject, targetGraphic.color, color, scaleDuration)
            .setOnUpdateColor((Color c) => { targetGraphic.color = c; })
            .setIgnoreTimeScale(true);
    }
}