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
    [SerializeField] private TextMeshProUGUI windText;
    [SerializeField] private TextMeshProUGUI resourceText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private TextMeshProUGUI hectaresText;



    public static event Action OnGameStarted; // Evento para indicar que el juego ha comenzado
    public static event Action OnGameEnded;


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
        windText.gameObject.SetActive(false); // Oculta el texto del viento
        resourceText.gameObject.SetActive(false); // Oculta el texto de recursos
        controlsText.gameObject.SetActive(false); // Oculta el texto de controles
        hectaresText.gameObject.SetActive(false); // Oculta el texto de hectáreas

        panelGameOver.SetActive(true); // Muestra el panel de Game Over
        cantHectareasBurnedText.text += $"{fireManager.countBurnedHectares}"; // Muestra la cantidad de hectáreas quemadas
        cantHectareasIntactText.text += $"{fireManager.totalHectareas-fireManager.countBurnedHectares}"; // Muestra la cantidad de hectáreas intactas
        OnGameEnded?.Invoke();// Dispara el evento de fin del juego para que otros componentes puedan reaccionar
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Restaura el tiempo normal para reiniciar el juego
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// Recarga la escena actual para reiniciar el juego
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f; // Restaura el tiempo normal para reiniciar el juego
        SceneManager.LoadScene(0); // Carga la escena del menú principal (asumiendo que es la primera escena en el build)
    }

    void Start()
    {
        OnGameStarted?.Invoke(); // Dispara el evento de inicio del juego para que otros componentes puedan reaccionar
    }
}
