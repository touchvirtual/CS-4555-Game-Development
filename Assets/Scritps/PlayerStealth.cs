using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStealth : MonoBehaviour
{
    public float stealthDuration = 5f;
    [Min(0f)] public float cooldown = 8f;
    public bool HasStealth { get; private set; }
    public bool IsStealthed { get; private set; }
    private float stealthEndTime;
    private float nextReadyTime;
    private PlayerHealth health;

    public float RemainingDuration => IsStealthed ? Mathf.Max(0f, stealthEndTime - Time.time) : 0f;
    public float RemainingNormalized => stealthDuration > 0f ? Mathf.Clamp01(RemainingDuration / stealthDuration) : 0f;
    public float CooldownRemaining => IsStealthed ? 0f : Mathf.Max(0f, nextReadyTime - Time.time);
    public float CooldownNormalized => cooldown > 0f ? Mathf.Clamp01(CooldownRemaining / cooldown) : 0f;
    public bool IsReady => HasStealth && !IsStealthed && Time.time >= nextReadyTime;

    private void Awake() { health = GetComponent<PlayerHealth>(); }
    public void UnlockStealth() { HasStealth = true; }
    public void RestoreUnlock(bool unlocked) { HasStealth = unlocked; ResetAbilityState(); }
    public void ResetAbilityState()
    {
        IsStealthed = false;
        stealthEndTime = nextReadyTime = 0f;
    }
    private void Update()
    {
        if (Time.timeScale <= 0f || (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        if (IsStealthed && Time.time >= stealthEndTime) IsStealthed = false;
        if (health != null && health.IsDead) return;
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame) ActivateStealth();
    }
    public void ActivateStealth()
    {
        if (!IsReady || (health != null && health.IsDead)) return;
        if (Time.timeScale <= 0f || (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        IsStealthed = true;
        stealthEndTime = Time.time + Mathf.Max(0.1f, stealthDuration);
        nextReadyTime = stealthEndTime + Mathf.Max(0f, cooldown);
    }
}
