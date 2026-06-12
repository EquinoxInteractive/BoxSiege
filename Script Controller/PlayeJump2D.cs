// PlayeJump2D.cs — Modified v4
// Fix: pemain dapat loncat saat berdiri di atas pemain lain.
//      CheckGrounded sekarang melakukan 2 pengecekan:
//        1. OverlapCircle dengan groundLayer (lantai/platform seperti biasa)
//        2. OverlapCircleAll untuk mendeteksi Rigidbody2D lain di bawah kaki
//           (= badan pemain lain). Ground/platform tidak punya Rigidbody2D
//           sehingga check ini aman dan tidak butuh tag/layer tambahan.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayeJump2D : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Descent Settings")]
    public float descentForce    = 15f;
    public float descentCooldown = 0.5f;
    public float descentMaxSpeed = 20f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canJump      = true;
    private bool canDescend   = true;
    private bool isDescending = false;

    private AudioManager audioManager;

    // ── Device guard ──────────────────────────────────────────────────────────
    private PlayerInput  m_PlayerInput;
    private string       m_AssetName;
    private InputAction  m_JumpAction;
    private InputAction  m_DescendAction;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (groundCheck == null)
            Debug.LogWarning("Ground Check transform belum di-assign pada PlayeJump2D!");

        m_PlayerInput   = GetComponent<PlayerInput>();
        m_AssetName     = m_PlayerInput?.actions?.name ?? string.Empty;
        m_JumpAction    = m_PlayerInput?.actions?.FindAction("Jump");
        m_DescendAction = m_PlayerInput?.actions?.FindAction("Descend");
    }

    void Update()
    {
        if (Time.timeScale != 0f)
        {
            CheckGrounded();

            if (isGrounded)
            {
                canJump      = true;
                canDescend   = true;
                isDescending = false;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  CheckGrounded
    //  Pengecekan 1: groundLayer (platform/lantai)
    //  Pengecekan 2: badan pemain lain — dicari via Rigidbody2D
    //               (ground statis tidak punya Rigidbody2D, jadi aman)
    // ──────────────────────────────────────────────────────────────────────────
    void CheckGrounded()
    {
        if (groundCheck == null) return;

        // --- Pengecekan 1: ground/platform biasa ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- Pengecekan 2: berdiri di atas pemain lain ---
        if (!isGrounded)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
            foreach (Collider2D hit in hits)
            {
                // Abaikan trigger dan collider milik diri sendiri (termasuk child)
                if (hit.isTrigger) continue;
                if (hit.transform.IsChildOf(transform) || hit.transform == transform) continue;

                // Jika collider ini punya Rigidbody2D → ini adalah pemain lain
                if (hit.attachedRigidbody != null)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  OnJump — dengan device guard
    // ──────────────────────────────────────────────────────────────────────────
    public void OnJump(InputValue value)
    {
        if (Time.timeScale == 0f) return;

        var activeDevice = m_JumpAction?.activeControl?.device;
        if (activeDevice is Gamepad g &&
            !ControllerAssignmentManager.IsDeviceAllowedFor(m_AssetName, g))
            return;

        if (isGrounded && canJump && SafeIsPressed(value))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            if (audioManager != null)
                audioManager.PlaySFX(audioManager.jump);

            canJump = false;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  OnDescend — dengan device guard
    // ──────────────────────────────────────────────────────────────────────────
    public void OnDescend(InputValue value)
    {
        if (Time.timeScale == 0f) return;

        var activeDevice = m_DescendAction?.activeControl?.device;
        if (activeDevice is Gamepad g &&
            !ControllerAssignmentManager.IsDeviceAllowedFor(m_AssetName, g))
            return;

        if (!isGrounded && canDescend && SafeIsPressed(value) && !isDescending)
            StartCoroutine(PerformDescent());
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  SafeIsPressed
    // ──────────────────────────────────────────────────────────────────────────
    private static bool SafeIsPressed(InputValue value)
    {
        try { return value.isPressed; }
        catch (InvalidOperationException)
        {
            try { return value.Get<Vector2>().magnitude > 0.5f; }
            catch { return false; }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  PerformDescent
    // ──────────────────────────────────────────────────────────────────────────
    private IEnumerator PerformDescent()
    {
        isDescending = true;
        canDescend   = false;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.down * descentForce, ForceMode2D.Impulse);

        while (!isGrounded)
        {
            if (rb.velocity.y < -descentMaxSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -descentMaxSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(descentCooldown);

        canDescend   = true;
        isDescending = false;
    }

    public void EnableJump()    => canJump    = true;
    public void EnableDescent() => canDescend = true;
    public bool IsGrounded()    => isGrounded;
    public bool IsDescending()  => isDescending;

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}