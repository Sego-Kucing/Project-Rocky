using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPowerUp : MonoBehaviour
{
    // --- TAMBAHAN: Kategori Power Up ---
    public enum PowerUpType { Movement, Shield }

    [System.Serializable]
    public class PowerUpConfig
    {
        public string powerUpTag;
        
        [Tooltip("Pilih tipe power up. Movement (Rocket/Wings) tidak bisa ditumpuk. Shield bisa ditumpuk dengan Movement.")]
        public PowerUpType powerUpType; 
        
        public GameObject powerUpChild;
        public float duration = 10f;

        [Header("Physics Settings (Hanya untuk tipe Movement)")]
        [Tooltip("Berapa kuat dorongan maju dari Rocket?")]
        public float rocketBoostForce = 15f;
        [Tooltip("Berapa kuat dorongan maju saat sayap sedang menukik/turun?")]
        public float wingGlideMultiplier = 1.5f;
    }

    [Header("Power Up Settings")]
    public List<PowerUpConfig> availablePowerUps = new List<PowerUpConfig>();

    private Rigidbody rb;
    
    // --- State untuk kategori MOVEMENT (Rocket / Wings) ---
    private Coroutine activeMovementRoutine;
    private GameObject currentMovementChild;
    private string activeMovementTag = "";
    private PowerUpConfig currentMovementConfig;

    // --- State untuk kategori SHIELD ---
    private Coroutine activeShieldRoutine;
    private GameObject currentShieldChild;
    [HideInInspector] public bool isShieldActive = false; // Bisa diakses script lain kalau butuh kebal

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Pastikan semua child mati di awal
        foreach (var config in availablePowerUps)
        {
            if (config.powerUpChild != null)
            {
                config.powerUpChild.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        foreach (var config in availablePowerUps)
        {
            if (other.CompareTag(config.powerUpTag))
            {
                Destroy(other.gameObject);
                ApplyPowerUp(config);
                break; 
            }
        }
    }

    private void ApplyPowerUp(PowerUpConfig config)
    {
        if (config.powerUpType == PowerUpType.Movement)
        {
            // Matikan Movement yang lama (misal lagi roket, diganti sayap)
            if (activeMovementRoutine != null) StopCoroutine(activeMovementRoutine);
            if (currentMovementChild != null) currentMovementChild.SetActive(false);
            
            activeMovementRoutine = StartCoroutine(MovementRoutine(config));
        }
        else if (config.powerUpType == PowerUpType.Shield)
        {
            // Matikan Shield yang lama (kalau ambil shield lagi, durasinya keriset)
            if (activeShieldRoutine != null) StopCoroutine(activeShieldRoutine);
            if (currentShieldChild != null) currentShieldChild.SetActive(false);
            
            activeShieldRoutine = StartCoroutine(ShieldRoutine(config));
        }
    }

    private IEnumerator MovementRoutine(PowerUpConfig config)
    {
        currentMovementChild = config.powerUpChild;
        if (currentMovementChild != null) currentMovementChild.SetActive(true);
        
        // Simpan data untuk dibaca di FixedUpdate
        activeMovementTag = config.powerUpTag;
        currentMovementConfig = config;

        yield return new WaitForSeconds(config.duration);

        // Reset setelah durasi habis
        if (currentMovementChild != null) currentMovementChild.SetActive(false);
        currentMovementChild = null;
        activeMovementTag = "";
        currentMovementConfig = null;
    }

    private IEnumerator ShieldRoutine(PowerUpConfig config)
    {
        currentShieldChild = config.powerUpChild;
        if (currentShieldChild != null) currentShieldChild.SetActive(true);
        
        isShieldActive = true;

        yield return new WaitForSeconds(config.duration);

        RemoveShield();
    }

    public void RemoveShield()
    {
        if (!isShieldActive) return;

        if (currentShieldChild != null) currentShieldChild.SetActive(false);
        if (activeShieldRoutine != null) StopCoroutine(activeShieldRoutine);
        
        isShieldActive = false;
        currentShieldChild = null;
    }

    void FixedUpdate()
    {
        if (string.IsNullOrEmpty(activeMovementTag) || currentMovementConfig == null) return;

        // CEK ROCKET
        if (activeMovementTag.Contains("Rocket"))
        {
            // Rocket dorong terus ke depan
            Vector3 forwardDirection = transform.forward;
            forwardDirection.y = 0; 
            
            rb.AddForce(forwardDirection.normalized * currentMovementConfig.rocketBoostForce, ForceMode.Acceleration);
        }
        else if (activeMovementTag.Contains("Wings"))
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 8f, ForceMode.Acceleration);
            }
            else if (rb.linearVelocity.y < 0)
            {
                Vector3 glideForce = transform.forward * Mathf.Abs(rb.linearVelocity.y) * currentMovementConfig.wingGlideMultiplier;
                rb.AddForce(glideForce, ForceMode.Acceleration);
                
                Vector3 currentVel = rb.linearVelocity;
                currentVel.y *= 0.92f; 
                rb.linearVelocity = currentVel;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Kalau shield sedang aktif dan kita nabrak sesuatu (seperti nabrak air/mantul)
        if (isShieldActive)
        {
            RemoveShield();
            
            Debug.Log("Shield hancur karena benturan!");
        }
    }
}