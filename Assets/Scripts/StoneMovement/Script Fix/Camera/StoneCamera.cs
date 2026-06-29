using UnityEngine;

public class StoneCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Masukkan object Rocky ke sini")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("Jarak kamera dari batu. X: Kiri/Kanan, Y: Atas/Bawah, Z: Depan/Belakang")]
    public Vector3 offset = new Vector3(0f, 3f, -7f);
    
    [Tooltip("Seberapa cepat kamera menyusul batu. Semakin besar = semakin kaku nempelnya.")]
    public float smoothSpeed = 10f;

    [Header("Cinematic Settings")]
    [Tooltip("Drag object Main Camera (yang punya komponen CinemachineBrain) ke sini")]
    public Behaviour cinemachineBrain;
    
    [Tooltip("Durasi cinematic berjalan (detik) sebelum kamera ngikutin batu. Isi 0 kalau tidak ada cinematic.")]
    public float cinematicDuration = 3f;

    [Tooltip("Otomatis mematikan Cinemachine Brain saat gameplay dimulai")]
    public bool overrideCinemachine = true;

    private Rigidbody targetRb;
    private float currentYaw;
    private bool isCinematicPlaying = false;

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            currentYaw = target.eulerAngles.y;
        }

        // Kalau durasi cinematic lebih dari 0, tunggu dulu. Kalau tidak, langsung mulai.
        if (cinematicDuration > 0f)
        {
            isCinematicPlaying = true;
            // Jalankan fungsi StartGameplayCamera setelah waktu cinematic habis
            Invoke(nameof(StartGameplayCamera), cinematicDuration);
        }
        else
        {
            StartGameplayCamera();
        }
    }

    // Fungsi ini akan dipanggil otomatis setelah cinematic selesai
    public void StartGameplayCamera()
    {
        isCinematicPlaying = false;

        // Matikan Cinemachine
        if (overrideCinemachine && cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

        if (target != null)
        {
            Quaternion flatRotation = Quaternion.Euler(0, currentYaw, 0);
            transform.position = target.position + (flatRotation * offset);
        }
    }

    void LateUpdate()
    {
        if (target == null || isCinematicPlaying) return;
        
        if (targetRb != null && targetRb.linearVelocity.sqrMagnitude > 2f)
        {
            Vector3 flatVelocity = new Vector3(targetRb.linearVelocity.x, 0, targetRb.linearVelocity.z);
            if (flatVelocity != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVelocity);
                currentYaw = Mathf.LerpAngle(currentYaw, targetRot.eulerAngles.y, Time.deltaTime * 5f);
            }
        }
        else
        {
            currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, Time.deltaTime * 10f);
        }

        Quaternion finalRotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 desiredPosition = target.position + (finalRotation * offset);

        // Gerakkan kamera secara mulus ke posisi yang dituju
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