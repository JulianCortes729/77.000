using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField]private FireManager fireManager;
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
        Debug.Log("¡El fuego ha sido extinguido completamente!"); // Aquí puedes implementar la lógica de victoria o transición de escena
        Debug.Log($"Hectáreas quemadas: {fireManager.countBurnedHectares}"); // Muestra el número de hectáreas quemadas como parte de las estadísticas finales
        Time.timeScale = 0f; // Detiene el juego para mostrar el resultado final
    }
}
