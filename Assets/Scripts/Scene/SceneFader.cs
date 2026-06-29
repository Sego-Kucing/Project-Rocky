using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    // Singleton agar script ini bisa dipanggil dari script manapun tanpa perlu Drag & Drop
    public static SceneFader Instance;

    [Header("Fade Settings")]
    [Tooltip("Masukkan komponen CanvasGroup dari UI Panel hitam ke sini")]
    public CanvasGroup fadeCanvasGroup;
    [Tooltip("Kecepatan default transisi (detik)")]
    public float defaultFadeDuration = 1.5f;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeRoutine(1f, 0f, defaultFadeDuration, string.Empty));
        }
    }

    public void FadeAndLoadScene(string sceneName, float duration)
    {
        if (fadeCanvasGroup != null)
        {
            // Cegah interaksi UI apa pun selama layar menggelap
            fadeCanvasGroup.blocksRaycasts = true; 
            StartCoroutine(FadeRoutine(0f, 1f, duration, sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, string sceneToLoad)
    {
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}