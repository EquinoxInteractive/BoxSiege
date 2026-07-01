// HazardPlayerCarrier.cs — Rewritten
// Menggunakan polling (OverlapBox setiap LateUpdate) untuk mendeteksi
// pemain di atas hazard, dan rb.MovePosition() untuk memindahkan pemain
// ikut bergerak — kompatibel dengan Rigidbody2D Dynamic pemain.
//
// CARA KERJA:
//   Setiap LateUpdate:
//   1. Hitung delta posisi hazard frame ini.
//   2. Lakukan OverlapBox di area ATAS collider hazard (strip tipis di
//      bagian atas) untuk menemukan pemain yang sedang menginjak.
//   3. Pindahkan Rigidbody2D setiap pemain yang terdeteksi menggunakan
//      rb.MovePosition(rb.position + delta) agar physics engine ikut update.
//   4. Pemain yang tidak terdeteksi di atas (di samping/bawah/jauh) tidak
//      ikut terbawa.
//
// CARA PAKAI:
//   1. Tambahkan komponen ini ke GameObject hazard yang bergerak.
//   2. Tidak perlu Rigidbody2D di hazard — script ini TIDAK menggunakan
//      OnCollision sama sekali, jadi tidak ada syarat RB di hazard.
//   3. Pastikan hazard memiliki Collider2D (non-trigger) agar bisa
//      diinjak pemain secara fisik.
//   4. Pastikan pemain memiliki tag "Player" dan Rigidbody2D Dynamic.

using UnityEngine;
using System.Collections.Generic;

public class HazardPlayerCarrier : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Ketebalan strip deteksi di bagian atas collider (dalam unit). " +
             "Nilai kecil = hanya mendeteksi pemain tepat di atas. " +
             "Default 0.15 sudah cukup untuk kebanyakan kasus.")]
    [SerializeField] private float topDetectionHeight = 0.15f;

    [Tooltip("Layer yang dimiliki oleh pemain, untuk OverlapBox. " +
             "Jika dibiarkan Nothing/Default, akan scan semua layer " +
             "(sedikit lebih lambat tapi tetap berfungsi).")]
    [SerializeField] private LayerMask playerLayer;

    // Collider2D hazard — dipakai untuk hitung posisi strip atas
    private Collider2D hazardCollider;

    // Posisi hazard frame sebelumnya
    private Vector3 previousPosition;

    // Cache Rigidbody2D semua pemain (dicari sekali di Start)
    private List<Rigidbody2D> allPlayerRigidbodies = new List<Rigidbody2D>();

    private void Awake()
    {
        hazardCollider   = GetComponent<Collider2D>();
        previousPosition = transform.position;

        if (hazardCollider == null)
            Debug.LogWarning($"[HazardPlayerCarrier] '{gameObject.name}' tidak memiliki " +
                             "Collider2D! Tambahkan Collider2D non-trigger ke object ini.");
    }

    private void Start()
    {
        // Cari semua Rigidbody2D pemain di scene sekali saat Start.
        // Ini lebih efisien daripada FindObjectsOfType tiap frame.
        foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
                allPlayerRigidbodies.Add(rb);
        }

        if (allPlayerRigidbodies.Count == 0)
            Debug.LogWarning("[HazardPlayerCarrier] Tidak ada GameObject bertag 'Player' " +
                             "dengan Rigidbody2D yang ditemukan di scene!");
    }

    private void LateUpdate()
    {
        // Hitung delta posisi hazard frame ini
        Vector3 delta = transform.position - previousPosition;
        previousPosition = transform.position;

        // Jika hazard tidak bergerak frame ini, skip
        if (delta.sqrMagnitude < 0.000001f) return;
        if (allPlayerRigidbodies.Count == 0) return;
        if (hazardCollider == null) return;

        // Hitung area strip atas collider hazard
        Bounds bounds     = hazardCollider.bounds;
        Vector2 stripSize = new Vector2(bounds.size.x * 0.9f, topDetectionHeight);
        Vector2 stripCenter = new Vector2(
            bounds.center.x,
            bounds.max.y + topDetectionHeight * 0.5f
        );

        // Deteksi collider di strip atas — gunakan OverlapBoxAll agar
        // bisa mengecek setiap collider yang ditemukan
        Collider2D[] hits = Physics2D.OverlapBoxAll(stripCenter, stripSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            if (hit.isTrigger) continue;
            if (!hit.CompareTag("Player")) continue;

            // Cari Rigidbody2D dari collider ini (bisa di parent jika hitbox di child)
            Rigidbody2D playerRb = hit.attachedRigidbody;
            if (playerRb == null) playerRb = hit.GetComponentInParent<Rigidbody2D>();
            if (playerRb == null) continue;

            // Pindahkan pemain ikut delta hazard menggunakan MovePosition
            // agar kompatibel dengan physics engine (tidak override velocity)
            playerRb.MovePosition(playerRb.position + new Vector2(delta.x, delta.y));
        }
    }

    // Dipanggil oleh HazardResetManager setiap awal round baru.
    public void ResetHazard()
    {
        previousPosition = transform.position;
        // allPlayerRigidbodies tetap valid — pemain tidak despawn antar round
    }
}