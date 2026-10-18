using UnityEngine;

public class PickUpVisuals : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("How fast the object moves up and down.")]
    public float speed = 2.0f;
    
    [Tooltip("How far the object moves from its original position.")]
    public float height = 0.5f;

    [Header("Optional Settings")]
    [Tooltip("Set to true if you want the object to spin as it floats.")]
    public bool rotate = false;
    public float rotationSpeed = 50.0f;

    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position so it bobs relative to where it was placed
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * height;

        // Apply the new position while keeping X and Z original
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Optional: Spin the object on its Y-axis
        if (rotate)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }
}
