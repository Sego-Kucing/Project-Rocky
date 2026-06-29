using UnityEngine;

public class StoneCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Masukin object batu ke sini")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("Jarak kamera dari batu, X: Kiri/Kanan, Y: Atas/Bawah, Z: Depan/Belakang")]
    public Vector3 offset = new Vector3(0f, -3f, -7f);
    public float smoothSpeed = 10f;

    [Header("Cinemachine Integration")]
    public bool overrideCinemachine = true;

    private Behaviour cinemachineBrain;
    private Rigidbody targetRb;
    private float currentYaw;

    void Start()
    {
        cinemachineBrain = GetComponent("CinemachineBrain") as Behaviour;

        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            currentYaw = target.eulerAngles.y;
            Quaternion flatRotation = Quaternion.Euler(0, currentYaw, 0);
            transform.position = target.position + (flatRotation * offset);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        // Memastikan cinemachine mati terlebih dahulu
        if (overrideCinemachine && cinemachineBrain != null && cinemachineBrain.enabled)
        {
            cinemachineBrain.enabled = false;
        }

        if (targetRb != null && targetRb.linearVelocity.sqrMagnitude > 2f)
        {
            Vector3 flatVelocity = new Vector3(targetRb.linearVelocity.x, 0, targetRb.linearVelocity.z);
            if (flatVelocity != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVelocity);
                currentYaw = Mathf.LerpAngle(currentYaw, targetRot.eulerAngles.y, Time.deltaTime * 5f);
            }
            else
            {
                currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, Time.deltaTime * 10f);
            }
        }

        Quaternion finalRotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 desiredPosition = target.position + (finalRotation * offset);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }

    public void ActivateGameplayCamera(bool activate)
    {
        this.enabled = activate;
        
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = !activate;
        }
    }
}
