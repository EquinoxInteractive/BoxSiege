// PlayerMovement.cs — Modified
// Tambahan: Device guard di OnMove
// Memastikan hanya gamepad yang di-assign yang bisa menggerakkan player ini.
//
// File ini menggantikan: Script Controller/PlayerMovement.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 defaultScale;
    private PlayerShooting playerShooting;
    private Vector2 moveInput;

    // ── Device guard ──────────────────────────────────────────────────────────
    private PlayerInput m_PlayerInput;
    private string      m_AssetName;
    private InputAction m_MoveAction;

    void Start()
    {
        defaultScale   = transform.localScale;
        playerShooting = GetComponent<PlayerShooting>();

        // Cache PlayerInput dan action untuk guard
        m_PlayerInput = GetComponent<PlayerInput>();
        m_AssetName   = m_PlayerInput?.actions?.name ?? string.Empty;
        m_MoveAction  = m_PlayerInput?.actions?.FindAction("Move");
    }

    void Update()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, 0f) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);

        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(defaultScale.x), defaultScale.y, defaultScale.z);
            playerShooting.SetFacingDirection(true);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(defaultScale.x), defaultScale.y, defaultScale.z);
            playerShooting.SetFacingDirection(false);
        }
    }

    public void OnMove(InputValue value)
    {
        // ── Device guard: hanya terima input dari gamepad yang di-assign ──────
        // Cek device yang terakhir men-trigger action Move
        var activeDevice = m_MoveAction?.activeControl?.device;
        if (activeDevice is Gamepad g &&
            !ControllerAssignmentManager.IsDeviceAllowedFor(m_AssetName, g))
        {
            // Input dari controller yang salah — reset moveInput ke nol
            moveInput = Vector2.zero;
            return;
        }
        // ─────────────────────────────────────────────────────────────────────

        moveInput = value.Get<Vector2>();
    }
}