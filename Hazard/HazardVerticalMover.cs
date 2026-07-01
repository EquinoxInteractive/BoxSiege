// HazardVerticalMover.cs
// Tambahkan komponen ini ke GameObject apapun agar bergerak ATAS-BAWAH
// secara looping. Posisi awal object dianggap sebagai posisi PALING BAWAH
// (bottom) dari pergerakan — object akan naik ke atas sejauh `upDistance`,
// lalu turun kembali ke posisi awal.
//
// SIKLUS PERGERAKAN (1 loop):
//   posisi awal (bawah) -> naik (upDistance) -> turun -> posisi awal -> (delay) -> ulang
//
// JIKA GameObject yang sama JUGA memiliki HazardHorizontalMover:
//   gunakan field `order` untuk menentukan apakah pergerakan vertical ini
//   berjalan LEBIH DULU (order = 1) atau SETELAH horizontal selesai satu
//   siklus (order = 2, default). Lihat HazardMoveCoordinator.

using System.Collections;
using UnityEngine;

public class HazardVerticalMover : MonoBehaviour
{
    [Header("Kecepatan & Jarak")]
    [Tooltip("Kecepatan pergerakan naik-turun (unit per detik).")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Seberapa jauh object naik ke ATAS dari posisi awal.")]
    [SerializeField] private float upDistance = 2f;

    [Tooltip("Seberapa jauh object turun ke BAWAH dari posisi awal.\n" +
             "Default 0 = posisi awal sudah menjadi titik paling bawah " +
             "(object hanya naik lalu kembali ke posisi awal).")]
    [SerializeField] private float downDistance = 0f;

    [Header("Urutan & Delay")]
    [Tooltip("Apakah bergerak ke ATAS dulu (true) atau ke BAWAH dulu (false) dalam satu siklus.\n\n" +
             "true  (Move Up First)  : posisi awal → atas → awal → bawah → awal → (delay) → ulang\n" +
             "false (Move Down First): posisi awal → bawah → awal → atas → awal → (delay) → ulang\n\n" +
             "Jika downDistance = 0, siklus hanya naik-turun ke posisi awal tanpa fase bawah.")]
    [SerializeField] private bool moveUpFirst = true;

    [Header("Delay")]
    [Tooltip("Delay (detik) setelah satu siklus penuh (naik+turun) selesai sebelum mengulang. Default 0 = tanpa delay.")]
    [SerializeField] private float loopDelay = 0f;

    [Header("Sinkronisasi dengan Horizontal Mover (jika object memiliki keduanya)")]
    [Tooltip("1 = bergerak vertical dulu, 2 = menunggu horizontal selesai 1 siklus dulu. " +
             "Tidak berpengaruh jika object hanya memiliki mover ini saja.")]
    [SerializeField] private int order = 2;

    private Vector3 originalPosition; // posisi paling bawah
    private HazardMoveCoordinator coordinator;

    private void Awake()
    {
        originalPosition = transform.position;

        coordinator = GetComponent<HazardMoveCoordinator>();
        if (coordinator == null) coordinator = gameObject.AddComponent<HazardMoveCoordinator>();
        coordinator.RegisterMover();
    }

    private void Start()
    {
        StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            while (!coordinator.CanMove(order))
                yield return null;

            if (moveUpFirst)
            {
                yield return MoveTo(originalPosition + Vector3.up * upDistance);
                yield return MoveTo(originalPosition);
                if (downDistance > 0f)
                {
                    yield return MoveTo(originalPosition + Vector3.down * downDistance);
                    yield return MoveTo(originalPosition);
                }
            }
            else
            {
                if (downDistance > 0f)
                {
                    yield return MoveTo(originalPosition + Vector3.down * downDistance);
                    yield return MoveTo(originalPosition);
                }
                yield return MoveTo(originalPosition + Vector3.up * upDistance);
                yield return MoveTo(originalPosition);
            }

            if (loopDelay > 0f)
                yield return new WaitForSeconds(loopDelay);

            coordinator.AdvanceTurn(order);
        }
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    // Berguna jika HazardFallOnTouch ingin menghentikan pergerakan saat object jatuh.
    public void StopMovement()
    {
        StopAllCoroutines();
    }

    // Dipanggil oleh HazardResetManager setiap awal round baru.
    // Mengembalikan posisi ke posisi awal dan me-restart loop pergerakan.
    public void ResetHazard()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        StartCoroutine(MoveLoop());
    }
}