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
}
