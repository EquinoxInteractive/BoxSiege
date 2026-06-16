// HazardDamager.cs — Updated: mendukung Collider2D trigger MAUPUN non-trigger
// (collision biasa), sehingga damage tetap berfungsi baik object di-set
// "Is Trigger" ataupun tidak (misal pemain "menginjak" object via collision
// normal dengan Rigidbody2D).
//
// Tambahkan komponen ini ke GameObject (dengan Collider2D) agar dapat
// memberikan damage ke pemain (P1-P4) saat bersentuhan.
//
// Bisa digabung dengan HazardHorizontalMover / HazardVerticalMover /
// HazardFallOnTouch pada GameObject yang sama — komponen ini berdiri
// sendiri dan tidak bergantung pada komponen lain.
//
// CARA PAKAI:
//   1. Pastikan GameObject memiliki Collider2D.
//      - Boleh "Is Trigger" dicentang ATAU tidak — keduanya didukung.
//   2. Atur `damageAmount` di Inspector sesuai damage yang diinginkan.
//   3. Atur `damageCooldown` jika ingin mencegah damage berulang setiap
//      frame selama pemain masih bersentuhan (default 1 detik).
//
// TROUBLESHOOTING jika damage tetap tidak berfungsi:
//   - Pastikan GameObject pemain memiliki tag "Player".
//   - Pastikan salah satu dari kedua object (hazard atau player) memiliki
//     Rigidbody2D (syarat Unity agar event collision/trigger terpanggil).
//   - Pastikan Collider2D pada hazard berada pada GameObject yang SAMA
//     dengan komponen HazardDamager ini (atau pada child — lihat catatan
//     di bawah jika collider berada di child object).

using UnityEngine;
using System.Collections.Generic;

public class HazardDamager : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Jumlah damage yang diberikan ke pemain saat bersentuhan.\n\n" +
             "Damage ini berlaku sebagai TOTAL — shield diserap dulu, sisanya\n" +
             "mengurangi health bar.\n\n" +
             "Contoh: Damage Amount = 3, pemain punya shield 2 charge + 3 health bar\n" +
             "→ shield habis (dikurangi 2), health bar berkurang 1, sisa 2 health bar.")]
    [SerializeField] private float damageAmount = 1f;

    [Tooltip("Jeda (detik) sebelum pemain yang sama bisa terkena damage lagi " +
             "selama masih bersentuhan dengan object ini. Mencegah damage " +
             "berulang setiap frame.")]
    [SerializeField] private float damageCooldown = 1f;

    // Menyimpan waktu terakhir setiap pemain (berdasarkan instance Collider2D)
    // menerima damage dari object ini.
    private Dictionary<Collider2D, float> lastDamageTime = new Dictionary<Collider2D, float>();

    // ─── Trigger Collider ───────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);
    private void OnTriggerStay2D(Collider2D other)  => TryDamage(other);

    private void OnTriggerExit2D(Collider2D other)
    {
        if (lastDamageTime.ContainsKey(other))
            lastDamageTime.Remove(other);
    }

    // ─── Non-Trigger (Collision) Collider ────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);
    private void OnCollisionStay2D(Collision2D collision)  => TryDamage(collision.collider);

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (lastDamageTime.ContainsKey(collision.collider))
            lastDamageTime.Remove(collision.collider);
    }

    // ─── Logic ────────────────────────────────────────────────────────────────
    private void TryDamage(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        float now = Time.time;
        if (lastDamageTime.TryGetValue(other, out float lastTime))
        {
            if (now - lastTime < damageCooldown) return;
        }

        lastDamageTime[other] = now;

        // Berlaku untuk ke-4 pemain (P1-P4) — coba ambil komponen Health
        // sesuai pemain mana yang bersentuhan. GetComponentInParent dipakai
        // sebagai fallback jika Collider2D pemain berada di child object
        // (misal child "Hitbox") sementara komponen Health berada di
        // GameObject induk (root player).
        Health   p1 = other.GetComponent<Health>()   ?? other.GetComponentInParent<Health>();
        HealthP2 p2 = other.GetComponent<HealthP2>() ?? other.GetComponentInParent<HealthP2>();
        HealthP3 p3 = other.GetComponent<HealthP3>() ?? other.GetComponentInParent<HealthP3>();
        HealthP4 p4 = other.GetComponent<HealthP4>() ?? other.GetComponentInParent<HealthP4>();

        if      (p1 != null) p1.TakeDamage(damageAmount);
        else if (p2 != null) p2.TakeDamage(damageAmount);
        else if (p3 != null) p3.TakeDamage(damageAmount);
        else if (p4 != null) p4.TakeDamage(damageAmount);
        else
            Debug.LogWarning($"[HazardDamager] Object '{other.name}' bertag 'Player' tapi komponen Health/HealthP2/3/4 tidak ditemukan.");
    }
}