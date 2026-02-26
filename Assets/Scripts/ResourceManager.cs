using System;
using UnityEngine;



public class ResourceManager : MonoBehaviour
{
    private int currentBudget; // Variable para almacenar el presupuesto actual
    public event Action<int> OnBudgetChanged; // Evento para notificar cambios en el presupuesto
    private int costPerFirewall = 100; // Costo fijo por cada cortafuegos colocado

    void Awake()
    {
        currentBudget = 5000; // Inicializa el presupuesto con un valor predeterminado
    }

    public bool TrySpendBudget()
    {
        // Aquí implementaremos la lógica para gastar el presupuesto
        // Por ejemplo, podríamos tener una variable que almacene el presupuesto actual
        // y verificar si es suficiente para gastar la cantidad solicitada.
        int presupuestoRestante = currentBudget - costPerFirewall; // Calcula el presupuesto restante después de gastar la cantidad  
        if (presupuestoRestante >= 0 )
        {
            currentBudget -= costPerFirewall; // Resta la cantidad al presupuesto actual
            OnBudgetChanged?.Invoke(currentBudget); // Dispara el evento para notificar el cambio en el presupuesto
            return true; // Devuelve true si se pudo gastar el presupuesto
        }
        return false;
    }

    public int GetCurrentBudget()
    {
        return currentBudget; // Devuelve el presupuesto actual
    }
    public int GetCostPerFirewall()
    {
        return costPerFirewall;
    }
}
