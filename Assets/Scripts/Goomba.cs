using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Goomba : MonoBehaviour
{
    [Header("Movement")]
    public LayerMask groundLayer;
    public float speed = 5.0f;
    public float deathJumpForce = 5.0f;
    public float animationTimer = 0.5f;
    public bool flipDir = true;

    [Header("Death")]
    public float fallYThreshold = -50.0f;
    public float squishYOffset = 0.2f;
    public float deathTimer = 1.0f;
    public int pointsAwarded = 100;

    [Header("Audio Clips")]
    public AudioClip stompDeathAudio;
    public AudioClip kickDeathAudio;

    SpriteRenderer sprite;
    BoxCollider2D col;
    Rigidbody2D rb;
    Animator anim;
    AudioSource sfx;

    float flipTimer = 0.0f;
    bool alive = true;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
    }

    void Update()
    {
        flipTimer -= Time.deltaTime;
        while (flipTimer <= 0.0f)
        {
            flipTimer += animationTimer;
            sprite.flipX = !sprite.flipX;
        }

        if (alive)
        {
            if (IsTouchingWall(rb.velocity.x > 0.0f))
            {
                flipDir = rb.velocity.x > 0.0f;
            }
            rb.velocity = new Vector2(flipDir ? -speed : speed, rb.velocity.y);

            if (transform.position.y < fallYThreshold)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    bool IsTouchingWall(bool right)
    {
        const float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(
            col.bounds.center,
            col.bounds.size - 0.5f * col.bounds.size.y * Vector3.up,
            0.0f,
            right ? Vector2.right : Vector2.left,
            checkDistance,
            groundLayer
        );
        return hit.collider != null; 
    }

    void SquishDeath()
    {
        alive = false;
        anim.SetTrigger("Death");
        sfx.PlayOneShot(stompDeathAudio);
        col.enabled = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0.0f;
        transform.position += Vector3.down * squishYOffset;
        PlayerStats.score += pointsAwarded;
    }

    void FlipDeath()
    {
        alive = false;
        sprite.flipY = true;
        col.enabled = false;
        rb.velocity = new Vector2(0.0f, deathJumpForce);
        sfx.PlayOneShot(kickDeathAudio);
        PlayerStats.score += pointsAwarded;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Collider2D playerCol = collision.gameObject.GetComponent<Collider2D>();
            if (playerCol.bounds.min.y > col.bounds.max.y - 0.1f)
            {
                SquishDeath();
            }
        }
        else if (collision.gameObject.CompareTag("FireBall"))
        {
            FlipDeath();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("InteractableBlock")
            && collision.gameObject.GetComponent<InteractableBlock>().IsMoving)
        {
            FlipDeath();
        }
    }
}
