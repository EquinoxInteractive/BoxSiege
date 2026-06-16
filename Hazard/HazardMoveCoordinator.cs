// HazardMoveCoordinator.cs
// Komponen pendukung untuk HazardHorizontalMover & HazardVerticalMover.
//
// FUNGSI:
//  Jika sebuah GameObject memiliki KEDUA komponen (HazardHorizontalMover DAN
//  HazardVerticalMover), komponen ini akan otomatis ditambahkan (via
//  RequireComponent tidak dipakai agar tetap opsional) dan bertugas
//  mengatur giliran: mover dengan `order` lebih kecil akan berjalan dulu
//  (1 = pertama). Setelah mover tersebut menyelesaikan SATU SIKLUS PENUH
//  (gerak keluar + balik ke posisi awal + delay), giliran berpindah ke
//  mover berikutnya.
//
//  Jika sebuah GameObject HANYA memiliki salah satu mover saja (horizontal
//  ATAU vertical saja), coordinator tidak diperlukan — mover akan berjalan
//  terus-menerus secara independen (lihat pengecekan di masing-masing mover).
//
// CARA PAKAI:
//  Tidak perlu di-assign manual. Cukup tambahkan HazardHorizontalMover dan/atau
//  HazardVerticalMover ke GameObject yang sama; coordinator akan otomatis
//  ter-pasang & terhubung saat Awake.

using UnityEngine;

public class HazardMoveCoordinator : MonoBehaviour
{
    // Urutan mover yang sedang aktif (1 atau 2). Mover dengan `order` yang
    // sama dengan nilai ini boleh bergerak; mover lain menunggu.
    public int ActiveOrder { get; private set; } = 1;

    private int totalMovers = 0;

    // Dipanggil oleh mover saat Awake untuk mendaftarkan diri.
    public void RegisterMover()
    {
        totalMovers++;
    }

    // Apakah mover dengan `order` tertentu boleh bergerak sekarang?
    public bool CanMove(int order)
    {
        // Jika hanya ada 1 mover terdaftar, selalu boleh bergerak
        // (tidak perlu bergiliran).
        if (totalMovers <= 1) return true;

        return ActiveOrder == order;
    }

    // Dipanggil oleh mover yang aktif setelah menyelesaikan satu siklus
    // penuh (keluar + kembali + delay). Memindahkan giliran ke mover lain.
    public void AdvanceTurn(int finishedOrder)
    {
        if (totalMovers <= 1) return;

        // Pindah ke order berikutnya. Hanya mendukung order 1 dan 2 —
        // jika finishedOrder == 1 -> giliran ke 2, dan sebaliknya.
        ActiveOrder = (finishedOrder == 1) ? 2 : 1;
    }
}