using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleSpawnConfig", menuName = "RockSkipping / Obstacle Spawn Config")]
public class ObstacleSpawnConfig : ScriptableObject
{
    [Serializable]
    public class ObstacleEntry
    {
        public GameObject prefab;
        [Tooltip("Makin tinggi bobot / weightnya, entrinya nanti makin besar")]
        public float weight = 1f;
    }

    [Header("Obstacle Pool")]
    public ObstacleEntry[] obstacles;

    [Header("Spawn Probability")]
    [Range(0f, 1f)]
    [Tooltip("Sisain beberapa titik kosong sebagai buat ruang ngiungiung")]
    public float spawnChancePerPoint = 0.6f;

    public GameObject GetRandomObstacle()
    {
        if (obstacles == null || obstacles.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var entry in obstacles) totalWeight += entry.weight;

        if (totalWeight <= 0f) return obstacles[0].prefab;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in obstacles)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.prefab;
        }

        return obstacles[obstacles.Length - 1].prefab; //Balik ke point sebelumnya
    }
}
