using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Player Data")]
    public PlayerData dataPemain;
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float moveSpeed = 5f;

    [Header("Shooting & Combat")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SimpleSpriteAnimator animator;
    private Vector2 moveInput;
    private bool isDead = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<SimpleSpriteAnimator>();
        playerInput = GetComponent<PlayerInput>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Start()
    {
        if (dataPemain != null)
        {
            if (dataPemain.maxHP > 0) maxHP = dataPemain.maxHP;
            if (dataPemain.moveSpeed > 0) moveSpeed = dataPemain.moveSpeed;
        }

        currentHP = maxHP;

        // Notifikasi ke HealthBar awal permainan
        if (HealthBarUI.Instance != null)
        {
            HealthBarUI.Instance.UpdateHealth(currentHP, maxHP);
        }
    }

    void Update()
    {
        if (isDead) return;

        // 1. Baca input gerak (4 arah)
        ReadMovementInput();

        // 2. Kontrol Animasi & Facing
        HandleAnimation();

        // 3. Baca input tembak / serang
        ReadAttackInput();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Gerakkan player menggunakan Rigidbody2D agar collision responsif dan halus
        if (rb != null)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            transform.Translate(moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void ReadMovementInput()
    {
        float h = 0f;
        float v = 0f;

        // Coba baca dari Input System jika tersedia
        if (playerInput != null && playerInput.actions != null)
        {
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                Vector2 inputVal = moveAction.ReadValue<Vector2>();
                h = inputVal.x;
                v = inputVal.y;
            }
        }

        // Fallback jika playerInput tidak aktif
        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        moveInput = new Vector2(h, v);
    }

    private void HandleAnimation()
    {
        if (animator == null) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            animator.SetFacing(moveInput.x);
            animator.PlayWalk();
        }
        else
        {
            animator.PlayIdle();
        }
    }

    private void ReadAttackInput()
    {
        bool attackPressed = false;

        if (playerInput != null && playerInput.actions != null)
        {
            var attackAction = playerInput.actions["Attack"];
            if (attackAction != null && attackAction.WasPressedThisFrame())
            {
                attackPressed = true;
            }
        }

        if (!attackPressed && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            attackPressed = true;
        }

        if (attackPressed)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (animator != null)
        {
            animator.PlayAttack();
        }

        if (bulletPrefab == null) return;

        Vector3 spawnPos = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;

        // Dapatkan arah menuju kursor mouse
        Vector3 mouseScreenPos = Input.mousePosition;
        if (Mouse.current != null)
        {
            mouseScreenPos = Mouse.current.position.ReadValue();
        }

        float cameraDistance = Camera.main != null ? Mathf.Abs(Camera.main.transform.position.z) : 10f;
        Vector3 mouseWorldPos = Camera.main != null ? 
            Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, cameraDistance)) : 
            transform.position + Vector3.right;
        mouseWorldPos.z = 0;

        Vector3 shootDir = (mouseWorldPos - spawnPos).normalized;
        if (shootDir.sqrMagnitude < 0.01f)
        {
            shootDir = (spriteRenderer != null && spriteRenderer.flipX) ? Vector3.left : Vector3.right;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        if (bulletObj != null)
        {
            bulletObj.SetActive(true);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetDirection(shootDir);
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[Player] Menerima damage: {dmg}. Sisa HP: {currentHP}/{maxHP}");

        if (HealthBarUI.Instance != null)
        {
            HealthBarUI.Instance.UpdateHealth(currentHP, maxHP);
        }

        StartCoroutine(FlashColor(Color.red));

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;

        Debug.Log($"[Player] Sembuh: +{amount} HP. HP sekarang: {currentHP}/{maxHP}");

        if (HealthBarUI.Instance != null)
        {
            HealthBarUI.Instance.UpdateHealth(currentHP, maxHP);
        }

        StartCoroutine(FlashColor(Color.green));
    }

    private IEnumerator FlashColor(Color color)
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = color;
            yield return new WaitForSeconds(0.12f);
            if (spriteRenderer != null) spriteRenderer.color = original;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.PlayDeath();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            TakeDamage(0.2f);
        }
    }
}
