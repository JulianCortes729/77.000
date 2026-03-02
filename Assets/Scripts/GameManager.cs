using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private FireManager fireManager;
    [SerializeField] private TextMeshProUGUI cantHectareasBurnedText;
    [SerializeField] private TextMeshProUGUI cantHectareasIntactText;
    public static event Action OnGameStarted; // Evento para indicar que el juego ha comenzado
    public static event Action<int> OnGameEnded;


    private void OnEnable()
    {
        fireManager.OnFireExtinguished += HandleGameOver; // Suscribe al evento de fuego extinguido
    }

    private void OnDisable()
    {
        fireManager.OnFireExtinguished -= HandleGameOver; // Desuscribe para evitar fugas de memoria
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f; // Detiene el juego para mostrar el resultado final
        panelGameOver.SetActive(true); // Muestra el panel de Game Over
        cantHectareasBurnedText.text += $"{fireManager.countBurnedHectares}"; // Muestra la cantidad de hectáreas quemadas
        cantHectareasIntactText.text += $"{fireManager.totalHectareas-fireManager.countBurnedHectares}"; // Muestra la cantidad de hectáreas intactas
        OnGameEnded?.Invoke(fireManager.countBurnedHectares); // Dispara el evento de fin del juego pasando la cantidad de hectáreas quemadas
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Restaura el tiempo normal para reiniciar el juego
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// Recarga la escena actual para reiniciar el juego
    }

    void Start()
    {
        OnGameStarted?.Invoke(); // Dispara el evento de inicio del juego para que otros componentes puedan reaccionar
    }
}
