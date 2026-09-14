using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FireManager fireManager;

    // Eventos de estado global
    public static event Action OnGameStarted;
    public static event Action<int, int> OnGameEnded; // Devuelve (Hect�reas Quemadas, Hect�reas Totales)

    private void OnEnable()
    {
        if (fireManager != null)
            fireManager.OnFireExtinguished += HandleGameOver;
        
    }

    private void OnDisable()
    {
        if (fireManager != null)
            fireManager.OnFireExtinguished -= HandleGameOver;
    }

    private void Start()
    {
        OnGameStarted?.Invoke();
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f; // Detiene el flujo del juego

        // Emite el evento con la data necesaria para que la UI se encargue de mostrarla
        OnGameEnded?.Invoke(fireManager.CountBurnedHectares, fireManager.TotalHectareas);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}