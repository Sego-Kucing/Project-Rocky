using UnityEngine;
using UnityEngine.Events;

public class StoneController : MonoBehaviour
{
    public enum State { Aiming, Charging, Flying, Sunk }

    [System.Serializable] public class BounceEvent : UnityEvent<int> { }

    [Header("References")]
    [Tooltip("What rotates to show current aim direction. Can be this stone itself, or a separate arrow/indicator child.")]
    public Transform aimPivot;

    [Header("Aim Settings")]
    [Tooltip("Max degrees left/right from forward the aim can rotate.")]
    public float maxAimAngle = 45f;
    [Tooltip("Degrees per second the aim rotates while holding A/D or Left/Right.")]
    public float aimRotateSpeed = 90f;

    [Header("Power Settings")]
    [Tooltip("How fast the power value oscillates between 0 and 1 while charging.")]
    public float powerOscillateSpeed = 1.5f;
    public float minThrowSpeed = 8f;
    public float maxThrowSpeed = 22f;
    [Tooltip("Upward angle (degrees) added to the throw. Keep this low (5-10) for a flat, realistic skim instead of a big lob.")]
    public float launchElevationAngle = 8f;

    [Header("Flight Settings")]
    public float gravity = 20f;
    [Tooltip("Acceleration applied while holding Up/W (or Down/S, inverted). IMPORTANT: keep this LOWER than gravity - holding Up should only soften a fall / stretch out a rise, it should never be able to cause a net climb on its own.")]
    public float verticalInputForce = 12f;
    [Tooltip("Max vertical speed allowed (up or down) during flight.")]
    public float maxVerticalSpeed = 10f;
    [Tooltip("Side movement speed while holding A/D or Left/Right during flight.")]
    public float horizontalMoveSpeed = 6f;

    [Header("Water & Bounce")]
    [Tooltip("World Y height of the water surface the stone bounces on.")]
    public float waterLevel = 0f;
    [Tooltip("Upward kick strength on a clean, gentle touch, before decay/softness are applied.")]
    public float baseBounceKick = 7f;
    [Tooltip("If downward speed on impact exceeds this, the stone hits too hard and sinks instead of skipping - like a real stone digging in instead of skimming.")]
    public float maxSafeImpactSpeed = 6f;
    [Tooltip("Each bounce's kick is multiplied by this (compounding), so skips naturally get lower over time even with perfect input.")]
    [Range(0f, 1f)] public float decayPerBounce = 0.85f;
    [Tooltip("Bounces needed before the trampoline boost should trigger (handled by another system via onReadyForBoost).")]
    public int bouncesBeforeBoost = 4;

    [Header("Events")]
    public UnityEvent onLaunch;
    public BounceEvent onBounce;
    public UnityEvent onReadyForBoost;
    public UnityEvent onSunk;

    public State CurrentState { get; private set; } = State.Aiming;
    public float CurrentPower01 { get; private set; }
    public int BounceCount { get; private set; }

    private float _aimAngle;
    private Vector3 _velocity;

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Aiming: HandleAiming(); break;
            case State.Charging: HandleCharging(); break;
            case State.Flying: HandleFlying(); break;
        }
    }

    private void HandleAiming()
    {
        float turn = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turn -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turn += 1f;

        _aimAngle = Mathf.Clamp(_aimAngle + turn * aimRotateSpeed * Time.deltaTime, -maxAimAngle, maxAimAngle);

        if (aimPivot != null)
            aimPivot.localRotation = Quaternion.Euler(0f, _aimAngle, 0f);

        if (Input.GetKeyDown(KeyCode.Space))
            CurrentState = State.Charging;
    }

    private void HandleCharging()
    {
        CurrentPower01 = Mathf.PingPong(Time.time * powerOscillateSpeed, 1f);

        if (Input.GetKeyDown(KeyCode.Space))
            Launch();
    }

    private void Launch()
    {
        float throwSpeed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, CurrentPower01);
        Quaternion direction = Quaternion.Euler(-launchElevationAngle, _aimAngle, 0f);

        _velocity = direction * Vector3.forward * throwSpeed;
        BounceCount = 0;
        CurrentState = State.Flying;
        onLaunch?.Invoke();
        Debug.Log($"Stone launched - speed: {throwSpeed:F1}, aim angle: {_aimAngle:F1}");
    }

    private void HandleFlying()
    {
        float vertical = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;

        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        _velocity.y += (vertical * verticalInputForce - gravity) * Time.deltaTime;
        _velocity.y = Mathf.Clamp(_velocity.y, -maxVerticalSpeed, maxVerticalSpeed);

        Vector3 sideOffset = Vector3.right * horizontal * horizontalMoveSpeed * Time.deltaTime;
        Vector3 nextPosition = transform.position + _velocity * Time.deltaTime + sideOffset;

        if (nextPosition.y <= waterLevel)
        {
            nextPosition.y = waterLevel;
            ResolveBounce();
        }

        transform.position = nextPosition;
    }

    private void ResolveBounce()
    {
        float incomingDownSpeed = Mathf.Abs(_velocity.y);

        if (incomingDownSpeed > maxSafeImpactSpeed)
        {
            CurrentState = State.Sunk;
            _velocity = Vector3.zero;
            onSunk?.Invoke();
            Debug.Log($"Stone sunk - hit too hard ({incomingDownSpeed:F1} > {maxSafeImpactSpeed:F1}). Ease off Up a bit before touchdown next time.");
            return;
        }

        // Gentler touches get a kick closer to baseBounceKick; harder (but still safe) touches get a smaller one.
        float softness = 1f - (incomingDownSpeed / maxSafeImpactSpeed);
        float decay = Mathf.Pow(decayPerBounce, BounceCount);
        float kick = baseBounceKick * Mathf.Lerp(0.4f, 1f, softness) * decay;

        _velocity.y = kick;
        BounceCount++;
        onBounce?.Invoke(BounceCount);
        Debug.Log($"Bounce #{BounceCount} - impact: {incomingDownSpeed:F1}, kick: {kick:F1}");

        if (BounceCount >= bouncesBeforeBoost)
        {
            onReadyForBoost?.Invoke();
            Debug.Log("Ready for trampoline boost!");
        }
    }

    /// <summary>Resets the stone back to the aiming state - call this on restart.</summary>
    public void ResetToAiming(Vector3 startPosition)
    {
        transform.position = startPosition;
        _velocity = Vector3.zero;
        _aimAngle = 0f;
        BounceCount = 0;
        CurrentState = State.Aiming;
    }
}