using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 30f;
    public float currentHP;
    public float moveSpeed = 2.2f;
    public float detectionRange = 6.5f;
    public float attackRange = 0.9f;
    public float attackDamage = 15f;

    [Header("Drops")]
    public GameObject[] dropItemPrefabs;
    [Range(0f, 1f)] public float dropChance = 0.5f;

    private Transform targetPlayer;
    private SimpleSpriteAnimator animator;
    private SpriteRenderer spriteRenderer;
    private bool isAttacking = false;
    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<SimpleSpriteAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        FindPlayer();
    }

    void Update()
    {
        if (isDead) return;

        if (targetPlayer == null)
        {
            FindPlayer();
            if (targetPlayer == null)
            {
                if (animator != null) animator.PlayIdle();
                return;
            }
        }

        if (isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        // Jika player dalam jangkauan deteksi, musuh menarget dan mendekati player
        if (distanceToPlayer <= detectionRange)
        {
            // Cek apakah sudah cukup dekat untuk menyerang
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                // Bergerak menuju player
                Vector2 direction = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);

                if (animator != null)
                {
                    animator.SetFacing(direction.x);
                    animator.PlayWalk();
                }
            }
        }
        else
        {
            if (animator != null)
            {
                animator.PlayIdle();
            }
        }
    }

    private void FindPlayer()
    {
        if (PlayerController.Instance != null)
        {
            targetPlayer = PlayerController.Instance.transform;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Hadapkan musuh ke arah player saat menyerang
        if (targetPlayer != null && animator != null)
        {
            float dirX = targetPlayer.position.x - transform.position.x;
            animator.SetFacing(dirX);
            animator.PlayAttack();
        }

        // Delay singkat agar animasi serang terlihat
        yield return new WaitForSeconds(0.35f);

        // Berikan damage ke player jika player masih ada
        if (targetPlayer != null)
        {
            PlayerController player = targetPlayer.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
                Debug.Log($"[Enemy] Musuh menyerang player! Memberikan {attackDamage} damage.");
            }
        }

        // Ketentuan Soal PAS: "Ketika sudah menyerang musuh akan hancur"
        yield return new WaitForSeconds(0.2f);
        Die(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        StartCoroutine(FlashRed());

        if (currentHP <= 0)
        {
            Die(true);
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = original;
        }
    }

    private void Die(bool canDropItem)
    {
        if (isDead) return;
        isDead = true;

        if (canDropItem && dropItemPrefabs != null && dropItemPrefabs.Length > 0)
        {
            if (Random.value <= dropChance)
            {
                int randomIndex = Random.Range(0, dropItemPrefabs.Length);
                if (dropItemPrefabs[randomIndex] != null)
                {
                    Instantiate(dropItemPrefabs[randomIndex], transform.position, Quaternion.identity);
                }
            }
        }

        if (animator != null)
        {
            animator.PlayDeath(() =>
            {
                Destroy(gameObject);
            });
            // Fallback safety destroy
            Destroy(gameObject, 0.6f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet") || collision.GetComponent<Bullet>() != null)
        {
            TakeDamage(20f);
        }
    }
}
