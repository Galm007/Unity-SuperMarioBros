using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Animator))]
public class FireBall : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public float speed = 10.0f;
    public float jumpHeight = 10.0f;
    public float gravity = 10.0f;

    CircleCollider2D col;
    Animator anim;
    Vector3 velocity;
    bool startGravity = false;

    void Start()
    {
        col = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();

        velocity = new Vector3(speed, -Mathf.Abs(speed) * 0.6f); // A bit less than 45 degrees
    }

    void Update()
    {
        if (startGravity)
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        if (IsTouchingFloor())
        {
            velocity.y = jumpHeight;
            startGravity = true;
        }
        else if (IsTouchingWall(velocity.x > 0.0f))
        {
            velocity = Vector3.zero;
            col.enabled = false;
            anim.SetTrigger("Explode");
        }

        transform.position += velocity * Time.deltaTime;
    }

    bool IsTouchingFloor()
    {
        const float groundCheckDistance = 0.1f;
        RaycastHit2D hit = Physics2D.CircleCast(
            col.bounds.center,
            col.radius,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        return hit.collider != null; 
    }

    bool IsTouchingWall(bool right)
    {
        const float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.CircleCast(
            col.bounds.center,
            col.radius,
            right ? Vector2.right : Vector2.left,
            checkDistance,
            groundLayer
        );
        return hit.collider != null; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            velocity = Vector3.zero;
            col.enabled = false;
            anim.SetTrigger("Explode");
        }
    }

    void OnExplodeAnimationFinished()
    {
        Destroy(gameObject);
    }
}
