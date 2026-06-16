// HazardHorizontalMover.cs
// Tambahkan komponen ini ke GameObject apapun agar bergerak KIRI-KANAN
// secara looping dari posisi asalnya (posisi awal = TENGAH/center dari
// pergerakan, object tidak berpindah tempat secara permanen — hanya
// berosilasi di sekitar posisi awalnya).
//
// SIKLUS PERGERAKAN (1 loop):
//   posisi awal -> kanan (rightDistance) -> posisi awal
//                -> kiri  (leftDistance)  -> posisi awal -> (delay) -> ulang
// Urutan kanan-dulu atau kiri-dulu TIDAK diatur di sini — gunakan
// `moveRightFirst` di bawah jika ingin mengubah urutan kiri/kanan dalam
// satu siklus horizontal ini.
//
// JIKA GameObject yang sama JUGA memiliki HazardVerticalMover:
//   gunakan field `order` untuk menentukan apakah pergerakan horizontal
//   ini berjalan LEBIH DULU (order = 1, default) atau SETELAH pergerakan
//   vertical selesai satu siklus (order = 2). Lihat HazardMoveCoordinator.

using System.Collections;
using UnityEngine;

public class HazardHorizontalMover : MonoBehaviour
{
    [Header("Kecepatan & Jarak")]
    [Tooltip("Kecepatan pergerakan kiri-kanan (unit per detik).")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Seberapa jauh object bergerak ke KANAN dari posisi awal.")]
    [SerializeField] private float rightDistance = 2f;

    [Tooltip("Seberapa jauh object bergerak ke KIRI dari posisi awal.")]
    [SerializeField] private float leftDistance = 2f;

    [Header("Urutan & Delay")]
    [Tooltip("Apakah bergerak ke KANAN dulu (true) atau ke KIRI dulu (false) dalam satu siklus.")]
    [SerializeField] private bool moveRightFirst = true;

    [Tooltip("Delay (detik) setelah satu siklus penuh selesai sebelum mengulang. Default 0 = tanpa delay.")]
    [SerializeField] private float loopDelay = 0f;

    [Header("Sinkronisasi dengan Vertical Mover (jika object memiliki keduanya)")]
    [Tooltip("1 = bergerak horizontal dulu, 2 = menunggu vertical selesai 1 siklus dulu. " +
             "Tidak berpengaruh jika object hanya memiliki mover ini saja.")]
    [SerializeField] private int order = 1;

    private Vector3 originalPosition;
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
            // Tunggu giliran jika object juga memiliki vertical mover
            // dengan order yang berbeda.
            while (!coordinator.CanMove(order))
                yield return null;

            if (moveRightFirst)
            {
                yield return MoveTo(originalPosition + Vector3.right * rightDistance);
                yield return MoveTo(originalPosition);
                yield return MoveTo(originalPosition + Vector3.left * leftDistance);
                yield return MoveTo(originalPosition);
            }
            else
            {
                yield return MoveTo(originalPosition + Vector3.left * leftDistance);
                yield return MoveTo(originalPosition);
                yield return MoveTo(originalPosition + Vector3.right * rightDistance);
                yield return MoveTo(originalPosition);
            }

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