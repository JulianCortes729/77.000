using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int initialBudget = 100000;
    [SerializeField] private int costPerFirewall = 100;

    // Propiedad auto-implementada con setter privado
    public int CurrentBudget { get; private set; }

    public event Action<int> OnBudgetChanged;

    private void Awake()
    {
        CurrentBudget = initialBudget;
    }

    private void Start()
    {
        // Emitimos el presupuesto inicial para que la UI se configure de entrada
        OnBudgetChanged?.Invoke(CurrentBudget);
    }

    public bool TrySpendBudget()
    {
        if (CurrentBudget >= costPerFirewall)
        {
            CurrentBudget -= costPerFirewall;
            OnBudgetChanged?.Invoke(CurrentBudget);
            return true;
        }
        return false;
    }

    public int GetCostPerFirewall() => costPerFirewall;
}
