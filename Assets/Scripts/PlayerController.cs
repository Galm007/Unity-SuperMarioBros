using System;
using UnityEngine;
using UnityEngine.SceneManagement;

enum State
{
    Idle,
    Running,
    Jumping,
    Falling,
    GrowingShrinking,
    FirePowerupTransition,
    Dying,
    FlagpoleSequence
}

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForce = 23.0f;
    public float jumpCancelMultiplier = 0.6f;
    public float acceleration = 50.0f;
    public float deceleration = 50.0f;
    public float topSpeed = 10.0f;
    public float killJumpForce = 10.0f;
    public float flagpoleWalkSpeed = 2.0f;

    [Header("Collision")]
    public LayerMask groundLayer;
    public float damageInvincibleWindow = 0.5f;
    public float deathJumpDelay = 0.5f;
    public float deathJumpForce = 20.0f;
    public float fallDeathThreshold = -200.0f;
    public float dieYPos = -5.0f;
    public Rect superMarioColliderSize = new(0.0f, -0.007f, 0.12f, 0.3f);

    [Header("Audio Clips")]
    public AudioClip jumpAudio;
    public AudioClip fireballAudio;
    public AudioClip deathAudio;
    public AudioClip damageAudio;
    public AudioClip powerupAudio;
    public AudioClip flagpoleAudio;
    public AudioClip winAudio;

    [Header("Misc")]
    public GameObject fireballPrefab;
    public GameUI gameUI;
    public Flagpole flagpole;

    public event Action OnLevelComplete;

    Rigidbody2D rb; 
    BoxCollider2D col;
    SpriteRenderer sprite;
    Animator anim;
    AudioSource sfx;

    float inputX = 0.0f;
    bool inputJump = false;
    bool inputJumpReleased = false;
    bool inputFireball = false;

    State state = State.Idle;
    Rect smallColliderSize;
    bool grounded = true;
    bool deathJumped = false;
    bool freezeInput = true;
    float invincibilityTimer = 0.0f;
    int flagpoleSequenceNumber = 0;
    float flagpoleSequenceTimer = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();

        PlayerStats.SoftReset();
        smallColliderSize = new(col.offset, col.size);
        gameUI.OnLevelStart += () => { freezeInput = false; };
        flagpole.OnFlagpoleTouched += TouchFlagpole;
    }

    void OnEnable()
    {
        PlayerStats.OnSuperMarioSet += OnSuperMarioSet;
        PlayerStats.OnFireSet += OnFireSet;
    }

    void OnDisable()
    {
        PlayerStats.OnSuperMarioSet -= OnSuperMarioSet;
        PlayerStats.OnFireSet -= OnFireSet;
    }

    void UpdateInputs()
    {
        if (freezeInput)
        {
            inputX = 0.0f;
            inputJump = false;
            inputJumpReleased = false;
            inputFireball = false;
        }
        else
        {
            inputX = Input.GetAxis("Horizontal");
            inputJump = Input.GetKeyDown(KeyCode.UpArrow);
            inputJumpReleased = Input.GetKeyUp(KeyCode.UpArrow);
            inputFireball = Input.GetKeyDown(KeyCode.X);
        }
    }

    void Update()
    {
        UpdateInputs();
        grounded = IsTouchingFloor();

        switch (state)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Running:
                UpdateRunning();
                break;
            case State.Jumping:
                UpdateJumping();
                break;
            case State.Falling:
                UpdateFalling();
                break;
            case State.GrowingShrinking:
                UpdateGrowingShrinking();
                break;
            case State.FirePowerupTransition:
                UpdateFirePowerupTransition();
                break;
            case State.Dying:
                UpdateDying();
                break;
            case State.FlagpoleSequence:
                UpdateFlagpoleSequence();
                break;
        }

        if (PlayerStats.FirePowerup && inputFireball)
        {
            FireBall fireball = Instantiate(
                fireballPrefab,
                transform.position,
                Quaternion.identity
            ).GetComponent<FireBall>();

            if (sprite.flipX)
            {
                fireball.speed *= -1.0f;
            }
            sfx.PlayOneShot(fireballAudio);
        }

        if (inputX != 0.0f)
        {
            sprite.flipX = inputX < 0.0f;
        }
        if (invincibilityTimer > 0.0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        if (!freezeInput)
        {
            PlayerStats.timeRemaining = Mathf.Max(PlayerStats.timeRemaining - Time.deltaTime, 0.0f);
        }
    }

    void UpdateIdle()
    {
        if (!grounded)
        {
            state = State.Falling;
        }
        if (inputJump)
        {
            Jump(jumpForce);
        }
        if (inputX != 0.0f)
        {
            state = State.Running;
        }

        anim.SetTrigger("Idle");
    }

    void UpdateRunning()
    {
        if (!grounded)
        {
            state = State.Falling;
        }
        if (inputJump)
        {
            Jump(jumpForce);
        }
        if (rb.velocity.x == 0.0f)
        {
            state = State.Idle;
        }

        if ((inputX < 0.0f && rb.velocity.x > 0.0f) || (inputX > 0.0f && rb.velocity.x < 0.0f))
        {
            anim.SetTrigger("Skidding");
        }
        else
        {
            anim.SetTrigger("Running");
        }

        MoveHorizontally();
    }

    void UpdateJumping()
    {
        if (rb.velocity.y < 0.0f)
        {
            state = grounded ? State.Idle : State.Falling;
        }
        if (inputJumpReleased)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCancelMultiplier);
        }

        anim.SetTrigger("Jumping");
        MoveHorizontally();
    }

    void UpdateFalling()
    {
        if (grounded)
        {
            state = inputX == 0.0f ? State.Idle : State.Running;
            anim.speed = 1.0f;
        }
        else
        {
            anim.speed = 0.0f;
        }

        if (transform.position.y < dieYPos)
        {
            Die();
        }

        MoveHorizontally();
    }

    void UpdateGrowingShrinking()
    {
        rb.velocity = Vector2.zero;
        anim.SetTrigger("GrowingShrinking");
    }

    void UpdateFirePowerupTransition()
    {
        rb.velocity = Vector2.zero;
        anim.SetTrigger("FireTransitioning");
    }

    void UpdateDying()
    {
        if (deathJumpDelay <= 0.0f)
        {
            if (!deathJumped)
            {
                rb.gravityScale = 5.0f;
                rb.velocity = new Vector2(rb.velocity.x, deathJumpForce);
                deathJumped = true;
            }
        }
        else
        {
            deathJumpDelay -= Time.deltaTime;
            rb.velocity = Vector2.zero;
        }

        if (deathJumped && transform.position.y < fallDeathThreshold)
        {
            PlayerStats.lives--;
            gameObject.SetActive(false);

            if (PlayerStats.lives == 0)
            {
                PlayerStats.lives = 3;
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        anim.SetTrigger("Death");
    }

    void UpdateFlagpoleSequence()
    {
        switch (flagpoleSequenceNumber)
        {
            case 0:
                anim.SetTrigger("Climbing");
                if (IsTouchingFloor())
                {
                    flagpoleSequenceNumber++;
                    flagpoleSequenceTimer = 0.5f;

                    rb.gravityScale = 5.0f;
                    transform.position += col.size.x * transform.localScale.x * Vector3.right;
                    sprite.flipX = true;
                    anim.speed = 0.0f;
                }
                break;
            case 1:
                if ((flagpoleSequenceTimer -= Time.deltaTime) < 0.0f)
                {
                    flagpoleSequenceNumber++;
                    flagpoleSequenceTimer = 6.5f;

                    sprite.flipX = false;
                    anim.speed = 0.5f;
                    sfx.PlayOneShot(winAudio);
                }
                break;
            case 2:
                rb.velocity = new Vector2(flagpoleWalkSpeed, rb.velocity.y);
                anim.SetTrigger("Running");
                if ((flagpoleSequenceTimer -= Time.deltaTime) < 0.0f)
                {
                    OnLevelComplete?.Invoke();
                }
                break;
            default:
                break;
        }
    }

    void Jump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, force);
        state = State.Jumping;
        sfx.PlayOneShot(jumpAudio);
    }

    void MoveHorizontally()
    {
        if (inputX == 0.0f)
        {
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, 0.0f, deceleration * Time.deltaTime),
                rb.velocity.y
            );
        }
        else
        {
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, inputX * topSpeed, acceleration * Time.deltaTime),
                rb.velocity.y
            );
        }
    }

    bool IsTouchingFloor()
    {
        const float groundCheckDistance = 0.2f;
        Vector3 from = col.bounds.center + Vector3.down * col.size.y;
        RaycastHit2D hit = Physics2D.BoxCast(
            from,
            col.bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        return hit.collider != null; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Collider2D otherCol = collision.gameObject.GetComponent<Collider2D>();
            if (col.bounds.min.y > otherCol.bounds.max.y - 0.1f)
            {
                rb.velocity = new Vector2(rb.velocity.x, killJumpForce);
                state = State.Jumping;
                rb.gravityScale = 5.0f;
            }
            else if (invincibilityTimer <= 0.0f)
            {
                invincibilityTimer = damageInvincibleWindow;
                if (PlayerStats.SuperMarioPowerup)
                {
                    PlayerStats.SetSuperMarioPowerup(false);
                }
                else
                {
                    Die();
                }
            }
        }
        else if (collision.gameObject.CompareTag("Castle"))
        {
            sprite.enabled = false;
        }
    }

    void Die()
    {
        state = State.Dying;
        col.enabled = false;
        rb.gravityScale = 0.0f;
        anim.speed = 1.0f;
        sfx.PlayOneShot(deathAudio);
    }

    public void TouchFlagpole(float dropSpeed)
    {
        state = State.FlagpoleSequence;
        freezeInput = true;
        rb.gravityScale = 0.0f;
        rb.velocity = Vector2.down * dropSpeed;
        anim.speed = 1.0f;
        sfx.PlayOneShot(flagpoleAudio);
    }

    void OnSuperMarioSet(bool superMario)
    {
        state = State.GrowingShrinking;
        rb.gravityScale = 0.0f;
        anim.speed = 1.0f;

        float heightOffset = superMarioColliderSize.height - smallColliderSize.height;
        if (superMario)
        {
            transform.position += Vector3.up * heightOffset;
            col.offset = superMarioColliderSize.position;
            col.size = superMarioColliderSize.size;
            sfx.PlayOneShot(powerupAudio);
        }
        else
        {
            transform.position += Vector3.down * heightOffset;
            col.offset = smallColliderSize.position;
            col.size = smallColliderSize.size;

            anim.SetBool("Fire", false);
            sfx.PlayOneShot(damageAudio);
        }
    }

    void OnGrowingShrinkingAnimationEnd()
    {
        state = State.Idle;
        rb.gravityScale = 5.0f;
        anim.SetBool("SuperMario", PlayerStats.SuperMarioPowerup);
    }

    void OnFireSet()
    {
        state = State.FirePowerupTransition;
        rb.gravityScale = 0.0f;
        anim.speed = 1.0f;
        sfx.PlayOneShot(powerupAudio);
    }

    void OnFireTransitionAnimationEnd()
    {
        state = State.Idle;
        rb.gravityScale = 5.0f;
        anim.SetBool("Fire", PlayerStats.FirePowerup);
    }
}
