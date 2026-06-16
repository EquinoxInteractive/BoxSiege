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

    [Tooltip("Seberapa jauh object naik ke ATAS dari posisi awal (posisi awal = posisi paling bawah).")]
    [SerializeField] private float upDistance = 2f;

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
            // Tunggu giliran jika object juga memiliki horizontal mover
            // dengan order yang berbeda.
            while (!coordinator.CanMove(order))
                yield return null;

            yield return MoveTo(originalPosition + Vector3.up * upDistance);
            yield return MoveTo(originalPosition);

            if (loopDelay > 0f)
                yield return new WaitForSeconds(loopDelay);

            // Satu siklus penuh selesai — beri giliran ke mover lain (jika ada).
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
}