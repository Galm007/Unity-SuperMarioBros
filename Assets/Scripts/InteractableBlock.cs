using UnityEngine;

public enum InteractableBlockType
{
    Brick,
    Mystery,
    BrickMystery,
    Hidden
}

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class InteractableBlock : MonoBehaviour
{
    public InteractableBlockType blockType;
    public GameObject collectableItem;
    public GameObject alternativeItem;
    public int coins = 1;
    public Sprite exhaustedSprite;
    public float itemSpawnOffset = 1.0f;
    public GameObject brickParticles;

    [Header("Movemenet")]
    public float jumpHeight = 0.35f;
    public float jumpSpeed = 3.0f;
    public bool IsMoving { get; private set; }

    [Header("Audio Clips")]
    public AudioClip bumpAudio;
    public AudioClip coinAudio;
    public AudioClip brickAudio;

    BoxCollider2D col;
    SpriteRenderer spriteRenderer;
    AudioSource sfx;

    float restY;
    float targetY;
    bool spawning = false;
    bool interactable = true;
    bool useAlternative = false;
    float disableColliderDelay = 0.0f;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();

        restY = transform.position.y;
        targetY = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(
            transform.position.x,
            Mathf.MoveTowards(transform.position.y, targetY, jumpSpeed * Time.deltaTime),
            transform.position.y
        );

        if (transform.position.y == targetY)
        {
            if (targetY == restY)
            {
                if (spawning)
                {
                    SpawnItem();
                    spriteRenderer.sprite = exhaustedSprite;
                    spawning = false;
                }
                IsMoving = false;
            }

            targetY = restY;
        }

        if (disableColliderDelay > 0.0f)
        {
            disableColliderDelay -= Time.deltaTime;
            if (disableColliderDelay <= 0.0f)
            {
                col.enabled = false;
            }
        }
    }

    void SpawnItem()
    {
        Instantiate(
            useAlternative ? alternativeItem : collectableItem,
            transform.position + Vector3.up * itemSpawnOffset,
            Quaternion.identity
        );
    }

    void PickupCoin()
    {
        SpawnItem();
        sfx.PlayOneShot(coinAudio);
        coins--;
        if (coins <= 0)
        {
            spriteRenderer.sprite = exhaustedSprite;
            interactable = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (interactable && collision.gameObject.CompareTag("Player"))
        {
            Collider2D playerCol = collision.gameObject.GetComponent<Collider2D>();

            if (playerCol.bounds.max.y < col.bounds.min.y + 0.2f)
            {
                targetY = restY + jumpHeight;
                IsMoving = true;

                if (blockType != InteractableBlockType.Brick)
                {
                    if (coins > 0)
                    {
                        PickupCoin();
                        PlayerStats.coins++;
                    }
                    else
                    {
                        spawning = true;
                        interactable = false;
                        useAlternative = PlayerStats.SuperMarioPowerup;
                    }
                }
                else if (PlayerStats.SuperMarioPowerup)
                {
                    Instantiate(brickParticles, transform.position, Quaternion.identity);
                    spriteRenderer.enabled = false;
                    disableColliderDelay = 0.1f;
                    sfx.PlayOneShot(brickAudio);
                }
                else
                {
                    sfx.PlayOneShot(bumpAudio);
                }
            }
        }
    }
}
