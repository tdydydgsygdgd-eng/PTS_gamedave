using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3.5f;
    public int maxConcurrentEnemies = 5;
    public float spawnRadiusMin = 4.0f;
    public float spawnRadiusMax = 7.5f;

    private float timer = 0f;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Update()
    {
        // Bersihkan referensi musuh yang sudah hancur
        activeEnemies.RemoveAll(item => item == null);

        if (enemyPrefab == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (activeEnemies.Count < maxConcurrentEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector3 center = transform.position;
        if (PlayerController.Instance != null)
        {
            center = PlayerController.Instance.transform.position;
        }

        // Tentukan posisi acak di sekitar player
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
        Vector3 spawnOffset = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
        Vector3 spawnPos = center + spawnOffset;

        GameObject spawned = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(spawned);
        Debug.Log($"[EnemySpawner] Spawn musuh baru di {spawnPos}. Total aktif: {activeEnemies.Count}");
    }
}
