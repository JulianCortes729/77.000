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
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Restaura el tiempo normal para reiniciar el juego
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);// Recarga la escena actual para reiniciar el juego
    }
}
