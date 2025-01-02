using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private int targetWidth = 1366;
    private int targetHeight = 768;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetResolution();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void SetResolution()
    {
        Screen.SetResolution(targetWidth, targetHeight, false);
        Screen.fullScreen = false; // Ensure it's always windowed
    }

    // Ensure the resolution is set correctly when a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetResolution();
    }

    // Cleanup when the script is destroyed
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
