using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;

public class StoneController : MonoBehaviour
{
    public enum State { Aiming, Charging, Flying, Sunk }
    [System.Serializable] public class BounceEvent : UnityEvent<int> { }

    [Header("References")]
    [Tooltip("Pivot point buat aiming si batu")]
    public Transform aimPivot;

    [Header("Aim Settings")]
    [Tooltip("Max Degree left / right forward aiming to rotate")]
    public float maxAimAngle = 45f;
    [Tooltip("Degress per second to rotate the aim")]
    public float aimRotateSpeed = 90f;

    [Header("Power Settings")]
    [Tooltip("Max power to throw the stone")]
    public float powerOscillateSpeed = 1.5f;
    public float minThrowSpeed = 8f;
    public float maxThrowSpeed = 22f;
    [Tooltip("Upward force to apply to the stone when thrown")]
    public float launchElevationAngle = 15f;

    [Header("Flight Settings")]
    public float gravity = 18f;
    [Tooltip("Accelaration during hit w/s")]
    [Range(0f, 1f)] public float verticalInputForce = 25f;
    public float maxVerticalSpeed = 12f;
    public float horizontalMoveSpeed = 6f;

    [Header("Water & Bounce Settings")]
    [Tooltip("World Y Height of the water surface")]
    public float waterLevel = 0f;
    [Tooltip("If the bounce is too low, the stone will sink")]
    public float minBounceVelocity =3f;
    [Tooltip("If bounce need before the trampoline boost should trigger")]
    public int bouncesBeforeBoost = 4;

    [Header("Events")]
    public UnityEvent onLaunch;
    public BounceEvent onBounce;
    public UnityEvent onReadyForBoost;
    public UnityEvent onSunk;

    public State CurrentState { get; private set; } = State.Aiming;
    public float CurretPower01 { get; private set; }
    public int BounceCount { get; private set; }

    private float _aimAngle;
    private Vector3 _velocity;

    private void Update()
    {
        switch(CurrentState)
        {
            case State.Aiming: HandleAiming(); break;
            case State.Charging: HandleCharging(); break;
            case State.Flying: HandleFlying(); break;
        }
    }

    private void HandleAiming()
    {
        float turn = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turn -= -1f;
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
        Debug.Log($"Batunya terbang - speed {throwSpeed:F1}, aim angle: {_aimAngle:F1}");
    }   
}
