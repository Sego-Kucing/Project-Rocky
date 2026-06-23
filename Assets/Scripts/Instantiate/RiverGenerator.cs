using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform whose Z position drives chunk spawning - usually the stone/player.")]
    public Transform tracker;

    [Tooltip("Possible chunk prefabs. A random one is picked each time a new chunk is needed.")]
    public RiverChunk[] chunkPrefabs;

    public ObstacleSpawnConfig obstacleConfig;

    [Header("Generation Settings")]
    [Tooltip("How many chunks stay active ahead of the player at once.")]
    public int activeChunkCount = 4;

    [Tooltip("Extra distance behind the player before a passed chunk gets recycled to the front.")]
    public float recycleBuffer = 10f;

    private readonly Dictionary<RiverChunk, Queue<RiverChunk>> _pools = new Dictionary<RiverChunk, Queue<RiverChunk>>();
    private readonly Dictionary<RiverChunk, RiverChunk> _instanceSource = new Dictionary<RiverChunk, RiverChunk>();
    private readonly Queue<RiverChunk> _activeChunks = new Queue<RiverChunk>();

    private float _nextSpawnZ;

    private void Start()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("RiverGenerator: no chunk prefabs assigned.");
            return;
        }

        foreach (var prefab in chunkPrefabs)
            _pools[prefab] = new Queue<RiverChunk>();

        _nextSpawnZ = tracker != null ? tracker.position.z : 0f;

        for (int i = 0; i < activeChunkCount; i++)
            SpawnNextChunk();
    }

    private void Update()
    {
        if (tracker == null || _activeChunks.Count == 0) return;

        RiverChunk firstChunk = _activeChunks.Peek();
        if (tracker.position.z > firstChunk.EndZ + recycleBuffer)
        {
            RecycleChunk(_activeChunks.Dequeue());
            SpawnNextChunk();
        }
    }

    private void SpawnNextChunk()
    {
        RiverChunk prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        RiverChunk chunk = GetFromPool(prefab);

        chunk.Activate(new Vector3(0f, 0f, _nextSpawnZ));
        SpawnObstaclesOn(chunk);

        _nextSpawnZ += chunk.length;
        _activeChunks.Enqueue(chunk);
    }

    private void RecycleChunk(RiverChunk chunk)
    {
        chunk.ClearObstacles();
        chunk.gameObject.SetActive(false);

        RiverChunk sourcePrefab = _instanceSource.TryGetValue(chunk, out var p) ? p : chunkPrefabs[0];
        _pools[sourcePrefab].Enqueue(chunk);
    }

    private RiverChunk GetFromPool(RiverChunk prefab)
    {
        Queue<RiverChunk> pool = _pools[prefab];
        if (pool.Count > 0)
            return pool.Dequeue();

        RiverChunk instance = Instantiate(prefab, transform);
        instance.name = prefab.name + " (Pooled)";
        _instanceSource[instance] = prefab;
        return instance;
    }

    private void SpawnObstaclesOn(RiverChunk chunk)
    {
        if (obstacleConfig == null) return;

        foreach (var point in chunk.obstacleSpawnPoints)
        {
            if (point == null) continue;
            if (Random.value > obstacleConfig.spawnChancePerPoint) continue;

            GameObject prefab = obstacleConfig.GetRandomObstacle();
            if (prefab == null) continue;

            GameObject obstacle = Instantiate(prefab, point.position, point.rotation, chunk.transform);
            chunk.RegisterSpawnedObstacle(obstacle);
        }
    }
}