using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public float fallSpeed = 0.5f; // Adjustable falling speed

    private float gravityEffect; // Custom gravity effect multiplier

    void Start()
    {
        // Initialize gravityEffect based on fallSpeed for fine control
        gravityEffect = fallSpeed;
    }

    void Update()
    {
        // Move the object downwards with slow speed
        transform.Translate(Vector3.down * gravityEffect * Time.deltaTime);
        
        // Optionally, you can apply a custom gravity effect (more control over speed)
        // gravityEffect = Mathf.Lerp(gravityEffect, fallSpeed, Time.deltaTime); 
    }
}
