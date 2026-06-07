using UnityEngine;
using UnityEngine.InputSystem; // Wajib untuk PlayerInput dan Mouse input

public class PlayerController : MonoBehaviour
{
    public PlayerData dataPemain;
    public float currentHP;

    private PlayerInput playerInput;
    private Vector2 moveInput;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        if (dataPemain != null) currentHP = dataPemain.maxHP;
    }

    void Update()
    {
        if (playerInput == null || dataPemain == null) return;

        // 1. Pergerakan Player
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        transform.Translate(new Vector3(moveInput.x, moveInput.y, 0) * dataPemain.moveSpeed * Time.deltaTime);

        // 2. Deteksi Input Tembak
        if (playerInput.actions["Attack"].WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Debug.Log("Player is shooting!");

        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not assigned!");
            return;
        }

        // Tentukan posisi spawn peluru
        Vector3 spawnPos = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;

        // Ambil posisi mouse menggunakan Unity Input System yang baru
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Konversi posisi mouse ke World Space (2D)
        float cameraZOffset = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, cameraZOffset));
        mouseWorldPos.z = 0; // Pastikan koordinat Z tetap 0 untuk game 2D

        // Hitung arah dari player/spawnPos ke arah mouse
        Vector3 shootDirection = (mouseWorldPos - spawnPos).normalized;

        Debug.Log($"Spawn Pos: {spawnPos}, Mouse World Pos: {mouseWorldPos}, Direction: {shootDirection}");

        // Spawn peluru (Instantiate)
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Catatan: Jika nanti ingin pakai Object Pooling, tinggal aktifkan baris di bawah ini dan hapus Instantiate di atas:
        // GameObject bulletObj = PooledObjects.Instance.GetPooledObject();

        if (bulletObj != null)
        {
            bulletObj.transform.position = spawnPos;
            bulletObj.transform.rotation = Quaternion.identity;
            bulletObj.SetActive(true);

            // Ambil komponen skrip 'Bullet' yang ada di objek peluru
            Bullet bullet = bulletObj.GetComponent<Bullet>();

            if (bullet != null)
            {
                // Kirim data arah tembakan ke skrip peluru
                bullet.SetDirection(shootDirection);
                Debug.Log($"Bullet direction set to: {shootDirection}");
            }
            else
            {
                Debug.LogError("Bullet component not found on prefab! Pastikan prefab peluru memiliki skrip bernama 'Bullet'.");
            }
            Debug.Log("Bullet spawned!");
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            TakeDamage(0.5f);
        }
    }

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            GameManager.Instance.GameOver();
        }
    }
}