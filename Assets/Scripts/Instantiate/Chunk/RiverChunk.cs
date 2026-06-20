using System.Collections.Generic;
using UnityEngine;

public class RiverChunk : MonoBehaviour
{
    [Header("Dimensi Chunk")]
    [Tooltip("Panjang chunk pada axis Z. Nanti dipake di RiverGenerator script yang gw buat.")]
    public float length = 50f;

    [Header("Obstacle Spawn Point")]
    [Tooltip("Taro, empty child GameObject di dalam Prefab si sungai")]
    public List<Transform> obstacleSpawnPoints = new List<Transform>();
    private readonly List<GameObject> _activeObstacles = new List<GameObject>();

    public void ClearObstacles()
    {
        foreach(var obstacle in _activeObstacles)
        {
            if (obstacle != null)
                Destroy(obstacle);
        }
        _activeObstacles.Clear();
    }

    public void RegisterSpawnedObstacle(GameObject obstacle)
    {
        _activeObstacles.Add(obstacle);
    }

    public float StartZ => transform.position.z;
    public float EndZ => transform.position.z + length;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var point in obstacleSpawnPoints)
        {
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, 0.5f);
        }

        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.forward * length;
        Gizmos.DrawLine(start, end);
    }
#endif
}
