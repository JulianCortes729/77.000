using TMPro;
using UnityEngine;

public class GameOverUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelGameOver;

    [Header("HUD Elements to Hide")]
    [SerializeField] private GameObject[] hudElements; // Arrastra aquí windText, resourceText, etc.

    [Header("Game Over Texts")]
    [SerializeField] private TextMeshProUGUI cantHectareasBurnedText;
    [SerializeField] private TextMeshProUGUI cantHectareasIntactText;

    private void Awake()
    {
        panelGameOver.SetActive(false); // Asegurarnos de que inicie apagado
    }

    private void OnEnable()
    {
        GameManager.OnGameEnded += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        GameManager.OnGameEnded -= ShowGameOverScreen;
    }

    private void ShowGameOverScreen(int burnedHectares, int totalHectares)
    {
        // 1. Ocultar la UI de gameplay (HUD)
        foreach (var element in hudElements)
        {
            if (element != null) element.SetActive(false);
        }

        // 2. Mostrar Panel de Game Over
        panelGameOver.SetActive(true);

        // 3. Asignar los valores calculados
        int intactHectares = totalHectares - burnedHectares;

        // Es más limpio sobreescribir el texto completo en lugar de concatenar (+=)
        cantHectareasBurnedText.text = $"Hectáreas Perdidas: {burnedHectares}";
        cantHectareasIntactText.text = $"Hectáreas Intactas: {intactHectares}";
    }
}
