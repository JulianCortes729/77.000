using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI presupuesto; // Referencia al TextMeshProUGUI para mostrar el presupuesto actual

    [SerializeField] private ResourceManager resourceManager; // Referencia al sistema de recursos para mostrar el presupuesto

    private void Start()
    {
        presupuesto.text = $"Presupuesto: {resourceManager.GetCurrentBudget()}"; // Inicializa el texto del presupuesto con el valor actual al iniciar
    }

    private void OnDisable()
    {
        resourceManager.OnBudgetChanged -= UpdateBudgetDisplay; // Desuscribe para evitar fugas de memoria   
    }

    private void OnEnable()
    {
        resourceManager.OnBudgetChanged += UpdateBudgetDisplay; // Suscribe al evento para actualizar la UI cuando cambie el presupuesto
    }

    void UpdateBudgetDisplay(int newBudget)
    {
        // Aquí implementaremos la lógica para actualizar la UI con el nuevo presupuesto
        // Por ejemplo, podríamos tener un TextMeshProUGUI que muestre el presupuesto actual
        // y actualizar su texto con el nuevo valor.
        presupuesto.text = $"Presupuesto: {newBudget}"; // Actualiza el texto del presupuesto con el nuevo valor
    }
}
