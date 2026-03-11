using System;
using TMPro;
using UnityEngine;

public class HectareaUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI burnedHectareasCountText;

    private void OnEnable()
    {
        FireManager.OnBurnedHectaresCountChanged += UpdateBurnedHectareasCount;
    }

    private void OnDisable()
    {
        FireManager.OnBurnedHectaresCountChanged -= UpdateBurnedHectareasCount;
    }

    private void UpdateBurnedHectareasCount(int cont)
    {
        burnedHectareasCountText.text = $"Hectáreas Perdidas: {cont}";
    }
}
