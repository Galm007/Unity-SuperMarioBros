using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CoinEffect : MonoBehaviour
{
    public float jumpForce = 10.0f;

    Rigidbody2D rb;

    float startY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.velocity = new Vector2(0.0f, jumpForce);
        startY = transform.position.y;
    }

    void Update()
    {
        if (transform.position.y < startY && rb.velocity.y < 0.0f)
        {
            Destroy(gameObject);
        }
    }
}
