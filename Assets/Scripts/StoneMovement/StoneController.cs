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
    [Tooltip("Upward angle (degrees) added to the throw so the stone arcs forward instead of going flat.")]
    public float launchElevationAngle = 15f;

    [Header("Flight Settings")]
    public float gravity = 18f;
    [Tooltip("Acceleration applied while holding Up/W (or Down/S, inverted) during flight.")]
    public float verticalInputForce = 25f;
    [Tooltip("Max vertical speed allowed (up or down) during flight.")]
    public float maxVerticalSpeed = 12f;
    [Tooltip("Side movement speed while holding A/D or Left/Right during flight.")]
    public float horizontalMoveSpeed = 6f;

    [Header("Water & Bounce")]
    [Tooltip("World Y height of the water surface the stone bounces on.")]
    public float waterLevel = 0f;
    [Tooltip("Upward kick on bounce = downward speed on impact * this. Below 1 = loses energy each bounce.")]
    [Range(0f, 1f)] public float bounceDamping = 0.7f;
    [Tooltip("If the bounce kick would be weaker than this, the stone sinks instead.")]
    public float minBounceVelocity = 3f;
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
        float kick = incomingDownSpeed * bounceDamping;

        if (kick < minBounceVelocity)
        {
            CurrentState = State.Sunk;
            _velocity = Vector3.zero;
            onSunk?.Invoke();
            Debug.Log("Stone sunk - bounce kick too weak.");
            return;
        }

        _velocity.y = kick;
        BounceCount++;
        onBounce?.Invoke(BounceCount);
        Debug.Log($"Bounce #{BounceCount} - kick: {kick:F1}");

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