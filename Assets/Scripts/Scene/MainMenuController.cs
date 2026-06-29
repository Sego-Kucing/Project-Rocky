using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Ketik nama scene gameplay kamu persis seperti di Build Settings")]
    public string gameplaySceneName = "Lake";
    
    [Tooltip("Durasi layar menggelap sebelum pindah scene")]
    public float fadeOutDuration = 1.5f;
    public void StartGame()
    {
        // Cek apakah ada SceneFader di scene ini
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeAndLoadScene(gameplaySceneName, fadeOutDuration);
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game keluar...");
        Application.Quit();
    }
}