using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TeleBehaviour : MonoBehaviour
{
    [Header("=== Target Settings ===")]
    [Tooltip("Object yang akan dipindahkan (contoh: Player / Nayla)")]
    public Transform targetTransform;

    [Tooltip("Lokasi tujuan perpindahan")]
    public Transform targetLocation;

    // ─────────────────────────────────────────────────────────
    [Header("=== Movement Settings ===")]

    [Tooltip("Jika dicentang → gerak smooth ke tujuan.\n" +
             "Jika tidak → langsung teleport instan.")]
    public bool useMovement = false;

    [Tooltip("Kecepatan gerak menuju targetLocation (unit/detik).\n" +
             "Aktif hanya jika Use Movement dicentang.")]
    [Min(0.1f)]
    public float moveSpeed = 5f;

    // ─────────────────────────────────────────────────────────
    [Header("=== Delay Settings ===")]

    [Tooltip("Aktifkan jika ingin ada jeda sebelum bergerak/teleport")]
    public bool useDelayedStart = false;

    [Tooltip("Berapa detik jeda sebelum bergerak (aktif jika Use Delayed Start dicentang)")]
    public float delayedTime = 1f;

    // ─────────────────────────────────────────────────────────
    [Header("=== On Complete Move ===")]

    [Tooltip("Event yang dijalankan setelah sampai di tujuan")]
    public UnityEvent onCompleteMove;

    // ── Private ───────────────────────────────────────────────
    private Vector3    _initialPosition;
    private Quaternion _initialRotation;
    private bool       _isMoving = false;


    void Start()
    {
        if (targetTransform != null)
        {
            _initialPosition = targetTransform.position;
            _initialRotation = targetTransform.rotation;
        }
    }

    public void BeginTeleport()
    {
        if (_isMoving) return;

        if (useDelayedStart)
            StartCoroutine(RunAfterDelay(() => ExecuteMove(targetLocation)));
        else
            ExecuteMove(targetLocation);
    }

    public void BeginTeleportInitialPosition()
    {
        if (_isMoving) return;

        if (useDelayedStart)
            StartCoroutine(RunAfterDelay(DoTeleportToInitial));
        else
            DoTeleportToInitial();
    }

    public void StopMove()
    {
        StopAllCoroutines();
        _isMoving = false;

        if (targetTransform != null)
        {
            CharacterController cc = targetTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
        }
    }

    void ExecuteMove(Transform destination)
    {
        if (targetTransform == null || destination == null)
        {
            Debug.LogWarning("[TeleBehaviour] Target Transform atau Target Location belum di-assign!");
            return;
        }

        if (useMovement)
            StartCoroutine(SmoothMoveRoutine(destination.position, destination.rotation));
        else
            InstantTeleport(destination.position, destination.rotation);
    }

    void DoTeleportToInitial()
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("[TeleBehaviour] Target Transform belum di-assign!");
            return;
        }

        if (useMovement)
            StartCoroutine(SmoothMoveRoutine(_initialPosition, _initialRotation));
        else
            InstantTeleport(_initialPosition, _initialRotation);
    }


    void InstantTeleport(Vector3 position, Quaternion rotation)
    {
        CharacterController cc = targetTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        targetTransform.position = position;
        targetTransform.rotation = rotation;

        if (cc != null) cc.enabled = true;

        onCompleteMove?.Invoke();
    }


    IEnumerator SmoothMoveRoutine(Vector3 destination, Quaternion destRotation)
    {
        _isMoving = true;

        CharacterController cc = targetTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        while (Vector3.Distance(targetTransform.position, destination) > 0.05f)
        {
            targetTransform.position = Vector3.MoveTowards(
                targetTransform.position,
                destination,
                moveSpeed * Time.deltaTime
            );

            targetTransform.rotation = Quaternion.Slerp(
                targetTransform.rotation,
                destRotation,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        targetTransform.position = destination;
        targetTransform.rotation = destRotation;

        if (cc != null) cc.enabled = true;

        _isMoving = false;
        onCompleteMove?.Invoke();
    }

    IEnumerator RunAfterDelay(System.Action action)
    {
        yield return new WaitForSeconds(delayedTime);
        action?.Invoke();
    }
}