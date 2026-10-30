using UnityEngine;

public class PlayerStealth : MonoBehaviour
{
    public float stealthDuration = 5f;

    public bool IsStealthed { get; private set; }

    private float stealthEndTime;

    public void ActivateStealth()
    {
        IsStealthed = true;
        stealthEndTime = Time.time + stealthDuration;

        Debug.Log("Stealth activated for 5 seconds!");
    }

    private void Update()
    {
        if (IsStealthed && Time.time >= stealthEndTime)
        {
            IsStealthed = false;
            Debug.Log("Stealth has ended.");
        }
    }
}