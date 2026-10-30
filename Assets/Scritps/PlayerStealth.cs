
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStealth : MonoBehaviour
{
    public float stealthDuration = 5f;

    public bool HasStealth { get; private set; }
    public bool IsStealthed { get; private set; }

    private float stealthEndTime;

    public void UnlockStealth()
    {
        HasStealth = true;
        Debug.Log("Stealth ability unlocked! Press C to activate.");
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        if (Keyboard.current == null)
            return;

        // Activate stealth with C.
        if (HasStealth &&
            !IsStealthed &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            ActivateStealth();
        }

        // End stealth when its duration expires.
        if (IsStealthed && Time.time >= stealthEndTime)
        {
            IsStealthed = false;
            Debug.Log("Stealth ended!");
        }
    }

    public void ActivateStealth()
    {
        if (!HasStealth || IsStealthed)
            return;

        IsStealthed = true;
        stealthEndTime = Time.time + stealthDuration;

        Debug.Log("Stealth activated for " + stealthDuration + " seconds!");
    }
}
