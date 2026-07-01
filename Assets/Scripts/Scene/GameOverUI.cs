using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Transition Settings")]
    public bool useFadeTransition = true;
    public float fadeDuration = 1.5f;
    [Tooltip("Kosongkan saja agar otomatis me-reload scene saat ini")]
    public string targetSceneName = "";

    // --- SISTEM MEMORI SEDERHANA ---
    // Variabel static ini tidak akan terhapus meskipun scene di-reload dari nol
    private static bool isRestarting = false;

    // Daftarkan event saat Scene baru selesai di-load
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeSceneLoadEvent()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Jika masuk scene ini karena klik "New Run"
        if (isRestarting)
        {
            isRestarting = false; // Reset status memori

            // BYPASS CINEMATIC (LANGSUNG AIMING)
            StoneCamera stoneCam = Object.FindFirstObjectByType<StoneCamera>();
            if (stoneCam != null)
            {
                // Pastikan kamera nyala (kalau sebelumnya diset mati oleh temenmu)
                stoneCam.enabled = true; 
                // Set waktu cinematic jadi 0 agar langsung masuk state Aiming
                stoneCam.cinematicDuration = 0f; 
            }
        }
    }

    // Fungsi ini dipanggil saat tombol "New Run" diklik
    public void RestartCurrentLevel()
    {
        // Titip pesan rahasia bahwa kita mau skip cinematic
        isRestarting = true;
        ExecuteTransition();
    }

    // Fungsi ini dipanggil saat tombol "Main Menu" diklik
    public void ReturnToMainMenu()
    {
        // Normal load (menu utama akan muncul, cinematic akan jalan)
        isRestarting = false;
        ExecuteTransition();
    }

    // Fungsi bantuan untuk menjalankan efek transisi
    private void ExecuteTransition()
    {
        // 1. SEMBUNYIKAN UI GAME OVER seketika
        gameObject.SetActive(false);

        // 2. BEKUKAN BATU (agar melayang rapi saat transisi layar gelap)
        GameObject rocky = GameObject.Find("Rocky");
        if (rocky != null)
        {
            Rigidbody rb = rocky.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true; 
        }

        // 3. Pastikan waktu berjalan normal
        Time.timeScale = 1f;

        // 4. MULAI TRANSISI / RELOAD SCENE
        string sceneToLoad = string.IsNullOrEmpty(targetSceneName) ? SceneManager.GetActiveScene().name : targetSceneName;

        if (useFadeTransition && SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeAndLoadScene(sceneToLoad, fadeDuration);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}