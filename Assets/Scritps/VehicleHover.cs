using UnityEngine;

public class VehicleHover : MonoBehaviour
{
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

        transform.localPosition =
            startPosition + Vector3.up * hover;
    }
}
