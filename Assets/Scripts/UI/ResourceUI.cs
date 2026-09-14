using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI presupuesto; // Referencia al TextMeshProUGUI para mostrar el presupuesto actual

    [SerializeField] private ResourceManager resourceManager; // Referencia al sistema de recursos para mostrar el presupuesto



    private void OnEnable()
    {
        if (resourceManager != null)
        {
            resourceManager.OnBudgetChanged += UpdateBudgetDisplay;
        }
    }

    private void OnDisable()
    {
        if (resourceManager != null)
        {
            resourceManager.OnBudgetChanged -= UpdateBudgetDisplay;
        }
    }
    void UpdateBudgetDisplay(int newBudget)
    {
        // Aquí implementaremos la lógica para actualizar la UI con el nuevo presupuesto
        // Por ejemplo, podríamos tener un TextMeshProUGUI que muestre el presupuesto actual
        // y actualizar su texto con el nuevo valor.
        presupuesto.text = $"Presupuesto: {newBudget}"; // Actualiza el texto del presupuesto con el nuevo valor
    }
}
