// HazardResetManager.cs
// Taruh komponen ini di SATU GameObject di scene (misalnya GameObject
// "GameManager" atau buat GameObject kosong baru bernama "HazardResetManager").
// Tidak perlu assign apapun — script ini otomatis mencari semua hazard
// di scene dan mereset posisi + state mereka setiap round.
//
// Dipanggil oleh GameManager (GameManger.cs) di PrepareNextRound via
// FindObjectOfType<HazardResetManager>()?.ResetAll().
//
// Yang direset:
//   - HazardHorizontalMover : posisi kembali ke posisi awal, loop restart
//   - HazardVerticalMover   : posisi kembali ke posisi awal, loop restart
//   - HazardFallOnTouch     : posisi kembali ke posisi saat Awake, bisa jatuh lagi
//   - HazardPlayerCarrier   : riding players list dikosongkan

using UnityEngine;
using System.Collections;

public class HazardResetManager : MonoBehaviour
{
    // Dipanggil oleh GameManager di awal setiap round baru.
    public void ResetAll()
    {
        // Reset semua HazardFallOnTouch (posisi + state jatuh)
        foreach (var fall in FindObjectsOfType<HazardFallOnTouch>())
            fall.ResetHazard();

        // Reset semua HazardHorizontalMover (posisi + restart loop)
        foreach (var h in FindObjectsOfType<HazardHorizontalMover>())
            h.ResetHazard();

        // Reset semua HazardVerticalMover (posisi + restart loop)
        foreach (var v in FindObjectsOfType<HazardVerticalMover>())
            v.ResetHazard();

        // Reset carrier — kosongkan daftar pemain yang riding
        foreach (var c in FindObjectsOfType<HazardPlayerCarrier>())
            c.ResetHazard();
    }
}