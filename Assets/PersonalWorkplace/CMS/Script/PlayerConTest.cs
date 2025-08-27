using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerConTest : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed; 
    }
}