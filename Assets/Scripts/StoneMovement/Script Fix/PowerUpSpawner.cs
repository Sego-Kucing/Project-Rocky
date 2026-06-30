using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Daftar Prefab powerup yang bisa muncul")]
    public GameObject[] powerUpPrefabs;

    [Tooltip("Berapa banyak power up yang mau di munculkan di area kumpulan")]
    public int amountToSpawn = 3;

    void Start()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return;
        }

        List<Transform> availableSpawnPoints = new List<Transform>();

        foreach(Transform child in transform)
        {
            availableSpawnPoints.Add(child);
        }

        int actualSpawnCount = Mathf.Min(amountToSpawn, availableSpawnPoints.Count);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            int randomPointIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform selectedPoint = availableSpawnPoints[randomPointIndex];

            int randomPrefabIndex = Random.Range(0, powerUpPrefabs.Length);
            GameObject selectedPrefab = powerUpPrefabs[randomPrefabIndex];

            Instantiate(selectedPrefab, selectedPoint.position, selectedPoint.rotation);
            availableSpawnPoints.RemoveAt(randomPointIndex);
        }
    }
}
