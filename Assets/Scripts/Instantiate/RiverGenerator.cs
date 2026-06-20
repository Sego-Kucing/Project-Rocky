using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Mengubah posisi Z menjadi chunk spawning untuk di ikuti player")]
    public Transform tracker;

    [Tooltip(" Setiap kali dibutuhkan chunk baru, salah satunya dipilih secara acak.")]
    public RiverChunk[] chunkPrefabs;
    public ObstacleSpawnConfig obstacleConfig;

    [Header("Generation Settings")]
    [Tooltip("Berapa chunk yang bakal tetep aktif di depan player")]
    public int activeChunkCount = 4;

    [Tooltip("Tambahan jarak di belakang player, sebelum ngelewatin chunknya")]
    public float recycleBuffer = 10f;

    private readonly Dictionary<RiverChunk, Queue<RiverChunk>> _pools = new Dictionary<RiverChunk, Queue<RiverChunk>>();
    private readonly Dictionary<RiverChunk, RiverChunk> _instanceSource = new Dictionary<RiverChunk, RiverChunk>();
    private readonly Queue<RiverChunk> _activeChunks = new Queue<RiverChunk>();

    private float _nextSpawnZ;
    private void Start()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("RiverGenerator: no chunk prefabs assigned");
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
    }
}
