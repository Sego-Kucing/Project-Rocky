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
}
