using UnityEngine;

public enum CollectableItemType
{
    Mushroom,
    FireFlower
}

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class CollectableItem : MonoBehaviour
{
    public CollectableItemType itemType = CollectableItemType.Mushroom;
    public LayerMask groundLayer;
    public float speed = 5.0f;
    public float showUpSpeed = 1.0f;
    public float showUpHeight = 1.0f;
    public AudioClip spawnAudio;
    public AudioClip oneUpAudio;

    CapsuleCollider2D col;
    Rigidbody2D rb;
    AudioSource sfx;

    bool flippedDir = false;
    bool showingUp = true;
    float showingUpTargetY;

    void Start()
    {
        col = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sfx = GetComponent<AudioSource>();

        showingUpTargetY = transform.position.y + showUpHeight;
        rb.gravityScale = 0.0f;
        sfx.PlayOneShot(spawnAudio);
    }

    void Update()
    {
        if (showingUp)
        {
            if (transform.position.y < showingUpTargetY)
            {
                transform.position += showUpSpeed * Time.deltaTime * Vector3.up;
            }
            else
            {
                rb.gravityScale = 3.0f;
                gameObject.layer = LayerMask.NameToLayer("Default");
                showingUp = false;
            }
        }
        else
        {
            if (IsTouchingWall(rb.velocity.x > 0.0f))
            {
                flippedDir = rb.velocity.x > 0.0f;
            }
            rb.velocity = new Vector2(flippedDir ? -speed : speed, rb.velocity.y);   
        }
    }

    bool IsTouchingWall(bool right)
    {
        const float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.CapsuleCast(
            col.bounds.center,
            col.bounds.size,
            CapsuleDirection2D.Vertical,
            0.0f,
            right ? Vector2.right : Vector2.left,
            checkDistance,
            groundLayer
        );
        return hit.collider != null; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switch (itemType)
            {
                case CollectableItemType.Mushroom:
                    PlayerStats.SetSuperMarioPowerup(true);
                    break;
                case CollectableItemType.FireFlower:
                    if (PlayerStats.FirePowerup)
                    {
                        collision.gameObject.GetComponent<AudioSource>().PlayOneShot(oneUpAudio);
                        PlayerStats.score += 1000;
                    }
                    else
                    {
                        PlayerStats.SetFirePowerup();
                    }
                    break;
            }
            Destroy(gameObject);
        }
    }
}
