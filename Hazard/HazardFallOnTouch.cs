// HazardFallOnTouch.cs
// Tambahkan komponen ini ke GameObject agar object JATUH KE BAWAH saat
// disentuh/diinjak oleh pemain (P1-P4). Cocok untuk platform yang runtuh
// saat diinjak.
//
// PERILAKU:
//   - Saat pemain pertama kali bersentuhan dengan object ini, object akan
//     bergerak turun (Vector3.down) sejauh `fallDistance` dengan kecepatan
//     `fallSpeed`, lalu BERHENTI di posisi tersebut (tidak kembali naik).
//   - Jika object juga memiliki HazardHorizontalMover dan/atau
//     HazardVerticalMover, kedua mover tersebut akan otomatis DIHENTIKAN
//     begitu object mulai jatuh (StopMovement()).
//   - Object hanya jatuh SATU KALI per object — sentuhan berikutnya tidak
//     memicu jatuh lagi (mencegah jatuh berulang/menumpuk).
//
// CARA PAKAI:
//   1. Pastikan GameObject memiliki Collider2D.
//      - Jika ingin trigger jatuh saat pemain MENDARAT DI ATAS (diinjak),
//        gunakan Collider2D non-trigger (collision normal) dan pastikan
//        pemain memiliki Rigidbody2D — atau set Collider2D ini sebagai
//        trigger jika ingin jatuh saat bersentuhan dari arah manapun.
//   2. Atur `fallSpeed` (kecepatan jatuh) dan `fallDistance`
//      (seberapa jauh object jatuh) di Inspector.

using System.Collections;
using UnityEngine;

public class HazardFallOnTouch : MonoBehaviour
{
    [Header("Fall Settings")]
    [Tooltip("Delay (detik) setelah disentuh pemain sebelum object mulai jatuh. Default 0 = langsung jatuh.")]
    [SerializeField] private float fallDelay = 0f;

    [Tooltip("Kecepatan jatuh object (unit per detik).")]
    [SerializeField] private float fallSpeed = 5f;

    [Tooltip("Seberapa jauh object jatuh ke bawah dari posisi saat disentuh.")]
    [SerializeField] private float fallDistance = 5f;

    private bool hasFallen = false;
    private Vector3 originalPosition;

    private HazardHorizontalMover horizontalMover;
    private HazardVerticalMover   verticalMover;

    private void Awake()
    {
        originalPosition = transform.position;
        horizontalMover  = GetComponent<HazardHorizontalMover>();
        verticalMover    = GetComponent<HazardVerticalMover>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerFall(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTriggerFall(collision.collider);
    }

    private void TryTriggerFall(Collider2D other)
    {
        if (hasFallen) return;
        if (!other.CompareTag("Player")) return;

        hasFallen = true;

        // Hentikan pergerakan kiri-kanan / atas-bawah jika ada,
        // agar object hanya bergerak jatuh ke bawah.
        if (horizontalMover != null) horizontalMover.StopMovement();
        if (verticalMover   != null) verticalMover.StopMovement();

        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        if (fallDelay > 0f)
            yield return new WaitForSeconds(fallDelay);

        Vector3 startPosition  = transform.position;
        Vector3 targetPosition = startPosition + Vector3.down * fallDistance;

        while ((transform.position - targetPosition).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    // Dipanggil oleh HazardResetManager setiap awal round baru.
    public void ResetHazard()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        hasFallen          = false;
        // Mover direset oleh HazardResetManager secara terpisah.
    }
}