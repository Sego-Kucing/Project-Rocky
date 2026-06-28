using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPowerUp : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpConfig
    {
        [Tooltip("Tag dari powerup object")]
        public string powerupTag;
        [Tooltip("Child object visual/mekanik yang akan dinyalakan")]
        public GameObject powerUpChild;
        [Tooltip("Durasi power up menyala")]
        public float duration = 10f;
    }

    [Header("Power Up Settings")]
    [Tooltip("Daftar semua power up yang bisa di ambil si batu")]
    public List<PowerUpConfig> availablePowerUps = new List<PowerUpConfig>();

    private Rigidbody rb;
    private Coroutine activePowerUpRoutine;
    private GameObject currentActiveChild;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            if (other.CompareTag(config.powerupTag))
            {
                Destroy(other.gameObject);
                ApplyPowerUp(config);
                break;
            }
        }
    }

    private void ApplyPowerUp(PowerUpConfig config)
    {
        if (activePowerUpRoutine != null)
        {
            StopCoroutine(activePowerUpRoutine);
        }
        if (currentActiveChild != null)
        {
            currentActiveChild.SetActive(false);
        }

        activePowerUpRoutine = StartCoroutine(PowerUpRoutine(config));
    }

    private IEnumerator PowerUpRoutine(PowerUpConfig config)
    {
        if (config.powerUpChild == null) yield break;

        currentActiveChild = config.powerUpChild;
        currentActiveChild.SetActive(true);

        // Nunggu durasi habis
        yield return new WaitForSeconds(config.duration);

        // Matiin lagi prefabnya
        if (currentActiveChild != null)
        {
            currentActiveChild.SetActive(false);
            currentActiveChild = null;
        }
    }
}
